using System;
using UnityEngine;

namespace DevTools.Console
{
    public class IntParemeterParser : IParameterParser
    {
        public bool CanParse(Type targetType) => targetType == typeof(int);

        public object Parse(string input, Type targetType, string paramName)
        {
            return int.Parse(input);
        }
    }
}
