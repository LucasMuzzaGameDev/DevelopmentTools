using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevTools.Console
{
	[CommandPrefix("scene")]
	public class SceneCommands : MonoBehaviour
	{
		[Command("load", Alias = "load_scene", CommandType = ConsoleCommandType.Runtime, Description = "Loads a given scene using the scenes name")]
		private static void LoadScene(string sceneName)
		{
			SceneManager.LoadScene(sceneName);
		}
		
		[Command("load_index", CommandType = ConsoleCommandType.Runtime, Description = "Loads a given scene using the build index")]
		private static void LoadSceneIndex(int sceneIndex)
		{
			SceneManager.LoadScene(sceneIndex);
		}

	}
}
