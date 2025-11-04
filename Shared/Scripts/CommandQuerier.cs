using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DevTools.Console
{
	public class CommandQuerier : ICommandQuerier
	{
		public string Name => "Command Querier";

		public CommandDiscoveryResult DiscoverCommands()
		{
			var discoveredCommands = new List<DiscoveredCommand>();
			
			try
			{
				DiscoverStaticCommands(discoveredCommands);
				DiscoverInstanceCommandsFromAssemblies(discoveredCommands);
				Debug.Log($"🔍 Command discovery complete: {discoveredCommands.Count} commands found");

				return CommandDiscoveryResult.SuccessResult(discoveredCommands);
			}
			catch (Exception ex)
			{
				Debug.LogError($"❌ Command discovery failed: {ex.Message}");
				return CommandDiscoveryResult.FailureResult(ex.Message);
			}
		}

		private void DiscoverStaticCommands(List<DiscoveredCommand> commands)
		{
			int staticCount = 0;

			foreach (var assembly in GetUserAssemblies())
			{
				foreach (var type in GetSafeTypes(assembly))
				{
					foreach (var method in GetStaticMethods(type))
					{
						var command = CreateCommandFromMethod(method, null);
						if (command != null)
						{
							commands.Add(command);
							staticCount++;
						}
					}
				}
			}

			Debug.Log($"Found {staticCount} static commands");
		}
		
		private void DiscoverInstanceCommandsFromAssemblies(List<DiscoveredCommand> commands)
		{
			int instanceCount = 0;

			foreach (var assembly in GetUserAssemblies())
			{
				foreach (var type in GetSafeTypes(assembly))
				{
					// only consider MonoBehaviours
					if (!typeof(MonoBehaviour).IsAssignableFrom(type)) 
						continue;

					// find methods with [Command]
					foreach (var method in GetInstanceMethods(type))
					{
						// Try to find an instance in the scene
						MonoBehaviour instance = UnityEngine.Object.FindAnyObjectByType(type) as MonoBehaviour;

						var command = CreateCommandFromMethod(method, instance);
						if (command != null)
						{
							commands.Add(command);
							instanceCount++;
						}
					}
				}
			}

			Debug.Log($"Found {instanceCount} instance commands (via assemblies)");
		}

		private DiscoveredCommand CreateCommandFromMethod(MethodInfo method, MonoBehaviour instance)
		{
			var attribute = method.GetCustomAttribute<CommandAttribute>(true);
			if (attribute == null) return null;

			var command = new DiscoveredCommand
			{
				Name = !string.IsNullOrEmpty(attribute.CommandName) 
						? attribute.CommandName.ToLower() 
						: method.Name.ToLower(),
				Description = attribute.Description ?? "",
				Alias = attribute.Alias ?? "",
				Method = method,
				IsStatic = method.IsStatic,
				Instance = instance,
				DeclaringType = method.DeclaringType,
				Parameters = method.GetParameters(),
				CommandType = attribute.CommandType
			};

			command.AnalyzeMethod();
			return command;
		}

		private IEnumerable<Assembly> GetUserAssemblies()
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				var name = asm.GetName().Name;
				if (name == "Assembly-CSharp" ||
					name.Contains("DevTools") ||
					name.StartsWith("Game") ||
					name.StartsWith("Project"))
					yield return asm;
			}
		}

		private IEnumerable<Type> GetSafeTypes(Assembly assembly)
		{
			try { return assembly.GetTypes(); }
			catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
		}

		private IEnumerable<MethodInfo> GetStaticMethods(Type type)
		{
			return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
					   .Where(m => m.GetCustomAttribute<CommandAttribute>() != null);
		}

		private IEnumerable<MethodInfo> GetInstanceMethods(Type type)
		{
			return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					   .Where(m => m.GetCustomAttribute<CommandAttribute>() != null);
		}
	}
}
