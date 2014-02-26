namespace HighlightWord
{
    using Microsoft.VisualStudio.Text.Tagging;

    class HighlightWordTag : TextMarkerTag
    {
        public HighlightWordTag()
            : base(Constants.HighlightWordName)
        {
            
        }
    }
}
