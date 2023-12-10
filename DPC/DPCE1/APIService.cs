using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPCE1;

internal class APIService
{
    public void Cout(string content = "")
    {
        Console.WriteLine(content);
    }
    public string Cin(string label = "", bool displayLabel = true)
    {
        string result = "";

        if (displayLabel) Console.Write($"{label}>> ");
        result = Console.ReadLine();

        return result;
    }
}