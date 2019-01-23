using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Windows.Forms;

using GoldParser;

namespace Parsing
{

    [Serializable()]
    public class SymbolException : System.Exception
    {
        public SymbolException(string message)
            : base(message)
        {
        }

        public SymbolException(string message,
            Exception inner)
            : base(message, inner)
        {
        }

        protected SymbolException(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

    }

    [Serializable()]
    public class RuleException : System.Exception
    {

        public RuleException(string message)
            : base(message)
        {
        }

        public RuleException(string message,
                             Exception inner)
            : base(message, inner)
        {
        }

        protected RuleException(SerializationInfo info,
                                StreamingContext context)
            : base(info, context)
        {
        }

    }

    // this class will construct a parser without having to process
    //  the CGT tables with each creation.  It must be initialized
    //  before you can call CreateParser()
    public sealed class ParserFactory
    {
        static Grammar m_grammar;
        static bool _init;

        private ParserFactory(ListView output)
        {
        }

        private static BinaryReader GetResourceReader(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            return new BinaryReader(stream);
        }

        public static void InitializeFactoryFromFile(string FullCGTFilePath)
        {
            if (!_init)
            {
                BinaryReader reader = new BinaryReader(new FileStream(FullCGTFilePath, FileMode.Open));
                m_grammar = new Grammar(reader);
                _init = true;
            }
        }

        public static void InitializeFactoryFromResource(string resourceName)
        {
            if (!_init)
            {
                BinaryReader reader = GetResourceReader(resourceName);
                m_grammar = new Grammar(reader);
                _init = true;
            }
        }

        public static Parser CreateParser(TextReader reader)
        {
            if (_init)
            {
                return new Parser(reader, m_grammar);
            }
            throw new Exception("You must first Initialize the Factory before creating a parser!");
        }
    }

    public partial class MyParser
    {
        MyParserContext m_context;
        string m_errorString;
        Parser m_parser;
        Result m_result;
        static ListView m_output;
        Dictionary<string, double> m_variables;
        Dictionary<string, string> m_functions;

        public MyParser(ListView output)
        {
            m_output = output;
            m_errorString = "";
            m_variables = new Dictionary<string, double>();
            m_functions = new Dictionary<string, string>();
        }

        public ListView Output
        {
            get
            {
                return m_output;
            }
        }       

        public int LineNumber
        {
            get
            {
                return m_parser.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return m_parser.LinePosition;
            }
        }

        public string ErrorString
        {
            get
            {
                return m_errorString;
            }
        }

        public string ErrorLine
        {
            get
            {
                return m_parser.LineText;
            }
        }

        public Result Result
        {
            get
            {
                return m_result;
            }
        }

        public bool Parse(string source)
        {
            return Parse(new StringReader(source));
        }

        public bool Parse(StringReader sourceReader)
        {
            m_parser = ParserFactory.CreateParser(sourceReader);
            m_parser.TrimReductions = true;
            m_context = new MyParserContext(m_parser, m_output, ref m_variables, ref m_functions);
            string error = "";

            while (true)
            {
                switch (m_parser.Parse())
                {
                    case ParseMessage.LexicalError:
                        m_errorString = string.Format("ERROR: Unexpected symbol: {0}", m_parser.TokenText);
                        return false;

                    case ParseMessage.SyntaxError:
                        StringBuilder text = new StringBuilder();
                        foreach (Symbol tokenSymbol in m_parser.GetExpectedTokens())
                        {
                            text.Append(' ');
                            text.Append(tokenSymbol.ToString());
                        }
                        m_errorString = string.Format("ERROR: Invalid syntax.");

                        return false;

                    case ParseMessage.Reduction:
                        m_parser.TokenSyntaxNode = m_context.ParseExpression(ref error);
                        if (error.Length > 0)
                        {
                            m_errorString = error;
                            return false;
                        }
                        break;

                    case ParseMessage.Accept:
                        m_result = m_parser.TokenSyntaxNode as Result;
                        m_errorString = null;
                        return true;

                    case ParseMessage.TokenRead:
                        m_parser.TokenSyntaxNode = m_context.GetTokenText(ref error);
                        if (error.Length > 0)
                        {
                            m_errorString = error;
                            return false;
                        }
                        break;

                    case ParseMessage.InternalError:
                        m_errorString = "INTERNAL ERROR: Something is horribly wrong.";
                        return false;

                    case ParseMessage.NotLoadedError:
                        m_errorString = "INTERNAL ERROR: Grammar Table is not loaded.";
                        return false;

                    case ParseMessage.CommentError:
                        m_errorString = "INTERNAL ERROR: Comment Error. Unexpected end of input.";

                        return false;

                    case ParseMessage.CommentBlockRead:
                    case ParseMessage.CommentLineRead:
                        // don't do anything 
                        break;
                }
            }
        }

    }

