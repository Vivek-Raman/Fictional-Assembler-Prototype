using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssemblerLibrary.Utils
{
internal static class Utilities
{
    public static List<List<string>> ConvertLinesTo2DArray(string[] toSplit)
    {
        List<List<string>> tokenMatrix = new List<List<string>>();
        foreach (string line in toSplit)
        {
            // regex truncates extra spaces : https://stackoverflow.com/a/206946
            List<string> toAdd = Regex.Replace(line, @"\s+", " ").
                Split(' ').ToList();
            tokenMatrix.Add(toAdd);
        }

        return tokenMatrix;
    }

    // takes (&vars+20) and returns 20
    public static int ProcessAddress(string token)
    {
        // TODO: swap "vars" for other base addresses, add enum for other bases
        Regex regex = new Regex(@"(.*)(&\w+\+)(\d+)(.*)");
        GroupCollection gc = regex.Match(token).Groups;
        // Logger.Instance.Log(gc[2].Value);
        int value = Convert.ToInt32(gc[3].Value);
        return value;
    }
}
}