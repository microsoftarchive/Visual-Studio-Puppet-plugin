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

using System.Diagnostics;
using System.Drawing;
using Microsoft.VisualStudio;

namespace Microsoft.PuppetLanguagePackage {

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

public class PuppetLanguageService : LanguageService
{

    private ColorableItem[] _colorableItems;

    // other code ¡­
    public PuppetLanguageService()
        : base()
    {
        _colorableItems = CreateColorableItems();
    }
    // other code ¡­
    #region IVsProvideColorableItems Members
    /* This specific language does not really need to provide colorable items because it
    * does not define any item different from the default ones, but the base class has
    * an empty implementation of IVsProvideColorableItems, so any language service that
    * derives from it must implement the methods of this interface, otherwise there are
    * errors when the shell loads an editor to show a file associated to this language.
    * */

    public override int GetColorableItem(int index, out IVsColorableItem item)
    {
        if (index < 1)
        {
            Debug.Assert(false, "GetColorableItem(index, item) - index is out of range");
            item = null;
            return VSConstants.S_FALSE;
        }

        item = _colorableItems[index - 1];
        return VSConstants.S_OK;
    }

    public override int GetItemCount(out int count)
    {
        count = 0;
        if (_colorableItems != null)
        {
            count = _colorableItems.Length;
        }

        return VSConstants.S_OK;
    }

    #endregion
    // other code ¡­
    public static ColorableItem[] CreateColorableItems()
    {
        ColorableItem[] items = new ColorableItem[]
            {
            new ColorableItem("MyKeyword",
                "CustomLanguage Keyword",
                COLORINDEX.CI_BLUE,
                COLORINDEX.CI_USERTEXT_BK,
                Color.Empty,
                Color.Empty,
                FONTFLAGS.FF_DEFAULT),
            new ColorableItem("MyComment",
                "CustomLanguage Comment",
                COLORINDEX.CI_DARKGREEN,
                COLORINDEX.CI_USERTEXT_BK,
                Color.Empty,
                Color.Empty,
                FONTFLAGS.FF_DEFAULT),
            new ColorableItem("MyIdentifier",
                "CustomLanguage Identifier",
                COLORINDEX.CI_SYSPLAINTEXT_FG,
                COLORINDEX.CI_USERTEXT_BK,
                Color.Empty,
                Color.Empty,
                FONTFLAGS.FF_DEFAULT),
            new ColorableItem("MyString",
                "CustomLanguage String",
                COLORINDEX.CI_MAROON,
                COLORINDEX.CI_USERTEXT_BK,
                Color.Empty,
                Color.Empty,
                FONTFLAGS.FF_DEFAULT),
            new ColorableItem("MyNumber",
                "CustomLanguage Number",
                COLORINDEX.CI_SYSPLAINTEXT_FG,
                COLORINDEX.CI_USERTEXT_BK,
                Color.Empty,
                Color.Empty,
                FONTFLAGS.FF_DEFAULT),
            new ColorableItem("MyText",
                "CustomLanguage Text",
                COLORINDEX.CI_SYSPLAINTEXT_FG,
                COLORINDEX.CI_USERTEXT_BK,
                Color.Empty,
                Color.Empty,
                FONTFLAGS.FF_DEFAULT),
            };

        return items;
    }



    // BEFORE

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
