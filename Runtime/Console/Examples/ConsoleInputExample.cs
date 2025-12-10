// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Spellbound.Core.Console.Examples {
    /// <summary>
    /// Example input handler for the developer console.
    /// This script demonstrates how to integrate console controls into your input system.
    /// 
    /// DEFAULT CONTROLS:
    /// - C Key: Toggle console visibility
    /// - Up Arrow: Navigate command history (previous)
    /// - Down Arrow: Navigate command history (next)
    /// 
    /// PRODUCTION INTEGRATION:
    /// Replace this component with your own input handling that hooks into your game's
    /// input/ui architecture. This is just an example script of how you can use and leverage
    /// the console API.
    /// </summary>
    [RequireComponent(typeof(ConsoleController))]
    public class ConsoleInputExample : MonoBehaviour {
        private ConsoleController _console;

        private void Awake() {
            _console = GetComponent<ConsoleController>();
            
            // Ensure EventSystem exists (user must configure input module manually)
            if (EventSystem.current != null) 
                return;

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
                
            Debug.LogWarning("[ConsoleInputExample] Created EventSystem. " +
                             "Select it in the Hierarchy and click 'Replace with InputSystemUIInputModule' " +
                             "or 'Add Default Input Module' to enable UI input.");
        }

        private void Start() {
            PrintWelcomeMessage();
        }

        private static void PrintWelcomeMessage() {
            ConsoleLogger.PrintToConsole("=== Developer Console Example ===");
            ConsoleLogger.PrintToConsole("");
            ConsoleLogger.PrintToConsole("Controls:");
            ConsoleLogger.PrintToConsole("  [C] - Toggle console visibility");
            ConsoleLogger.PrintToConsole("  [Up Arrow] - Previous command");
            ConsoleLogger.PrintToConsole("  [Down Arrow] - Next command");
            ConsoleLogger.PrintToConsole("");
            ConsoleLogger.PrintToConsole("Type 'help' to see available commands");
            ConsoleLogger.PrintToConsole("");
            ConsoleLogger.PrintToConsole("IMPORTANT: If you can't type in the input field:");
            ConsoleLogger.PrintToConsole("1. Select 'EventSystem' in the Hierarchy");
            ConsoleLogger.PrintToConsole("2. Click 'Add Default Input Module'");
            ConsoleLogger.PrintToConsole("");
            ConsoleLogger.PrintToConsole("================================");
        }

#if ENABLE_INPUT_SYSTEM
        private void Update() {
            if (Keyboard.current == null) 
                return;

            // Toggle console
            if (Keyboard.current.cKey.wasPressedThisFrame)
                _console.ToggleConsole();
            
            if (!_console.IsVisible) 
                return;

            // Command history navigation
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                _console.NavigateHistoryUp();
                
            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                _console.NavigateHistoryDown();
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        private void Update() {
            // Toggle console
            if (Input.GetKeyDown(KeyCode.C))
                _console.ToggleConsole();
            
            if (!_console.IsVisible) 
                return;
            
            // Command history navigation
            if (Input.GetKeyDown(KeyCode.UpArrow))
                _console.NavigateHistoryUp();
                
            if (Input.GetKeyDown(KeyCode.DownArrow))
                _console.NavigateHistoryDown();
        }
#endif
    }
}