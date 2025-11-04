using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevTools.Console
{
	/// <summary>
	/// Base ConsoleView that both Editor and Runtime use.
	/// Handles input, suggestions, and binds to ConsoleModel events.
	/// </summary>
	public class ConsoleView
	{
		public VisualElement Root => _root;
		public VisualElement resizeHandle;

		private VisualElement _root;
		private ScrollView _output;
		private TextField _input;
		private VisualElement _suggestionsContainer;
		private ListView _suggestionsList;

		private bool _isDragging;
		private bool _isResizing;

		private Vector2 _dragStartMouse;
		private Vector2 _dragStartPos;

		private Vector2 _resizeStartMouse;
		private Vector2 _resizeStartSize;

		private const float HANDLE_SIZE = 8f;
		private const float MIN_WIDTH = 600f;
		private const float MIN_HEIGHT = 400f;

		private VisualElement _header;

		private ConsoleModel _model => ConsoleModel.Instance;

		public void Initialize(VisualElement root)
		{
			_root = root;

			_output = root.Q<ScrollView>("outputView");
			_input = root.Q<TextField>("textField");
			_suggestionsContainer = root.Q<VisualElement>("suggestionsContainer");
			_suggestionsList = root.Q<ListView>("suggestionsList");
			_header = root.Q<VisualElement>("consoleHeader");

			_suggestionsContainer.style.display = DisplayStyle.None;

			_input.RegisterCallback<KeyDownEvent>(OnInputKeyDown, TrickleDown.TrickleDown);
			_input.RegisterValueChangedCallback(evt => UpdateSuggestions(evt.newValue));
			_input.Focus();

			InitializeSuggestionsList();

			_suggestionsList.itemsChosen += OnSuggestionChosen;
			_suggestionsList.selectionChanged += OnSuggestionSelected;

			// Subscribe to model events
			_model.OnLogAdded += AddOutputLine;
			_model.OnLogsCleared += ClearLogs;

			// Add existing logs if any
			foreach (var log in _model.GetLogs())
				AddOutputLine(log, "info");

		}

		private void OnInputKeyDown(KeyDownEvent evt)
		{
			bool suggestionsVisible = _suggestionsContainer.style.display == DisplayStyle.Flex;

			if (evt.keyCode == KeyCode.Tab)
			{
				evt.StopPropagation();

				if (_suggestionsList.itemsSource is List<string> items && items.Count > 0)
				{
					string completion = items[_suggestionsList.selectedIndex >= 0
						? _suggestionsList.selectedIndex
						: 0];

					_input.SetValueWithoutNotify(completion);
					_input.cursorIndex = _input.selectIndex = completion.Length;
					_suggestionsContainer.style.display = DisplayStyle.None;
				}

				return;
			}

			if (evt.keyCode == KeyCode.Return)
			{
				_model.ExecuteCommand(_input.value);
				_input.value = "";
				_suggestionsContainer.style.display = DisplayStyle.None;
				evt.StopPropagation();
				return;
			}

			if (evt.keyCode == KeyCode.UpArrow && !suggestionsVisible)
			{
				_input.value = _model.GetPreviousHistory();
				evt.StopPropagation();
			}

			if (evt.keyCode == KeyCode.DownArrow && !suggestionsVisible)
			{
				_input.value = _model.GetNextHistory();
				evt.StopPropagation();
			}

			if (suggestionsVisible)
			{
				if (evt.keyCode == KeyCode.DownArrow)
				{
					int index = Mathf.Min(_suggestionsList.selectedIndex + 1,
						_suggestionsList.itemsSource.Count - 1);
					_suggestionsList.selectedIndex = index;
					_suggestionsList.ScrollToItem(index);
					evt.StopPropagation();
				}
				else if (evt.keyCode == KeyCode.UpArrow)
				{
					int index = Mathf.Max(_suggestionsList.selectedIndex - 1, 0);
					_suggestionsList.selectedIndex = index;
					_suggestionsList.ScrollToItem(index);
					evt.StopPropagation();
				}
			}
		}

		private void UpdateSuggestions(string input)
		{
			var matches = _model.GetSuggestions(input);


			if (matches.Count == 0)
			{
				_suggestionsContainer.style.display = DisplayStyle.None;
				return;
			}

			_suggestionsContainer.style.display = DisplayStyle.Flex;
			_suggestionsList.itemsSource = matches;
			_suggestionsList.Rebuild();
		}

		private void OnSuggestionChosen(IEnumerable<object> chosenItems)
		{
			string chosen = chosenItems.FirstOrDefault() as string;
			if (chosen == null) return;

			_input.SetValueWithoutNotify(chosen);
			_input.cursorIndex = _input.selectIndex = chosen.Length;
			_suggestionsContainer.style.display = DisplayStyle.None;
		}

		private void OnSuggestionSelected(IEnumerable<object> selectedItems)
		{
			// Optionally highlight in preview or tooltip
		}

		private void AddOutputLine(string text, string messageType = "info")
		{
			Label line = new Label(text);
			line.AddToClassList("consoleOutputLabel");
			line.AddToClassList(messageType);
			_output.Add(line);
			_output.scrollOffset = new Vector2(0, _output.contentContainer.layout.height);
		}

		private void ClearLogs() => _output.Clear();

		#region Resize & Drag
		public void InitializeResize()
		{
			resizeHandle = new VisualElement { name = "resizeHandle" };
			resizeHandle.style.display = DisplayStyle.None;

			resizeHandle.style.position = Position.Absolute;

			resizeHandle.style.right = 0;
			resizeHandle.style.bottom = 0;

			resizeHandle.style.width = HANDLE_SIZE;
			resizeHandle.style.height = HANDLE_SIZE;

			resizeHandle.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);

			_root.Add(resizeHandle);

			_root.style.width = 600;
			_root.style.height = 400;
		}

		public void RegisterDragEvents()
		{
			_header?.RegisterCallback<PointerDownEvent>(evt =>
			{
				_isDragging = true;
				_dragStartMouse = evt.position;
				_dragStartPos = new Vector2(_root.resolvedStyle.left, _root.resolvedStyle.top);
				_header.CapturePointer(evt.pointerId);
			});

			_header?.RegisterCallback<PointerMoveEvent>(evt =>
			{
				if (!_isDragging || !_header.HasPointerCapture(evt.pointerId)) return;

				Vector3 dragStartPos = new Vector3(_dragStartMouse.x, _dragStartMouse.y, 0);
				Vector2 delta = evt.position - dragStartPos;
				Vector2 newPos = _dragStartPos + delta;

				float width = _root.resolvedStyle.width;
				float height = _root.resolvedStyle.height;
				newPos.x = Mathf.Clamp(newPos.x, 0, Screen.width - width);
				newPos.y = Mathf.Clamp(newPos.y, 0, Screen.height - height);

				_root.style.left = newPos.x;
				_root.style.top = newPos.y;
			});

			_header?.RegisterCallback<PointerUpEvent>(evt =>
			{
				if (!_isDragging) return;
				_isDragging = false;
				_header.ReleasePointer(evt.pointerId);
			});
		}

		public void RegisterResizeEvents()
		{
			resizeHandle.RegisterCallback<PointerDownEvent>(evt =>
			{
				_isResizing = true;
				_resizeStartMouse = evt.position;
				_resizeStartSize = new Vector2(_root.resolvedStyle.width, _root.resolvedStyle.height);
				resizeHandle.CapturePointer(evt.pointerId);
			});

			resizeHandle.RegisterCallback<PointerMoveEvent>(evt =>
			{
				if (!_isResizing || !resizeHandle.HasPointerCapture(evt.pointerId)) return;

				Vector3 resizeStartPos = new Vector3(_resizeStartMouse.x, _resizeStartMouse.y, 0);
				Vector2 delta = evt.position - resizeStartPos;

				float newWidth = Mathf.Max(MIN_WIDTH, _resizeStartSize.x + delta.x);
				float newHeight = Mathf.Max(MIN_HEIGHT, _resizeStartSize.y + delta.y);

				_root.style.width = newWidth;
				_root.style.height = newHeight;
			});

			resizeHandle.RegisterCallback<PointerUpEvent>(evt =>
			{
				if (!_isResizing) return;
				_isResizing = false;
				resizeHandle.ReleasePointer(evt.pointerId);
			});
		}
		#endregion
		
		private void InitializeSuggestionsList()
		{
			// Set up custom item creation for the suggestions list
			_suggestionsList.makeItem = MakeSuggestionItem;
			_suggestionsList.bindItem = BindSuggestionItem;

			// Optional: Set fixed height for items
			_suggestionsList.fixedItemHeight = 24;
			_suggestionsList.style.minHeight = 120;
		}

		private VisualElement MakeSuggestionItem()
		{
			var item = new Label();
			item.AddToClassList("suggestion-item");
			return item;
		}

		private void BindSuggestionItem(VisualElement element, int index)
		{
			if (element is Label label && _suggestionsList.itemsSource is List<string> items)
			{
				if (index >= 0 && index < items.Count)
				{
					var suggestion = items[index];
					label.text = suggestion;

					label.AddToClassList("consoleOutputLabel");
				}	
			}
		}
	}
}
