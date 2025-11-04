using System.Reflection;
using UnityEngine;
using UnityEngine.Events;



namespace DevTools.Console
{
	public class BuiltInCommands : MonoBehaviour
	{
		public static UnityEvent OnConsoleCleared = new UnityEvent();

		[Command("clear", Description = "Clears the console output")]
		private static void ClearConsole()
		{
			OnConsoleCleared?.Invoke();
		}
	}
}