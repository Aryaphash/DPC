using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DPCE1.Lexer;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DPCE1;

internal class Lexer
{
    private static List<Token> _Tokens = new List<Token>();
    private string _line = "";
    private int _position = 0;
    private char _ch
    {
        get
        {
            if (_position < _line.Length) return _line[_position];
            return '\0';
        }
    }
    private char _NextTokenShow(int index = 1)
    {
        if ((_position + index) < _line.Length) return _line[_position + index];
        return '\0';
    }
    private char _PreviousTokenShow(int index = 1)
    {
        if ((_position - index) > 0) return _line[_position - index];
        return '\0';
    }
    private Token lastToken()
    {
        if (_Tokens.Count == 0) return null;
        return _Tokens[_Tokens.Count - 1];
    }
    public bool ErrorCall = false;
    private void _Error_handler(string msg = "")
    {
        if (msg.Trim() != "")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        GetInputLine(""); // For reset data

        ErrorCall = true;
    }
    public class Token
    {
        public string Value;
        public TokenType Type;
        public int StartPos;
        public int EndPos;

        public Token(string value, TokenType type, int start, int end)
        {
            Value = value;
            Type = type;
            StartPos = start;
            EndPos = end;

            _Tokens.Add(this);
        }
    }
    public enum TokenType
    {
        KEYWORD,
        OPERATOR,
        FLAG,
        IDENTIFIER,
        STRING,
        INT,
        FLOAT,
        BOOLEAN,
        NULL
    }
    private static void _NewToken(string value, TokenType type, int start, int end)
    {
        new Token(value, type, start, end);
    }
    public void GetInputLine(string sosource)
    {
        /*
           I created this method for reusability of this class.
           Despite this method, there is no need to create a new
           object when receiving input, and when the program starts,
           only one object is created and its internal value is updated
           for each input.

           But this method has other benefits, such as keeping the information
           of the entire program in itself. When there is no need to make successive
           objects, we can store the required data in the same object.
         */

        _Tokens.Clear();
        _line = sosource;
        _position = 0;
        ErrorCall = false;
    }
    private void _ThrowInvalidError()
    {
        int startPos = _position;
        string invalidToken = "";

        while (_position < _line.Length)
        {
            if (_ch == ' ') break;

            invalidToken += _ch;

            _position++;
        }

        _position--;

        _Error_handler($"Syntax Error: Invalid token. <TOKEN : '{invalidToken}' | START POSITION : '{startPos}' | END POSITION : '{_position}'>");
    }
    private bool _IsSeparators(char tk)
    {
        switch (tk)
        {
            case '+':
                return true;
            case '-':
                return true;
            case '*':
                return true;
            case '/':
                return true;
            case '(':
                return true;
            case ')':
                return true;
            case '{':
                return true;
            case '}':
                return true;
            case ':':
                return true;
            case '.':
                return true;
            case '&':
                return true;
            case '|':
                return true;
            case '=':
                return true;
            case '!':
                return true;
            case '>':
                return true;
            case '<':
                return true;
            case ' ':
                return true;
            case '\0':
                return true;
            case '\n':
                return true;
            case ';':
                return true;
            default:
                return false;
        }

    }
    private bool _GetFlags()
    {
        bool result = false;

        if (_ch == '-' && _NextTokenShow(1) == '-' && Char.IsLetter(_NextTokenShow(2)))
        {
            result = true;

            _position += 2;

            string getFlagValue = "";
            int startPos = _position;

            while (_position < _line.Length)
            {
                if (_IsSeparators(_ch) && _ch != '-') break;

                getFlagValue += _ch;

                _position++;
            }

            _NewToken(getFlagValue, TokenType.FLAG, startPos, _position);
        }

        return result;
    }
    private bool _GetString()
    {
        bool result = false;

        if (_ch == '"')
        {
            result = true;

            int startPos = _position;
            string getStr = "";
            _position++;

            while (true)
            {
                if (_ch == '"')
                {
                    _position++;
                    break;
                }
                else if (_ch == '\0')
                {
                    result = false;
                    _Error_handler($"Syntax Error: You defined a string, but you forgot to close that. <TOKEN TYPE : 'STRING' | START POSITION : '{startPos}' | END POSITION : '{_position}'>");
                    break;
                }

                getStr += _ch;

                _position++;
            }

            if (result) _NewToken(getStr, TokenType.STRING, startPos, _position);
            else return true; // This returns True when there was an error getting the string. Despite this, after exiting this function (in the main process in the _build function), the process starts again, and this causes the end error of the main process not to call.
        }

        return result;
    }
    private bool _GetNumbers()
    {
        bool result = false;

        if (
            (_ch == '+' | _ch == '-') && Char.IsDigit(_NextTokenShow()) ||
            (Char.IsDigit(_ch))
            )
        {
            result = true;

            int startPos = _position;
            string getNum = "";
            var type = TokenType.INT;
            int floatCounter = 0;

            if (_ch == '+' || _ch == '-')
            {
                var lastToken = (_Tokens.Count == 0) ? null : _Tokens[_Tokens.Count - 1];
                if (
                    (lastToken == null) || (lastToken.Type == TokenType.OPERATOR && lastToken.Value != ")") ||
                    (lastToken == null) || (lastToken.Type == TokenType.KEYWORD) ||
                    (lastToken == null) || (lastToken.Type == TokenType.FLAG)
                    )
                {
                    getNum += _ch;
                    _position++;
                }
                else
                {
                    return false;
                }
            }

            while (_position < _line.Length)
            {
                if (_ch == '.')
                {
                    if (floatCounter >= 1)
                    {
                        _Error_handler($"Syntax Error: You defined floating-point number with more than one dot '.'. <TOKEN : '{getNum}' | TOKEN TYPE : 'FLOAT' | START POSITION : '{startPos}' | END POSITION : '{_position}'>");
                        result = false;
                        break;
                    }
                    else
                    {
                        type = TokenType.FLOAT;
                        floatCounter++;
                        getNum += _ch;
                        _position++;
                        continue;
                    }
                }

                if (!Char.IsDigit(_ch)) break;

                getNum += _ch;

                _position++;
            }

            if (result) _NewToken(getNum, type, startPos, _position);
            else return true;
        }

        return result;
    }
    private bool _GetOperators()
    {
        bool result = true;
        string tokenValue = "";
        int startPos = _position;
        int endPos = 0;

        switch (_ch)
        {
            case '+':
                tokenValue = "+";
                endPos = ++_position;
                break;
            case '-':
                tokenValue = "-";
                endPos = ++_position;
                break;
            case '*':
                tokenValue = "*";
                endPos = ++_position;
                break;
            case '/':
                tokenValue = "/";
                endPos = ++_position;
                break;
            case '(':
                tokenValue = "(";
                endPos = ++_position;
                break;
            case ')':
                tokenValue = ")";
                endPos = ++_position;
                break;
            case '{':
                tokenValue = "{";
                endPos = ++_position;
                break;
            case '}':
                tokenValue = "}";
                endPos = ++_position;
                break;
            case ':':
                tokenValue = ":";
                endPos = ++_position;
                break;
            case '.':
                tokenValue = ".";
                endPos = ++_position;
                break;
            case '&':
                tokenValue = "&";
                endPos = ++_position;
                break;
            case '|':
                tokenValue = "|";
                endPos = ++_position;
                break;
            case '=':
                if (_NextTokenShow() == '=') tokenValue = "==";
                else tokenValue = "=";
                _position += (tokenValue.Length);
                endPos = _position;
                break;
            case '!':
                if (_NextTokenShow() == '=') tokenValue = "!=";
                else break;
                endPos = _position += 2;
                break;
            case '>':
                tokenValue = ">";
                endPos = ++_position;
                break;
            case '<':
                tokenValue = "<";
                endPos = ++_position;
                break;
            default:
                result = false;
                break;
        }

        if (result) _NewToken(tokenValue, TokenType.OPERATOR, startPos, endPos);

        return result;
    }
    private bool _GetIdentifiers()
    {
        bool result = false;

        if (_ch == '$' && Char.IsLetter(_NextTokenShow()))
        {
            result = true;

            int startPos = _position;
            _position++;

            string getId = "";

            while (_position < _line.Length)
            {
                if (_IsSeparators(_ch)) break;

                getId += _ch;

                _position++;
            }

            _NewToken(getId, TokenType.IDENTIFIER, startPos, _position);
        }

        return result;
    }
    private bool _GetCallingMethod()
    {
        bool result = false;

        if (lastToken() != null && lastToken().Type == TokenType.OPERATOR && lastToken().Value == "." && Char.IsLetter(_ch))
        {
            result = true;

            int startPos = _position;
            string getId = "";

            while (_position < _line.Length)
            {
                if (_IsSeparators(_ch)) break;

                getId += _ch;

                _position++;
            }

            _NewToken(getId, TokenType.IDENTIFIER, startPos, _position);
        }

        return result;
    }
    private bool _GetKeyword()
    {
        string[] keywordList = new string[] { "API", "if", "for", "switch", "return", "true", "false", "null" };

        int startPos = _position;
        string continueLine = _line;
        continueLine = continueLine.Substring(_position);

        for (int i = 0; i < keywordList.Length; i++)
        {
            if (_IsSeparators(_PreviousTokenShow()) && continueLine.StartsWith(keywordList[i]) && _IsSeparators(_NextTokenShow(keywordList[i].Length)))
            {
                _position += keywordList[i].Length;
                if (keywordList[i] == "true" || keywordList[i] == "false") _NewToken(keywordList[i], TokenType.BOOLEAN, startPos, _position);
                else if (keywordList[i] == "null") _NewToken(keywordList[i], TokenType.NULL, startPos, _position);
                else _NewToken(keywordList[i], TokenType.KEYWORD, startPos, _position);
                return true;
            }
        }

        return false;
    }
    private void _build()
    {
        // Start process
        while (_position < _line.Length && (!ErrorCall))
        {
            if (_GetFlags()) continue;
            if (_GetString()) continue;
            if (_GetNumbers()) continue;
            if (_GetOperators()) continue;
            if (_GetIdentifiers()) continue;
            if (_GetCallingMethod()) continue;
            if (_GetKeyword()) continue;

            if (_ch == ' ')
            {
                _position++;
                continue;
            }

            _ThrowInvalidError(); // error handler
        }
        // Finish process
    }
    public List<Token> BuildTokenFlow()
    {
        _build();
        return _Tokens;
    }
}
