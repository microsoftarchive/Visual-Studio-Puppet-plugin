

using System.Collections.Generic;

namespace MicrosoftOpenTech.PuppetProject
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Project.Automation;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(PuppetForgeToolWindow))]
    [ProvideProjectFactory(typeof(PuppetProjectFactory),
        null,
        "Puppet Module Files (*.ppm);*.ppm",
        "ppm",
        "ppm",
        @".\\NullPath",
        LanguageVsTemplate = "PuppetLab")]
    [ProvideObject(typeof(GeneralPropertyPage))]

    [Guid(GuidList.guidPuppetProjectPkgString)]
    public sealed class PuppetProjectPackage : ProjectPackage
    {

        public EnvDTE.DTE DteService { get; private set; }
        public IVsOutputWindowPane OutputWindow {  get; private set; }

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public PuppetProjectPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(PuppetForgeToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        public override string ProductUserContext
        {
            get { return ""; }
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            this.RegisterProjectFactory(new PuppetProjectFactory(this));
            this.DteService = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE)); 

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                var menuCommandId = new CommandID(GuidList.guidPuppetProjectCmdSet, (int)PkgCmdIDList.cmdidCreatePuppetForgeModule);
                var menuItem = new MenuCommand(PublishModuleToPuppetForge, menuCommandId);

                mcs.AddCommand( menuItem );

                // Create the command for the tool window
                var toolwndCommandId = new CommandID(GuidList.guidPuppetProjectCmdSet, (int)PkgCmdIDList.cmdidPuppetForgeWindow);
                var menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandId);
                mcs.AddCommand( menuToolWin );

               // Get Output Window.
                var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                Guid guidGeneral = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
                IVsOutputWindowPane pane;
                if (outputWindow == null 
                    || ErrorHandler.Failed(outputWindow.CreatePane(guidGeneral, "Puppet", 1, 0))
                    || ErrorHandler.Failed(outputWindow.GetPane(guidGeneral, out pane))
                    || pane == null)
                {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
               }

                pane.Activate();
                OutputWindow = pane;
            }
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void PublishModuleToPuppetForge(object sender, EventArgs e)
        {
            // Get selected project 
            var projects = DteService.ActiveSolutionProjects as Array;

            if (null == projects || projects.Length <= 0)
            {
                this.MessageBox(Resources.NoActivePuppetModule, Resources.TarballCreationFailed);
                return;
            }

            var project = projects.GetValue(0) as OAProject;

            if (project == null)
            {
                this.MessageBox(Resources.NoActivePuppetModule, Resources.TarballCreationFailed);
                return;
            }

            var puppetProjectNode = project.Project as PuppetProjectNode;

            if (null == puppetProjectNode)
            {
                this.MessageBox(Resources.SelectedProjectIsNotAPuppetModule, Resources.TarballCreationFailed);
                return;;
            }

            // Get project directory
            var projectDir = puppetProjectNode.ProjectMgr.BaseURI.Directory;

            // Create PuppetForgeModules dir to store tarballs for Puppet Forge
            var projDirInfo = new DirectoryInfo(projectDir);
            var pfpDirInfo = projDirInfo.CreateSubdirectory(Conatants.PuppetForgePackagesDirName);

            // Get files with subdirs from the project
            var filesToPack = new List<Tuple<FileInfo, string>>();

            foreach (var projectItem in project.ProjectItems)
            {
                var fileItem = projectItem as OAFileItem;
                if (fileItem != null)
                {
                    var fileNode = fileItem.Object as FileNode;
                    if (fileNode != null)
                    {
                        filesToPack.Add(new Tuple<FileInfo, string>(new FileInfo(fileNode.Url), string.Empty));
                    }
                }
                else if (projectItem  is OAFolderItem)
                {
                    var q = new Queue<object>();
                    q.Enqueue(projectItem);

                    var subfolder = string.Empty;
                    while (q.Count > 0)
                    {
                        var folderItem = q.Dequeue() as OAFolderItem;
                        if (folderItem == null) continue;
                        subfolder = Path.Combine(subfolder, folderItem.Name);
                        foreach (var item in folderItem.ProjectItems)
                        {
                            if (item is OAFolderItem)
                            {
                                q.Enqueue(item);
                            }
                            else if (item is OAFileItem)
                            {
                                var fileNode = (item as OAFileItem).Object as FileNode;
                                if (fileNode != null)
                                {
                                    filesToPack.Add(new Tuple<FileInfo, string>(new FileInfo(fileNode.Url), subfolder));
                                }
                            }
                        }
                    }
                }

                // ignore other types
            }

            // Add files to an archive
            if (filesToPack.Count > 0)
            {
                var forgeUserName = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeUserName, false);
                //var forgeUserPassword = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeUserPassword, false);
                var forgeModuleName = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleName, false);
//                        var forgeModuleVersion = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleVersion, false);
//                        var forgeModuleDependency = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleDependency, false);
//                        var forgeModuleSummary = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleSummary, false);
//                        var forgeModuleDescription = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleDescription, false);

                var archiveName = string.Format("{0}-{1}.tar.gz", forgeUserName, forgeModuleName);
                var archiveFullName = Path.Combine(pfpDirInfo.ToString(), archiveName);
                    
                if (File.Exists(archiveFullName))
                {
                    OutputWindow.OutputStringThreadSafe(string.Format(Resources.DeletingExistingArchiveFile, project.Name, archiveFullName) + "\n");
                    File.Delete(archiveFullName);
                }

                // Pack module files to an archive
                using (ZipArchive archiveFile = ZipFile.Open(archiveFullName, ZipArchiveMode.Create))
                {
                    foreach (var fileToPack in filesToPack)
                    {
                        var fileInfo = fileToPack.Item1;
                        var subdir = fileToPack.Item2;
                        var fileArchiveName = Path.Combine(subdir, fileInfo.Name);
                        archiveFile.CreateEntryFromFile(fileInfo.FullName, fileArchiveName);
                        OutputWindow.OutputStringThreadSafe(string.Format(Resources.FileAddedToArchve, project.Name, fileInfo.Name, archiveFullName) + "\n");
                    }
                }
            }
        }

        private void MessageBox(string message, string caption)
        {
            // Show a Message Box to prove we were here
            var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            var clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                0,
                ref clsid,
                caption,
                string.Format(CultureInfo.CurrentCulture,"{0}", message),
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0,        // false
                out result));
        }

    }
}