    public enum DisplayType : int
    {
        BINARY,
        DECIMAL,
        HEXADECIMAL,
        OCTAL
    };

    public partial class Result
    {
        private double m_number;
        private string m_expression;
        private DisplayType m_type;

        public Result(double number)
        {
            m_number = number;
            m_expression = Convert.ToString(number);
            m_type = DisplayType.DECIMAL;
        }
        public Result(double number, string expression)
        {
            m_number = number;
            m_expression = expression;
            m_type = DisplayType.DECIMAL;
        }
        public Result(double number, string expression, DisplayType type)
        {
            m_number = number;
            m_expression = expression;
            m_type = type;
        }
        public double Value
        {
            get
            {
                return m_number;
            }
        }
        public string StringValue
        {
            get
            {
                if (m_type == DisplayType.BINARY)
                {
                    return String.Format("0b{0}", Convert.ToString((int)m_number, 2));
                }
                else if (m_type == DisplayType.HEXADECIMAL)
                {
                    return String.Format("0x{0}", Convert.ToString((int)m_number, 16));
                }
                else if (m_type == DisplayType.OCTAL)
                {
                    return String.Format("0o{0}", Convert.ToString((int)m_number, 8));
                }
                else
                {
                    return Convert.ToString(m_number);
                }
            }
        }
        public string Expression
        {
            get
            {
                return m_expression;
            }
        }
    }

    public partial class MyParserContext
    {

        // instance fields
        private Parser m_parser;
        private ListView m_output;
        private Dictionary<string, double> m_variables;
        private Dictionary<string, string> m_functions;

        //private TextReader m_inputReader;	

        // constructor
        public MyParserContext(Parser prser, ListView output, ref Dictionary<string, double> variables, ref Dictionary<string, string> functions)
        {
            m_parser = prser;
            m_output = output;
            m_variables = variables;
            m_functions = functions;
        }


        public string GetTokenText(ref string error)
        {
            // delete any of these that are non-terminals.

            switch (m_parser.TokenSymbol.Index)
            {
                case (int)SymbolConstants.SYMBOL_INTEGER:
                case (int)SymbolConstants.SYMBOL_FLOAT:
                case (int)SymbolConstants.SYMBOL_BINARY:
                case (int)SymbolConstants.SYMBOL_HEXADECIMAL:
                case (int)SymbolConstants.SYMBOL_OCTAL:
                case (int)SymbolConstants.SYMBOL_IDENTIFIER:
                    return m_parser.TokenString;
                case (int)SymbolConstants.SYMBOL_FLOATNO0:
                    return "0" + m_parser.TokenString;
                case (int)SymbolConstants.SYMBOL_VARIABLE:
                    // Pull the index from the dollar sign and convert it to the listview index
                    int index = Convert.ToInt32(m_parser.TokenString.Substring(m_parser.TokenString.IndexOf('$') + 1));
                    index = (index * 2) - 1;
                    if (index > m_output.Items.Count - 1)
                    {
                        error = "ERROR: Result index out of range.";
                        return "NaN";
                    }
                    string answer = m_output.Items[index].Text;
                    if (answer.Contains("ERROR"))
                    {
                        error = "ERROR: Invalid Result.";
                        return "NaN";
                    }
                    // Remove the unnessary stuff from the number
                    answer = answer.Substring(answer.IndexOf("= ") + 1);
                    return answer;
                default:
                    return null;
            }

        }

