﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Compiler
{
    public class Program
    {

        static void Main(string[] args)
        {
            Compiler compiler = new Compiler();

            // get input
            List<List<string>> input = new List<List<string>>();
            foreach (string line in File.ReadAllLines(args[0]))
            {
                // regex removes extra spaces : https://stackoverflow.com/a/206946
                List<string> toAdd = Regex.Replace(line, @"\s+", " ").
                    Split(" ").ToList();
                input.Add(toAdd);
            }

            compiler.Compile(input);

            Console.WriteLine("Reached end of program.");
        }
    }
}