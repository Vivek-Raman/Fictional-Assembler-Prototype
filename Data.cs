using System;
using System.Collections.Generic;

namespace Compiler
{
    public class Data
    {
        public Dictionary<int, Variable> varsTable = new Dictionary<int, Variable>();

        public void AddNewVariable(int index, string varName)
        {
            varsTable.Add(index, new Variable(varName));
        }

        public bool TryGetVariableValue(string varName, out string value)
        {
            int varIndex = SearchVarsTable(varName);
            if (varIndex >= 0)
            {
                value = varsTable[varIndex].GetValue();
                return true;
            }

            value = null;
            return false;
        }

        public void SetVariableValue(string varName, string value)
        {
            int varIndex = SearchVarsTable(varName);
            if (varIndex >= 0)
            {
                varsTable[varIndex].SetValue(value);
            }
            else
            {
                // TODO: fail condition
            }
        }

        private int SearchVarsTable(string token)
        {
            foreach (var variable in varsTable)
            {
                if (token == variable.Value.Name)
                {
                    return variable.Key;
                }
            }

            return -1;
        }
    }
}