﻿// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
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
