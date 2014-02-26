
namespace HighlightWord
{
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;
    using System.ComponentModel.Composition;
    using System.Windows.Media;

    [Export(typeof(EditorFormatDefinition))]
    [Name(Constants.HighlightWordName)]
    [UserVisible(true)]
    class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
            this.BackgroundColor = Colors.Beige;
            this.DisplayName = Constants.HighlightWordDisplayName;
            this.ZOrder = 5;
        }
    }
}
