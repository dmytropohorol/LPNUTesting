using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLib
{
    public class Calc
    {
        public static double Sum(double a, double b)
        {
            double res;
            res = a + b;
            if (double.IsInfinity(res))
                throw new OverflowException();
            return res;
           
            
        }
        public static double Div(double a, double b)
        {
            if (b == 0)
                throw new DivideByZeroException();
            return a / b;
        }
    }
}
