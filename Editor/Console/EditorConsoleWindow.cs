using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DevTools
{
	namespace Console
	{
		public class EditorConsoleWindow : EditorWindow
		{
			public static EditorConsoleWindow Instance;
			[SerializeField] private VisualTreeAsset consoleVisualAsset;
			[SerializeField] private StyleSheet consoleStyleSheet;
			

			private CommandExecutor _executor;
			
			private List<string> _allCommands = new List<string>();
			
			
			[MenuItem("Tools/Developer Console")]
			public static void ShowEditor()
			{
				EditorConsoleWindow wnd = GetWindow<EditorConsoleWindow>();
				wnd.titleContent = new GUIContent("Developer Console");
			}
			private VisualElement _consoleView;
			private VisualElement _suggestionsContainer;
			
			private ScrollView _outputScrollView;
			private TextField _inputField;
			private ListView _suggestionsList;

			private List<string> commandHistory = new List<string>();
			private int historyIndex = -1;

			private void OnEnable()
			{
				Instance = this;
			}

			private void OnDisable()
			{
				if (Instance == this) Instance = null;
			}

			private void CreateGUI()
			{
				consoleVisualAsset.CloneTree(rootVisualElement);
				rootVisualElement.styleSheets.Add(consoleStyleSheet);

				_consoleView = rootVisualElement.Q<VisualElement>("consoleView");
				_suggestionsContainer = _consoleView.Q<VisualElement>("suggestionsContainer");
				_inputField = rootVisualElement.Q<TextField>("textField");
				_outputScrollView = rootVisualElement.Q<ScrollView>("outputView");
				_suggestionsList = rootVisualElement.Q<ListView>("suggestionsList");

				_inputField.RegisterCallback<KeyDownEvent>(OnInputKeyDown, TrickleDown.TrickleDown);
				_inputField.RegisterValueChangedCallback(evt => UpdateSuggestions(evt.newValue));
				
				// Ensure input field is focused and ready
				_inputField.Focus();
				
				// When user clicks or presses Enter on a suggestion
				_suggestionsList.itemsChosen += OnSuggestionChosen;
				_suggestionsList.selectionChanged += OnSuggestionSelected;
								
				Application.logMessageReceived += HandleLog;

				CommandQuerier commandQuerier = new CommandQuerier();
				commandQuerier.DiscoverCommands();
				
				_executor = new CommandExecutor();
				_allCommands = new List<string>(_executor.GetAvailableCommands());

				BuiltInCommands.OnConsoleCleared.AddListener(ClearLogs);
			}
			
			private void ExecuteCommand()
			{
				string command = _inputField.value;
				if (!string.IsNullOrEmpty(command))
				{
					commandHistory.Add(command);
					historyIndex = commandHistory.Count;
					_inputField.value = "";

					_executor.Execute(command);
				}
			}
			
			
			private void OnInputKeyDown(KeyDownEvent evt)
			{
				bool suggestionsVisible = _suggestionsContainer.style.display == DisplayStyle.Flex;
				
				// Autocomplete Suggestion
				if (evt.keyCode == KeyCode.Tab)
				{
					rootVisualElement.focusController.IgnoreEvent(evt);

					if (_suggestionsList.itemsSource is List<string> items && items.Count > 0)
					{
						string completion = items[_suggestionsList.selectedIndex >= 0 ? _suggestionsList.selectedIndex : 0];

						_inputField.SetValueWithoutNotify(completion);
						int caretPos = completion.Length;
						_inputField.cursorIndex = caretPos;
						_inputField.selectIndex = caretPos;
						_inputField.Focus();

						_suggestionsContainer.style.display = DisplayStyle.None;
					}

					evt.StopPropagation();
					return;
				}

				// If suggestions are visible, use arrows to move between them
				if (suggestionsVisible && _suggestionsList.itemsSource is List<string> suggestions && suggestions.Count > 0)
				{
					int selectedIndex = _suggestionsList.selectedIndex;

					if (evt.keyCode == KeyCode.DownArrow)
					{
						selectedIndex = Mathf.Min(selectedIndex + 1, suggestions.Count - 1);
						_suggestionsList.selectedIndex = selectedIndex;
						_suggestionsList.ScrollToItem(selectedIndex);
						evt.StopPropagation();
						return;
					}
					else if (evt.keyCode == KeyCode.UpArrow)
					{
						selectedIndex = Mathf.Max(selectedIndex - 1, 0);
						_suggestionsList.selectedIndex = selectedIndex;
						_suggestionsList.ScrollToItem(selectedIndex);
						evt.StopPropagation();
						return;
					}
					else if (evt.keyCode == KeyCode.Return)
					{
						// Confirm the selected suggestion
						if (_suggestionsList.selectedIndex >= 0)
						{
							string chosen = suggestions[_suggestionsList.selectedIndex];
							_inputField.SetValueWithoutNotify(chosen);
							_inputField.cursorIndex = _inputField.selectIndex = chosen.Length;
							_suggestionsContainer.style.display = DisplayStyle.None;
							_inputField.Focus();
							evt.StopPropagation();
							return;
						}
					}
				}

				// If no suggestions are open, handle command history navigation
				if (!suggestionsVisible)
				{
					if (evt.keyCode == KeyCode.UpArrow)
					{
						if (commandHistory.Count > 0)
						{
							historyIndex = Mathf.Max(historyIndex - 1, 0);
							_inputField.value = commandHistory[historyIndex];
						}
						evt.StopPropagation();
					}
					else if (evt.keyCode == KeyCode.DownArrow)
					{
						if (commandHistory.Count > 0 && historyIndex < commandHistory.Count - 1)
						{
							historyIndex++;
							_inputField.value = commandHistory[historyIndex];
						}
						else
						{
							historyIndex = commandHistory.Count;
							_inputField.value = "";
						}
						evt.StopPropagation();
					}
					else if (evt.keyCode == KeyCode.Return)
					{
						ExecuteCommand();
						evt.StopPropagation();
					}
				}
			}

			
			private void UpdateSuggestions(string input)
			{
				if (string.IsNullOrWhiteSpace(input))
				{
					_suggestionsContainer.style.display = DisplayStyle.None;
					return;
				}

				// Filter commands based on user input
				var matches = _allCommands
					.Where(cmd => cmd.StartsWith(input, System.StringComparison.OrdinalIgnoreCase))
					.ToList();

				if (matches.Count == 0)
				{
					_suggestionsContainer.style.display = DisplayStyle.None;
					return;
				}

				_suggestionsContainer.style.display = DisplayStyle.Flex;

				// Update the ListView
				_suggestionsList.itemsSource = matches;
				_suggestionsList.Rebuild();
			}


			private void HandleLog(string logString, string stackTrace, LogType type)
			{
				// Format the log message
				string messageType = type switch
				{
					LogType.Error => "error",
					LogType.Assert => "error",
					LogType.Warning => "warning",
					LogType.Log => "info",
					LogType.Exception => "error",
					_ => "info"
				};

				AddOutputLine(logString, messageType);
			}

			public void AddOutputLine(string text, string messageType = "info")
			{
				Label line = new Label(text);
				line.AddToClassList("consoleOutputLabel");
				line.AddToClassList(messageType);
				_outputScrollView.Add(line);

				// Scroll to bottom
				_outputScrollView.scrollOffset = new Vector2(0, _outputScrollView.contentContainer.layout.height);
			}

			private void OnDestroy()
			{
				Application.logMessageReceived -= HandleLog;
			}
			
			public static void ClearLogs()
			{
				Instance._outputScrollView.Clear();	
				
				var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
				var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
				clearMethod?.Invoke(null, null);
			}
			
			private void OnSuggestionChosen(IEnumerable<object> chosenItems)
			{
				string chosen = chosenItems.FirstOrDefault() as string;
				if (!string.IsNullOrEmpty(chosen))
				{
					// Set the input text to the selected command
					_inputField.SetValueWithoutNotify(chosen);
					
					// Move cursor to the end of the line and remove selection
					int caretPos = chosen.Length;
					_inputField.cursorIndex = caretPos;
					_inputField.selectIndex = caretPos;
					_inputField.Focus();

					// Optionally hide suggestions after choosing
					// _suggestionsContainer.style.display = DisplayStyle.None;
				}
			}

			private void OnSuggestionSelected(IEnumerable<object> selectedItems)
			{
				
			}
		}
	}
}
