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
        public char head {
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
        public String prefix(int size) {
            size = (_end - _begin) < size ? (_end - _begin) : size;
            return _str.Substring(_begin, size).ToLower();
        }
        public bool eat() {
            if(IsEnd) return false;
            _begin++;
            return true;
        }
        public bool eat(int size) {
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
                return _strRange.head;
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
        public String prefix(int size) {
            return _strRange.prefix(size);
        }
        public bool eat() {
            return _strRange.eat();
        }
        public bool eat(int size) {
            return _strRange.eat(size);
        }

    }
    public class Ex0 : NonTerminal<EvalExpr> {
        public override EvalExpr Parse() {
            if(IsEnd) {
                throw new Exception("Parse error");
            }
            if(head == 'x') {
                eat();
                return new Id();
            }
            else if(head <= '9' && head >= '0') {
                int num = head - '0';
                eat();
                while(!IsEnd && head <= '9' && head >= '0') {
                    num *= 10;
                    num += head - '0';
                    eat();
                }
                return new Const(num);
            }
            else if(head == 'e') {
                eat();
                return new Const(MathF.E);
            }
            else if(prefix(2) == "pi") {
                eat(2);
                return new Const(MathF.PI);
            }
            else if(head == '(') {
                eat();
                var e = Born<Ex12>().Parse();
                eat();
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
            if(prefix(3) == "sin") {
                eat(3);
                return ("sin", x => MathF.Sin(x));
            }
            if(prefix(3) == "cos") {
                eat(3);
                return ("cos", x => MathF.Cos(x));
            }
            if(prefix(2) == "ln") {
                eat(2);
                return ("ln", x => MathF.Log(x));
            }
            return (null, null);
        }
    }
    public class Ex2 : TwoOpEx<Ex1> {
        public override (String name, Func<float, float, float> op) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(head == '^') {
                eat();
                return ("pow", (x, y) => MathF.Pow(x, y));
            }
            return (null, null);
        }
    }
    public class Ex3 : TwoOpEx<Ex2> {
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(head == '*') {
                eat();
                return ("*", (x, y) => x * y);
            }
            if(head == '/') {
                eat();
                return ("/", (x, y) => x / y);
            }
            return (null, null);
        }
    }
    public class Ex4 : OneOpEx<Ex3> {
        public override (String, Func<float, float>) ParseOneOp() {
            if(IsEnd) return (null, null);
            if(head == '+') {
                eat(1);
                return (null, null);
            }
            if(head == '-') {
                eat(1);
                return ("-", x => -x);
            }
            if (head == '~') {
                eat(1);
                return ("~", x => x == 0 ? 1 : 0);
            }
            return (null, null);
        }
    }
    public class Ex6 : TwoOpEx<Ex4> {
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(head == '+') {
                eat();
                return ("+", (x, y) => x + y);
            }
            if(head == '-') {
                eat();
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
                eat();
                if(head == '=') {
                    eat();
                    return ("<=", (x, y) => boolToFloat(x <= y));
                }
                else {
                    return ("<", (x, y) => boolToFloat(x < y));
                }
            }
            if(head == '>') {
                eat();
                if(head == '=') {
                    eat();
                    return (">=", (x, y) => boolToFloat(x >= y));
                }
                else {
                    return (">", (x, y) => boolToFloat(x > y));
                }
            }
            if(prefix(2) == "==") {
                eat(2);
                return ("==", (x, y) => boolToFloat(x == y));
            }
            if(prefix(2) == "~=") {
                eat(2);
                return ("~=", (x, y) => boolToFloat(x != y));
            }
            return (null, null);
        }
    }

    public class Ex11 : TwoOpEx<Ex8> {
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(prefix(2) == "&&") {
                eat(2);
                return ("&&", (x, y) => (x != 0) && (y != 0) ? 1 : 0);
            }
            return (null, null);
        }
    }
    public class Ex12 : TwoOpEx<Ex11> {
        public override (String, Func<float, float, float>) ParseTwoOp() {
            if(IsEnd) return (null, null);
            if(prefix(2) == "||") {
                eat(2);
                return ("||", (x, y) => (x != 0) || (y != 0) ? 1 : 0);
            }
            return (null, null);
        }
    }
}