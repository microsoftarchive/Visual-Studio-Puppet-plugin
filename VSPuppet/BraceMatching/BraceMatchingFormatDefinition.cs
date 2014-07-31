// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
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
    [Order(After = Priority.Default, Before = Priority.High)] 
    class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
//            this.BackgroundColor = Colors.Brown;
            this.ForegroundColor = Colors.DarkCyan;
            this.DisplayName = Constants.BraceMatchingDisplayName;
            this.ZOrder = 5;
        }
    }
}
