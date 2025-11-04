using UnityEngine;
using UnityEngine.Events;

namespace DevTools.Console
{
    public class ConsoleCommands : MonoBehaviour
	{
		public static UnityEvent OnConsoleCleared = new UnityEvent();

		[Command("clear", Alias = "console_clear", Description = "Clears the console output")]
		private static void ClearConsole()
		{
			OnConsoleCleared?.Invoke();
		}
	}
}
