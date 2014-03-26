// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
namespace MicrosoftOpenTech.PuppetProject
{
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Project.Automation;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Json;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Script.Serialization;
    using FilesToPack = System.Collections.Generic.List<System.Tuple<System.IO.FileInfo, string>>;
    using SELF = PuppetProjectPackage;

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
        "displayProjectFileExtensionsResourceID",
        "ppm",
        "ppm",
        @".\\NullPath",
        LanguageVsTemplate = "Puppet Labs")]
    [ProvideObject(typeof(GeneralPropertyPage))]

    [Guid(GuidList.guidPuppetProjectPkgString)]
    public sealed class PuppetProjectPackage : ProjectPackage
    {

        public EnvDTE.DTE DteService { get; private set; }
        public IVsOutputWindowPane OutputWindow {  get; private set; }

        private SecureString password;

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
                var menuCommandIdUpload = new CommandID(GuidList.guidPuppetProjectCmdSet, (int)PkgCmdIDList.cmdidUploadPuppetForgeModule);
                var menuItemUpload = new MenuCommand(this.CreateTarballAndUploadToPuppetForge, menuCommandIdUpload);
                mcs.AddCommand(menuItemUpload);

                var menuCommandIdCreate = new CommandID(GuidList.guidPuppetProjectCmdSet, (int)PkgCmdIDList.cmdidCreatePuppetForgeModule);
                var menuItemCreate = new MenuCommand(this.CreateTarballLocally, menuCommandIdCreate);
                mcs.AddCommand(menuItemCreate);

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

        private FilesToPack GetActiveProjectStruture(out PuppetProjectNode puppetProjectNode)
        {
            // Get selected project 
            var projects = DteService.ActiveSolutionProjects as Array;

            if (null == projects || projects.Length <= 0)
            {
                throw new NullReferenceException(Resources.NoActivePuppetModule);
            }

            var project = projects.GetValue(0) as OAProject;

            if (project == null)
            {
                throw new NullReferenceException(Resources.NoActivePuppetModule);
            }

            puppetProjectNode = project.Project as PuppetProjectNode;

            if (null == puppetProjectNode)
            {
                throw new NullReferenceException(Resources.SelectedProjectIsNotAPuppetModule);
            }

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
                else if (projectItem is OAFolderItem)
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

            if (filesToPack.Count == 0)
                throw new Exception(Resources.EmptyModule);

            return filesToPack;
        }

        private ForgeJsonMetadata CreateJsonMetadata(PuppetProjectNode puppetProjectNode, FilesToPack filesToPack)
        {
            if(null == puppetProjectNode)
                throw new ArgumentNullException("puppetProjectNode");

            if (null == filesToPack)
                throw new ArgumentNullException("filesToPack");

            ForgeJsonMetadata forgeJsonMetadata = null;
            
            if (filesToPack.Count > 0)
            {
                // Create json metadata file from project options

                var forgeUserName = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeUserName, false);
                var forgeModuleName = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleName,false);
                var forgeModuleVersion = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleVersion, false);
                var forgeModuleDependency = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleDependency, false);
                var forgeModuleSummary = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleSummary, false);
                var forgeModuleDescription = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleDescription, false);

                forgeJsonMetadata = new ForgeJsonMetadata
                {
                    name = string.Format("{0}-{1}", forgeUserName, forgeModuleName.ToLower()),
                    author = forgeUserName,
                    version = forgeModuleVersion,
                    summary = forgeModuleSummary,
                    description = forgeModuleDescription
                };

                // Parse dependency
                
                var depNameVer = forgeModuleDependency.Replace(" ", string.Empty).Split(',');
                if (depNameVer.Length != 2)
                {
                    throw new Exception("Bad Dependency format: expected: 'module','version'");
                }

                const char sq = '\'';
                const char dq = '"';

                // In case of user uses double quotes
                var module = depNameVer[0].Replace(sq, dq);
                // Check quotes exist
                if (module.Length < 3 || module[0] != dq || module[module.Length - 1] != dq)
                {
                    throw new Exception("Bad Dependency madule format");
                }

                var version = depNameVer[1].Replace(sq, dq);
                if (version.Length < 3 || version[0] != dq || version[version.Length - 1] != dq)
                {
                    throw new Exception("Bad Dependency version format");
                }

                forgeJsonMetadata.dependencies.Add(new ForgeJsonMetadata.Dependency
                {
                    name = module.Replace(string.Empty + dq, string.Empty), // Remove quotes
                    version_requirement = version.Replace(string.Empty + dq, string.Empty)
                });

                // Add MD5 checksums to json

                using (var md5 = MD5.Create())
                {
                    foreach (var fileToPack in filesToPack)
                    {
                        var fileInfo = fileToPack.Item1;
                        var subdir = fileToPack.Item2;
                        var combinedName = string.IsNullOrWhiteSpace(subdir) 
                            ? fileInfo.Name
                            : string.Format("{0}/{1}", subdir.Replace(Path.DirectorySeparatorChar, '/'), fileInfo.Name);

                        using (var stream = File.OpenRead(fileToPack.Item1.FullName))
                        {
                            var byteArray = md5.ComputeHash(stream);
                            forgeJsonMetadata.checksums.Add(combinedName,
                                string.Join("", byteArray.Select(b => b.ToString("x2"))));
                        }
                    }
                }
            }

            return forgeJsonMetadata;
        }

        private static string TarGz(PuppetProjectNode puppetProjectNode, FilesToPack filesToPack, ForgeJsonMetadata forgeJsonMetadata)
        {
            if (null == puppetProjectNode)
                throw new ArgumentNullException("puppetProjectNode");

            if (null == filesToPack)
                throw new ArgumentNullException("filesToPack");

            if (null == forgeJsonMetadata)
                throw new ArgumentNullException("forgeJsonMetadata");

            // Add files to a Tarball

            if (filesToPack.Count > 0)
            {
                var forgeUserName = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeUserName, false);
                var forgeModuleName = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleName, false);
                var forgeModuleVersion = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleVersion, false);

                // Create a temp directory.

                var tmpDir = new DirectoryInfo(Path.GetTempPath());
                var puppetTmpDir = tmpDir.CreateSubdirectory("PuppetLab");
                var rndDir = puppetTmpDir.CreateSubdirectory(Path.GetRandomFileName());
                
                var moduleDirName = string.Format("{0}-{1}-{2}", forgeUserName, forgeModuleName, forgeModuleVersion).ToLower();
                var moduleDir = rndDir.CreateSubdirectory(moduleDirName);
                
                Directory.SetCurrentDirectory(rndDir.ToString());

                // Create json metadata file in module directory

                MemoryStream ms = null;
                FileStream fs = null;
                
                try
                {

                    ms = new MemoryStream();
                    fs = File.Create(Path.Combine(moduleDir.ToString(), "metadata.json"));

                    var sr = new StreamReader(ms);
                    var sw = new StreamWriter(fs);

                    var s = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
                    var jonSerializer = new DataContractJsonSerializer(typeof(ForgeJsonMetadata), s);

                    jonSerializer.WriteObject(ms, forgeJsonMetadata);

                    // Remove backslashes that DataContractJsonSerializer adds before forward slashes (don't know how to disable this)
                    ms.Position = 0;
                    sw.WriteLine(sr.ReadToEnd().Replace("\\", string.Empty));

                    sw.Close();
                    sr.Close();

                    ms = null;
                    fs = null;

                }
                finally
                {
                    if (ms != null) ms.Dispose();
                    if (fs != null) fs.Dispose();
                }

                // Copy module's files to a tmp dir considering module dir tree

                foreach (var fileToPack in filesToPack)
                {
                    var srcFileInfo = fileToPack.Item1;
                    var relSubdir = fileToPack.Item2;
                    if (!string.IsNullOrEmpty(relSubdir))
                    {
                        moduleDir.CreateSubdirectory(relSubdir);
                    }
                    var absSubdir = Path.Combine(moduleDir.ToString(), relSubdir);
                    var dstFileName = Path.Combine(absSubdir, srcFileInfo.Name);
                    srcFileInfo.CopyTo(dstFileName);
                }

                // Create a TAR GZ archive

                var tarFileName = moduleDirName + ".tar";
                var gzFileName = tarFileName + ".gz";

                CreateTarGz(gzFileName, moduleDir.ToString());

                return gzFileName;
            }

            return null;
        }

        private static void CreateTarGz(string tgzFilename, string sourceDirectory)
        {
            Stream outStream = null;
            try
            {
                outStream = File.Create(tgzFilename);
                var gzoStream = new GZipOutputStream(outStream);
                using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
                {
                    outStream = null;
                    // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
                    // and must not end with a slash, otherwise cuts off first char of filename
                    // This is scheduled for fix in next release
                    tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
                    if (tarArchive.RootPath.EndsWith("/"))
                        tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

                    AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);
                }
            }
            finally
            {
                if (outStream != null) outStream.Dispose();
            }
        }

        private static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {

            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            //
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            // Write each file to the tar.
            //
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }

        string GetAccessToken(string username)
        {
            string result = null;

            // Setup the variables necessary to make the Request 
            const string grantType = "password";
            const string applicationId = "a6417131cac7b42b5e9d69daeccb06e5dbf2ea71b767d00ecb12439634233632";
            const string clientSecret = "d5a0b991b8361fe86b7e96336862d813aec811a35278361bd7ad2b08a14a9d7b";
            const string url = "https://forgeapi.puppetlabs.com/oauth/token";

            HttpWebResponse response = null;

            try
            {
                // Create the data to send
                var data = new StringBuilder();
                data.Append("grant_type=" + Uri.EscapeDataString(grantType));
                data.Append("&client_id=" + Uri.EscapeDataString(applicationId));
                data.Append("&client_secret=" + Uri.EscapeDataString(clientSecret));
                data.Append("&username=" + Uri.EscapeDataString(username));
                data.Append("&password=" + Uri.EscapeDataString(SELF.ConvertToUnsecureString(this.password)));

                // Create a byte array of the data to be sent
                byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());

                // Setup the Request
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                // Write data
                Stream postStream = request.GetRequestStream();
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                // Send Request & Get Response

                response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();

                if (responseStream == null) return null;


                using (var reader = new StreamReader(responseStream))
                {
                    // Get the Response Stream
                    string json = reader.ReadLine();

                    // Retrieve and Return the Access Token
                    var ser = new JavaScriptSerializer();
                    var x = (Dictionary<string, object>)ser.DeserializeObject(json);
                    var accessToken = x["access_token"].ToString();
                    result = accessToken;
                }
            }
            finally
            {
                if (response != null) { response.Close(); }
            }

            return result;
        }

        void PublishModule(string accessToken, string owner, string moduleName, string moduleModuleTarballPath)
        {
            // Setup the variables necessary to make the Request 
            const string url = "https://forgeapi.puppetlabs.com/v2/releases";

            var nvc = new NameValueCollection
            {
                {"owner", owner}, 
                {"module", moduleName}
            };

            this.HttpUploadFile(accessToken, url, moduleModuleTarballPath, "file", "application/gzip", nvc);
        }

        private void HttpUploadFile(string accessToken, string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;

            Stream rs = wr.GetRequestStream();

            const string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }

            rs.Write(boundarybytes, 0, boundarybytes.Length);

            const string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();
            rs = null;

            WebResponse wresp = null;

            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                if (stream2 != null)
                {
                    var reader2 = new StreamReader(stream2);
                    var msg = reader2.ReadToEnd();
                    this.MessageBox(Resources.PuppetModuleUploadedSuccessfully, Resources.PuppetModuleUploadStatus);
                }
            }
            finally
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }

                wr = null;
            }
        }

        public static string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void CreateTarballAndUploadToPuppetForge(object sender, EventArgs e)
        {
            try
            {
                PuppetProjectNode puppetProjectNode;
                var filesToPack = this.GetActiveProjectStruture(out puppetProjectNode);

                var username = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeUserName, false);
                var forgeModuleName = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleName, false);
                var modulename = forgeModuleName.ToLower();
                var moduleversion = puppetProjectNode.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleVersion, false); ;

                var forgePublishWindow = new ForgePublishWindow
                {
                    tbAccountName = {Text = username},
                    tbModuleName = {Text = modulename},
                    tbModuleVersion = {Text = moduleversion}
                };

                if (this.password != null && this.password.Length != 0)
                {
                    forgePublishWindow.pwdAcountPassword.Password = SELF.ConvertToUnsecureString(this.password);
                    forgePublishWindow.btnPublish.IsEnabled = true;
                }

                var res = forgePublishWindow.ShowDialog();

                if (!res.HasValue || res.Value == false)
                {
                    return;
                }

                this.password = forgePublishWindow.pwdAcountPassword.SecurePassword;

                var jsonMetadata = this.CreateJsonMetadata(puppetProjectNode, filesToPack);
                var gzFileName = SELF.TarGz(puppetProjectNode, filesToPack, jsonMetadata);
                
                var forgeAccessToken = this.GetAccessToken(username);
                this.PublishModule(forgeAccessToken, username, modulename, gzFileName);
            }
            catch (WebException ex)
            {
                // This exception will be raised if the server didn't return 200 - OK
                // Retrieve more information about the error
                if (ex.Response != null)
                {
                    using (var err = (HttpWebResponse)ex.Response)
                    {
                        var info = string.Format(Resources.ServerReturnedTemplate,
                            err.StatusDescription, err.StatusCode, err.StatusCode);

                        this.MessageBox(info, Resources.PuppetModuleUploadStatus);
                    }
                }
                else
                {
                    this.MessageBox(ex.Message, Resources.PuppetModuleUploadStatus);
                }

            }
            catch (Exception ex)
            {
                this.MessageBox(ex.Message, Resources.TarballCreationStatus);
            }
        }

        private void CreateTarballLocally(object sender, EventArgs e)
        {
            try
            {
                PuppetProjectNode puppetProjectNode;
                var filesToPack = this.GetActiveProjectStruture(out puppetProjectNode);

                var jsonMetadata = this.CreateJsonMetadata(puppetProjectNode, filesToPack);
                var gzFileName = SELF.TarGz(puppetProjectNode, filesToPack, jsonMetadata);

                var packagesDir = new DirectoryInfo(puppetProjectNode.BaseURI.Directory).CreateSubdirectory("packages");
                var destFile = System.IO.Path.Combine(packagesDir.ToString(), gzFileName);
                File.Copy(gzFileName, destFile, true);
                this.MessageBox(destFile, Resources.TarballSavedSuccessfully);

            }
            catch (Exception ex)
            {
                this.MessageBox(ex.Message, Resources.TarballCreationStatus);
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
