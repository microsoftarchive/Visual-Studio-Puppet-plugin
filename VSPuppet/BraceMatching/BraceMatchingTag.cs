// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
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
