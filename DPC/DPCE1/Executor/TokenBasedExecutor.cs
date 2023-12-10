using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPCE1.Executor;

internal class TokenBasedExecutor
{
    private APIService _APIService = new APIService();
    private Lexer lexer = new Lexer();
    private List<Lexer.Token> tokens = new List<Lexer.Token>();
    private int position = 0;
    private Lexer.Token token
    {
        get
        {
            if (position < tokens.Count) return tokens[position];
            return null;
        }
    }
    private bool errorCall = false;
    private void ErrorHandler(string msg = "")
    {
        if (msg.Trim() != "")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        position = 0;
        tokens.Clear();

        errorCall = true;
    }
    private Lexer.Token MoveTokenShow(int index = 0)
    {
        if ((position + index) < tokens.Count && (position + index) >= 0) return tokens[(position + index)];
        return null;
    }
    private bool MatchType(Lexer.Token token, Lexer.TokenType type)
    {
        if (token != null && token.Type == type) return true;
        return false;
    }
    private bool MatchValue(Lexer.Token token, string value)
    {
        if (token != null && token.Value == value) return true;
        return false;
    }
    private bool MatchBoth(Lexer.Token token, Lexer.TokenType type, string value)
    {
        if (token != null && token.Type == type && token.Value == value) return true;
        return false;
    }
    private bool IsDataType(Lexer.Token token)
    {
        if (MatchType(token, Lexer.TokenType.INT) || MatchType(token, Lexer.TokenType.FLOAT) || MatchType(token, Lexer.TokenType.STRING) || MatchType(token, Lexer.TokenType.BOOLEAN) || MatchType(token, Lexer.TokenType.NULL)) return true;
        else return false;
    }
    private class APIArgs
    {
        public string type = "";
        public string value = "";
        public APIArgs(string type, string value)
        {
            this.type = type;
            this.value = value;
        }
    }
    private string APICall(string call, List<APIArgs> args)
    {
        string result = "";

        int argPos = 0;
        APIArgs arg;
        if (argPos < args.Count) arg = args[argPos];
        else arg = null;

        if (call == "Cout") // API.Cout --output-value "Hello"
        {
            if (argPos < args.Count && arg != null && arg.type == "FLAG" && arg.value == "output-value")
            {
                argPos++;
                if (argPos < args.Count) arg = args[argPos];
                else arg = null;

                if (argPos < args.Count && arg != null)
                {
                    _APIService.Cout(arg.value);
                }
            }
        }
        else if (call == "Cin") // API.Cin --label "Enter" --display-label true
        {
            string label = "";
            bool displayLabelShow = true;

            do
            {
                if (argPos < args.Count && arg != null && arg.type == "FLAG" && arg.value == "label")
                {
                    argPos++;
                    if (argPos < args.Count) arg = args[argPos];
                    else arg = null;

                    if (argPos < args.Count && arg != null && arg.type == "STRING")
                    {
                        label = arg.value;

                        argPos++;
                        if (argPos < args.Count) arg = args[argPos];
                        else arg = null;

                        continue;
                    }
                    else
                    {
                        ErrorHandler($"Unexpected argument: API.Cin 'label' must give a string type. <TOKEN : '{token.Value}' | TOKEN TYPE : '{token.Type.ToString()}' | TOKEN START : '{token.StartPos}' | TOKEN END : '{token.EndPos}'>");
                    }
                }
                else if (argPos < args.Count && arg != null && arg.type == "FLAG" && arg.value == "display-label")
                {
                    argPos++;
                    if (argPos < args.Count) arg = args[argPos];
                    else arg = null;

                    if (argPos < args.Count && arg != null && arg.type == "BOOLEAN")
                    {
                        displayLabelShow = (arg.value == "true") ? true : false;

                        argPos++;
                        if (argPos < args.Count) arg = args[argPos];
                        else arg = null;

                        continue;
                    }
                    else
                    {
                        ErrorHandler($"Unexpected argument: API.Cin 'display-label' must give a boolean type. <TOKEN : '{token.Value}' | TOKEN TYPE : '{token.Type.ToString()}' | TOKEN START : '{token.StartPos}' | TOKEN END : '{token.EndPos}'>");
                    }
                }

                argPos++;
                if (argPos < args.Count) arg = args[argPos];
                else arg = null;
            } while (argPos < args.Count);

            result = _APIService.Cin(label, displayLabelShow);
        }

        return result;
    }
    private bool APIExecute()
    {
        bool result = false;

        do
        {
            if (MatchBoth(token, Lexer.TokenType.KEYWORD, "API"))
            {
                if (MatchBoth(MoveTokenShow(1), Lexer.TokenType.OPERATOR, "."))
                {
                    result = true;

                    position += 2;

                    if (MatchType(token, Lexer.TokenType.IDENTIFIER))
                    {
                        var getArgs = new List<APIArgs>();
                        string callValue = token.Value;

                        position++;

                        do
                        {
                            if (MatchType(token, Lexer.TokenType.FLAG))
                            {
                                if (IsDataType(MoveTokenShow(1)))
                                {
                                    getArgs.Add(new APIArgs("FLAG", token.Value));
                                    position++;
                                    getArgs.Add(new APIArgs(token.Type.ToString(), token.Value));
                                }
                                else
                                {
                                    getArgs.Add(new APIArgs("FLAG", token.Value));
                                    position++;
                                }
                            }

                            position++;
                        } while (position < tokens.Count);

                        APICall(callValue, getArgs);

                        break;
                    }
                    else
                    {
                        if (token == null) ErrorHandler($"Unexpected or invalid token. <EMPTY>");
                        else ErrorHandler($"Unexpected or invalid token. <TOKEN : '{token.Value}' | TOKEN TYPE : '{token.Type.ToString()}' | TOKEN START : '{token.StartPos}' | TOKEN END : '{token.EndPos}'>");
                        break;
                    }
                }
                else break;
            }
            else break;
        } while (position < tokens.Count);

        return result;
    }
    private void _Run()
    {
        while (position < tokens.Count && (!errorCall))
        {
            if (APIExecute()) continue;

            if (token == null) ErrorHandler($"Unexpected or invalid token. <EMPTY>");
            else ErrorHandler($"Unexpected of invalid token. <TOKEN : '{token.Value}' | TOKEN TYPE : '{token.Type.ToString()}' | TOKEN START : '{token.StartPos}' | TOKEN END : '{token.EndPos}'>");
        }
    }
    public void Run(string source)
    {
        position = 0;
        tokens.Clear();
        errorCall = false;
        lexer.GetInputLine(source);
        tokens = lexer.BuildTokenFlow();
        _Run();
    }
}
