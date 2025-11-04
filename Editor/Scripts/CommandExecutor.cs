using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevTools
{
	namespace Console
    {
        public class CommandExecutor
		{
			private readonly Dictionary<string, DiscoveredCommand> _commandLookup;
			private readonly CommandQuerier _commandQuerier;

			public CommandExecutor()
			{
				_commandQuerier = new CommandQuerier();

				var result = _commandQuerier.DiscoverCommands();
				if (!result.Success)
				{
					Debug.LogError($"Command discovery failed: {result.ErrorMessage}");
					_commandLookup = new Dictionary<string, DiscoveredCommand>();
					return;
				}

				// Build lookup dictionary (name + alias)
				_commandLookup = new Dictionary<string, DiscoveredCommand>(StringComparer.OrdinalIgnoreCase);
				foreach (var cmd in result.Commands)
				{
					if (!_commandLookup.ContainsKey(cmd.Name))
						_commandLookup.Add(cmd.Name, cmd);

					if (!string.IsNullOrEmpty(cmd.Alias) && !_commandLookup.ContainsKey(cmd.Alias))
						_commandLookup.Add(cmd.Alias, cmd);
				}
			}

			/// <summary>
			/// Executes a console command string, like "teleport 1,2,3"
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
					object[] parsedParams = CommandParameterParser.ParseParameters(
						new CommandData(command.Name, command.Parameters),
						args
					);

					object target = command.IsStatic ? null : command.Instance;
					if (target == null && !command.IsStatic)
					{
						Debug.LogError($"[{commandName}] requires an instance but none was found");
						return;
					}

					object result = command.Method.Invoke(target, parsedParams);

					// if (result != null)
					// 	Debug.Log($"[{command.Method.Name}] returned: {result}");
					// else
					// 	Debug.Log($"[{command.Method.Name}] executed successfully");
				}
				catch (Exception ex)
				{
					Debug.LogError($"Error executing command [{commandName}]: {ex.Message}");
				}
			}

			/// <summary>
			/// Returns a list of all available commands.
			/// </summary>
			public IEnumerable<string> GetAvailableCommands()
			{
				return _commandLookup.Keys.OrderBy(k => k);
			}

			/// <summary>
			/// Tries to get a command’s description for help UI.
			/// </summary>
			public string GetCommandHelp(string commandName)
			{
				if (_commandLookup.TryGetValue(commandName, out var cmd))
					return $"{cmd.Name} {cmd.ParameterSignature} — {cmd.Description}";
				
				return $"Command '{commandName}' not found.";
			}
		}
    }
}