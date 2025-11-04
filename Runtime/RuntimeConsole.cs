using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DevTools.Console
{
	[RequireComponent(typeof(UIDocument))]
	public class RuntimeConsole : MonoBehaviour
	{	
		[SerializeField] private InputActionReference toggleAction;
		[SerializeField] private bool _isConsoleOpen;

		private ConsoleView _view;
		private ConsoleModel _consoleModel;

		private VisualElement _rootVisualElement;
		private VisualElement _consoleView;
		private VisualElement _consoleHeader;
		private TextField _textField;
		
		private VisualTreeAsset _consoleUXML;
		private StyleSheet _styleSheet;
		private PanelSettings _panelSettings;


		private void Awake()
		{
			var doc = GetComponent<UIDocument>();

			// Load assets from Resources (inside package)
			if (_consoleUXML == null) _consoleUXML = Resources.Load<VisualTreeAsset>("ConsoleUIDoc");
			if (_styleSheet == null) _styleSheet = Resources.Load<StyleSheet>("ConsoleUIStyleSheet");
			if (_panelSettings == null) _panelSettings = Resources.Load<PanelSettings>("ConsolePanelSettings");

			if (_consoleUXML == null)
				Debug.LogError("Could not load ConsolePanel UXML from Resources!");
			if (_panelSettings == null)
				Debug.LogError("Could not load ConsolePanelSettings from Resources!");

			// Assign the VisualTreeAsset to the UIDocument
			doc.visualTreeAsset = _consoleUXML;

			_rootVisualElement = doc.rootVisualElement;

			_consoleView = _rootVisualElement.Q<VisualElement>("consoleView");
			_consoleView.style.display = DisplayStyle.None;

			_consoleHeader = _rootVisualElement.Q<VisualElement>("consoleHeader");
			_consoleHeader.style.display = DisplayStyle.None;

			_textField = _consoleView.Q<TextField>("textField");

			_consoleModel = ConsoleModel.Instance ?? new ConsoleModel();

			_view = new ConsoleView();
			_view.Initialize(_rootVisualElement);

			_view.InitializeResize();
			_view.RegisterDragEvents();
			_view.RegisterResizeEvents();
		}
		
		private void OnEnable()
		{
			if (toggleAction != null)
			{
				toggleAction.action.Enable();
				toggleAction.action.performed += OnTogglePerformed;
			}
		}

		private void OnDisable()
		{
			if (toggleAction != null) toggleAction.action.performed -= OnTogglePerformed;
		}
		
		private void OnTogglePerformed(InputAction.CallbackContext ctx)
		{
			if (_isConsoleOpen) CloseConsole();
			else OpenConsole();
		}
		
		private void OpenConsole()
		{
			Debug.Log("opening runtime console");
			
			_isConsoleOpen = true;
			
			_consoleView.style.display = DisplayStyle.Flex;
			_consoleHeader.style.display = DisplayStyle.Flex;
			_view.resizeHandle.style.display = DisplayStyle.Flex;
			
			_textField.pickingMode = PickingMode.Position;
		}
		
		private void CloseConsole()
		{
			Debug.Log("closing runtime console");
			
			_isConsoleOpen = false;
			
			_consoleView.style.display = DisplayStyle.None;
			_consoleHeader.style.display = DisplayStyle.None;
			_view.resizeHandle.style.display = DisplayStyle.None;
			
			_textField.pickingMode = PickingMode.Ignore;
		}
	}
}
