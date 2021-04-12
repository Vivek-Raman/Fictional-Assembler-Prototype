using System;

namespace AssemblerLibrary
{
    public class Variable
    {
        private string name;
        private string value;

        public void SetValue(string toSet) => value = toSet;

        public string GetValue() => value;

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