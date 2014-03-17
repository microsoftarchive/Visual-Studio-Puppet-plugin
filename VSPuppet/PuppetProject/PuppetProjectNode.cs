
namespace MicrosoftOpenTech.PuppetProject
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Project;
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
    using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;


    public class PuppetProjectNode : ProjectNode
    {
        private PuppetProjectPackage package;

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
            var rs = ass.GetManifestResourceStream("MicrosoftOpenTech.PuppetProject.Resources.PuppetProjectNode.bmp");
            var imList = Utilities.GetImageList(rs);


            try
            {
                imageList = Utilities
                    .GetImageList(typeof(PuppetProjectNode)
                        .Assembly
                        .GetManifestResourceStream("MicrosoftOpenTech.PuppetProject.Resources.PuppetProjectNode.bmp"));
            }
            catch
            {
                Debug.WriteLine("Cant get resource");
                throw;
            }

            if(null == imageList)
                Debug.WriteLine("Cant get bitmap");
        }

        public PuppetProjectNode(PuppetProjectPackage package)
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

        private const VsCommands2K ExploreFolderInWindowsCommand = (VsCommands2K)1635;

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cmd)
                {
                    case VsCommands.Copy:
                    case VsCommands.Paste:
                    case VsCommands.Cut:
                    case VsCommands.Rename:
                    case VsCommands.ProjectSettings:
                    case VsCommands.PropSheetOrProperties:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                    case VsCommands.ViewForm:
                        if (this.HasDesigner)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            return VSConstants.S_OK;
                        }
                        break;
                    case VsCommands.NewFolder:
                    case VsCommands.AddNewItem:
                    case VsCommands.AddExistingItem:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case ExploreFolderInWindowsCommand:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == GuidList.guidPuppetProjectCmdSet)
            {
                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                return VSConstants.S_OK;
            }

            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
            return VSConstants.S_OK;
        }

    }
}
