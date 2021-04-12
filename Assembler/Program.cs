using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssemblerLibrary
{
    public class Program
    {

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("-----------------");
            Console.WriteLine("--- ASSEMBLER ---");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Assembler assembler = new Assembler();

            // get input
            List<List<string>> input = new List<List<string>>();
            foreach (string line in File.ReadAllLines(args[0]))
            {
                // regex truncates extra spaces : https://stackoverflow.com/a/206946
                List<string> toAdd = Regex.Replace(line, @"\s+", " ").
                    Split(" ").ToList();
                input.Add(toAdd);
            }

            assembler.Compile(input);

            Console.WriteLine("Reached end of program.");
        }
    }
}