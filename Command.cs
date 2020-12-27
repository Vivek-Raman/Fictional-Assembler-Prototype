using System;
using System.Collections.Generic;

namespace Compiler
{
    class Command
    {
        public static List<string> commandList = new List<string>();

        public string command;
        public string description;
        public Func<List<string>, int> action;

        public Command(string command, string description, Func<List<string>, int> action)
        {
            this.command = command;
            this.description = description;
            this.action = action;

            commandList.Add(command);
        }
    }
}