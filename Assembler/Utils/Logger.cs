using System;

namespace AssemblerLibrary.Utils
{
public abstract class Logger
{
    public string Log(Object message)
    {
        return Log(message.ToString());
    }

    // overridden by target (Unity)
    public abstract string Log(string message);
}
}