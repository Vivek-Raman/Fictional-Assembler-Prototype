using System;
using System.Collections.Generic;

using AssemblerLibrary.Models;
using AssemblerLibrary.Utils;

namespace AssemblerLibrary
{
public class Assembler
{
    #region Flag Management

    private const int FLAG_COUNT = 1;
    private const int INTERNAL_FLAG_COUNT = 3;
    private enum Flags
    { NULL = -1, StringMode }
    private enum InternalFlags
    { NULL = -1, Begun, InProcedureDefinition, ScheduleEnd }
    private bool[] flagSet = new bool[FLAG_COUNT];
    private bool[] internalFlagSet = new bool[INTERNAL_FLAG_COUNT];
    
    #endregion

    #region Data

    private List<Command> commands = new List<Command>();
    private Data data = new Data();
    private Dictionary<string, int> procedures = new Dictionary<string, int>();
    private Stack<int> pcProcedureStack = new Stack<int>();
    private int programCounter;
    private string accumulator;

    private Logger logger = null;

    // End "Data"
    #endregion

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

    private bool F_MustEnd
    {
        get => internalFlagSet[(int) InternalFlags.ScheduleEnd];
        set => internalFlagSet[(int) InternalFlags.ScheduleEnd] = value;
    }

    // End "Convenience Conditions"
    #endregion

    #region Public Methods

    public Assembler(string accumulator = "")
    {
        this.accumulator = accumulator;
        CreateCommands();
    }

    // called by Unity
    public void Compile(string[] code, Logger loggerInstance)
    {
        logger = loggerInstance;
        Compile(Utilities.ConvertLinesTo2DArray(code));
    }

    private void Compile(List<List<string>> input)
    {
        for (programCounter = 0; programCounter < input.Count; ++programCounter)
        {
            List<string> line = input[programCounter];

            if (line[0].StartsWith("#") || line[0] == "")
            {
                // # indicates a comment, and "" is empty line
                // these are ignored
                // logger.Log("comment!");
                continue;
            }

            if (F_MustEnd) return;

            if (!F_HasBegun)
            {
                if (line[0].StartsWith("procdef"))
                {
                    // logger.Log($"detected procedure {line[1]}");
                    F_IsInProcedureDefinition = true; // elaborate here to avoid local functions
                    procedures.Add(line[1], programCounter);
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
                logger.Log($"-----\nUNIDENTIFIED TOKEN ON LINE {programCounter + 1}: \"{line[0]}\"\n-----\n");
            }
        }
    }

    // End "Public Methods"
    #endregion

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
                F_MustEnd = true;
                return 0;
            }));

        #region Data Management

        commands.Add(new Command(
            "def", "Defines an alias to a memory address.",
            line =>
            {
                string varName = line[1];
                string address = line[2];
                for (int i = 3; i < line.Count; ++i)
                {
                    address += line[i];
                }

                try
                {
                    int varIndex = Utilities.ProcessAddress(address);
                    data.AddNewVariable(varIndex, varName);
                    return 0;
                }
                catch (Exception e)
                {
                    logger.Log("pepehands");
                    logger.Log(e);
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

        // end "Data Management"
        #endregion

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

        // end "Flag Manipulation"
        #endregion

        #region Procedure Execution

        commands.Add(new Command(
            "proc", "Calls a previously declared procedure. Create procedure with procdef <proc name>",
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

        // end "Procedure Execution"
        #endregion

        #region Branching Commands

        commands.Add(new Command(
            "cmp", "Returns the sign of the difference between ACC and the given value.",
            line =>
            {
                int difference = (Convert.ToInt32(accumulator) -
                                  Convert.ToInt32(ParseArguments(line[1])));
                accumulator = Math.Sign(difference).ToString();
                return 0;
            }));

        commands.Add(new Command(
            "jmp", "Jumps program control to the given line.",
            line =>
            {
                JumpToLine_OneIndexed(Convert.ToInt32(line[1]));
                return 0;
            }));

        commands.Add(new Command(
            "jaz", "Jumps to the given line if ACC is zero.",
            line =>
            {
                if (Convert.ToInt32(accumulator) == 0)
                    JumpToLine_OneIndexed(Convert.ToInt32(line[1]));
                return 0;
            }));

        commands.Add(new Command(
            "jap", "Jumps to the given line if ACC is positive.",
            line =>
            {
                if (Convert.ToInt32(accumulator) > 0)
                    JumpToLine_OneIndexed(Convert.ToInt32(line[1]));
                return 0;
            }));

        commands.Add(new Command(
            "jan", "Jumps to the given line if ACC is negative.",
            line =>
            {
                if (Convert.ToInt32(accumulator) < 0)
                    JumpToLine_OneIndexed(Convert.ToInt32(line[1]));
                return 0;
            }));

        // End "Branching Commands"
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

        #region Utility Commands

        commands.Add(new Command(
            "dump", "Provides a dump of the current state of memory.",
            line =>
            {
                logger.Log("----- DUMP -----");
                logger.Log($"ACC   : {accumulator}");
                logger.Log($"PC    : {programCounter}");

                logger.Log("DATA  : ");
                foreach (var variable in data.varsTable)
                {
                    logger.Log($"&{variable.Key} \"{variable.Value.Name}\" : {variable.Value.GetValue()}");
                }

                logger.Log("FLAGS : ");
                for (int i = 0; i < flagSet.Length; i++)
                {
                    logger.Log($"{(Flags) i} : {flagSet[i]}");
                }
                logger.Log("--- END-DUMP ---");
                return 0;
            }));

        commands.Add(new Command(
            "print", "Logs accumulator value to the terminal.",
            line =>
            {
                logger.Log($"ACC : \"{accumulator}\"");
                return 0;
            }));

        commands.Add(new Command(
            "help", "Prints a list of available commands to the terminal.",
            line =>
            {
                logger.Log("-----------");
                logger.Log("LIST OF COMMANDS");
                logger.Log($"Count: {commands.Count}");
                foreach(Command command in commands)
                {
                    logger.Log($"{command.name}\t{command.description}");
                }
                logger.Log("-----------");
                return 0;
            }));

        // end "Utility Commands"
        #endregion
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
            int result = commands[commandIndex].action.Invoke(line);
            if (result != 0)
            {
                logger.Log($"Command \"{commands[commandIndex].name} returned error code \"{result}\".");
            }
        }
        catch (Exception e)
        {
            logger.Log(e);
            throw;
        }
    }

    // End "Commands"
    #endregion

    #region Utility Methods

    private void ResetCompiler()
    {
        F_HasBegun = false;
        F_MustEnd = false;
        F_IsInProcedureDefinition = false;
    }

    private string ParseArguments(string token)
    {
        if (!flagSet[(int) Flags.StringMode] && data.TryGetVariableValue(token, out string value))
        {
            return value;
        }

        return token;
    }

    private void JumpToLine_OneIndexed(int line)
    {
        // one for index base adjustment
        // one for loop counter increment
        programCounter = line - 2;
    }

    #endregion

    #region Debugging Utilities

    private void ShowParseInfo(List<string> line)
    {
        // return;

        logger.Log("----------");
        Console.Write($"Line: ");
        for (int i = 0; i < line.Count; i++)
        {
            Console.Write($"\"{line[i]}\" ");
        }
        // logger.Log("----------");
        // logger.Log();
    }

    #endregion
}
}