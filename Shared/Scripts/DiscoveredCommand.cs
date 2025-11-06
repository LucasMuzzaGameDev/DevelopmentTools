using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DevTools.Console
{
	[Serializable]
	public class DiscoveredCommand
	{
		public string Name { get; }
		public string Description { get; }
		public string Alias { get; }
		public string ParameterSignature { get; }
		public MethodInfo Method { get; }
		public ConsoleCommandType CommandType { get;  }
		public MonoBehaviorTargetType TargetType { get; }
		public Type DeclaringType { get; }
		public IReadOnlyList<DiscoveredParameter> Parameters { get; }
		public bool IsStatic => Method.IsStatic;

		private readonly object _boundInstance;
		private readonly List<object> _boundInstances;

		public object Instance => _boundInstance;
		public IReadOnlyList<object> Instances => _boundInstances?.AsReadOnly();

		public DiscoveredCommand(MethodInfo method, CommandAttribute attr, object instance = null, string prefix = "")
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			Method = method;
			DeclaringType = method.DeclaringType;
			CommandType = attr.CommandType;
			TargetType = attr?.MonoBehaviorTargetType ?? MonoBehaviorTargetType.Single;

			var rawName = !string.IsNullOrEmpty(attr?.CommandName) ? attr.CommandName : method.Name;
			Name = string.IsNullOrEmpty(prefix)
				? rawName.ToLowerInvariant()
				: $"{prefix}{rawName.ToLowerInvariant()}";

			Description = attr?.Description ?? string.Empty;
			Alias = attr?.Alias ?? string.Empty;

			// Build parameter list
			var paramInfos = method.GetParameters();
			var paramList = new List<DiscoveredParameter>(paramInfos.Length);
			foreach (var p in paramInfos)
				paramList.Add(new DiscoveredParameter(p));

			Parameters = paramList.AsReadOnly();

			ParameterSignature = string.Join(" ", paramInfos.Select(p =>
			{
				var repr = $"{p.Name}:{SimplifyType(p.ParameterType)}";
				if (p.IsOptional || p.HasDefaultValue)
					repr = $"[{repr}]";
				return repr;
			}));

			// Instance binding: prefer provided instance if non-null, else resolve based on TargetType
			if (!IsStatic)
			{
				if (instance != null)
				{
					_boundInstance = instance;
				}
				else
				{
					// ✅ Safe guard — only search Unity objects if the type actually inherits from UnityEngine.Object
					if (typeof(UnityEngine.Object).IsAssignableFrom(method.DeclaringType))
					{
						switch (TargetType)
						{
							case MonoBehaviorTargetType.Single:
								_boundInstance = UnityEngine.Object.FindFirstObjectByType(
									method.DeclaringType,
									FindObjectsInactive.Exclude
								);
								break;

							case MonoBehaviorTargetType.All:
								_boundInstances = new List<object>(
									UnityEngine.Object.FindObjectsByType(
										method.DeclaringType,
										FindObjectsSortMode.None
									).Cast<object>()
								);
								break;

							case MonoBehaviorTargetType.SingleInactive:
								_boundInstance = UnityEngine.Object.FindFirstObjectByType(
									method.DeclaringType,
									FindObjectsInactive.Include
								);
								break;

							case MonoBehaviorTargetType.AllInactive:
								_boundInstances = new List<object>(
									UnityEngine.Object.FindObjectsByType(
										method.DeclaringType,
										FindObjectsInactive.Include,
										FindObjectsSortMode.None
									).Cast<object>()
								);
								break;

							case MonoBehaviorTargetType.Singleton:
								_boundInstance = GetOrCreateSingleton(method.DeclaringType);
								break;
						}
					}
					else
					{
						// Non-Unity type (like ConsoleModel, CommandExecutor, etc.)
						// Leave instance null — will be handled as pure C# command
						_boundInstance = null;
					}
				}
			}
		}

		private string SimplifyType(Type t)
		{
			if (t == typeof(int)) return "int";
			if (t == typeof(float)) return "float";
			if (t == typeof(bool)) return "bool";
			if (t == typeof(string)) return "string";
			return t.Name;
		}

		private static object GetOrCreateSingleton(Type type)
		{
			var existing = UnityEngine.Object.FindFirstObjectByType(type, FindObjectsInactive.Include);
			if (existing != null)
				return existing;

			var go = new GameObject($"{type.Name}_Singleton");
			return go.AddComponent(type);
		}

		/// <summary>
		/// Executes this command respecting its binding type.
		/// </summary>
		public void Invoke(params object[] args)
		{
			if (IsStatic)
			{
				Method.Invoke(null, args);
				return;
			}

			switch (TargetType)
			{
				case MonoBehaviorTargetType.Argument:
					if (args == null || args.Length == 0 || args[0] == null)
						throw new ArgumentException($"Command {Name} expects a MonoBehaviour instance as its first argument.");

					var instanceArg = args[0];
					var actualArgs = new object[args.Length - 1];
					Array.Copy(args, 1, actualArgs, 0, actualArgs.Length);
					Method.Invoke(instanceArg, actualArgs);
					break;

				case MonoBehaviorTargetType.All:
					if (_boundInstances == null) return;
					foreach (var inst in _boundInstances)
						Method.Invoke(inst, args);
					break;
				case MonoBehaviorTargetType.AllInactive:
					if (_boundInstances == null) return;
					foreach (var inst in _boundInstances)
						Method.Invoke(inst, args);
					break;

				default:
					if (_boundInstance != null)
						Method.Invoke(_boundInstance, args);
					break;
			}
		}
	}

	[Serializable]
	public class DiscoveredParameter
	{
		[field: SerializeField] public string Name { get; private set; }
		[field: SerializeField] public Type Type { get; private set; }
		public ParameterInfo ParameterInfo { get; }
		public SuggestionsAttribute Suggestion { get; }

		public DiscoveredParameter(ParameterInfo info)
		{
			ParameterInfo = info ?? throw new ArgumentNullException(nameof(info));
			Name = info.Name;
			Type = info.ParameterType;
			Suggestion = info.GetCustomAttribute<SuggestionsAttribute>();
		}
	}
}
