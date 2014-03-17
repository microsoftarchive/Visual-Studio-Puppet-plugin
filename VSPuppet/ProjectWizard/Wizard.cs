
namespace ProjectWizard
{
    using EnvDTE;
    using Microsoft.VisualStudio.TemplateWizard;
    using MicrosoftOpenTech.PuppetProject;
    using System.Collections.Generic;

    class Wizard : IWizard
    {
        private string forgeUserName;
        private string forgeUserPassword = "UNSUPPORTED";
        private string forgeModuleName;
        private string forgeModuleVersion;
        private string forgeModuleDependency;
        private string forgeModuleSummary;
        private string forgeModuleDescription;

        // This method is called before opening any item that 
        // has the OpenInEditor attribute.
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
            var puppetProjectNode = project.Object as PuppetProjectNode;

            if (puppetProjectNode != null)
            {
                puppetProjectNode.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeUserName, this.forgeUserName);
                puppetProjectNode.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeUserPassword, this.forgeUserPassword);
                puppetProjectNode.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleName, this.forgeModuleName);
                puppetProjectNode.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleVersion, this.forgeModuleVersion);
                puppetProjectNode.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleDependency, this.forgeModuleDependency);
                puppetProjectNode.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleSummary, this.forgeModuleSummary);
                puppetProjectNode.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleDescription, this.forgeModuleDescription);
            }
        }

        // This method is only called for item templates,
        // not for project templates.
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        // This method is called after the project is created.
        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            // Display a form to the user. The form collects 
            var mainWindow = new MainWindow();

            const string UNKNOWN = "empty";

            string defaultForgeUsername;
            replacementsDictionary.TryGetValue("$username$", out defaultForgeUsername);
            if (!string.IsNullOrEmpty(defaultForgeUsername))
            {
                mainWindow.UserName = defaultForgeUsername;
            }

            string defaultForgeModulename;
            replacementsDictionary.TryGetValue("$safeprojectname$", out defaultForgeModulename);
            if (!string.IsNullOrEmpty(defaultForgeModulename))
            {
                mainWindow.ModuleName = defaultForgeModulename;
            }

            var dr = mainWindow.ShowDialog();

            if (dr.HasValue && dr.Value)
            {
                this.forgeUserName = 
                    string.IsNullOrEmpty(mainWindow.UserName)
                        ? !string.IsNullOrEmpty(defaultForgeUsername) ? defaultForgeUsername : UNKNOWN
                        : mainWindow.UserName;
                replacementsDictionary.Add("$forgeusername$", this.forgeUserName);

                this.forgeModuleName =
                    string.IsNullOrEmpty(mainWindow.ModuleName)
                        ? !string.IsNullOrEmpty(defaultForgeModulename) ? defaultForgeModulename : UNKNOWN
                        : mainWindow.ModuleName;
                replacementsDictionary.Add("$forgemodulename$", this.forgeModuleName);

                this.forgeModuleVersion = 
                    string.IsNullOrEmpty(mainWindow.Version)
                        ? "0.0.1"
                        : mainWindow.Version;
                replacementsDictionary.Add("$forgemoduleversion$", this.forgeModuleVersion);

                this.forgeModuleDependency = 
                    string.IsNullOrEmpty(mainWindow.Dependency)
                        ? string.Empty
                        : mainWindow.Dependency;
                replacementsDictionary.Add("$forgemoduledependency$", this.forgeModuleDependency);

                this.forgeModuleSummary = 
                    string.IsNullOrEmpty(mainWindow.Summary)
                        ? string.Empty
                        : mainWindow.Summary;
                replacementsDictionary.Add("$forgemodulesummary$", this.forgeModuleSummary);
                
                this.forgeModuleDescription =
                    string.IsNullOrEmpty(mainWindow.Description)
                        ? string.Empty
                        : mainWindow.Description;
                replacementsDictionary.Add("$forgemoduledescription$", this.forgeModuleDescription);
            }
            else
            {
                throw new WizardBackoutException();
            }
        }

        // This method is only called for item templates,
        // not for project templates.
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
