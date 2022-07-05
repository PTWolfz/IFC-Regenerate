using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IFC_Regenerate
{
    public static class DoubleExtension
    {
        public static double _MmToFeet(this double milimeter)
        {
            return milimeter /304.8;
        }
        public static List<string> _ToListString(this MatchCollection matchCollection)
        {
            List<string> list = new List<string>();
            foreach (Match match in matchCollection)
            {
               if(match.Length!=0)
                list.Add(match.ToString());
            }
            return list;
        }
    }
    
}
