using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Compiler
{
public class Compiler
{
    #region Flag Management

    private const int FLAG_COUNT = 1;
    private const int INTERNAL_FLAG_COUNT = 2;
    private enum Flags
    { NULL = -1, StringMode }
    private enum InternalFlags
    { NULL = -1, Begun, InProcedureDefinition }
    private bool[] flagSet = new bool[FLAG_COUNT];
    private bool[] internalFlagSet = new bool[INTERNAL_FLAG_COUNT];
    
    #endregion
    
    private List<Command> commands = new List<Command>();

    private Data data = new Data();
    private Dictionary<string, int> procedures = new Dictionary<string, int>();
    private Stack<int> pcProcedureStack = new Stack<int>();
    private int programCounter;
    private string accumulator;

    #region Convenience Conditions

    private bool F_IsInProcedureDefinition
    {
        get => internalFlagSet[(int) InternalFlags.InProcedureDefinition];
        set => internalFlagSet[(int) InternalFlags.InProcedureDefinition] = value;
    }

    private bool F_HasBegun
    {
        get => internalFlagSet[(int) InternalFlags.Begun];
        set => internalFlagSet[(int) InternalFlags.Begun] = value;
    }

    #endregion

    public Compiler(string accumulator = "")
    {
        this.accumulator = accumulator;
        CreateCommands();
    }

    public void Compile(List<List<string>> input)
    {
        for (programCounter = 0; programCounter < input.Count; ++programCounter)
        {
            List<string> line = input[programCounter];

            if (line[0].StartsWith('#') || line[0] == "")
            {
                // # indicates a comment, and "" is empty line
                // these are ignored
                // Console.WriteLine("comment!");
                continue;
            }

            if (!F_HasBegun)
            {
                if (line[0].StartsWith("procdef"))
                {
                    // Console.WriteLine($"detected procedure {line[1]}");
                    F_IsInProcedureDefinition = true; // elaborate here to avoid local functions
                    procedures.Add(line[1], programCounter + 1);
                    continue;
                }
                if (F_IsInProcedureDefinition)
                {
                    if (line[0] == "endproc")
                        F_IsInProcedureDefinition = false;
                    continue;
                }


            }

            if (IsCommand(line[0], out int commandIndex))
            {
                if (!F_HasBegun)
                {
                    if (commandIndex != 0)
                        throw new Exception("no begin encountered"); // TODO: formalize errors
                }

                ProcessCommand(commandIndex, line);
            }
            else
            {
                Console.WriteLine($"-----\nUNIDENTIFIED TOKEN ON LINE {programCounter + 1}: \"{line[0]}\"\n-----\n");
            }
        }
    }

    private string ParseArguments(string token)
    {
        if (!flagSet[(int) Flags.StringMode] && data.TryGetVariableValue(token, out string value))
        {
            return value;
        }

        return token;
    }

    #region Commands

    private void CreateCommands()
    {
        commands.Add(new Command(
            "begin", "Denotes the start of the module.",
            line =>
            {
                F_HasBegun = true;
                return 0;
            }));

        commands.Add(new Command(
            "end", "Denotes the end of the module.",
            line =>
            {
                F_HasBegun = false;
                return 0;
            }));

        commands.Add(new Command(
            "dump", "Provides a dump of the current state of memory.",
            line =>
            {
                Console.WriteLine("-----DUMP-----");
                Console.WriteLine($"ACC : {accumulator}");
                Console.WriteLine($"PC  : {programCounter}");

                Console.WriteLine("Flags: ");

                for (int i = 0; i < flagSet.Length; i++)
                {
                    Console.WriteLine($"{(Flags) i} : {flagSet[i]}");
                }
                Console.WriteLine("---END-DUMP---");
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
                    int varIndex = ProcessAddress(next);
                    data.AddNewVariable(varIndex, varName);
                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("pepehands");
                    Console.WriteLine(e);
                    throw;
                }
            }));

        commands.Add(new Command(
            "ld", "Loads a value into accumulator.",
            line =>
            {
                accumulator = ParseArguments(line[1]);
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

        commands.Add(new Command(
            "print", "Logs accumulator value to the terminal.",
            line =>
            {
                Console.WriteLine($"ACC : \"{accumulator}\"");
                return 0;
            }));

        #region Arithmetic Commands

        commands.Add(new Command(
            "add", "Adds a number to accumulator.",
            line =>
            {
                accumulator = (Convert.ToInt32(accumulator) +
                               Convert.ToInt32(ParseArguments(line[1]))
                    ).ToString();
                return 0;
            }));


        commands.Add(new Command(
            "sub", "Subtracts a number from accumulator.",
            line =>
            {
                accumulator = (Convert.ToInt32(accumulator) -
                               Convert.ToInt32(ParseArguments(line[1]))
                    ).ToString();
                return 0;
            }));


        commands.Add(new Command(
            "mul", "Multiplies a number to accumulator.",
            line =>
            {
                accumulator = (Convert.ToInt32(accumulator) *
                               Convert.ToInt32(ParseArguments(line[1]))
                    ).ToString();
                return 0;
            }));


        commands.Add(new Command(
            "div", "Divides the accumulator by a given non-zero number.",
            line =>
            {
                int divisor = Convert.ToInt32(ParseArguments(line[1]));
                if (divisor == 0) return 1;
                accumulator = (Convert.ToInt32(accumulator) / divisor).ToString();
                return 0;
            }));

        // End "Arithmetic Commands"
        #endregion

        #region Flag Manipulation

        commands.Add(new Command(
            "setf", "Sets a specified flag.",
            line =>
            {
                int flag = Convert.ToInt32(line[1]);
                flagSet[flag] = true;
                return 0;
            }));

        commands.Add(new Command(
            "rstf", "Resets a specified flag.",
            line =>
            {
                int flag = Convert.ToInt32(line[1]);
                flagSet[flag] = false;
                return 0;
            }));

        #endregion

        #region Procedure Execution

        commands.Add(new Command(
            "proc", "Calls a previously declared procedure.",
            line =>
            {
                pcProcedureStack.Push(programCounter);
                programCounter = procedures[line[1]];
                return 0;
            }));

        commands.Add(new Command(
            "endproc", "Returns control to parent procedure.",
            line =>
            {
                programCounter = pcProcedureStack.Pop();
                return 0;
            }));

        #endregion

        // commands.Add(new Command(
        //     "", "",
        //     line =>
        //     {
        //         return 0;
        //     }));

        // commands.Add(new Command(
        //     "", "",
        //     line =>
        //     {
        //         return 0;
        //     }));
    }

    // tests if token is in command list
    private bool IsCommand(string token, out int index)
    {
        index = Command.commandList.IndexOf(token);
        return index >= 0;
    }

    private void ProcessCommand(int commandIndex, List<string> line)
    {
        // TODO: handle errors
        try
        {
            commands[commandIndex].action.Invoke(line);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // End "Commands"
    #endregion

    // takes (&vars+20) and returns 20
    private int ProcessAddress(string token)
    {
        // TODO: swap "vars" for other base addresses, add enum for other bases
        Regex regex = new Regex(@"(.*)(&\w+\+)(\d+)(.*)");
        GroupCollection gc = regex.Match(token).Groups;
        // Console.WriteLine(gc[2].Value);
        int value = Convert.ToInt32(gc[3].Value);
        return value;
    }

    #region Debugging Utilities

    private void ShowParseInfo(List<string> line)
    {
        // return;

        Console.WriteLine("----------");
        Console.Write($"Line: ");
        for (int i = 0; i < line.Count; i++)
        {
            Console.Write($"\"{line[i]}\" ");
        }
        // Console.WriteLine("----------");
        Console.WriteLine();
    }

    #endregion
}
}