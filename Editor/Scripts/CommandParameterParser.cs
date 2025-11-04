using UnityEngine;
using System;
using System.Reflection;

namespace DevTools
{
	namespace Console
    {
        public class CommandParameterParser
		{
			public static object[] ParseParameters(CommandData command, string[] inputArgs)
			{
				ParameterInfo[] parameters = command.Parameters;
				object[] methodParams = new object[parameters.Length];

				if (inputArgs.Length < command.RequiredParameterCount)
				{
					throw new ArgumentException(
						$"Command '{command.Name}' requires at least {command.RequiredParameterCount} parameters. " +
						$"Got {inputArgs.Length}. Usage: {command.Name} {command.ParameterSignature}");
				}

				for (int i = 0; i < parameters.Length; i++)
				{
					ParameterInfo param = parameters[i];
					
					if (i < inputArgs.Length)
					{
						// Parse provided argument
						methodParams[i] = ConvertParameter(inputArgs[i], param.ParameterType, param.Name);
					}
					else if (param.IsOptional || param.HasDefaultValue)
					{
						// Use default value for optional parameters
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
				try
				{
					// Handle common Unity types
					if (targetType == typeof(string))
						return input;
						
					if (targetType == typeof(int))
						return int.Parse(input);
						
					if (targetType == typeof(float))
						return float.Parse(input);
						
					if (targetType == typeof(bool))
					{
						if (bool.TryParse(input, out bool result))
							return result;
							
						// Support 1/0, yes/no, on/off for booleans
						return input.ToLower() switch
						{
							"1" or "true" or "yes" or "on" => true,
							"0" or "false" or "no" or "off" => false,
							_ => throw new FormatException($"Invalid boolean value: {input}")
						};
					}
					
					if (targetType.IsEnum)
					{
						return Enum.Parse(targetType, input, true); // Ignore case
					}
					
					if (targetType == typeof(Vector3))
					{
						// Parse "1,2,3" as Vector3
						string[] parts = input.Split(',');
						if (parts.Length == 3)
						{
							return new Vector3(
								float.Parse(parts[0]),
								float.Parse(parts[1]),
								float.Parse(parts[2])
							);
						}
						throw new FormatException($"Vector3 requires 3 values separated by commas");
					}

					// Fallback for other types
					return Convert.ChangeType(input, targetType);
				}
				catch (Exception ex)
				{
					throw new ArgumentException($"Failed to parse parameter '{paramName}': {ex.Message}");
				}
			}

			private static object GetDefaultValue(Type type)
			{
				return type.IsValueType ? Activator.CreateInstance(type) : null;
			}
		}
    }
}