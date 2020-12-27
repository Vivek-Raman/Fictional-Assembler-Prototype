using System.Collections.Generic;

namespace Compiler
{
    public class Data
    {
        public Dictionary<int, Variable> varsTable = new Dictionary<int, Variable>();

        public void AddNewVariable(int index, string variableName)
        {
            varsTable.Add(index, new Variable(variableName));
        }
    }
}