using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevTools.Console
{
	public class EditorConsoleWindow : EditorWindow
	{
		[SerializeField] private VisualTreeAsset consoleVisualAsset;
		private ConsoleModel consoleModel;
		private ConsoleView _view;
		
		private VisualElement _rootVisualElement;
		private VisualElement _consoleView;
		private VisualElement _consoleHeader;
		private TextField _textField;
		
		[MenuItem("DevTools/Console")]
		public static void Open()
		{
			var window = GetWindow<EditorConsoleWindow>();
			window.titleContent = new GUIContent("Developer Console");
			window.Show();
		}
		
		public void CreateGUI()
		{
			if (consoleVisualAsset == null)
			{
				Debug.LogError("Assign console UXML in EditorConsoleWindow.");
				return;
			}

			VisualElement root = consoleVisualAsset.Instantiate();
			root.style.flexGrow = 1;

			_consoleView = root.Q<VisualElement>("consoleView");
			_consoleView.style.display = DisplayStyle.Flex;
			
			_consoleHeader = root.Q<VisualElement>("consoleHeader");
			_consoleHeader.style.display = DisplayStyle.Flex;

			_textField = _consoleView.Q<TextField>("textField");
			_textField.pickingMode = PickingMode.Position;
			
			if (ConsoleModel.Instance == null)
			{
				consoleModel = new ConsoleModel();
			}
			
			else
			{
				consoleModel = ConsoleModel.Instance;
			}
			
			_view = new ConsoleView();
			_view.Initialize(root);
			rootVisualElement.Add(root);
		}
	}
}
