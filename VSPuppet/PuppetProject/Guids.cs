// Guids.cs
// MUST match guids.h
namespace MicrosoftOpenTech.PuppetProject
{
    using System;

    static class GuidList
    {
        public const string guidPuppetProjectPkgString = "95b555a5-d116-4f4d-b7da-88f0fb38e0aa";
        public const string guidPuppetProjectCmdSetString = "9f9b8a19-35b9-494c-9e62-7cf18362f90e";
        public const string guidToolWindowPersistanceString = "df615e6e-5ef1-4f5f-b49d-b5b0c28ca039";
        public const string guidPuppetProjectFactoryString = "FD63057A-0BA7-4B6E-9033-09AF317E5532";


        public static readonly Guid guidPuppetProjectCmdSet = new Guid(guidPuppetProjectCmdSetString);
        public static readonly Guid guidPuppetProjectFactory = new Guid(guidPuppetProjectFactoryString);

    };
}