using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Compiler
{
    public class Compiler
    {
        private List<Command> commands = new List<Command>();
        private Data data = new Data();
        private string accumulator;
        private bool hasBegun = false;    // TODO: enforce begin/end check

        public Compiler(string accumulator = "")
        {
            this.accumulator = accumulator;
            CreateCommands();
        }

        public void Compile(List<List<string>> input)
        {
            for (var lineCount = 0; lineCount < input.Count; lineCount++)
            {
                List<string> line = input[lineCount];
                // ShowParseInfo(line);

                if (line[0].StartsWith('#') || line[0] == "")
                {
                    // # indicates a comment, \n is newline
                    // Console.WriteLine("comment!");
                    continue;
                }
                else if (IsCommand(line[0], out int commandIndex))
                {
                    ProcessCommand(commandIndex, line);
                }
                else
                {
                    Console.WriteLine($"-----\nUNIDENTIFIED TOKEN ON LINE {lineCount+1}: \"{line[0]}\"\n-----\n");
                }
            }
        }

        private void CreateCommands()
        {
            commands.Add(new Command(
                "begin", "Denotes the start of the module.",
                line =>
                {
                    hasBegun = true;
                    return 0;
                }));

            commands.Add(new Command(
                "end", "Denotes the end of the module.",
                line =>
                {
                    hasBegun = false;
                    return 0;
                }));

            commands.Add(new Command(
                "def", "Defines an alias to a memory address.",
                line =>
                {
                    string varName = line[1];
                    string next = line[2];
                    for (int i = 3; i < line.Count; ++i)
                    {
                        next += line[i];
                    }

                    try
                    {
                        var varIndex = ProcessAddress(next);
                        data.AddNewVariable(varIndex, varName);
                        return 0;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("pepehands");
                        Console.WriteLine(e);
                        return 1;
                    }
                }));

            commands.Add(new Command(
                "ld", "Loads a value into accumulator.",
                line =>
                {
                    string expression = line[1];
                    if (data.GetVariableValue(expression, out string value))
                    {
                        accumulator = value;
                    }
                    else
                    {
                        accumulator = expression;
                    }

                    return 0;
                }));

            commands.Add(new Command(
                "sav", "Stores accumulated data into a memory address.",
                line =>
                {
                    string varName = line[1];
                    data.SetVariableValue(varName, accumulator);

                    return 0;
                }));

            // commands.Add(new Command(
            //     "", "",
            //     line =>
            //     {
            //         return 0;
            //     }));

            // TODO: turn this into a proc
            // commands.Add(new Command(
            //     "lsdefs", "",
            //     line =>
            //     {
            //
            //         return 0;
            //     }));

            // commands.Add(new Command(
            //     "", "",
            //     line =>
            //     {
            //         return 0;
            //     }));
        }

        // takes (&vars+20) and returns 20
        private static int ProcessAddress(string token)
        {
            // TODO: swap "vars" for other base addresses, add enum for other bases
            Regex regex = new Regex(@"(.*)(&vars\+)(\d+)(.*)");
            GroupCollection gc = regex.Match(token).Groups;
            int value = Convert.ToInt32(
                gc[3].Value);
            return value;
        }

        private bool IsCommand(string token, out int index)
        {
            index = Command.commandList.IndexOf(token);
            return index >= 0;
        }

        private void ProcessCommand(int commandIndex, List<string> line)
        {
            commands[commandIndex].action.Invoke(line);
        }

        #region Debugging Utilities

        // private void ShowParseInfo(string token, List<string> line)
        // {
        //     Console.WriteLine("----------");
        //     Console.Write($"Token: \"{token}\" in line: ");
        //     for (int i = 0; i < line.Count; i++)
        //     {
        //         Console.Write($"{line[i]} ");
        //     }
        //     Console.WriteLine();
        //     Console.WriteLine("----------");
        //     Console.WriteLine();
        // }

        private void ShowParseInfo(List<string> line)
        {
            // return;

            Console.WriteLine("----------");
            Console.Write($"Line: ");
            for (int i = 0; i < line.Count; i++)
            {
                Console.Write($"\"{line[i]}\" ");
            }
            Console.WriteLine($"ACC = {accumulator}");
            // Console.WriteLine("----------");
            Console.WriteLine();
        }

        #endregion
    }
}