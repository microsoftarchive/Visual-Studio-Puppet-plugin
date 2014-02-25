/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

/*
namespace Puppet
{
    using Microsoft.VisualStudio.Package;
    using System.Runtime.InteropServices;

    /*
     * The Puppet.LanguageService class is needed to register the VS language service.  
     * This class derives from the Puppet.PuppetLanguageService base class which provides all the necessary 
     * functionality for a babel-based language service.  This class can be used to override and extend that 
     * base class if necessary.
     * 
     * Note that the Puppet.PuppetLanguageService class derives from the Managed 
     * Package Framework's LanguageService class.
     *     
     /

    [Guid("90BECFDE-F4AF-4797-9519-2B5278CC18C5")]
    class LanguageService : PuppetLanguageService
    {
        public override string GetFormatFilterList()
        {
            return "Puppet manifest (*.pp)\n*.pp";
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {

            switch (req.Reason)
            {
                case ParseReason.QuickInfo:
                    Source source = (Source)this.GetSource(req.FileName);
                    return new PuppetAuthoringScope(source); 
                default:
                    return base.ParseSource(req);
            }
        } 
    }
}
*/