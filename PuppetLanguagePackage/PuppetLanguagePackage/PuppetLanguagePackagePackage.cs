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

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

/// <summary>
/// This is the class that implements the package exposed by this assembly.
///
/// The minimum requirement for a class to be considered a valid package for Visual Studio
/// is to implement the IVsPackage interface and register itself with the shell.
/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
/// to do it: it derives from the Package class that provides the implementation of the 
/// IVsPackage interface and uses the registration attributes defined in the framework to 
/// register itself and its components with the shell.
/// </summary>
// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
// a package.
[PackageRegistration(UseManagedResourcesOnly = true)]
// This attribute is used to register the information needed to show this package
// in the Help/About dialog of Visual Studio.
[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
// This attribute is needed to let the shell know that this package exposes some menus.
//[ProvideMenuResource("Menus.ctmenu", 1)]
//[Guid(GuidList.guidPuppetLanguagePackagePkgString)]
//[ProvideService(typeof(PuppetLanguageService), ServiceName = "Puppet Language Service")]
//[ProvideLanguageService(typeof(PuppetLanguageService),
//                            "Puppet Language",
//                            106,             // resource ID of localized language name
//                            CodeSense = true,             // Supports IntelliSense
//                            RequestStockColors = false,   // Supplies custom colors
//                            EnableCommenting = true,      // Supports commenting out code
//                            EnableAsyncCompletion = true  // Supports background parsing
//                            )]
//[ProvideLanguageExtension(typeof(PuppetLanguageService), ".pp")]
public sealed class PuppetLanguagePackagePackage : Package, IOleComponent
{

    private uint componentId;

    /// <summary>
    /// Default constructor of the package.
    /// Inside this method you can place any initialization code that does not require 
    /// any Visual Studio service because at this point the package object is created but 
    /// not sited yet inside Visual Studio environment. The place to do all the other 
    /// initialization is the Initialize method.
    /// </summary>
    public PuppetLanguagePackagePackage()
    {
        Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
    }

    /////////////////////////////////////////////////////////////////////////////
    // Overridden Package Implementation
    #region Package Members

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override void Initialize()
    {
        Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
        base.Initialize();  // required

        // Proffer the service.
        var serviceContainer = this as IServiceContainer;
        var langService = new PuppetLanguageService();
        langService.SetSite(this);
        serviceContainer.AddService(typeof(PuppetLanguageService),
                                    langService,
                                    true);

        // Register a timer to call our language service during
        // idle periods.
        var mgr = GetService(typeof(SOleComponentManager))
                                    as IOleComponentManager;
        if (this.componentId == 0 && mgr != null)
        {
            var crinfo = new OLECRINFO[1];
            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
            crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                            (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
            crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                            (uint)_OLECADVF.olecadvfRedrawOff |
                                            (uint)_OLECADVF.olecadvfWarningsOff;
            crinfo[0].uIdleTimeInterval = 1000;
            int hr = mgr.FRegisterComponent(this, crinfo, out this.componentId);
        }
        // Add our command handlers for menu (commands must exist in the .vsct file)
        var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

        if (null == mcs) return;
            
        // Create the command for the menu item.
        var menuCommandId = new CommandID(GuidList.guidPuppetLanguagePackageCmdSet, (int)PkgCmdIDList.cmdidMyCommand);
        var menuItem = new MenuCommand(MenuItemCallback, menuCommandId );
        mcs.AddCommand( menuItem );
    }

    #endregion

    protected override void Dispose(bool disposing)
    {
        if (this.componentId != 0)
        {
            var mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
            if (mgr != null)
            {
                var hr = mgr.FRevokeComponent(this.componentId);
            }

            this.componentId = 0;
        }

        base.Dispose(disposing);
    }


    #region IOleComponent Members

    public int FDoIdle(uint grfidlef)
    {
        bool bPeriodic = (grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0;
        // Use typeof(TestLanguageService) because we need to
        // reference the GUID for our language service.
        var service = GetService(typeof(PuppetLanguageService)) as LanguageService;
        if (service != null)
        {
            service.OnIdle(bPeriodic);
        }
        return 0;
    }

    public int FContinueMessageLoop(uint uReason,
                                    IntPtr pvLoopData,
                                    MSG[] pMsgPeeked)
    {
        return 1;
    }

    public int FPreTranslateMessage(MSG[] pMsg)
    {
        return 0;
    }

    public int FQueryTerminate(int fPromptUser)
    {
        return 1;
    }

    public int FReserved1(uint dwReserved,
                            uint message,
                            IntPtr wParam,
                            IntPtr lParam)
    {
        return 1;
    }

    public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
    {
        return IntPtr.Zero;
    }

    public void OnActivationChange(IOleComponent pic,
                                    int fSameComponent,
                                    OLECRINFO[] pcrinfo,
                                    int fHostIsActivating,
                                    OLECHOSTINFO[] pchostinfo,
                                    uint dwReserved)
    {
    }

    public void OnAppActivate(int fActive, uint dwOtherThreadId)
    {
    }

    public void OnEnterState(uint uStateId, int fEnter)
    {
    }

    public void OnLoseActivation()
    {
    }

    public void Terminate()
    {
    }

    #endregion

    /// <summary>
    /// This function is the callback used to execute a command when the a menu item is clicked.
    /// See the Initialize method to see how the menu item is associated to this function using
    /// the OleMenuCommandService service and the MenuCommand class.
    /// </summary>
    private void MenuItemCallback(object sender, EventArgs e)
    {
        // Show a Message Box to prove we were here
        var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
        var clsid = Guid.Empty;
        int result;

        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                    0,
                    ref clsid,
                    "PuppetLanguagePackage",
                    string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
                    string.Empty,
                    0,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_INFO,
                    0,        // false
                    out result));
    }
}

} // namespace Microsoft.PuppetLanguagePackage
