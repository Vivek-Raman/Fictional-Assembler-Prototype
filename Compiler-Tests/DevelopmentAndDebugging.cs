using System.IO;
using AssemblerLibrary;
using AssemblerLibrary.Utils;
using NUnit.Framework;

namespace Compiler_Tests
{
    public class DevelopmentAndDebugging
    {
        private static string INPUT_FILE_PATH = "C:\\Projects\\.NET\\Compiler\\Assembler\\input.txt";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CompileCodeFromInputFilePath()
        {
            Assembler assembler = new Assembler();
            assembler.Compile(File.ReadAllLines(INPUT_FILE_PATH), new StandardLogger());
            Assert.Pass();
        }
    }
}