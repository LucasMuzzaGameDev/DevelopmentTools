using System;
using UnityEngine;

namespace DevTools.Console
{
    public class GameObjectParameterParser : IParameterParser
    {
		// We allow parsing for GameObject or any Component types
		public bool CanParse(Type targetType)
		{
			return typeof(GameObject).IsAssignableFrom(targetType);
		}

        // Parses input string to the appropriate GameObject or Component instance
        public object Parse(string input, Type targetType, string paramName)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException($"Parameter '{paramName}' cannot be empty.");

            // Find the GameObject by name
            GameObject obj = GameObject.Find(input);
            if (obj == null)
                throw new ArgumentException($"GameObject '{input}' not found for parameter '{paramName}'.");

            // If the target type is GameObject, just return it
            if (targetType == typeof(GameObject))
                return obj;

            // If the target type is a Component, try to get the component from the GameObject
            if (typeof(Component).IsAssignableFrom(targetType))
            {
                Component comp = obj.GetComponent(targetType);
                if (comp == null)
                    throw new ArgumentException($"GameObject '{input}' does not have a component of type '{targetType.Name}' for parameter '{paramName}'.");
                return comp;
            }

            throw new ArgumentException($"Cannot parse parameter '{paramName}' to type '{targetType.Name}'.");
        }
    }
}
