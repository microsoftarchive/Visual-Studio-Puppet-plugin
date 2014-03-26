// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
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
