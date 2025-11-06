using UnityEngine;
using UnityEngine.Events;

namespace DevTools.Console
{
	[CommandPrefix("console")]
	public class ConsoleCommands : MonoBehaviour
	{
		public static UnityEvent OnConsoleCleared = new UnityEvent();

		[Command("clear", CommandType = ConsoleCommandType.Runtime_Editor, Description = "Clears the console output")]
		private static void ClearConsole()
		{
			OnConsoleCleared?.Invoke();
		}
		
		[Command("refresh_commands", CommandType = ConsoleCommandType.Editor, Description = "Refresh all commands caches")]
		private static void RefreshConsoleCommands()
		{
			CommandRegistry.Instance.DiscoverCommands(forceRefresh: true);
		}
	}
}
