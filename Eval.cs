using System;
using System.Collections.Generic;
namespace Curry {
    public abstract class EvalExpr {
        public static EvalExpr Compile(String str, int begin, int end) {
            EatEmpty(str, ref begin, ref end);
            if(begin == end) {
                return null;
            }
            EvalExpr l = EatToken(str, ref begin, ref end);
            if(l == null) return null;
            Func<EvalExpr, EvalExpr, EvalExpr> op = null;
            op = EatTwoOp(str, ref begin, ref end);
            while(op != null) {
                EvalExpr r = EatToken(str, ref begin, ref end);
                l = op(l, r);
                op = EatTwoOp(str, ref begin, ref end);
            }
            return l;
        }
        public static void EatEmpty(String str, ref int begin, ref int end) {
            while(begin != end && str[begin] == ' ') {
                begin++;
            }
        }
        public static EvalExpr EatToken(String str, ref int begin, ref int end) {
            EatEmpty(str, ref begin, ref end);
            if(begin == end) {
                return null;
            }
            if(str[begin] == '-') {
                begin++;
                EvalExpr token = EatToken(str, ref begin, ref end);
                return new OneOp(x => -x, token);
            }
            if(str.StartsWith("sin")) {
                begin += 3;
                EvalExpr token = EatToken(str, ref begin, ref end);
                return new OneOp(x => MathF.Sin(x), token);
            }
            if(str.StartsWith("cos")) {
                begin += 3;
                EvalExpr token = EatToken(str, ref begin, ref end);
                return new OneOp(x => MathF.Cos(x), token);
            }
            if(str[begin] == '(') {
                for(var j = begin + 1; j != end; j++) {
                    if(str[j] == ')') {
                        EvalExpr eval = Compile(str, begin + 1, j);
                        begin = j + 1;
                        return eval;
                    }
                }
                return null;
            }
            if(str[begin] == 'x') {
                begin++;
                return new Id();
            } 
            int i = begin;
            for(; i != end && str[i] <= '9' && str[i] >= '0'; i++);
            if(i == begin) {
                return null;
            }
            int num = System.Convert.ToInt32(str.Substring(begin, i - begin));
            begin = i;
            return new Const(num);
        }
        public static Func<EvalExpr, EvalExpr, EvalExpr> EatTwoOp(String str, ref int begin, ref int end) {
            EatEmpty(str, ref begin, ref end);
            if(begin == end) return null;
            Func<float, float, float> op;
            switch (str[begin]) {
                case '+':
                    op = (x, y) => x + y;
                    break;
                case '*':
                    op = (x, y) => x * y;
                    break;
                case '/':
                    op = (x, y) => x / y;
                    break;
                case '-':
                    op = (x, y) => x - y;
                    break;
                case '>':
                    op = (x, y) => x > y ? 1 : 0;
                    break;
                case '=':
                    op = (x, y) => x == y ? 1 : 0;
                    break;
                case '<':
                    op = (x, y) => x < y ? 1 : 0;
                    break;
                case '&':
                    op = (x, y) => x != 0 && y != 0 ? 1 : 0;
                    break;
                case '|':
                    op = (x, y) => x != 0 || y != 0 ? 1 : 0;
                    break;
                case '^':
                    op = (x, y) => MathF.Pow(x, y);
                    break;
                default:
                    return null;
            }
            begin++;
            return (l, r) => new TwoOp(op, l , r);
        }
        public static EvalExpr Compile(String str) {
            return Compile(str, 0, str.Length);
        }
        public abstract float Eval(float x);
    }
    public class Id : EvalExpr {
        public override float Eval(float x) {
            return x;
        }
    }
    public class OneOp : EvalExpr {
        private EvalExpr _eval;
        private Func<float, float> _op;
        public OneOp(Func<float,float> op, EvalExpr eval) {
            _op = op;
            _eval = eval;
        }
        public override float Eval(float x) {
            return _op(_eval.Eval(x));
        }
    }
    public class TwoOp : EvalExpr {
        private EvalExpr _left;
        private EvalExpr _right;
        private Func<float, float, float> _op;
        public TwoOp(Func<float, float, float> op, EvalExpr left, EvalExpr right) {
            _op = op;
            _left = left;
            _right = right;
        }
        public override float Eval(float x) {
            return _op(_left.Eval(x), _right.Eval(x));
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
    }
}