// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
namespace Puppet
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.TextManager.Interop;

    public class PuppetSource : Source
    {
        public PuppetSource(PuppetLanguageService service, IVsTextLines textLines, Colorizer colorizer)
            : base(service, textLines, colorizer)
        {
        }

        private object parseResult;
        public object ParseResult
        {
            get { return parseResult; }
            set { parseResult = value; }
        }

        private IList<TextSpan[]> braces;
        public IList<TextSpan[]> Braces
        {
            get { return braces; }
            set { braces = value; }
        }

        public override CommentInfo GetCommentFormat()
        {
             return Configuration.PuppetCommentInfo;
        }
    }
}
