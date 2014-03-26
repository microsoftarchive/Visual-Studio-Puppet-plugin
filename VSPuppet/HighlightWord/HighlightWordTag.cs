// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
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
