// *********************************************************************************
// 
//     Microsoft Open Tech 
//     VSPuppet
//     Created by Vlad Shcherbakov (Akvelon)  03 2014
// 
// *********************************************************************************

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
