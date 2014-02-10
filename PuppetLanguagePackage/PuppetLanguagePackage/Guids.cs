// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.PuppetLanguagePackage
{
    static class GuidList
    {
        public const string guidPuppetLanguagePackagePkgString = "4b5b3edf-e096-49e0-b3ff-a460cb7184db";
        public const string guidPuppetLanguagePackageCmdSetString = "fdfb6435-c656-4672-b8bb-a8302de240af";

        public static readonly Guid guidPuppetLanguagePackageCmdSet = new Guid(guidPuppetLanguagePackageCmdSetString);
    };
}