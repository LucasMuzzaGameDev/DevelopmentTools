using System;
using UnityEngine;

namespace DevTools.Console
{
	public class Vector3ParemeterParser : IParameterParser
	{
		public bool CanParse(Type targetType) => targetType == typeof(Vector3);

        public object Parse(string input, Type targetType, string paramName)
        {
            string[] parts = input.Split(',');
			if (parts.Length != 3) throw new ArgumentException($"Parameter '{paramName}' must have 3 values.");
			return new Vector3(
				float.Parse(parts[0]),
				float.Parse(parts[1]),
				float.Parse(parts[2])
			);
        }
    }
}