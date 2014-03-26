// *********************************************************************************
// 
//     Microsoft Open Tech 
//     VSPuppet
//     Created by Vlad Shcherbakov (Akvelon)  03 2014
// 
// *********************************************************************************

namespace BraceMatching
{
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;
    using System.ComponentModel.Composition;
    using System.Windows.Media;

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.BraceMatchingName)]
    [UserVisible(true)]
    class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
            this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.DarkBlue;
            this.DisplayName = Constants.BraceMatchingDisplayName;
            this.ZOrder = 5;
        }
    }
}