        public Result ParseExpression(ref string error)
        {
            switch (m_parser.ReductionRule.Index)
            {
                case (int)RuleConstants.RULE_ASSIGN_SET_IDENTIFIER_EQ:
                    string id = Str(1);
                    if (m_variables.ContainsKey(id))
                    {
                        m_variables[id] = Num(3);
                    }
                    else
                    {
                        m_variables.Add(id, Num(3));
                    }
                    return new Result(Num(3), String.Format("{0} = {1}", id, Expr(3)));
                case (int)RuleConstants.RULE_ASSIGN_SET_IDENTIFIER_LPARAN_RPARAN_EQ:
                    // I made a sporting attempt at getting custom functions working, but
                    // the reduction logic cause me all kinds of problems
                    error = String.Format("ERROR: Custom functions aren't supported.");
                    return new Result(Convert.ToDouble("NaN"), Str(0));
                    //id = Str(1);
                    //if (m_functions.ContainsKey(id))
                    //{
                    //    m_functions[id] = Str(3);
                    //}
                    //else
                    //{
                    //    m_functions.Add(id, Str(3));
                    //}
                    //return new Result(Num(3), String.Format("{0} = {1}", id, Expr(3)));
                case (int)RuleConstants.RULE_NUMBER_FLOAT:
                case (int)RuleConstants.RULE_NUMBER_INTEGER:
                case (int)RuleConstants.RULE_NUMBER_FLOATNO0:
                case (int)RuleConstants.RULE_NUMBER_VARIABLE:
                    string number = Str(0);
                    try
                    {
                        return new Result(Convert.ToDouble(number));
                    }
                    catch (FormatException)
                    {
                        error = String.Format("ERROR: \"{0}\" is a string value.", number.Trim());
                        return new Result(Convert.ToDouble("NaN"), Str(0));
                    }
                case (int)RuleConstants.RULE_NUMBER_BINARY:
                    string expr = Str(0);
                    number = expr.Substring(expr.IndexOf('b') + 1);
                    return new Result((double)Convert.ToInt32(number, 2), expr);
                case (int)RuleConstants.RULE_NUMBER_HEXADECIMAL:
                    expr = Str(0);
                    number = expr.Substring(expr.IndexOf('x') + 1);
                    return new Result((double)Convert.ToInt32(number, 16), expr);
                case (int)RuleConstants.RULE_NUMBER_OCTAL:
                    expr = Str(0);
                    number = expr.Substring(expr.IndexOf('o') + 1);
                    return new Result((double)Convert.ToInt32(number, 8), expr);
                case (int)RuleConstants.RULE_VALUE_BIN_LPARAN_RPARAN:
                    return new Result(Num(2), String.Format("bin({0})", Expr(2)), DisplayType.BINARY);
                case (int)RuleConstants.RULE_VALUE_HEX_LPARAN_RPARAN:
                    return new Result(Num(2), String.Format("hex({0})", Expr(2)), DisplayType.HEXADECIMAL);
                case (int)RuleConstants.RULE_VALUE_OCT_LPARAN_RPARAN:
                    return new Result(Num(2), String.Format("oct({0})", Expr(2)), DisplayType.OCTAL);
                case (int)RuleConstants.RULE_NUMBER_PI:
                    return new Result(Math.PI);
                case (int)RuleConstants.RULE_NUMBER_EXP:
                    return new Result(Math.E);
                case (int)RuleConstants.RULE_NUMBER_IDENTIFIER:
                    if (Str(0).Equals("ans"))
                    {
                        string ans = m_output.Items[m_output.Items.Count - 1].Text;
                        if (ans.Contains("ERROR"))
                        {
                            error = "ERROR: Invalid Result.";
                            return new Result(Convert.ToDouble("NaN"), Str(0));
                        }
                        // Remove the unnessary stuff from the number
                        ans = ans.Substring(ans.IndexOf("= ") + 2);
                        return new Result(Convert.ToDouble(ans), Str(0));
                    }
                    else if (m_variables.ContainsKey(Str(0)))
                    {
                        return new Result(m_variables[Str(0)], Str(0));
                    }
                    else
                    {
                        error = String.Format("ERROR: \"{0}\" has not been set.", Str(0));
                        return new Result(Convert.ToDouble("NaN"), Str(0));
                    }
                case (int)RuleConstants.RULE_VARIABLELIST_SEMI_IDENTIFIER:
                    return new Result(0);
                case (int)RuleConstants.RULE_VARIABLELIST_IDENTIFIER:
                    id = Str(0);
                    if (!m_variables.ContainsKey(id))
                    {
                        m_variables.Add(id, 0);
                    }
                    return new Result(0);
                case (int)RuleConstants.RULE_VALUE_IDENTIFIER_LPARAN_RPARAN:
                    error = String.Format("ERROR: Custom functions aren't supported.");
                    return new Result(Convert.ToDouble("NaN"), Str(0));
                case (int)RuleConstants.RULE_EXPRESSION:
                case (int)RuleConstants.RULE_MULTEXPR:
                case (int)RuleConstants.RULE_NEGATEEXPR:
                case (int)RuleConstants.RULE_EXPON:
                case (int)RuleConstants.RULE_VALUE:
                case (int)RuleConstants.RULE_ASSIGN:
                    return new Result(Num(0));
                case (int)RuleConstants.RULE_EXPRESSION_PLUS:
                    return new Result(Num(0) + Num(2), String.Format("{0} + {1}", Expr(0), Expr(2)));
                case (int)RuleConstants.RULE_EXPRESSION_MINUS:
                    return new Result(Num(0) - Num(2), String.Format("{0} - {1}", Expr(0), Expr(2)));
                case (int)RuleConstants.RULE_MULTEXPR_TIMES:
                    return new Result(Num(0) * Num(2), String.Format("{0} * {1}", Expr(0), Expr(2)));
                case (int)RuleConstants.RULE_MULTEXPR_DIV:
                    return new Result(Num(0) / Num(2), String.Format("{0} / {1}", Expr(0), Expr(2)));
                case (int)RuleConstants.RULE_EXPON_CARET:
                    return new Result(Math.Pow(Num(0), Num(2)), String.Format("{0}^{1}", Expr(0), Expr(2)));
                case (int)RuleConstants.RULE_VALUE_LPARAN_RPARAN:
                    return new Result(Num(1), "(" + Expr(1) + ")");
                case (int)RuleConstants.RULE_NEGATEEXPR_MINUS:
                    return new Result(-Num(1), "-" + Expr(1));
                case (int)RuleConstants.RULE_VALUE_ABS_LPARAN_RPARAN:
                    return new Result(Math.Abs(Num(2)), String.Format("abs({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_ACOS_LPARAN_RPARAN:
                    return new Result(Math.Acos(Num(2)), String.Format("acos({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_ASIN_LPARAN_RPARAN:
                    return new Result(Math.Asin(Num(2)), String.Format("asin({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_ATAN_LPARAN_RPARAN:
                    return new Result(Math.Atan(Num(2)), String.Format("atan({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_COS_LPARAN_RPARAN:
                    return new Result(Math.Cos(Num(2)), String.Format("cos({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_COSH_LPARAN_RPARAN:
                    return new Result(Math.Cosh(Num(2)), String.Format("cosh({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_LOG_LPARAN_RPARAN:
                    return new Result(Math.Log(Num(2)), String.Format("log({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_LOG10_LPARAN_RPARAN:
                    return new Result(Math.Log10(Num(2)), String.Format("log10({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_SIN_LPARAN_RPARAN:
                    return new Result(Math.Sin(Num(2)), String.Format("sin({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_SINH_LPARAN_RPARAN:
                    return new Result(Math.Sinh(Num(2)), String.Format("sinh({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_SQRT_LPARAN_RPARAN:
                    return new Result(Math.Sqrt(Num(2)), String.Format("sqrt({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_TAN_LPARAN_RPARAN:
                    return new Result(Math.Tan(Num(2)), String.Format("tan({0})", Expr(2)));
                case (int)RuleConstants.RULE_VALUE_TANH_LPARAN_RPARAN:
                    return new Result(Math.Tanh(Num(2)), String.Format("tanh({0})", Expr(2)));
                default:
                    throw new RuleException("Unknown rule: Does your CGT Match your Code Revision?");
            }

        }

        private double Num(int index)
        {
            return ((Result)m_parser.GetReductionSyntaxNode(index)).Value;
        }
        private string Expr(int index)
        {
            return ((Result)m_parser.GetReductionSyntaxNode(index)).Expression;
        }
        private string Str(int index)
        {
            return (string)m_parser.GetReductionSyntaxNode(index);
        }
    }

    enum SymbolConstants : int
    {
        SYMBOL_EOF = 0, // (EOF)
        SYMBOL_ERROR = 1, // (Error)
        SYMBOL_WHITESPACE = 2, // Whitespace
        SYMBOL_MINUS = 3, // '-'
        SYMBOL_LPARAN = 4, // '('
        SYMBOL_RPARAN = 5, // ')'
        SYMBOL_TIMES = 6, // '*'
        SYMBOL_DIV = 7, // '/'
        SYMBOL_SEMI = 8, // ';'
        SYMBOL_CARET = 9, // '^'
        SYMBOL_PLUS = 10, // '+'
        SYMBOL_EQ = 11, // '='
        SYMBOL_ABS = 12, // abs
        SYMBOL_ACOS = 13, // acos
        SYMBOL_ASIN = 14, // asin
        SYMBOL_ATAN = 15, // atan
        SYMBOL_BIN = 16, // bin
        SYMBOL_BINARY = 17, // Binary
        SYMBOL_COS = 18, // cos
        SYMBOL_COSH = 19, // cosh
        SYMBOL_EXP = 20, // Exp
        SYMBOL_FLOAT = 21, // Float
        SYMBOL_FLOATNO0 = 22, // 'FloatNo0'
        SYMBOL_HEX = 23, // hex
        SYMBOL_HEXADECIMAL = 24, // Hexadecimal
        SYMBOL_IDENTIFIER = 25, // Identifier
        SYMBOL_INTEGER = 26, // Integer
        SYMBOL_LOG = 27, // log
        SYMBOL_LOG10 = 28, // 'log10'
        SYMBOL_OCT = 29, // oct
        SYMBOL_OCTAL = 30, // Octal
        SYMBOL_PI = 31, // Pi
        SYMBOL_SET = 32, // set
        SYMBOL_SIN = 33, // sin
        SYMBOL_SINH = 34, // sinh
        SYMBOL_SQRT = 35, // sqrt
        SYMBOL_TAN = 36, // tan
        SYMBOL_TANH = 37, // tanh
        SYMBOL_VARIABLE = 38, // Variable
        SYMBOL_ASSIGN = 39, // <Assign>
        SYMBOL_EXPON = 40, // <Expon>
        SYMBOL_EXPRESSION = 41, // <Expression>
        SYMBOL_EXPRESSIONLIST = 42, // <Expression List>
        SYMBOL_MULTEXPR = 43, // <Mult Expr>
        SYMBOL_NEGATEEXPR = 44, // <Negate Expr>
        SYMBOL_NUMBER = 45, // <Number>
        SYMBOL_VALUE = 46, // <Value>
        SYMBOL_VARIABLELIST = 47  // <Variable List>
    };

    enum RuleConstants : int
    {
        RULE_ASSIGN_SET_IDENTIFIER_EQ = 0, // <Assign> ::= set Identifier '=' <Expression>
        RULE_ASSIGN_SET_IDENTIFIER_LPARAN_RPARAN_EQ = 1, // <Assign> ::= set Identifier '(' <Variable List> ')' '=' <Expression>
        RULE_ASSIGN = 2, // <Assign> ::= <Expression>
        RULE_VARIABLELIST_SEMI_IDENTIFIER = 3, // <Variable List> ::= <Variable List> ';' Identifier
        RULE_VARIABLELIST_IDENTIFIER = 4, // <Variable List> ::= Identifier
        RULE_EXPRESSION_PLUS = 5, // <Expression> ::= <Expression> '+' <Mult Expr>
        RULE_EXPRESSION_MINUS = 6, // <Expression> ::= <Expression> '-' <Mult Expr>
        RULE_EXPRESSION = 7, // <Expression> ::= <Mult Expr>
        RULE_MULTEXPR_TIMES = 8, // <Mult Expr> ::= <Mult Expr> '*' <Negate Expr>
        RULE_MULTEXPR_DIV = 9, // <Mult Expr> ::= <Mult Expr> '/' <Negate Expr>
        RULE_MULTEXPR = 10, // <Mult Expr> ::= <Negate Expr>
        RULE_NEGATEEXPR_MINUS = 11, // <Negate Expr> ::= '-' <Expon>
        RULE_NEGATEEXPR = 12, // <Negate Expr> ::= <Expon>
        RULE_EXPON_CARET = 13, // <Expon> ::= <Value> '^' <Value>
        RULE_EXPON = 14, // <Expon> ::= <Value>
        RULE_VALUE = 15, // <Value> ::= <Number>
        RULE_VALUE_LPARAN_RPARAN = 16, // <Value> ::= '(' <Expression> ')'
        RULE_VALUE_ABS_LPARAN_RPARAN = 17, // <Value> ::= abs '(' <Expression> ')'
        RULE_VALUE_ACOS_LPARAN_RPARAN = 18, // <Value> ::= acos '(' <Expression> ')'
        RULE_VALUE_ASIN_LPARAN_RPARAN = 19, // <Value> ::= asin '(' <Expression> ')'
        RULE_VALUE_ATAN_LPARAN_RPARAN = 20, // <Value> ::= atan '(' <Expression> ')'
        RULE_VALUE_COS_LPARAN_RPARAN = 21, // <Value> ::= cos '(' <Expression> ')'
        RULE_VALUE_COSH_LPARAN_RPARAN = 22, // <Value> ::= cosh '(' <Expression> ')'
        RULE_VALUE_LOG_LPARAN_RPARAN = 23, // <Value> ::= log '(' <Expression> ')'
        RULE_VALUE_LOG10_LPARAN_RPARAN = 24, // <Value> ::= 'log10' '(' <Expression> ')'
        RULE_VALUE_SIN_LPARAN_RPARAN = 25, // <Value> ::= sin '(' <Expression> ')'
        RULE_VALUE_SINH_LPARAN_RPARAN = 26, // <Value> ::= sinh '(' <Expression> ')'
        RULE_VALUE_SQRT_LPARAN_RPARAN = 27, // <Value> ::= sqrt '(' <Expression> ')'
        RULE_VALUE_TAN_LPARAN_RPARAN = 28, // <Value> ::= tan '(' <Expression> ')'
        RULE_VALUE_TANH_LPARAN_RPARAN = 29, // <Value> ::= tanh '(' <Expression> ')'
        RULE_VALUE_BIN_LPARAN_RPARAN = 30, // <Value> ::= bin '(' <Expression> ')'
        RULE_VALUE_HEX_LPARAN_RPARAN = 31, // <Value> ::= hex '(' <Expression> ')'
        RULE_VALUE_OCT_LPARAN_RPARAN = 32, // <Value> ::= oct '(' <Expression> ')'
        RULE_VALUE_IDENTIFIER_LPARAN_RPARAN = 33, // <Value> ::= Identifier '(' <Expression List> ')'
        RULE_EXPRESSIONLIST_SEMI = 34, // <Expression List> ::= <Expression List> ';' <Expression>
        RULE_EXPRESSIONLIST = 35, // <Expression List> ::= <Expression>
        RULE_NUMBER_INTEGER = 36, // <Number> ::= Integer
        RULE_NUMBER_FLOAT = 37, // <Number> ::= Float
        RULE_NUMBER_FLOATNO0 = 38, // <Number> ::= 'FloatNo0'
        RULE_NUMBER_PI = 39, // <Number> ::= Pi
        RULE_NUMBER_EXP = 40, // <Number> ::= Exp
        RULE_NUMBER_VARIABLE = 41, // <Number> ::= Variable
        RULE_NUMBER_BINARY = 42, // <Number> ::= Binary
        RULE_NUMBER_HEXADECIMAL = 43, // <Number> ::= Hexadecimal
        RULE_NUMBER_OCTAL = 44, // <Number> ::= Octal
        RULE_NUMBER_IDENTIFIER = 45  // <Number> ::= Identifier
    };

}
