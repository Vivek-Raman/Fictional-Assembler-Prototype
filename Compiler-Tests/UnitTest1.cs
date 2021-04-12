using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AssemblerLibrary;
using NUnit.Framework;

namespace Compiler_Tests
{
    public class Tests
    {
        private static string INPUT_FILE_PATH = "C:\\Projects\\.NET\\Compiler\\Assembler\\input.txt";
        private List<List<string>> input = new List<List<string>>();

        [SetUp]
        public void Setup()
        {
            foreach (string line in File.ReadAllLines(INPUT_FILE_PATH))
            {
                // regex truncates extra spaces : https://stackoverflow.com/a/206946
                List<string> toAdd = Regex.Replace(line, @"\s+", " ").
                    Split(" ").ToList();
                input.Add(toAdd);
            }
        }

        [Test]
        public void CompileCodeFromInputFilePath()
        {
            Assembler assembler = new Assembler();
            assembler.Compile(input);
            Assert.Pass();
        }
    }
}