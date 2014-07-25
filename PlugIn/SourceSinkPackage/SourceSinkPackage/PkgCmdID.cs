// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace Microsoft.SourceSinkPackage
{
    static class PkgCmdIDList
    {
        public const int cmdidSetAsSource = 0x2000;
        public const int cmdidSetAsSink = 0x2001;
        public const int cmdidSetAsAnalyzingFunction = 0x2002;
        public const int cmdidAnalyze = 0x2003;
        public const int cmdidClearAll = 0x2004;
        public const int cmdidCastAnalysis = 0x2005;
    };
}