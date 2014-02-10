/* 
*    Copyright © Microsoft Open Technologies, Inc.
*    All Rights Reserved       
*    Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
*    http://www.apache.org/licenses/LICENSE-2.0
*
*     THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
*
*    See the Apache 2 License for the specific language governing permissions and limitations under the License.
*/

namespace Microsoft.PuppetLanguagePackage {

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

public class PuppetLanguageService : LanguageService
{
    private LanguagePreferences languagePreferences;

    private const string name = "Puppet Labs Manifest Language";

    public override string GetFormatFilterList()
    {
        throw new System.NotImplementedException();
    }

    public override LanguagePreferences GetLanguagePreferences()
    {
        if (null == this.languagePreferences)
        {
            this.languagePreferences = new LanguagePreferences();
            //this.languagePreferences.Init();
        }

        return this.languagePreferences;
    }

    public override IScanner GetScanner(IVsTextLines buffer)
    {
        return new PuppetLanguageScanner(buffer);
    }

    public override string Name
    {
        get { return PuppetLanguageService.name; }
    }

    public override AuthoringScope ParseSource(ParseRequest req)
    {
        return new PuppetLanguageAuthoringScope();
    }
}

} // namespace Microsoft.PuppetLanguagePackage
