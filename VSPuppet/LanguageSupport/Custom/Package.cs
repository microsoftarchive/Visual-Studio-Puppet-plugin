/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace Puppet
{
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using MicrosoftOpenTech.LanguageSupport;

    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly=true)]
    [ProvideService(typeof(PuppetLanguageService), ServiceName = "VS Puppet Language Support")]
    [ProvideLanguageExtension(typeof(PuppetLanguageService), Configuration.Extension)]
    [ProvideLanguageService(typeof(PuppetLanguageService), 
        Configuration.Name, 
        106,
        CodeSense = true,
        EnableCommenting = true,
        MatchBraces = true,
        MatchBracesAtCaret = true,
        ShowMatchingBrace = true,
        ShowCompletion = true,
        AutoOutlining = true,
        EnableAsyncCompletion = true,
        RequestStockColors = false,
        CodeSenseDelay = 0)]
    [Guid(GuidList.guidLanguageSupportPkgString)]
    class Package : PuppetPackage
    {
    }
}
