// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace BraceMatching
{
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;
    using System.ComponentModel.Composition;

    [Export(typeof (IViewTaggerProvider))]
    [ContentType(Constants.ContentType)]
    [TagType(typeof (TextMarkerTag))]
    internal class BraceMatchingTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView == null)
                return null;

            //provide highlighting only on the top-level buffer 
            if (textView.TextBuffer != buffer)
                return null;

            return new BraceMatchingTagger(textView, buffer) as ITagger<T>;
        }
    }
}
