using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFC_Regenerate
{
    public static class DoubleExtension
    {
        public static double _MmToFeet(this double milimeter)
        {
            return milimeter /304.8;
        }
    }
}
