using System;
using Curry;

namespace CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(EvalExpr.Compile("x + 5").Eval(10));
            Console.WriteLine(EvalExpr.Compile("(x - 3) * 5").Eval(10));
            Console.WriteLine(EvalExpr.Compile("(x > 10) * x ^ 2 - x * 3").Eval(10));
            Console.WriteLine(EvalExpr.Compile("ln e").Eval(10));
            Console.WriteLine(EvalExpr.Compile("sin(pi / 2)").Eval(10));
            Console.WriteLine(EvalExpr.Compile("(x + 5) * (x - 3)").Eval(9));
        }
    }
}
