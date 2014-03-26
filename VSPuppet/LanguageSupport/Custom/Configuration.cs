// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
namespace Puppet
{
    using Microsoft.VisualStudio.Package;

    public static class Configuration
    {

        public const string Name = "Puppet Language";
        public const string Extension = ".pp";

        static readonly CommentInfo puppetCommentInfo;
        public static CommentInfo PuppetCommentInfo { get { return Configuration.puppetCommentInfo; } }

        static Configuration()
        {
            Configuration.puppetCommentInfo.BlockEnd = "/*";
            Configuration.puppetCommentInfo.BlockStart = "*/";
            Configuration.puppetCommentInfo.LineStart = "#";
            Configuration.puppetCommentInfo.UseLineComments = true;
        }
    }
}