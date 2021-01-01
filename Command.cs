using System;
using System.Collections.Generic;

namespace Compiler
{
    class Command
    {
        public static List<string> commandList = new List<string>();

        public string name;
        public string description;
        public Func<List<string>, int> action;

        public Command(string name, string description, Func<List<string>, int> action)
        {
            this.name = name;
            this.description = description;
            this.action = action;

            commandList.Add(name);
        }
    }
}