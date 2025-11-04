using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevTools.Console
{
	/// <summary>
	/// Shared logic and data for the Developer Console.
	/// Keeps logs, command history, and command execution synchronized across Editor and Runtime.
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
					Debug.Log("Creating console model");
					_instance = new ConsoleModel();
				}
				return _instance;
			}
		}


		public event Action<string, string> OnLogAdded;
		public event Action OnLogsCleared;

		private CommandExecutor _executor;
		private readonly List<string> _commandHistory = new();
		private readonly List<string> _logs = new();
		private readonly List<string> _availableCommands = new();

		private int _historyIndex = -1;

		public ConsoleModel()
		{
			Application.logMessageReceived += HandleUnityLog;
			
			BuiltInCommands.OnConsoleCleared.AddListener(Clear);

			_executor = new CommandExecutor();
			_availableCommands = _executor.GetAvailableCommands().ToList();
		}

		private void HandleUnityLog(string logString, string stackTrace, LogType type)
		{
			string messageType = type switch
			{
				LogType.Error => "error",
				LogType.Assert => "error",
				LogType.Warning => "warning",
				LogType.Log => "info",
				LogType.Exception => "error",
				_ => "info"
			};

			AddLog(logString, messageType);
		}

		public void AddLog(string message, string type = "info")
		{
			_logs.Add(message);
			OnLogAdded?.Invoke(message, type);
		}

		public IReadOnlyList<string> GetLogs() => _logs.AsReadOnly();
		public IReadOnlyList<string> GetCommandHistory() => _commandHistory.AsReadOnly();
		public IReadOnlyList<string> GetAvailableCommands() => _availableCommands.AsReadOnly();

		public void ExecuteCommand(string command)
		{
			if (string.IsNullOrWhiteSpace(command))
				return;

			_commandHistory.Add(command);
			_historyIndex = _commandHistory.Count;

			_executor.Execute(command);
		}

		public void Clear()
		{
			_logs.Clear();
			OnLogsCleared?.Invoke();
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

		public List<string> GetSuggestions(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return new List<string>();

			return _availableCommands
				.Where(cmd => cmd.StartsWith(input, StringComparison.OrdinalIgnoreCase))
				.ToList();
		}
	}
}
