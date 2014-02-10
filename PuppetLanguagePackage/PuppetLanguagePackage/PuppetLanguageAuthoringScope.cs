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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

class PuppetLanguageAuthoringScope : AuthoringScope
{
    public override string GetDataTipText(int line, int col, out TextSpan span)
    {
        span = new TextSpan();
        return null;
    }

    public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
    {
        return null;
    }

    public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
    {
        span = new TextSpan();
        return null;
    }

    public override Methods GetMethods(int line, int col, string name)
    {
        return null;
    }
}

} // namespace Microsoft.PuppetLanguagePackage
