using System.Linq;
using UnityEngine;

namespace DevTools.Console
{
	[CommandPrefix("graphic_")]
	public class GraphicCommands : MonoBehaviour
	{
		[Command("set_fullscreen", Alias = "fullscreen", CommandType = ConsoleCommandType.Runtime_Editor, Description = "Sets the value of the fullscreen")]
		private static void SetFullscreen(bool isFullScreen)
		{
			Screen.fullScreen = isFullScreen;
		}
		
		[Command("get_supported_resolutions", Alias = "supported_resolutions", CommandType = ConsoleCommandType.Runtime_Editor, Description = "Gets the supported resolutions of the user")]
		private static void GetSupportedResolutions()
		{
			foreach (var resolution in Screen.resolutions)
            {
				ConsoleModel.Instance.AddLog($"> {resolution}", "info");
            }
		}
		
		[Command("set_resolution", Alias = "resolution", CommandType = ConsoleCommandType.Runtime_Editor, Description = "Sets the defined resolution to be the current resolution")]
		private static void SetResolution(Resolution resolution)
		{
			if (!Screen.resolutions.Contains(resolution))
			{
				Debug.LogError("This resolution {resolution} is not supported!");
				return;
			}

			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
		}
	}
}