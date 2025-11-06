using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DevTools.Console
{
    [Serializable]
    internal class SerializableCommand
    {
        public string Name;
        public string Alias;
        public string Description;
        public string CommandType;
        public string DeclaringType;
        public string MethodName;
        public List<string> ParameterTypes;
        public bool IsStatic;
    }

    [Serializable]
    internal class SerializableCommandCache
    {
        public List<SerializableCommand> Commands = new();
    }

    internal static class CommandRegistryCache
    {
        private static readonly string CachePath =
#if UNITY_EDITOR
            Path.Combine(Application.dataPath, "../Library/DevTools_CommandCache.json");
#else
            Path.Combine(Application.persistentDataPath, "DevTools_CommandCache.json");
#endif

        // Save discovered commands
        public static void Save(IEnumerable<DiscoveredCommand> commands)
        {
            var cache = new SerializableCommandCache
            {
                Commands = commands.Select(FromDiscoveredCommand).ToList()
            };

            var json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(CachePath, json);
#if UNITY_EDITOR
            Debug.Log($"[DevTools] Command cache saved: {cache.Commands.Count} commands.");
#endif
        }

        // Load commands at startup
        public static List<DiscoveredCommand> Load()
        {
            if (!File.Exists(CachePath))
                return new List<DiscoveredCommand>();

            var json = File.ReadAllText(CachePath);
            var cache = JsonUtility.FromJson<SerializableCommandCache>(json);
            return cache.Commands
                .Select(ToDiscoveredCommand)
                .Where(cmd => cmd != null)
                .ToList();
        }

        // Convert from runtime command to serializable
        private static SerializableCommand FromDiscoveredCommand(DiscoveredCommand cmd)
        {
            return new SerializableCommand
            {
                Name = cmd.Name,
                Alias = cmd.Alias,
                Description = cmd.Description,
                CommandType = cmd.CommandType.ToString(),
                DeclaringType = cmd.Method.DeclaringType.AssemblyQualifiedName,
                MethodName = cmd.Method.Name,
                ParameterTypes = cmd.Parameters
                    .Select(p => p.ParameterInfo.ParameterType.AssemblyQualifiedName)
                    .ToList(),
                IsStatic = cmd.IsStatic
            };
        }

        // Convert from serialized back to runtime reflection object
        private static DiscoveredCommand ToDiscoveredCommand(SerializableCommand sCmd)
        {
            try
            {
                var declaringType = Type.GetType(sCmd.DeclaringType);
                if (declaringType == null)
                {
                    Debug.LogWarning($"[DevTools] Missing type: {sCmd.DeclaringType}");
                    return null;
                }

                var paramTypes = sCmd.ParameterTypes
                    .Select(Type.GetType)
                    .Where(t => t != null)
                    .ToArray();

                var method = declaringType.GetMethod(
                    sCmd.MethodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                    null,
                    paramTypes,
                    null
                );

                if (method == null)
                {
                    Debug.LogWarning($"[DevTools] Missing method: {sCmd.MethodName} in {declaringType}");
                    return null;
                }

                object instance = null;
                if (!sCmd.IsStatic)
                    instance = UnityEngine.Object.FindObjectOfType(declaringType);

                // 🔧 FIX: Recreate a valid CommandAttribute
                var attribute = new CommandAttribute(
                    sCmd.Name);

                return new DiscoveredCommand(method, attribute, instance, prefix: null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DevTools] Error restoring command: {sCmd.Name} - {ex.Message}");
                return null;
            }
        }
    }
}
