using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadraticEquationLib
{
    public class QuadraticEquation
    {
        public static (double, double) Solving(double a, double b, double c)
        {
            if (a != 0)
            {
                double d = b * b - 4 * a * c;
                if (double.IsInfinity(d))
                    throw new OverflowException();

                if (d >= 0)
                {
                    double sqrt = Math.Sqrt(d);
                    if (double.IsInfinity(sqrt))
                        throw new OverflowException();

                    return (Math.Round((-b - sqrt) / (2 * a), 4), Math.Round((-b + sqrt) / (2 * a), 4));
                }
                throw new Exception("The equation has no valid solutions");
            }
            throw new ArgumentException("The equation is not quadratic");
        }
    }
}
