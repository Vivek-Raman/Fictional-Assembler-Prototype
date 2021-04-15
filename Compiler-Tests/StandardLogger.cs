using System;
using AssemblerLibrary.Utils;

namespace Compiler_Tests
{
    public class StandardLogger : Logger
    {
        public override string Log(string message)
        {
            Console.WriteLine(message);
            return message;
        }
    }
}