using System.Reflection;
using System;
using System.Linq;
using System.Data;

namespace DevTools
{
	namespace Console
	{
		/// <summary>
		/// Contains all data needed to execute a console command
		/// </summary>
		public class CommandData
		{
			public string Name { get; set; }
			public string Description { get; set; }
			public string Usage { get; set; }
			public string Alias { get; set; }
			public ConsoleCommandType ConsoleCommandType { get; set; }
			public MonoBehaviorTargetType TargetType { get; set; }
			public MethodInfo Method { get; set; }
			public Type DeclaringType { get; set; }
			public ParameterInfo[] Parameters { get; set; }
			
			public CommandData(string name, ConsoleCommandType consoleCommandType, ParameterInfo[] parameters)
			{
				Name = name;
				ConsoleCommandType = consoleCommandType;
				Parameters = parameters ?? Array.Empty<ParameterInfo>();
				AnalyzeParameters();
			}
						
			// Additional parameter analysis
			public int RequiredParameterCount { get; private set; }
			public int OptionalParameterCount { get; private set; }
			public string ParameterSignature { get; private set; }

		public void AnalyzeParameters()
		{
			if (Parameters == null) 
			{
				Parameters = new ParameterInfo[0];
				return;
			}

			RequiredParameterCount = Parameters.Count(p => !p.IsOptional && !p.HasDefaultValue);
			OptionalParameterCount = Parameters.Length - RequiredParameterCount;
			
			// Build parameter signature for display
			ParameterSignature = string.Join(" ", Parameters.Select(p => 
			{
				string paramDesc = $"{p.Name}:{p.ParameterType.Name}";
				if (p.IsOptional || p.HasDefaultValue)
					paramDesc = $"[{paramDesc}]"; // Brackets indicate optional
				return paramDesc;
			}));
		}
		}
	}
}