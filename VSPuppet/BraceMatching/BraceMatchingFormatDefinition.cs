// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------

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
