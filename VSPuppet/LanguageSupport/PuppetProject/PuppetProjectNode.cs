using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.Project;

namespace MicrosoftOpenTech.LanguageSupport.PuppetProject
{
    public class PuppetProjectNode : ProjectNode
    {
        private Puppet.Package package;

        private static ImageList imageList;

        internal static int imageIndex;
        public override int ImageIndex
        {
            get { return imageIndex; }
        }

        static PuppetProjectNode()
        {

            var ass = typeof (PuppetProjectNode).Assembly;
            var rm = ass.GetManifestResourceNames();
            var rs = ass.GetManifestResourceStream("MicrosoftOpenTech.LanguageSupport.Resources.PuppetProjectNode.bmp");
            var imList = Utilities.GetImageList(rs);


            try
            {
                imageList = Utilities
                    .GetImageList(typeof(PuppetProjectNode)
                        .Assembly
                        .GetManifestResourceStream("MicrosoftOpenTech.LanguageSupport.Resources.PuppetProjectNode.bmp"));
            }
            catch
            {
                Debug.WriteLine("Can't get resource");
            }

            if(null == imageList)
                Debug.WriteLine("Can't get bitmap");
        }

        public PuppetProjectNode(Puppet.Package package)
        {
            this.package = package;

            imageIndex = this.ImageHandler.ImageList.Images.Count;

            foreach (Image img in imageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }
        }

        public override Guid ProjectGuid
        {
            get { return GuidList.guidPuppetProjectFactory; }
        }
        public override string ProjectType
        {
            get { return "PuppetProjectType"; }
        }

        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(GeneralPropertyPage).GUID;
            return result;
        }
        protected override Guid[] GetPriorityProjectDesignerPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(GeneralPropertyPage).GUID;
            return result;
        }

        //public override void AddFileFromTemplate(string source, string target)
        //{
        //    string nameSpace =
        //            this.FileTemplateProcessor.GetFileNamespace(target, this);
        //    string className = Path.GetFileNameWithoutExtension(target);

        //    this.FileTemplateProcessor.AddReplace("$nameSpace$", nameSpace);
        //    this.FileTemplateProcessor.AddReplace("$className$", className);

        //    this.FileTemplateProcessor.UntokenFile(source, target);
        //    this.FileTemplateProcessor.Reset();
        //}
    }
}
