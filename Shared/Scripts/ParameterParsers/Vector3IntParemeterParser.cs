using System;
using UnityEngine;

namespace DevTools.Console
{
	public class Vector3IntParemeterParser : IParameterParser
	{
		public bool CanParse(Type targetType) => targetType == typeof(Vector3Int);

        public object Parse(string input, Type targetType, string paramName)
        {
            string[] parts = input.Split(',');
			if (parts.Length != 3) throw new ArgumentException($"Parameter '{paramName}' must have 3 values.");
			return new Vector3Int(
				int.Parse(parts[0]),
				int.Parse(parts[1]),
				int.Parse(parts[2])
			);
        }
    }
}