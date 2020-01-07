using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SikiLang
{

    

    interface INum: ITerm
    {
        Double GetDoubleValue();
    }

    //memo ITermを継承するのが普通?　かと思いきやFuncはない。
    interface ITime
    {
        String ToString();
        void Add(TimeSpan ts); //memo TimeSpanでいいのか？ TimeLengthは？
        void Sub(TimeSpan ts);
    }

    interface IToken
    {
        int Match(String s);
        //bool AllowReady(List<Object> tokens, int index); // 処理すべきトークンか?　Ready前に
        bool Ready(List<Object> tokens, int index); // 今のところ処理したそばから計算してる。構築しない。
    }

    interface INilToken
    {
    }

    interface ITerm
    {
        String ToString();
        ITerm Func(String ope, ITerm a, ITerm b);
    }

    enum OpePriority
    {
        Default = 3,
        A = 50,
        B = 40, // */
        C = 30,
        D = 20, // +-
        E = 10,
    }


    interface IOpe
    {
        //ITerm Func(ITerm a, ITerm b);
        //OpePriority Priority { get; }
        //String ToString();
        //bool Prompt(List<Object> tokens); //即座に実行
        //bool Parse(List<Object> tokens); //構築
        //ITerm Run(); //Parseしたものを実行
    }

    class Test
    {
        static public void Manual(String source, Double answer)
        {
            var r = new Runner(source);
            if (r == null)
                throw new Exception("");
            if (r.Lex() == false)
                throw new Exception("");
            var res = r.Run();
            if (res == null)
                throw new Exception("");

            Double res2 = Double.Parse(res.ToString());
            Double ans2 = answer - answer * (1.0 / 1000000000000); //memo 精度はある程度無視する。1兆円で1円。バグ見つけが目的。
            Double ans3 = answer + answer * (1.0 / 1000000000000);

            if (answer > 0)
            {
                if (!(ans2 < res2 && res2 < ans3))
                    throw new Exception(res.ToString());
            }
            else if (answer < 0)
            {
                if (!(ans3 < res2 && res2 < ans2))
                    throw new Exception(res.ToString());
            }
            else if (answer == 0)
            {
                if (res2 != answer)
                    throw new Exception(res.ToString());
            }
            else
                throw new Exception("");
        }

        static public void Auto()
        {
            Manual("1 + 2 * 3^2 + 4", 23);
            Manual("1 + 2 * 3 * 4^2 + 5", 102);
            Manual("1 + 2 * 3 * 4^2 * 3 + 5", 294);
            Manual("1 + 2 + 3 * 4 * 5", 63);
            Manual("1 * 2 + 3", 5);
            Manual("1 * 2 * 3 + 4 + 5", 15);
            Manual("1 + 2^2 * 3 + 4", 17);

            string line;
            var file = new System.IO.StreamReader("ForLinCal.txt");
            while ((line = file.ReadLine()) != null)
            {
                var m = Regex.Match(line, @"(.*)=(.*)");
                string source = m.Groups[1].ToString();
                double ans;
                if (Double.TryParse(m.Groups[2].ToString(), out ans))
                {
                    Manual(source, ans);
                }
            }
            file.Close();
        }
    }


    class StackFIFO<T>
    {
        List<T> items = new List<T>();

        public StackFIFO()
        {
        }

        public T Pop()
        {
            if (items.Count > 0)
            {
                T a = items[0];
                items.RemoveAt(0);
                return a;
            }
            else
                return default(T);
        }

        public void Put(T item)
        {
            items.Insert(0, item);
        }

        public void Push(T item)
        {
            items.Add(item);
        }

        public T Peek()
        {
            if (items.Count() > 0)
                return items[0];
            else
                return default(T);
        }

        public StackFIFO<T> CloneInstance()
        {
            var a = new StackFIFO<T>();
            a.items = new List<T>(items);
            return a;
        }
    }

    class TokenList : IEnumerable
    {
        String direction;
        IToken[] tokens;
        bool __ld = false;
        bool __rd = false;
        public bool LD { get { return __ld; } }
        public bool RD { get { return __rd; } }


        public TokenList(String direction, params IToken[] tokens)
        {
            this.direction = direction;

            if (Regex.IsMatch(direction, @"[lL](eft)?"))
                __ld = true;
            else if (Regex.IsMatch(direction, @"[rR](eft)?"))
                __rd = true;
            else
                throw new Exception("");

            this.tokens = new IToken[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
                this.tokens[i] = tokens[i];
        }

        public IEnumerator GetEnumerator()
        {
            return new TokenListEnumerator(this);
        }

        private class TokenListEnumerator: IEnumerator
        {
            private int index;
            private TokenList list;

            public TokenListEnumerator(TokenList list)
            {
                this.list = list;
                Reset();
            }

            public object Current
            {
                get { return this.list.tokens[index]; }
            }

            public bool MoveNext()
            {
                if (index < list.tokens.Length - 1)
                    index++;
                else
                    return false;
                return true;
                //if (Regex.IsMatch(list.direction, @"[lL](eft)?"))
                //{
                //    if (index < list.tokens.Length - 1)
                //        index++;
                //    else
                //        return false;
                //}
                //else if (Regex.IsMatch(list.direction, @"[rR](eft)?"))
                //{
                //    if (index > 0)
                //        index--;
                //    else
                //        return false;
                //}
                //else
                //    throw new Exception("");
                //return true;
            }

            public void Reset()
            {
                index = -1;
                //if (Regex.IsMatch(list.direction, @"[lL](eft)"))
                //    this.index = -1;
                //else if (Regex.IsMatch(list.direction, @"[rR](eft)?"))
                //    this.index = list.tokens.Length; //memo -1になりうる
                //else
                //{
                //    Debug.Assert(false);
                //    throw new Exception("");
                //}
            }
        }
    }

    class Runner
    {
        String source { get; set; }
        List<Object> tokens; //memo 専用クラス作ったほうが便利かも
        public TokenList[] TokenOrder = new TokenList[] { //memo これはpublicにして追加できるようにしたほうがいいか?
            new TokenList("Left", new Time(), new Dec(), new Hex(), new Bit()),
            new TokenList("Left", new RoundBrackets()),
            new TokenList("Right", new Posi(), new Pow(), new Log(), new Nega()),
            new TokenList("Left", new Mul(), new Div(), new Mod()),
            new TokenList("Left", new Add(), new Sub()),
        };

        public Runner(String s)
        {
            source = s;
            tokens = new List<Object>();
        }

        static public List<Object> Lex(String source)
        {
            Runner r = new Runner(source);
            if (r.Lex() == false)
                return null;
            return r.tokens;
        }

        public bool Lex()
        {
            var reg = new Regex(@"\S.*");
            var m = reg.Match(source, 0);
            while (m.Success)
            {
                int len = 0;
                foreach (TokenList tl in TokenOrder)
                {
                    foreach (IToken t in tl)
                    {
                        if ((len = t.Match(m.Value)) > 0)
                        {
                            tokens.Add(m.Value.Substring(0, len));
                            goto MATCH;
                        }
                    }
                }
                return false;
            MATCH:
                m = reg.Match(m.Value.Substring(len));
            }
            return true;
        }

        static public ITerm Run(List<Object> tokens)
        {
            Runner r = new Runner("");
            r.tokens = tokens;
            return r.Run();
        }

        public ITerm Run()
        {
            foreach (TokenList tl in TokenOrder)
            {
                if (tl.LD)
                {
                    for (int i = 0; i < tokens.Count; i++)
                        foreach (IToken t in tl)
                            if (t.Ready(tokens, i))
                            { i = -1; break; } //memo これでいいのか？
                            else
                            { }
                }
                else if (tl.RD)
                {
                    for (int i = tokens.Count - 1; i >= 0; i--)
                        foreach(IToken t in tl)
                            if (t.Ready(tokens, i))
                            { i = tokens.Count; break; } //memo ここも
                            else
                            { }
                }
                else
                        throw new Exception("");
            }
            if (tokens.Count == 1 && tokens[0] is ITerm)
                return (ITerm)tokens[0];
            else
                return null;
        }


        //private ITerm runSub2(StackFIFO<IOpe> opeStack, StackFIFO<ITerm> termStack)
        //{
        //    ITerm term1, term2;
        //    IOpe ope1 = opeStack.Pop();
        //    if (ope1 == null)
        //        return termStack.Pop();

        //    term1 = termStack.Pop();
        //    IOpe ope2 = opeStack.Peek();
        //    if (ope2 != null && ope2.Priority > ope1.Priority)
        //    {
        //        term2 = runSub2(opeStack, termStack);
        //        termStack.Put(ope1.Func(term1, term2));
        //        return runSub2(opeStack, termStack);
        //    }
        //    else if (ope2 != null && ope2.Priority < ope1.Priority)
        //    {
        //        term2 = termStack.Pop();
        //        return ope1.Func(term1, term2);
        //    }
        //    else
        //    {
        //        term2 = termStack.Pop();
        //        termStack.Put(ope1.Func(term1, term2));
        //        return runSub2(opeStack, termStack);
        //    }
        //}

        //private ITerm runSub3(StackFIFO<IOpe> opeStack, StackFIFO<ITerm> termStack, int level)
        //{
        //    ITerm term1, term2;
        //    IOpe ope1 = opeStack.Pop();
        //    if (ope1 == null)
        //        return termStack.Peek();

        //    term1 = termStack.Pop();
        //    IOpe ope2 = opeStack.Peek();
        //    if (ope2 != null && ope2.Priority > ope1.Priority)
        //    {
        //        termStack.Put(runSub3(opeStack, termStack, level + 1));
        //        termStack.Put(term1);
        //        opeStack.Put(ope1);
        //        return runSub3(opeStack, termStack, level);
        //    }
        //    else if (ope2 != null && ope2.Priority < ope1.Priority && level > 0)
        //    {
        //        term2 = termStack.Pop();
        //        return ope1.Func(term1, term2);
        //    }
 
        //    term2 = termStack.Pop();
        //    termStack.Put(ope1.Func(term1, term2));
        //    return runSub3(opeStack, termStack, level);

        //}
      
        //private ITerm runSub(Queue opeStack, Queue termStack)
        //{
        //    IOpe ope1, ope2;
        //    ITerm term1, term2;

        //    term1 = (ITerm)termStack.Dequeue();
        //    ope1 = (IOpe)opeStack.Dequeue();
        //    if (ope1 == null)
        //        return term1;

        //    do
        //    {
        //        ope2 = (IOpe)opeStack.Peek();
        //        if (ope2 != null)
        //        {
        //            if (ope2.Priority > ope1.Priority)
        //            {
        //                term2 = runSub(opeStack, termStack);
        //            }
        //            else if (ope2.Priority < ope1.Priority)
        //            {
        //                term2 = (ITerm)termStack.Dequeue();
        //                return ope1.Func(term1, term2);
        //            }
        //            else
        //                term2 = (ITerm)termStack.Dequeue();
        //        }
        //        else
        //            term2 = (ITerm)termStack.Dequeue();

        //        term1 = ope1.Func(term1, term2);
        //        ope1 = (IOpe)opeStack.Dequeue();
        //    } while (ope1 != null);
            
        //    return term1;
        //}
    }

    abstract class TokenPackBase
    {
        abstract protected IToken[] tokens { get; }

        virtual public int Match(String s)
        {
            return Token.Match(s, tokens);
        }

        virtual public bool Ready(List<Object> tokens, int index)
        {
            return Token.Ready(tokens, index);
        }
    }

    static class Token
    {
        //static public int Match<T>(String s, params IToken<T>[] args)// where T: IToken<T>
        //{
        //    int n = 0;
        //    foreach (IToken<T> current in args)
        //    {
        //        n = current.Match(s);
        //        if (n > 0)
        //            return n;
        //    }
        //    return n;
        //}

        static public int Match(String s, params IToken[] args)
        {
            int n = 0;
            foreach (IToken current in args)
            {
                n = current.Match(s);
                if (n > 0)
                    return n;
            }
            return n;
        }

        static public bool MatchToken<T>(List<Object> tokens, int index)
        {
            if (index < 0 || tokens.Count <= index)
                return false;
            return tokens[index] is T;
        }

        //なぜか必要だと思った。
        //static public bool NotMatchToken<T>(List<Object> tokens, int index)
        //{
        //    if (index < 0 || tokens.Count <= index)
        //        return true;
        //    return !(tokens[index] is T);
        //}

        static public bool Ready(List<Object> tokens, int index, params IToken[] args)
        {
            foreach (IToken current in args)
            {
                if (current.Ready(tokens, index) == true)
                    return true;
            }
            return false;
        }


        //static public T NewInstance<T>(String s, params IToken<T>[] args)// where T : IToken<T>
        //{
        //    foreach (IToken<T> current in args)
        //    {
        //        if (current.Match(s) > 0)
        //        {
        //            return  current.NewInstance(s);
        //        }
        //    }
        //    Debug.Assert(false);
        //    return default(T);
        //}
    }

    abstract class TokenBase: IToken
    {
        abstract protected String pat { get; }

        virtual public int Match(String s)
        {
            var m = Regex.Match(s, pat);
            return m.Value.Length;
        }

        //abstract public bool AllowReady(List<Object> tokens, int index);
        abstract public bool Ready(List<Object> tokens, int index);

        //virtual public T NewInstance(String s)
        //{
        //    Debug.Assert(Match(s) > 0);
        //    var m = Regex.Match(s, pat);
        //    return NewSelf(m.Value);
        //}

        //abstract public T NewSelf(String s);
    }

    abstract class OpeBase : TokenBase, IToken, IOpe
    {
        abstract protected String ope { get; }

        override public bool Ready(List<Object> tokens, int index)
        {
            if (Token.MatchToken<ITerm>(tokens, index-1) && Token.MatchToken<String>(tokens, index) && Token.MatchToken<ITerm>(tokens, index+1))
            {
                if (Regex.IsMatch(((String)tokens[index]), pat))
                {
                    ITerm a = (ITerm)tokens[index - 1];
                    ITerm b = (ITerm)tokens[index + 1];
                    if (a == null || b == null)
                        return false;
                    ITerm c = a.Func(ope, a, b);
                    if (c == null)
                        return false;
                    tokens[index] = c;
                    tokens.RemoveAt(index - 1);
                    tokens.RemoveAt(index); //index+1だったものを削除
                    return true;
                }
            }
            return false; //memo 上のfalseとこのfalseの意味が違う? どうしよう?
        }
    }

    abstract class UnaryOpeBase : TokenBase, IToken, IOpe
    {
        abstract protected String ope { get; }

        override public bool Ready(List<Object> tokens, int index)
        {
            if (!Token.MatchToken<ITerm>(tokens, index-1) && Token.MatchToken<String>(tokens, index) && Token.MatchToken<ITerm>(tokens, index+1))
            {
                if (Regex.IsMatch(((String)tokens[index]), pat))
                {
                    ITerm a = (ITerm)tokens[index + 1];
                    if (a == null)
                        return false;
                    ITerm c = a.Func(ope, a, null);
                    if (c == null)
                        return false;
                    tokens[index] = c;
                    tokens.RemoveAt(index + 1);
                    return true;
                }
            }
            return false; //memo 上のfalseとこのfalseの意味が違う? どうしよう?
        }
    }

    class Nega : UnaryOpeBase, IOpe
    {
        override protected String ope
        {
            get { return "nega"; }
        }
        override protected String pat
        {
            get { return @"^\-"; }
        }
    }

    class Posi : UnaryOpeBase, IOpe
    {
        override protected String ope
        {
            get { return "posi"; }
        }
        override protected String pat
        {
            get { return @"^\+"; }
        }
    }

    class Add : OpeBase, IToken
    {
        override protected String ope
        {
            get { return "+"; }
        }
        override protected String pat
        {
            get { return @"^\+"; }
        }
    }

    class Sub : OpeBase
    {
        override protected String ope
        {
            get { return "-"; }
        }
        protected override string pat
        {
            get { return @"^-"; }
        }
    }

    class Mul : OpeBase
    {
        override protected String ope
        {
            get { return "*"; }
        }
        protected override string pat
        {
            get { return @"^\*"; }
        }
    }

    class Div : OpeBase
    {
        override protected String ope
        {
            get { return "/"; }
        }
        protected override string pat
        {
            get { return @"^/"; }
        }
    }

    class Pow : OpeBase
    {
        override protected String ope
        {
            get { return "^"; }
        }
        protected override string pat
        {
            get { return @"^((\^)|(\*\*))"; }
        }
    }

    class Log : OpeBase
    {
        override protected String ope
        {
            get { return "_"; }
        }
        protected override string pat
        {
            get { return @"^_"; }
        }
    }

    class Mod : OpeBase
    {
        override protected String ope
        {
            get { return "%"; }
        }
        protected override string pat
        {
            get { return @"^%"; }
        }
    }

    class RoundBrackets : IToken
    {
        public int Match(String s)
        {
            if (s[0] == '(' || s[0] == ')')
                return 1;
            else
                return 0;
        }

        public bool Ready(List<Object> tokens, int index)
        {
            if (tokens[index] is String && ((String)tokens[index] == "("))
            {
                for (int i = index+1; i < tokens.Count; i++)
                {
                    if (tokens[i] is String)
                    {
                        if (((String)tokens[i]) == "(")
                            return false;
                        if (((String)tokens[i]) == ")")
                        {
                            var a = new List<Object>();
                            for (int j = index + 1; j < i; j++)
                                a.Add(tokens[j]);

                            ITerm b = Runner.Run(a); //index+2からi-1までをParse(今のところRunがそれ)
                            if (b == null) return false;
                            tokens.RemoveRange(index + 1, i - (index + 1) + 1);//index+1からiまでを削除
                            tokens[index] = b; //indexに結果を入れる
                            return true;
                        }
                    }
                }
            }
            return false; //memo 括弧が足りないとかのエラー処理はどうしよう?
        }
    }


    //class Term: IToken<ITerm>, ITerm
    //{
    //    ITerm term;
    //    IToken<ITerm>[] termList = new IToken<ITerm>[]{
    //            new Num(),
    //            new Time(),
    //        };

    //    public int Match(String s)
    //    {
    //        return Token.Match(s, termList);
    //    }

    //    public ITerm NewInstance(String s)
    //    {
    //        Debug.Assert(Match(s) > 0);
    //        ITerm a  = Token.NewInstance(s, termList);
    //        if (a == null)
    //            return null;
    //        var b = new Term();
    //        b.term = a;
    //        return b;
    //    }

    //    public Term()
    //    {
    //        this.term = new Dec(10); //memo とりあえず。
    //    }

    //    public Term(ITerm it)
    //    {
    //        this.term = it;
    //    }

    //    public ITerm Func(String ope, ITerm a, ITerm b)
    //    {
    //        if (a is Term && b is Term)
    //        {
    //            if (((Term)a).term != null)
    //            {
    //                ITerm it = term.Func(ope, ((Term)a).term, ((Term)b).term);
    //                return new Term(it);
    //            }
    //            else
    //                return null; //memo エラー処理する？
    //        }
    //        else
    //            return null; //memo エラー処理する?
    //    }

    //    public override string ToString()
    //    {
    //        return term.ToString();
    //    }
    //}

    class Time : ITerm, IToken
    {
        ITime time;
        IToken[] timeList = new IToken[] {
                new Date(),
                new TimeLength(),
            };

        public int Match(String s)
        {
            return Token.Match(s, timeList);
        }

        public bool Ready(List<Object> tokens, int index)
        {
            if (Token.Ready(tokens, index, timeList) == true)
            {
                var t = new Time();
                Debug.Assert(tokens[index] is Date || tokens[index] is TimeLength);
                t.time = (ITime)tokens[index];
                tokens[index] = t;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            if (this.time != null)
                return this.time.ToString();
            return base.ToString();
        }

        public ITerm Func(String ope, ITerm a, ITerm b)
        {
            if (a is Time && b is Time)
            {
                Time ta = (Time)a;
                Time tb = (Time)b;

                if (ta.time is Date && tb.time is TimeLength)
                {
                    Date d = (Date)ta.time;
                    TimeLength tl = (TimeLength)tb.time;

                    if (ope == "+")
                        return AddToDate(d, tl);
                    if (ope == "-")
                        return SubToDate(d, tl);
                    return null;
                }
                else if (ta.time is Date && tb.time is Date)
                {
                    Date d1 = (Date)ta.time;
                    Date d2 = (Date)tb.time;

                    if (ope == "-")
                        return SubToTimeLength(d1, d2);
                    return null;
                }
                else if (ta.time is TimeLength && tb.time is TimeLength)
                {
                    TimeLength tl1 = (TimeLength)ta.time;
                    TimeLength tl2 = (TimeLength)tb.time;

                    if (ope == "+")
                        return AddToTimeLength(tl1, tl2);
                    else if (ope == "-")
                        return SubToTimeLength(tl1, tl2);
                    return null;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //memo publicにする必要があるか?　staticも
        // 日付+時間 -> 日付(Evaでいう日付逆算)
        static public ITerm AddToDate(Date a, TimeLength b)
        {
            Time n = new Time();
            n.time = new Date(a);
            n.time.Add(b.GetTimeSpan());
            return n;
        }

        static public ITerm SubToDate(Date a, TimeLength b)
        {
            Time n = new Time();
            n.time = new Date(a);
            n.time.Sub(b.GetTimeSpan());
            return n;
        }

        // 日付-日付 -> 時間(期間計算)
        static public ITerm SubToTimeLength(Date a, Date b)
        {
            Time n = new Time();
            n.time = new TimeLength(a.GetDateTime().Subtract(b.GetDateTime()));
            return n;
        }

        // 時間-時間 -> 時間
        static public ITerm SubToTimeLength(TimeLength a, TimeLength b)
        {
            Time n = new Time();
            n.time = new TimeLength(a);
            n.time.Sub(b.GetTimeSpan());
            return n;
        }


        static public ITerm AddToTimeLength(TimeLength a, TimeLength b)
        {
            Time n = new Time();
            n.time = new TimeLength(a);
            n.time.Add(b.GetTimeSpan());
            return n;
        }
    }

    // DayとかHourとか分けようかと思ったけど、TimeSpanだけで表せるからまとめた。
    class TimeLength : IToken, ITime
    {
        TimeSpan ts;
        String dayPat = @"^(\d+)((日)|(d(ay)?)|(D(ay)?))";
        String hourPat = @"^(\d+)((時間)|(h(our)?)|(H(our)?))";
        String minPat = @"^(\d+)((分)|(m(in)?)|(M(in)?))";
        String secPat = @"^(\d+)((秒)|(s(ec)?)|(S(ec)?))";

        public TimeLength()
        {
        }

        public TimeLength(TimeSpan a)
        {
            this.ts = a;
        }

        public TimeLength(TimeLength a)
        {
            this.ts = a.ts;
        }

        public override string ToString()
        {
            if (ts != null)
            {
                String s = "";
                if (ts.Days != 0) s += ts.Days.ToString() + "day";
                if (ts.Hours != 0) s += ts.Hours.ToString() + "hour";
                if (ts.Minutes != 0) s += ts.Minutes.ToString() + "min";
                if (ts.Seconds != 0) s += ts.Seconds.ToString() + "sec";
                return s;
            }
            return base.ToString();
        }

        public int Match(String s)
        {
            int len = 0;
            var a = new String[] { dayPat, hourPat, minPat, secPat };
            for(int i = 0; i < a.Length; i++)
            {
                var m = Regex.Match(s.Substring(len), a[i]);
                len += m.Length;
            }

            return len;
        }

        public bool Ready(List<Object> tokens, int index)
        {
            int day = 0;
            int hour = 0;
            int min = 0;
            int sec = 0;
            String s;

            if (Token.MatchToken<String>(tokens, index)) //memo ちょっとあぶないかも。なんでも処理される?
                s = ((String)tokens[index]);
            else
                return false;

            int len = 0;
            var pats = new String[] { dayPat, hourPat, minPat, secPat };
            foreach(String pat in pats)
            {
                var m = Regex.Match(s.Substring(len), pat);
                if (m.Length > 0)
                {
                    if (dayPat == pat)
                        day = int.Parse(m.Groups[1].ToString());
                    else if (hourPat == pat)
                        hour = int.Parse(m.Groups[1].ToString());
                    else if (minPat == pat)
                        min = int.Parse(m.Groups[1].ToString());
                    else if (secPat == pat)
                        sec = int.Parse(m.Groups[1].ToString());
                }
                len += m.Length;
            }
            if (len <= 0)
                return false;

            var t = new TimeLength();
            t.ts = new TimeSpan(day, hour, min, sec);
            if (t.ts != null)
            {
                tokens[index] = t;
                return true;
            }
            else
                return false;
        }

        public TimeSpan GetTimeSpan()
        {
            return ts;
        }

        public void Add(TimeSpan a)
        {
            this.ts = ts.Add(a);
        }

        public void Sub(TimeSpan a)
        {
            this.ts = ts.Subtract(a);
        }
    }

    class Date : ITime, IToken
    {
        DateTime dt;
        //String pat = @"^D(\d\d\d\d)?/?(\d?\d)?/?(\d?\d)\s*(\d?\d)?:?(\d?\d)?:?(\d?\d)?";
        String pat = @"^D(\d{4})?/?(\d{1,2})/(\d{1,2})(?:\s(\d{1,2}):(\d{1,2}):(\d{1,2}))?";

        public Date()
        {
        }

        public Date(Date a)
        {
            this.dt = a.dt;
        }

        public int Match(String s)
        {
            var m = Regex.Match(s, pat);
            return m.Value.Length;
        }

        public bool Ready(List<Object> tokens, int index)
        {
            if (!(tokens[index] is String))
                return false;
            if (Match((String)tokens[index]) <= 0)
                return false;
            var m = Regex.Match(((String)tokens[index]), pat);
            var date = new Date();
            int year, month, day, hour, min, sec;

            if (m.Groups[1].ToString() == "")
                year = DateTime.Now.Year;
            else
                year = int.Parse(m.Groups[1].ToString());

            if (m.Groups[2].ToString() == "")
                month = DateTime.Now.Month;
            else
                month = int.Parse(m.Groups[2].ToString());

            if (m.Groups[3].ToString() == "")
                day = DateTime.Now.Day;
            else
                day = int.Parse(m.Groups[3].ToString());

            if (m.Groups[4].ToString() == "")
                hour = DateTime.Now.Hour;
            else
                hour = int.Parse(m.Groups[4].ToString());

            if (m.Groups[5].ToString() == "")
                min = DateTime.Now.Minute;
            else
                min = int.Parse(m.Groups[5].ToString());

            if (m.Groups[6].ToString() == "")
                sec = DateTime.Now.Second;
            else
                sec = int.Parse(m.Groups[6].ToString());

            try
            {
                date.dt = new DateTime(year, month, day, hour, min, sec);
            }
            catch
            {
                return false; // dayが0のとき
            }
            if (date.dt != null)
            {
                tokens[index] = date;
                return true;
            }

            return false;
        }

        override public String ToString()
        {
            if (this.dt != null)
                return this.dt.ToString();
            return base.ToString();
        }

        public DateTime GetDateTime()
        {
            return dt;
        }

        public void Add(TimeSpan ts)
        {
            dt = dt.Add(ts);
        }

        public void Sub(TimeSpan ts)
        {
            dt = dt.Subtract(ts);
        }
    }

    //memo もう使わなくなったかも
    class Num : TokenPackBase
    {
        override protected IToken[] tokens
        {
            get
            {
                return new IToken[] {
                    new Dec(),
                    new Hex(),
                };
            }
        }
    }

    abstract class NumBase: TokenBase, INum
    {
        virtual protected Double dv {get; set;}

        public NumBase()
        {
            this.dv = Double.NaN;
        }

        public NumBase(Double a)
        {
            this.dv = a;
        }

        abstract public INum NewSelf(Double a);
        abstract public INum NewSelf(String s);

        public Double GetDoubleValue()
        {
            return dv;
        }

        override public bool Ready(List<Object> tokens, int index)
        {
            if (tokens[index] is String && Match(((String)tokens[index])) > 0) //memo いちいちMatchする必要あるか?
            {
                var a = NewSelf(((String)tokens[index]));
                if (a == null)
                    return false;
                tokens[index] = a;
            }
            return false;
        }

        public ITerm Func(String ope, ITerm a, ITerm b)
        {
            if (a is INum && b is INum)
            {
                INum ta = (INum)a;
                INum tb = (INum)b;

                switch(ope)
                {
                    case "+":
                        return Add(ta, tb);
                    case "-":
                        return Sub(ta, tb);
                    case "*":
                        return Mul(ta, tb);
                    case "/":
                        return Div(ta, tb);
                    case "^":
                        return Pow(ta, tb);
                    case "_":
                        return Log(ta, tb);
                    case "%":
                        return Mod(ta, tb);
                    default:
                        return null;
                }
            }
            else if (a is INum && b == null)
            {
                INum ta = (INum)a;

                switch (ope)
                {
                    case "nega":
                        return Nega(ta);
                    case "posi":
                        return Posi(ta);
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        public ITerm Add(INum a, INum b)
        {
            var c = NewSelf(a.GetDoubleValue() + b.GetDoubleValue()); //memo Funcのほうで何とかするか?

            if (c != null) //memo ここ必要か?
                return c;
            else
                return null;
        }

        public ITerm Sub(INum a, INum b)
        {
            return NewSelf(a.GetDoubleValue() - b.GetDoubleValue());
        }

        public ITerm Mul(INum a, INum b)
        {
            return NewSelf(a.GetDoubleValue() * b.GetDoubleValue());
        }

        public ITerm Div(INum a, INum b)
        {
            return NewSelf(a.GetDoubleValue() / b.GetDoubleValue()); //memo ここは何とかすべきかも。それかFuncで処理するか?
        }

        public ITerm Pow(INum a, INum b)
        {
            return NewSelf(Math.Pow(a.GetDoubleValue(), b.GetDoubleValue()));
        }

        public ITerm Log(INum a, INum b)
        {
            
            return NewSelf(Math.Log(a.GetDoubleValue()) / Math.Log(b.GetDoubleValue()));
        }

        public ITerm Mod(INum a, INum b)
        {

            return NewSelf(a.GetDoubleValue() % b.GetDoubleValue());
        }

        public ITerm Nega(INum a)
        {
            return NewSelf(-a.GetDoubleValue());
        }

        public ITerm Posi(INum a)
        {
            return NewSelf(a.GetDoubleValue());
        }

    }

    class Dec : NumBase
    {
        override protected String pat {
            get { return @"^\d+(?![xb])(\.?\d*(e[\+\-]?\d+)?)?"; } //memo ここでhexやbitを意識するものか?
        }

        public Dec() : base() { }
        public Dec(Double d) : base(d) { }

        public override string ToString()
        {
            return this.dv.ToString();
        }

        public override INum NewSelf(Double a)
        {
            return new Dec(a);
        }

        public override INum NewSelf(string s)
        {
            return new Dec(Double.Parse(s));
        }
    }

    class Bit : NumBase
    {
        override protected String pat
        {
            get { return @"^0b[01]+"; }
        }

        public Bit() :base() {}
        public Bit(Double a): base(a) {}

        public override string ToString()
        {
            return "0b" + Convert.ToString(Convert.ToInt32(this.dv), 2);
        }

        public override INum NewSelf(Double a)
        {
            return new Bit(a);
        }

        public override INum NewSelf(string s)
        {
            int a = 0;
            try
            {
                a = Convert.ToInt32(s.Substring(2), 2);
            }
            catch
            {
                return null;
            }

            return new Bit(a);
        }
    }

    class Hex : NumBase
    {
        override protected String pat
        {
            get { return @"^0x[0-9a-f]+"; }
        }
        public Hex() : base() { }
        public Hex(Double a) : base(a) { }

        public override string ToString()
        {
            return "0x" + Convert.ToString(Convert.ToInt32(this.dv), 16);
        }

        public override INum NewSelf(Double a)
        {
            return new Hex(a);
        }

        public override INum NewSelf(string s)
        {
            return new Hex(Convert.ToInt32(s, 16));
        }
    }
}




/*　こっちの方がいい気がしてきた。
 * TAdd
	Eval
		Operator.Add(left.Eval(), right.Eval()
 */



namespace WindowsFormsApplication1
{
    class TextBoxEx : TextBox
    {
        public TextBoxEx()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
        }
    }

    class Siki: SplitContainer
    {
        TextBoxEx exp;
        TextBoxEx res;
        public TextBoxEx Exp { get{return exp;}}
        public TextBoxEx Res { get { return res; } }

        public Siki()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            exp = new TextBoxEx();
            res = new TextBoxEx();

            this.Panel1.Controls.Add(exp);
            this.Panel2.Controls.Add(res);

            this.Size = new System.Drawing.Size(this.Width, exp.Height);
            this.SplitterDistance = (int)(this.Size.Width * 0.6);

            this.Exp.TextChanged += new EventHandler(_TextChanged);
        }

        //なぜか無理
        //protected override void OnSplitterMoved(SplitterEventArgs args)
        //{
        //}

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Control ac = this.ActiveControl;
            TextBoxEx t = ac is TextBoxEx ? (TextBoxEx)ac : null;
            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    if (t != null)
                    {
                        if (t.SelectionLength <= 0)
                        {
                            //Clipboard.SetText(this.Exp.Text + " = " + this.Res.Text); //なぜかエラーがまれに起こる
                            Clipboard.SetDataObject(this.Exp.Text + " = " + this.Res.Text, true, 20, 200);

                        }
                        else
                            Clipboard.SetDataObject(t.SelectedText, true, 20, 200);
                    }
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void _TextChanged(Object sender, EventArgs e)
        {
            var r = new SikiLang.Runner(exp.Text);
            if (r.Lex())
            {
                var t = r.Run();
                if (t != null)
                    res.Text = t.ToString();
                else
                    res.Text = "run error";
            }
            else
                res.Text = "lex error";

            base.OnTextChanged(e);
        }
    }

    class SikiGrid : ContainerControl
    {
        int _LineSize = 0;
        int _RowSize = 0;
        public int LineSize { get { return _LineSize; } }
        public int RowSize { get { return _RowSize; } }
        int _PadX = 30;
        int _PadY = 3;
        public int PadX { get { return _PadX; } set { _PadX = value; } }
        public int PadY { get { return _PadY; } set { _PadY = value; } }


        [DllImport("user32")]
        private static extern bool SendMessage(IntPtr hWnd, int msg, bool wParam, IntPtr lParam);

        public SikiGrid()
        {
            createCell(0, 0);
            _LineSize = 1;
            _RowSize = 1;
            
            //parent.Resize += new EventHandler(Parent_Resize);
        }

        public void SetMinimumSize()
        {
            this.MinimumSize = new Size(MinimumSize.Width, this.Controls[0].Size.Height);
        }

        public Size GetViewSize()
        {
            SikiOnGrid sog = GetByIndex(this.LineSize - 1, this.RowSize - 1);
            return new Size(sog.Location.X + sog.Size.Width, sog.Location.Y + sog.Size.Height);
        }

        //private void Parent_Resize(object sender, System.EventArgs e)
        //{
        protected override void OnResize (EventArgs e)
        {
            //memo ちらつく

            //SendMessage(parent.Handle, 0xb, false, IntPtr.Zero);
            //parent.Invalidate(true);
            //parent.DoubleBuffered = true;
            for (int i = 0; i < RowSize; i++)
            {
                adjustLineCell(i);
            }
            //parent.Invalidate(false);
            //parent.Update();
            //SendMessage(parent.Handle, 0xb, true, IntPtr.Zero);
            //parent.Invalidate();
            //parent.Refresh();
           
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Control ac = this.ActiveControl;
            SikiOnGrid sog = null;
            if (ac is SikiOnGrid)
            {
                sog = (SikiOnGrid)ac;
            }
            if (sog == null)
                throw new Exception("");

            if ((keyData & Keys.Alt) != 0)
            {
                    this.Parent.ContextMenu.Show(sog, new Point(0, sog.Size.Height));
                    return true;
            }
      

            SikiOnGrid nsog;
            switch (keyData) //memo なんか重複してる
            {
                case Keys.Tab: //右のセルへ(ループ) //memo ついでにここで新たな列を作るか?
                    nsog = GetByIndex(sog.Line+1 >= LineSize? 0 : sog.Line+1, sog.Row);
                    nsog.Exp.Focus(); //memo Expに触れていいのか?
                    return true;　//memo Resへの移動はなくていいのか?
                case Keys.Tab | Keys.Shift:
                    nsog = GetByIndex(sog.Line - 1 < 0 ? 0 : sog.Line - 1, sog.Row);
                    nsog.Exp.Focus();
                    return true;
                case Keys.Enter: //次の行の先頭へ　//memo ついでにここで新たな行を作るか?
                    nsog = GetByIndex(0, sog.Row + 1 >= this.RowSize ? 0 : sog.Row + 1);
                    if (nsog != null)
                        nsog.Exp.Focus();
                    return true;
                case Keys.Enter | Keys.Shift:
                    nsog = GetByIndex(0, sog.Row > 0 ? sog.Row - 1 : 0);
                    nsog.Exp.Focus();
                    return true;
                case Keys.Up | Keys.Control: //上のセルへ
                case Keys.NumPad8 | Keys.Control:
                    nsog = GetByIndex(sog.Line, sog.Row - 1);
                    if (nsog != null)
                        nsog.Exp.Focus();
                    return true;
                case Keys.Down | Keys.Control:  //下のセルへ
                case Keys.NumPad2 | Keys.Control:
                    nsog = GetByIndex(sog.Line, sog.Row + 1);
                    if (nsog != null)
                        nsog.Exp.Focus();
                    return true;
                case Keys.Left | Keys.Control:  //左のセルへ
                case Keys.NumPad4 | Keys.Control:
                    nsog = GetByIndex(sog.Line - 1, sog.Row);
                    if (nsog != null)
                        nsog.Exp.Focus();
                    return true;
                case Keys.Right | Keys.Control:  //右のセルへ
                case Keys.NumPad6 | Keys.Control:
                    nsog = GetByIndex(sog.Line + 1, sog.Row);
                    if (nsog != null)
                        nsog.Exp.Focus();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // lineSizeとrowSizeを操作しない。cellサイズの調整もしない。
        void createCell(int line, int row)
        {
            var a = new SikiOnGrid(line, row);
            a.SplitterMoving += new SplitterCancelEventHandler(OnSplitterMoving);
            //a.Exp.PreviewKeyDown += new PreviewKeyDownEventHandler(OnPreviewKeyDown);
            SikiOnGrid prevX = null;
            if (line > 0)
                prevX = GetByIndex(line - 1, row);
            SikiOnGrid prevY = null;
            if (row > 0)
                prevY = GetByIndex(line, row - 1);
            a.Location = new Point((prevX == null ? 0 : prevX.Location.X + prevX.Size.Width + PadX),
                (prevY == null ? 0 : prevY.Location.Y + prevY.Size.Height + PadY));

            this.Controls.Add(a);
        }

        public SikiOnGrid GetByIndex(int line, int row)
        {
            foreach (Control s in this.Controls)
            {
                if (s.Name == "SikiOnGrid")
                {
                    SikiOnGrid sog = (SikiOnGrid)s;
                    if (sog.Line == line && sog.Row == row)
                        return sog;
                }
            }
            return null;
        }

        public void adjustLineCell(int row)
        {
            List<Control> cs = new List<Control>();
            Control c;

            int i = 0;
            while ((c = GetByIndex(i, row)) != null)
            {
                cs.Add(c);
                i++;
            }

            int cellSize = (this.Size.Width  -((i - 1) * PadX)) / i;

            for (int j = 0; j < i; j++)
            {
                cs[j].Location = new Point(cellSize * j + PadX * j, cs[j].Location.Y);
                cs[j].Size = new Size(cellSize, cs[j].Size.Height);
            }

        }

        public void ExpandLine(int size)
        {
            for (int i = 0; i < RowSize; i++)
            {
                for (int j = LineSize; j < LineSize+size; j++)
                {
                    createCell(j, i);
                    adjustLineCell(i);
                }
            }
            _LineSize = LineSize + size;
        }

        public void ExpandRow(int size)
        {
            for (int i = RowSize; i < RowSize + size; i++)
            {
                for (int j = 0; j < LineSize; j++)
                    createCell(j, i);
                adjustLineCell(i);
            }
            _RowSize = RowSize + size;
        }

        public void ChopLine(int size)
        {
            if (LineSize - size <= 0)
                return;
            for (int i = 0; i < RowSize; i++)
            {
                for (int j = LineSize - 1; j >= LineSize - size; j--)
                    this.Controls.Remove(GetByIndex(j, i));
            }
            _LineSize -= size;

            for (int i = 0; i < RowSize; i++) //memo ResizeともChopRowともかぶる。
            {
                adjustLineCell(i);
            }
        }

        public void ChopRow(int size)
        {
            if (RowSize - size <= 0)
                return;
            for (int i = RowSize - 1; i >= RowSize - size; i--)
            {
                for (int j = 0; j < LineSize; j++)
                    this.Controls.Remove(GetByIndex(j, i));
            }
            _RowSize -= size;

            for (int i = 0; i < RowSize; i++)
            {
                adjustLineCell(i);
            }
        }

        public void OnSplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            if (sender is SikiOnGrid)
            {
                SikiOnGrid sog = (SikiOnGrid)sender;
                for (int row = 0; row < RowSize; row++)
                {
                    SikiOnGrid t = GetByIndex(sog.Line, row);
                    if (t != sender)
                    {
                        t.SplitterDistance = e.SplitX;
                    }
                }
            }
        }

        public void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                if (((Control)sender).Parent.Parent is SikiOnGrid)
                {
                    SikiOnGrid sog = (SikiOnGrid)((Control)sender).Parent.Parent; //memo いいのかこんなの？
                    SikiOnGrid next = GetByIndex(sog.Line + 1, sog.Row);
                    e.IsInputKey = true;
                    next.Focus();
                }
            }
        }


    }

    //memo このクラスいらないかも SikiGridを複数配置すること考えたり、高速化を考えたりしたら、SikiGridないでやる？
    //memo というか、TextBoxExやSikiOnGridっているのか？
    class SikiOnGrid : Siki 
    {
        int _line;
        public int Line {get {return _line;}}
        int _row;
        public int Row {get {return _row;}}

        public SikiOnGrid(int line, int row) :base()
        {
            //this.Name = String.Concat("SikiOnGrid@", line.ToString(), ",", row.ToString());
            this.Name = "SikiOnGrid";
            this._line = line;
            this._row = row;
            //SplitterMoving += new SplitterCancelEventHandler(OnSplitterMoving);
            //this.Exp.KeyDown += new KeyEventHandler(onKeyDown);
            //this.Exp.PreviewKeyDown += new PreviewKeyDownEventHandler(SikiOnGrid_PreviewKeyDown);
            this.Exp.GotFocus += new EventHandler(_GotFocus);
            this.Res.GotFocus += new EventHandler(_GotFocus);
            this.Exp.LostFocus += new EventHandler(_LostFocus);
            this.Res.LostFocus += new EventHandler(_LostFocus);

            this.Exp.BackColor = Color.Plum;
            this.Res.BackColor = Color.Plum;
        }

        private void _GotFocus(Object sender, EventArgs e)
        {
            this.Exp.BackColor = Color.LightSteelBlue;
            this.Res.BackColor = Color.LightSteelBlue;
        }

        private void _LostFocus(Object sender, EventArgs e)
        {
            this.Exp.BackColor = Color.Plum;
            this.Res.BackColor = Color.Plum;
        } 
    }
}
