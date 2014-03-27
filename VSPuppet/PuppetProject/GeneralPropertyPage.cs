// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace MicrosoftOpenTech.PuppetProject
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Project;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    [Guid("75F97284-5759-44B0-A7B7-0DCEB97B8216")]
    public class GeneralPropertyPage : SettingsPage
    {
        private string forgeUserName;
        private string forgeModuleName;
        private string forgeModuleVersion;
        private string forgeModuleDependency;
        private string forgeModuleSummary;
        private string forgeModuleDescription;

        public GeneralPropertyPage()
        {
            this.Name = "Puppet Forge";
        }

        [Category(Conatants.PuppetForgeAccountProperties)]
        [DisplayName("User Name")]
        [Description("Puppet Forge Account Name.")]
        public string ForgeUserName
        {
            get { return this.forgeUserName; }
            set
            {
                this.forgeUserName = value;
                this.IsDirty = true;
            }
        }

        [Category(Conatants.PuppetForgeModuleProperties)]
        [DisplayName("Module Name")]
        [Description("The name that is used to create a forge module name.")]
        public string ForgeModuleName
        {
            get { return this.forgeModuleName; }
            set
            {
                this.forgeModuleName = value;
                this.IsDirty = true;
            }
        }

        [Category(Conatants.PuppetForgeModuleProperties)]
        [DisplayName("Version")]
        [Description("The current version of the module. This should be a semantic version.")]
        public string ForgeModuleVersion
        {
            get { return this.forgeModuleVersion; }
            set
            {
                this.forgeModuleVersion = value;
                this.IsDirty = true;
            }
        }

        [Category(Conatants.PuppetForgeModuleProperties)]
        [DisplayName("Dependency")]
        [Description("A module that this module depends on.")]
        public string ForgeModuleDependency
        {
            get { return this.forgeModuleDependency; }
            set
            {
                this.forgeModuleDependency = value;
                this.IsDirty = true;
            }
        }

        [Category(Conatants.PuppetForgeModuleProperties)]
        [DisplayName("Summary")]
        [Description("A one-line description of the module.")]
        public string ForgeModuleSummary
        {
            get { return this.forgeModuleSummary; }
            set
            {
                this.forgeModuleSummary = value;
                this.IsDirty = true;
            }
        }

        [Category(Conatants.PuppetForgeModuleProperties)]
        [DisplayName("Description")]
        [Description("A more complete description of the module.")]
        public string ForgeModuleDescription
        {
            get { return this.forgeModuleDescription; }
            set
            {
                this.forgeModuleDescription = value;
                this.IsDirty = true;
            }
        }

        protected override void BindProperties()
        {
            this.forgeUserName = this.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeUserName, true);
            this.forgeModuleName = this.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleName, true);
            this.forgeModuleVersion = this.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleVersion, true);
            this.forgeModuleDependency = this.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleDependency, true);
            this.forgeModuleSummary = this.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleSummary, true);
            this.forgeModuleDescription = this.ProjectMgr.GetProjectProperty(Conatants.PuppetForgeModuleDescription, true);
        }

        protected override int ApplyChanges()
        {
            this.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeUserName, this.forgeUserName);
            this.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleName, this.forgeModuleName);
            this.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleVersion, this.forgeModuleVersion);
            this.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleDependency, this.ForgeModuleDependency);
            this.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleSummary, this.forgeModuleSummary);
            this.ProjectMgr.SetProjectProperty(Conatants.PuppetForgeModuleDescription, this.ForgeModuleDescription);

            this.IsDirty = false;

            return VSConstants.S_OK;
        }
    }
}
