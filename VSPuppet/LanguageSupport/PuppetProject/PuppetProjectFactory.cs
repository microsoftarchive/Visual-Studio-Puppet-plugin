using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Project;

namespace MicrosoftOpenTech.LanguageSupport.PuppetProject
{
    [Guid(GuidList.guidPuppetProjectFactoryString)]
    class PuppetProjectFactory : ProjectFactory
    {
        private Puppet.Package package;

        public PuppetProjectFactory(Puppet.Package package)
            : base(package)
        {
            this.package = package;
        }
        protected override ProjectNode CreateProject()
        {
            PuppetProjectNode project = new PuppetProjectNode(this.package);

            project.SetSite((IServiceProvider)((System.IServiceProvider)this.package).GetService(typeof(IServiceProvider)));
            return project;
        }
    }
}
