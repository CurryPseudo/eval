using System;
using System.Collections.Generic;
using Curry;
namespace Curry {
    public class StringRange {
        public String _str;
        public int _begin;
        public int _end;

        public StringRange(string str) : this(str, 0, str.Length) {}
        public StringRange(string str, int begin, int end)
        {
            _str = str;
            _begin = begin;
            _end = end;
        }
        public char Head {
            get {
                return this[0];
            }
        }
        public char this[int offset] {
            get {
                return _str[_begin + offset];
            }
        }
        public bool IsEnd {
            get {
                return _begin == _end;
            }
        }
        public String Prefix(int size) {
            size = (_end - _begin) < size ? (_end - _begin) : size;
            return _str.Substring(_begin, size).ToLower();
        }
        public bool Eat() {
            if(IsEnd) return false;
            _begin++;
            return true;
        }
        public bool Eat(int size) {
            if(_begin + size > _end) {
                return false;
            }
            _begin += size;
            return true;
        }
    }
    public abstract class NonTerminal<T> {
        private StringRange _strRange;
        public N Born<N>() where N : NonTerminal<T>, new() {
            return Generate<N>(_strRange);
        }
        public static N Generate<N>(StringRange strRange) where N : NonTerminal<T>, new() {
            N n = new N();
            n._strRange = strRange;
            return n;
        }
        public abstract T Parse();
        public char head {
            get {
                return _strRange.Head;
            }
        }
        public char this[int offset] {
            get {
                return _strRange[offset];
            }
        }
        public bool IsEnd {
            get {
                return _strRange.IsEnd;
            }
        }
        public String Prefix(int size) {
            return _strRange.Prefix(size);
        }
        public bool Eat() {
            return _strRange.Eat();
        }
        public bool Eat(int size) {
            return _strRange.Eat(size);
        }

    }
    public class Ex0 : NonTerminal<EvalExpr> {
        public override EvalExpr Parse() {
            if(IsEnd) {
                throw new Exception("Parse error");
            }
            if(head == 'x') {
                Eat();
                return new Id();
            }
            else if(head <= '9' && head >= '0') {
                int num = head - '0';
                Eat();
                while(!IsEnd && head <= '9' && head >= '0') {
                    num *= 10;
                    num += head - '0';
                    Eat();
                }
                return new Const(num);
            }
            else if(head == 'e') {
                Eat();
                return new Const(MathF.E);
            }
            else if(Prefix(2) == "pi") {
                Eat(2);
                return new Const(MathF.PI);
            }
            else if(head == '(') {
                Eat();
                var e = Born<Ex12>().Parse();
                Eat();
                return e;
            }
            else {
                throw new Exception("Parse error");
            }
        }
    }

    public abstract class OneOpEx<N> : NonTerminal<EvalExpr> where N : NonTerminal<EvalExpr>, new() {
        public abstract (String name, Func<float, float> op) ParseOneOp();
        public override EvalExpr Parse() {
            (String name, Func<float, float> op) t = ParseOneOp();
            var expr = Born<N>().Parse();
            if(t.op != null) {
                return new OneOp(t.op, expr, t.name);
            }
            return expr;
        }
    }
    public abstract class TwoOpEx<N> : NonTerminal<EvalExpr> where N : NonTerminal<EvalExpr>, new() {
        public abstract (String name, Func<float, float, float> op) ParseTwoOp();
        public override EvalExpr Parse() {
            var fst = Born<N>().Parse();
            (String name, Func<float, float, float> op) t = ParseTwoOp();
            while(t.op != null) {
                var snd = Born<N>().Parse();
                fst = new TwoOp(t.op, fst, snd, t.name);
                t = ParseTwoOp();
            }
            return fst;
        }
    }
    public class Ex1 : OneOpEx<Ex0> {
        public override (String name, Func<float, float> op) ParseOneOp() {
            if(Prefix(3) == "sin") {
                Eat(3);
                return ("sin", x => MathF.Sin(x));
            }
            if(Prefix(3) == "cos") {
                Eat(3);
                return ("cos", x => MathF.Cos(x));
            }
            if(Prefix(2) == "ln") {
                Eat(2);
                return ("ln", x => MathF.Log(x));
            }
            return (null, null);
        }
    }
    public class Ex2 : TwoOpEx<Ex1> {
        public override (String name, Func<float, float, float> op) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(head == '^') {
                Eat();
                return ("pow", (x, y) => MathF.Pow(x, y));
            }
            return (null, null);
        }
    }
    public class Ex3 : TwoOpEx<Ex2> {
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(head == '*') {
                Eat();
                return ("*", (x, y) => x * y);
            }
            if(head == '/') {
                Eat();
                return ("/", (x, y) => x / y);
            }
            return (null, null);
        }
    }
    public class Ex4 : OneOpEx<Ex3> {
        public override (String, Func<float, float>) ParseOneOp() {
            if(IsEnd) return (null, null);
            if(head == '+') {
                Eat(1);
                return (null, null);
            }
            if(head == '-') {
                Eat(1);
                return ("-", x => -x);
            }
            if (head == '~') {
                Eat(1);
                return ("~", x => x == 0 ? 1 : 0);
            }
            return (null, null);
        }
    }
    public class Ex6 : TwoOpEx<Ex4> {
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(head == '+') {
                Eat();
                return ("+", (x, y) => x + y);
            }
            if(head == '-') {
                Eat();
                return ("-", (x, y) => x - y);
            }
            return (null, null);
        }
    }
    public class Ex8 : TwoOpEx<Ex6> {
        private float boolToFloat(bool b) {
            return b ? 1 : 0;
        }
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(head == '<') {
                Eat();
                if(head == '=') {
                    Eat();
                    return ("<=", (x, y) => boolToFloat(x <= y));
                }
                else {
                    return ("<", (x, y) => boolToFloat(x < y));
                }
            }
            if(head == '>') {
                Eat();
                if(head == '=') {
                    Eat();
                    return (">=", (x, y) => boolToFloat(x >= y));
                }
                else {
                    return (">", (x, y) => boolToFloat(x > y));
                }
            }
            if(Prefix(2) == "==") {
                Eat(2);
                return ("==", (x, y) => boolToFloat(x == y));
            }
            if(Prefix(2) == "~=") {
                Eat(2);
                return ("~=", (x, y) => boolToFloat(x != y));
            }
            return (null, null);
        }
    }

    public class Ex11 : TwoOpEx<Ex8> {
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(Prefix(2) == "&&") {
                Eat(2);
                return ("&&", (x, y) => (x != 0) && (y != 0) ? 1 : 0);
            }
            return (null, null);
        }
    }
    public class Ex12 : TwoOpEx<Ex11> {
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(Prefix(2) == "||") {
                Eat(2);
                return ("||", (x, y) => (x != 0) || (y != 0) ? 1 : 0);
            }
            return (null, null);
        }
    }
}