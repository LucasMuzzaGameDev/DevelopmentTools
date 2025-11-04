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
		private ConsoleModel consoleModel;

		private VisualElement _rootVisualElement;
		private VisualElement _consoleView;
		private VisualElement _consoleHeader;
		private TextField _textField;


		private void Awake()
		{
			var doc = GetComponent<UIDocument>();
			_rootVisualElement = doc.rootVisualElement;

			_consoleView = _rootVisualElement.Q<VisualElement>("consoleView");
			_consoleView.style.display = DisplayStyle.None;

			_consoleHeader = _rootVisualElement.Q<VisualElement>("consoleHeader");
			_consoleHeader.style.display = DisplayStyle.None;

			_textField = _consoleView.Q<TextField>("textField");
			
			if (ConsoleModel.Instance == null)
			{
				consoleModel = new ConsoleModel();
			}
			
			else
			{
				consoleModel = ConsoleModel.Instance;
			}
			
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
