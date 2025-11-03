using System.Reflection;
using UnityEngine;
using UnityEngine.Events;



namespace DevTools.Console
{
	public class ExampleCommands : MonoBehaviour
	{
		public static UnityEvent OnConsoleCleared = new UnityEvent();
		
		[Command("player.Health", Description = "Gets player health")]
		private float GetPlayerHealth() => 100f;

		[Command("enemy.KillAll", Description = "Kills all enemies")]
		private void KillAllEnemies() => Debug.Log("Killing enemies");

		[Command("game.Pause", Description = "Pauses the game")]
		private static void PauseGame(bool paused) => Debug.Log($"Game paused: {paused}");

		[Command("player.Heal", Alias = "heal", Description = "Heals the player")]
		private void HealPlayer(int amount) => Debug.Log($"Healing for {amount}");

		[Command("clear", Description = "Clears the console output")]
		private void ClearConsole()
		{
			OnConsoleCleared?.Invoke();
		}
	}
}