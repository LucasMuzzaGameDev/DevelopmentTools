using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevTools.Console
{
	public class CommandExecutor
	{
		private readonly Dictionary<string, DiscoveredCommand> _commandLookup;

		public CommandExecutor()
		{
			// Use the centralized registry
			var registry = CommandRegistry.Instance;

			// Ensure commands are discovered
			registry.DiscoverCommands();

			_commandLookup = new Dictionary<string, DiscoveredCommand>(
				registry.Commands, 
				StringComparer.OrdinalIgnoreCase
			);
		}

		/// <summary>
		/// Executes a console command string, e.g. "teleport 1,2,3"
		/// </summary>
		public void Execute(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				Debug.LogWarning("Empty command input");
				return;
			}

			string[] parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
			string commandName = parts[0];
			string[] args = parts.Skip(1).ToArray();

			if (!_commandLookup.TryGetValue(commandName, out var command))
			{
				Debug.LogError($"Unknown command: '{commandName}'");
				return;
			}

			try
			{
				bool isRuntimeOnly = command.CommandType == ConsoleCommandType.Runtime;
				bool isEditorOnly = command.CommandType == ConsoleCommandType.Editor;

				if (!Application.isPlaying && isRuntimeOnly)
				{
					Debug.LogWarning($"Command '{commandName}' can only be used at runtime.");
					return;
				}
				else if (Application.isPlaying && isEditorOnly)
				{
					Debug.LogWarning($"Command '{commandName}' can only be used in the editor.");
					return;
				}
				
				var parameterInfos = command.Parameters
					.Select(p => p.ParameterInfo)
					.ToArray();

				object[] parsedParams = CommandParameterParser.ParseParameters(
					new CommandData(command.Name, command.CommandType, parameterInfos),
					args
				);

				object result;

				if (command.IsStatic)
				{
					result = command.Method.Invoke(null, parsedParams);
				}
				else
				{
					command.Invoke(parsedParams);
					result = null;
				}

				if (result != null)
					Debug.Log($"[{command.Method.Name}] returned: {result}");
				else
					Debug.Log($"[{command.Method.Name}] executed successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error executing command [{commandName}]: {ex.Message}");
			}

		}


		/// <summary>
		/// Returns a list of all available command names.
		/// </summary>
		public IEnumerable<string> GetAvailableCommands()
		{
			return _commandLookup.Keys.OrderBy(k => k);
		}
	}
}
