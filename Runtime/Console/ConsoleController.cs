// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spellbound.Core.Console {
    /// <summary>
    /// The main controller for the developer console.
    /// Handles input, execution, and UI coordination.
    /// </summary>
    public class ConsoleController : MonoBehaviour {
        [Header("UI References"), SerializeField]
        private GameObject consolePanel;

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentContainer;

        [Header("Settings"), SerializeField] private int maxOutputLines = 100;

        private readonly List<string> _outputHistory = new();
        private readonly List<string> _commandHistory = new();
        private int _commandHistoryIndex = -1;

        public bool IsOpen => consolePanel != null && consolePanel.activeSelf;

        private void Awake() {
            // Initialize command registry
            CommandRegistry.Instance.AutoRegisterCommands();

            // Hide console initially
            if (consolePanel != null)
                consolePanel.SetActive(false);
        }

        /// <summary>
        /// TEMPORARY
        /// </summary>
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;

        private void Update() {
            if (Input.GetKeyDown(toggleKey)) ToggleConsole();

            if (!IsOpen)
                return;

            if (Input.GetKeyDown(KeyCode.UpArrow))
                NavigateHistoryUp();

            else if (Input.GetKeyDown(KeyCode.DownArrow))
                NavigateHistoryDown();
        }

        private void OnEnable() {
            if (inputField != null)
                inputField.onSubmit.AddListener(OnSubmitInput);
        }

        private void OnDisable() {
            if (inputField != null)
                inputField.onSubmit.RemoveListener(OnSubmitInput);
        }

        /// <summary>
        /// Toggle console visibility.
        /// Call this from your game's input system.
        /// </summary>
        public void ToggleConsole() => SetConsoleActive(!IsOpen);

        /// <summary>
        /// Open the console.
        /// </summary>
        public void OpenConsole() => SetConsoleActive(true);

        /// <summary>
        /// Close the console.
        /// </summary>
        public void CloseConsole() => SetConsoleActive(false);

        /// <summary>
        /// Set the console active state.
        /// </summary>
        private void SetConsoleActive(bool active) {
            if (consolePanel == null)
                return;

            consolePanel.SetActive(active);

            if (!active || inputField == null)
                return;

            // Focus input field when opening
            inputField.Select();
            inputField.ActivateInputField();
        }

        /// <summary>
        /// Called when the user submits input.
        /// </summary>
        private void OnSubmitInput(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                // Re-focus if empty submit
                if (inputField != null)
                    inputField.ActivateInputField();

                return;
            }

            // Echo the command
            LogInput($"> {input}");

            // Add to command history
            _commandHistory.Add(input);
            _commandHistoryIndex = _commandHistory.Count;

            // Execute the command
            ExecuteCommand(input);

            // Clear and refocus input
            if (inputField != null) {
                inputField.text = string.Empty;
                inputField.ActivateInputField();
            }

            // Scroll to bottom
            ScrollToBottom();
        }

        /// <summary>
        /// Navigate through the command history.
        /// </summary>
        public void NavigateHistoryUp() => NavigateCommandHistory(-1);

        /// <summary>
        /// Navigate through the command history.
        /// </summary>
        public void NavigateHistoryDown() => NavigateCommandHistory(1);

        /// <summary>
        /// Execute a command string.
        /// </summary>
        private void ExecuteCommand(string input) {
            try {
                var result = CommandRegistry.Instance.ExecuteCommand(input);

                if (result.Success) {
                    if (!string.IsNullOrEmpty(result.Message))
                        LogOutput(result.Message);
                }
                else
                    LogError(result.Message);
            }
            catch (Exception ex) {
                LogError($"Error executing command: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse input string into command and arguments.
        /// Handles quoted strings as single arguments.
        /// </summary>
        private string[] ParseInput(string input) {
            var parts = new List<string>();
            var inQuotes = false;
            var currentPart = "";

            foreach (var c in input) {
                switch (c) {
                    case '"':
                        inQuotes = !inQuotes;

                        break;
                    case ' ' when !inQuotes: {
                        if (currentPart.Length > 0) {
                            parts.Add(currentPart);
                            currentPart = "";
                        }

                        break;
                    }
                    default:
                        currentPart += c;

                        break;
                }
            }

            if (currentPart.Length > 0)
                parts.Add(currentPart);

            return parts.ToArray();
        }

        /// <summary>
        /// Navigate through the command history.
        /// </summary>
        private void NavigateCommandHistory(int direction) {
            if (_commandHistory.Count == 0 || inputField == null)
                return;

            _commandHistoryIndex += direction;
            _commandHistoryIndex = Mathf.Clamp(_commandHistoryIndex, 0, _commandHistory.Count);

            if (_commandHistoryIndex < _commandHistory.Count) {
                inputField.text = _commandHistory[_commandHistoryIndex];
                inputField.caretPosition = inputField.text.Length;
            }
            else
                inputField.text = "";
        }

        /// <summary>
        /// Scroll output to bottom
        /// </summary>
        private void ScrollToBottom() {
            if (scrollRect == null || contentContainer == null || outputText == null) 
                return;
            
            var textHeight = outputText.preferredHeight;
            contentContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
            
            Canvas.ForceUpdateCanvases();
            
            scrollRect.verticalNormalizedPosition = 0f;
        }

        /// <summary>
        /// Log user input to console.
        /// </summary>
        private void LogInput(string message) => AddOutput($"<color=#00FF00>{message}</color>");

        /// <summary>
        /// Log standard output to the console.
        /// </summary>
        public void LogOutput(string message) => AddOutput(message);

        /// <summary>
        /// Log error to console.
        /// </summary>
        public void LogError(string message) => AddOutput($"<color=#FF4444>{message}</color>");

        /// <summary>
        /// Add text to the output display.
        /// </summary>
        private void AddOutput(string message) {
            _outputHistory.Add(message);

            // Trim old entries
            if (_outputHistory.Count > maxOutputLines)
                _outputHistory.RemoveAt(0);

            // Update display
            if (outputText != null)
                outputText.text = string.Join("\n", _outputHistory);
        }

        /// <summary>
        /// Clear the console output.
        /// </summary>
        public void ClearOutput() {
            _outputHistory.Clear();

            if (outputText != null)
                outputText.text = "";
        }
    }
}