using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace DevTools
{
	namespace Console
	{
		public class CommandParameterParser
		{
			private static readonly List<IParameterParser> Parsers = new()
			{
				new BoolParemeterParser(),
				new IntParemeterParser(),
				new DoubleParemeterParser(),
				new FloatParemeterParser(),
				new Vector2ParemeterParser(),
				new Vector2IntParemeterParser(),
				new Vector3ParemeterParser(),
				new Vector3ParemeterParser(),
				new GameObjectParameterParser(),
			};
			
			
			public static object[] ParseParameters(CommandData command, string[] inputArgs)
			{
				var parameters = command.Parameters;
				object[] methodParams = new object[parameters.Length];

				if (inputArgs.Length < command.RequiredParameterCount)
					throw new ArgumentException($"Command '{command.Name}' requires at least {command.RequiredParameterCount} parameters.");

				for (int i = 0; i < parameters.Length; i++)
				{
					ParameterInfo param = parameters[i];

					if (i < inputArgs.Length)
					{
						methodParams[i] = ConvertParameter(inputArgs[i], param.ParameterType, param.Name);
					}
					else if (param.IsOptional || param.HasDefaultValue)
					{
						methodParams[i] = param.DefaultValue ?? GetDefaultValue(param.ParameterType);
					}
					else
					{
						throw new ArgumentException($"Missing required parameter: {param.Name}");
					}
				}

				return methodParams;
			}

			private static object ConvertParameter(string input, Type targetType, string paramName)
			{
				foreach (var parser in Parsers)
				{
					if (parser.CanParse(targetType))
						return parser.Parse(input, targetType, paramName);
				}

				// Fallback to default Convert.ChangeType
				try
				{
					return Convert.ChangeType(input, targetType);
				}
				catch (Exception ex)
				{
					throw new ArgumentException($"Failed to parse parameter '{paramName}': {ex.Message}");
				}
			}

			private static object GetDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
		}
	}
}