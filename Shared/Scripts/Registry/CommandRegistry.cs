using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DevTools.Console
{
	public class CommandRegistry
	{
		public static CommandRegistry Instance { get; } = new CommandRegistry();

		public IReadOnlyDictionary<string, DiscoveredCommand> Commands => _commands;
		private readonly Dictionary<string, DiscoveredCommand> _commands = new();

		private bool _hasDiscovered;
		private readonly HashSet<Type> _cachedTypes = new();

		public void DiscoverCommands(bool forceRefresh = false)
		{
			if (_hasDiscovered && !forceRefresh)
				return;

			_commands.Clear();

			// Try to load from cache first
			var cachedCommands = CommandRegistryCache.Load();
			if (cachedCommands.Count > 0 && !forceRefresh)
			{
				foreach (var cmd in cachedCommands)
					_commands[cmd.Name] = cmd;

				_hasDiscovered = true;
	#if UNITY_EDITOR
				Debug.Log($"[DevTools] Loaded {_commands.Count} commands from cache.");
	#endif
				return;
			}

			// Otherwise perform reflection discovery
			DiscoverCommandsViaReflection();

			// Save to cache for next session
			CommandRegistryCache.Save(_commands.Values);
			_hasDiscovered = true;
		}

		public void DiscoverCommandsViaReflection(bool forceRefresh = false)
		{
			if (forceRefresh)
			{
				_cachedTypes.Clear();
				_commands.Clear();
			}

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (_cachedTypes.Contains(type)) continue;
					if (type.GetCustomAttribute<IgnoreAttribute>() != null) continue;

					_cachedTypes.Add(type);

					var prefixAttr = type.GetCustomAttribute<CommandPrefixAttribute>();
					var prefix = prefixAttr != null && !string.IsNullOrWhiteSpace(prefixAttr.Prefix)
						? $"{prefixAttr.Prefix}."
						: string.Empty;

					foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
					{
						if (method.GetCustomAttribute<CommandAttribute>() is { } cmdAttr)
						{
							var cmd = new DiscoveredCommand(method, cmdAttr, null, prefix);
							_commands[cmd.Name] = cmd;
						}
					}
				}
			}
		}
	}
}
