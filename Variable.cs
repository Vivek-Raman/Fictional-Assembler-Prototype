using System;

namespace Compiler
{
    public class Variable
    {
        private string name;
        private string value;

        public int GetValueAsInt()
        {
            return Convert.ToInt32(value);
        }

        public string Name => name;

        public Variable(string name)
        {
            this.name = name;
            this.value = "";
        }

    }
}