using DPCE1.Executor;
using System;
using static DPCE1.Lexer;

namespace DPCE1;

class Program
{
    static void Main(string[] args)
    {
        var API = new APIService();
        var execute = new TokenBasedExecutor();

        // Start get input flow
        while (true)
        {
            string source = API.Cin();

            if (source.Trim() == ".exit" || source.Trim() == "kill" || source.Trim() == "die") break;

            execute.Run(source);
        }
        // Finish get input flow
    }
}