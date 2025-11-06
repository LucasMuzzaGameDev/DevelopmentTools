using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevTools.Console
{
	/// <summary>
	/// Shared logic and data for the Developer Console.
	/// Handles logs, command history, and command execution.
	/// Integrates directly with the CommandRegistry and Suggestion system.
	/// </summary>
	public class ConsoleModel
	{
		private static ConsoleModel _instance;
		public static ConsoleModel Instance
		{
			get
			{
				if (_instance == null)
				{
					Debug.Log("Creating ConsoleModel");
					_instance = new ConsoleModel();
				}
				return _instance;
			}
		}

		public event Action<string, string> OnLogAdded;
		public event Action OnLogsCleared;

		private readonly CommandExecutor _executor;
		private readonly List<string> _commandHistory = new();
		private readonly List<string> _logs = new();
		private int _historyIndex = -1;

		// Cached view of commands from registry
		private IReadOnlyDictionary<string, DiscoveredCommand> _commands => CommandRegistry.Instance.Commands;

		public ConsoleModel()
		{
			Application.logMessageReceived += HandleUnityLog;
			ConsoleCommands.OnConsoleCleared.AddListener(Clear);

			CommandRegistry.Instance.DiscoverCommands(forceRefresh: !Application.isPlaying);
			_executor = new CommandExecutor();	

			AddLog(">Console initializing...");
			AddLog(">Gathering commands...");

			AddLog($">Console initialized. Commands discovered: {_commands.Count}");
		}

		#region Logs

		private void HandleUnityLog(string logString, string stackTrace, LogType type)
		{
			string messageType = type switch
			{
				LogType.Error or LogType.Assert or LogType.Exception => "error",
				LogType.Warning => "warning",
				_ => "info"
			};
			AddLog(logString, messageType);
		}

		public void AddLog(string message, string type = "info")
		{
			_logs.Add(message);
			OnLogAdded?.Invoke(message, type);
		}

		public void Clear()
		{
			_logs.Clear();
			OnLogsCleared?.Invoke();
		}

		public IReadOnlyList<string> GetLogs() => _logs.AsReadOnly();

		#endregion

		#region Command Execution and History

		public void ExecuteCommand(string command)
		{
			if (string.IsNullOrWhiteSpace(command))
				return;

			_commandHistory.Add(command);
			_historyIndex = _commandHistory.Count;

			try
			{
				_executor.Execute(command);
			}
			catch (Exception ex)
			{
				AddLog($"Command failed: {ex.Message}", "error");
			}
		}

		public string GetPreviousHistory()
		{
			if (_commandHistory.Count == 0) return string.Empty;
			_historyIndex = Mathf.Max(_historyIndex - 1, 0);
			return _commandHistory[_historyIndex];
		}

		public string GetNextHistory()
		{
			if (_commandHistory.Count == 0) return string.Empty;
			if (_historyIndex < _commandHistory.Count - 1)
			{
				_historyIndex++;
				return _commandHistory[_historyIndex];
			}
			_historyIndex = _commandHistory.Count;
			return string.Empty;
		}

		public IReadOnlyList<string> GetCommandHistory() => _commandHistory.AsReadOnly();

		#endregion

		#region Command Discovery and Suggestions

		/// <summary>
		/// Refreshes all command metadata (useful after code reload or dynamic assembly load).
		/// </summary>
		public void RefreshCommands()
		{
			CommandRegistry.Instance.DiscoverCommands(forceRefresh: true);
			AddLog($"Commands refreshed. Total: {_commands.Count}");
		}



		[Command("get_all_commands", CommandType = ConsoleCommandType.Runtime, Description = "get all commands")]
		public List<string> GetAvailableCommands()
		{
			// return copy to avoid modification of the underlying dictionary keys
			return _commands.Keys.ToList();
		}

		/// <summary>
		/// Returns top-level autocomplete suggestions for the given input.
		/// </summary>
		public List<string> GetSuggestions(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return new List<string>();

			var parts = input.Split(' ');
			string commandName = parts[0];

			// Stage 1: suggest command names
			if (parts.Length == 1)
			{
				return _commands.Keys
					.Where(cmd => cmd.StartsWith(commandName, StringComparison.OrdinalIgnoreCase))
					.ToList();
			}

			// Stage 2: suggest parameter values for a known command
			if (_commands.TryGetValue(commandName, out var discoveredCommand))
			{
				var currentParamIndex = parts.Length - 2; // because first part is command name
				if (currentParamIndex < discoveredCommand.Parameters.Count)
				{
					var parameter = discoveredCommand.Parameters[currentParamIndex];
					var suggestionAttr = parameter.Suggestion;

					if (suggestionAttr != null)
					{
						try
						{
							// pass command instance if available (some suggestors might need it)
							var contextInstance = discoveredCommand.Instance;
							var suggestions = suggestionAttr.GetSuggestions(contextInstance, parameter.ParameterInfo) ?? Array.Empty<string>();

							return suggestions
								.Where(s => s.StartsWith(parts.Last(), StringComparison.OrdinalIgnoreCase))
								.ToList();
						}
						catch (Exception ex)
						{
							AddLog($"Suggestion error: {ex.Message}", "warning");
						}
					}
				}
			}

			return new List<string>();
		}

		#endregion
	}
}
