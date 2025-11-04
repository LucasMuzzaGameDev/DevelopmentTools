using System;
using UnityEngine;

namespace DevTools
{
	namespace Console
	{
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
		public class CommandAttribute : Attribute
		{
			public string CommandName { get; }
			public string Description { get; set; }
			public ConsoleCommandType CommandType { get; set; }
			public MonoBehaviorTargetType MonoBehaviorTargetType { get; set; }
			public string Alias { get; set; }

			public CommandAttribute(string commandName = "")
			{
				CommandName = commandName;
				CommandType = ConsoleCommandType.Runtime;
			}
			
			public CommandAttribute(string commandName, ConsoleCommandType commandType)
			{
				CommandName = commandName;
				CommandType = commandType;
			}
			
			public CommandAttribute(string commandName, ConsoleCommandType commandType, MonoBehaviorTargetType monoBehaviorTargetType)
			{
				CommandName = commandName;
				CommandType = commandType;
				MonoBehaviorTargetType = monoBehaviorTargetType;
			}
		}

		public enum ConsoleCommandType
		{
			Runtime,
			Editor
		}
	}
}