using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevTools.Console
{
	public class SceneCommands : MonoBehaviour
	{
		[Command("scene_load", Alias = "load_scene", CommandType = ConsoleCommandType.Runtime, Description = "Loads a given scene using the scenes name")]
		private static void LoadScene(string sceneName)
		{
			SceneManager.LoadScene(sceneName);
		}
		
		[Command("scene_load_index", Alias = "load_scene_index", CommandType = ConsoleCommandType.Runtime, Description = "Loads a given scene using the build index")]
		private static void LoadSceneIndex(int sceneIndex)
		{
			SceneManager.LoadScene(sceneIndex);
		}

	}
}
