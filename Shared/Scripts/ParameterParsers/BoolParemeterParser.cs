using System;
using UnityEngine;

namespace DevTools.Console
{
    public class BoolParemeterParser : IParameterParser
	{
		public bool CanParse(Type targetType) => targetType == typeof(bool);

        public object Parse(string input, Type targetType, string paramName)
        {
           	return input.ToLower() switch
			{
				"1" or "true" or "yes" or "on" => true,
				"0" or "false" or "no" or "off" => false,
				_ => throw new ArgumentException($"Parameter '{paramName}' is not a valid boolean.")
			};
        }
    }
}