using QuadraticEquationLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("5x^2 + 2x - 3\t" + QuadraticEquation.Solving(5, 2, -3));
                Console.WriteLine("2x^2 + 2x + 3\t" + QuadraticEquation.Solving(2, 2, 3));      
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
