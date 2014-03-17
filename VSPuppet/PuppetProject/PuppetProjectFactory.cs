
namespace MicrosoftOpenTech.PuppetProject
{
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Project;

    [Guid(GuidList.guidPuppetProjectFactoryString)]
    class PuppetProjectFactory : ProjectFactory
    {
        private PuppetProjectPackage package;

        public PuppetProjectFactory(PuppetProjectPackage package)
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
