using System;
using UnityEngine;

namespace DevTools.Console
{
    public class FloatParemeterParser : IParameterParser
    {
        public bool CanParse(Type targetType) => targetType == typeof(float);

        public object Parse(string input, Type targetType, string paramName)
        {
            return float.Parse(input);
        }
    }
}
