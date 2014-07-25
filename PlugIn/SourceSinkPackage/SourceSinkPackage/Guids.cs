// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.SourceSinkPackage
{
    static class GuidList
    {
        public const string guidSourceSinkPackagePkgString = "5771b765-6567-48c1-b5d7-9730d87cf4a0";
        public const string guidSourceSinkPackageCmdSetString = "4a4ddfd1-e8d6-4a79-9201-d37d0e07c6a7";

        public static readonly Guid guidSourceSinkPackageCmdSet = new Guid(guidSourceSinkPackageCmdSetString);
    };
}