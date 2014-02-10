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
using System;

class PuppetLanguageScanner : IScanner
{
    private IVsTextBuffer buffer;

    private string source;
    private int offset;

    private static string[] keywords =
    {
        "namespace",
        "class",
        "public",
        "void",
        "int"
    };

    public PuppetLanguageScanner(IVsTextBuffer buffer)
    {
        this.buffer = buffer;
    }

    bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
    {

        if (this.offset >= this.source.Length)
        {
            return false;
        }

        foreach (var kw in PuppetLanguageScanner.keywords)
        {
            var firstCharacter = this.source.IndexOf(kw, this.offset, StringComparison.Ordinal);

            if (firstCharacter >=0 )
            {
                tokenInfo.Type = TokenType.Keyword;
                tokenInfo.Color = TokenColor.Keyword;
                tokenInfo.StartIndex = firstCharacter;
                tokenInfo.EndIndex = firstCharacter + kw.Length -1;
                this.offset = tokenInfo.EndIndex + 1;
                return true;
            }
        }

        return false;
    }

    void IScanner.SetSource(string source, int offset)
    {
        if (string.IsNullOrEmpty(source))
        {
            throw new ArgumentNullException("source");
        }

        this.source = source;
        this.offset = offset;
    }
}

} // namespace Microsoft.PuppetLanguagePackage
