
namespace BraceMatching
{
    using Microsoft.VisualStudio.Text.Tagging;

    class BraceMatchingTag : TextMarkerTag
    {
        public BraceMatchingTag()
            : base(Constants.BraceMatchingName)
        {
            
        }
    }
}
