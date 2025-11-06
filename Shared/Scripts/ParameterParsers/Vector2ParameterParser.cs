using System;
using UnityEngine;

namespace DevTools.Console
{
	public class Vector2ParemeterParser : IParameterParser
	{
		public Type TargetType => typeof(Vector2);

		public bool CanParse(Type targetType) => targetType == typeof(Vector2);

        public object Parse(string input, Type targetType, string paramName)
        {
            string[] parts = input.Split(',');
			if (parts.Length != 3) throw new ArgumentException($"Parameter '{paramName}' must have 3 values.");
			return new Vector2(
				int.Parse(parts[0]),
				int.Parse(parts[1])
			);
        }
    }
}