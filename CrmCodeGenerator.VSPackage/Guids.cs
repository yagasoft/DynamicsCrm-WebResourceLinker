// Guids.cs
// MUST match guids.h
using System;

namespace WebResourceLinkerExt.VSPackage
{
    static class GuidList
    {
        public const string guidWebResLinkExt_VSPackagePkgString = "d57cc223-354b-12cc-b5d0-814a71ce2d61";
        public const string guidWebResLinkExt_VSPackageCmdSetString = "275315bd-6cf8-4fa5-f6fb-9c03e9691b82";
        public const string guidWebResLinkExt_SimpleGenerator = "BB66ADDB-6AF5-4E29-B563-F918D86D7CC0";

        public static readonly Guid guidWebResLinkExt_VSPackageCmdSet = new Guid(guidWebResLinkExt_VSPackageCmdSetString);
    };
}