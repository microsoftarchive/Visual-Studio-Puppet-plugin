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
    [ProvideServiceAttribute(typeof(Puppet.LanguageService), ServiceName = "VS Puppet Language Support")]
    [ProvideLanguageExtension(typeof(Puppet.LanguageService), Configuration.Extension)]
    [ProvideLanguageServiceAttribute(typeof(Puppet.LanguageService), 
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
//        RequestStockColors = true,
        CodeSenseDelay = 0)]
    [Guid(GuidList.guidLanguageSupportPkgString)]
    class Package : PuppetPackage
    {
    }
}
