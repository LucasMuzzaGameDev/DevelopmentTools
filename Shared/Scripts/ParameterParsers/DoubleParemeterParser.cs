using System;
using UnityEngine;

namespace DevTools.Console
{
    public class DoubleParemeterParser : IParameterParser
    {
        public bool CanParse(Type targetType) => targetType == typeof(double);

        public object Parse(string input, Type targetType, string paramName)
        {
            return double.Parse(input);
        }
    }
}
