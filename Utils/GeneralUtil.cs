using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetyAnalysis.Util
{
    public class GeneralUtil
    {
        public static string ConvertToFilename(string str)
        {
            var newstr = String.Empty;
            foreach(var ch in str)
            {
                if (Char.IsLetterOrDigit(ch)
                    || ch == '.')
                    newstr += ch;
                else
                    newstr += "-";
            }
            if (String.IsNullOrEmpty(newstr))
                throw new NotSupportedException(str + " cannot  be converted to a filename");            
            return newstr;
        }

        public static string ConvertToFilename(Phx.FunctionUnit funit)
        {
            var typename = PhxUtil.GetTypeName(funit.FunctionSymbol.EnclosingAggregateType);
            var methodname = PhxUtil.GetFunctionName(funit.FunctionSymbol);
            var filename = GeneralUtil.ConvertToFilename(typename + "::" + methodname);
            filename = filename.Substring(filename.LastIndexOf('.') + 1);
            filename += funit.FunctionSymbol.ParameterSymbols.Count;
            return filename;
        }
    }
}
