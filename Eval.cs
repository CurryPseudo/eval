using System;
using System.Collections.Generic;
namespace Curry {
    public abstract class EvalExpr {
        private static String removeSpace(String str) {
            char[] ary = str.ToCharArray();
            List<char> cl = new List<char>();
            foreach(char c in ary) {
                if(c != ' ') {
                    cl.Add(c);
                }
            }
            return new String(cl.ToArray());
        }
        public static EvalExpr Compile(String str) {
            var p = NonTerminal<EvalExpr>.Generate<Ex12>(new StringRange(removeSpace(str)));
            return p.Parse();
        }
        public abstract float Eval(float x);
    }
    public class Id : EvalExpr {
        public override float Eval(float x) {
            return x;
        }
        public override String ToString() {
            return "x";
        }
    }
    public class OneOp : EvalExpr {
        private EvalExpr _eval;
        private Func<float, float> _op;
        private String _str;
        public OneOp(Func<float,float> op, EvalExpr eval, String str = "op") {
            _op = op;
            _eval = eval;
            _str = str;
        }
        public override float Eval(float x) {
            return _op(_eval.Eval(x));
        }
        public override String ToString() {
            return _str;
        }
    }
    public class TwoOp : EvalExpr {
        private EvalExpr _left;
        private EvalExpr _right;
        private Func<float, float, float> _op;
        private String _str;
        public TwoOp(Func<float, float, float> op, EvalExpr left, EvalExpr right, String str = "op") {
            _op = op;
            _left = left;
            _right = right;
            _str = str;
        }
        public override float Eval(float x) {
            return _op(_left.Eval(x), _right.Eval(x));
        }
        public override String ToString() {
            return _str;
        }
    }
    public class Const : EvalExpr {
        private float _num;
        public Const(float num) {
            _num = num;
        }
        public override float Eval(float x) {
            return _num;
        }
        public override String ToString() {
            return _num.ToString();
        }
    }
}