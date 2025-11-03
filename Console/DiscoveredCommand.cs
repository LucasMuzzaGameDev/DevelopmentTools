using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DevTools.Console
{
    [Serializable]
    public class DiscoveredCommand
    {
        public string Name;
        public string Description;
        public string Alias;
        
        // Method information
        public MethodInfo Method;
        public bool IsStatic;
        public ParameterInfo[] Parameters;
        
        // Context information
        public MonoBehaviour Instance; // For instance methods
        public Type DeclaringType;
        
        // Metadata
        public ConsoleCommandType CommandType;
        public string Usage;
        public string ParameterSignature;

        public DiscoveredCommand()
        {
            Parameters = new ParameterInfo[0];
        }

        public void AnalyzeMethod()
        {
            // Build parameter signature
            var paramStrings = new List<string>();
            foreach (var param in Parameters)
            {
                string paramDesc = $"{GetTypeName(param.ParameterType)} {param.Name}";
                if (param.IsOptional || param.HasDefaultValue)
                    paramDesc = $"[{paramDesc}]";
                paramStrings.Add(paramDesc);
            }
            
            ParameterSignature = string.Join(" ", paramStrings);
            Usage = $"{Name} {ParameterSignature}";
        }

        private string GetTypeName(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(void)) return "void";
            return type.Name;
        }

        public override string ToString()
        {
            return $"{Name} (Static: {IsStatic}, Type: {DeclaringType?.Name})";
        }
    }
}