# README

[Documentation](https://spell-bound-studios.gitbook.io/spellbound-docs/)

# Introduction

The Spellbound Core system is the only dependency of all Spellbound Unity packages. It comes equipped with some handy abstractions, attributes, and helpers that are leveraged across our systems. Feel free to build on top of them in your own game as this package is completely free. It also comes with a console that all packages and future packages make use of for added features and functionality.

The Spellbound Console System provides a powerful, extensible command-line interface for Unity games. It's designed with clean architecture principles, separating the console API from input handling to give you flexibility.

---

## Console System Overview

The console supports three types of commands:

1. **ICommand Classes** - Self-contained command implementations (e.g., `help`, `clear`)
2. **Utility Commands** - Static methods decorated with `[ConsoleUtilityCommand]`
3. **Preset Commands** - Methods that operate on ObjectPresets with specific modules

### Key Features
- ✅ Automatic command discovery via attributes
- ✅ Type-safe parameter parsing (int, float, bool, Vector3, Vector2, enums, etc.)
- ✅ Command aliasing
- ✅ Command history navigation
- ✅ Dual input system support (Legacy & New Input System)
- ✅ Clean API separation - no input polling in core controller
- ✅ Extensible architecture
- Auto-complete coming soon

### Included Commands
The Core package aims to be as lightweight as possible and therefore only comes with two simple commands:
- **help** - Lists all available commands
- **clear** - Clears console output

Each Spellbound package you download will come with more built-in commands and will auto register with the console for seamless integration.

---

## Quick Start

### Two Prefabs, Two Use Cases

The package includes two prefabs for different scenarios:

#### 1. ConsoleCanvas (Example Prefab)
**Use this for:** Quick testing and learning how the system works

**What it includes:**
- Pre-configured Canvas with EventSystem
- ConsoleController with UI already wired up
- `ConsoleInputExample` script demonstrating input integration
- Works out-of-the-box - just drag into scene and hit play

**Controls:**
- `C` - Toggle console
- `Up Arrow` - Previous command
- `Down Arrow` - Next command

#### 2. ConsolePrefab (Production Prefab)
**Use this for:** Integrating into your existing game UI

**What it includes:**
- Console UI hierarchy with ConsoleController
- Input fields and output text already configured
- NO input handling script - ready for your integration

**How to use:**
1. Drag ConsolePrefab into your existing Canvas
2. Ensure your scene has an EventSystem
3. Click the EventSystem and press "Replace with InputSystemUIInputModule" (or "Add Default Input Module") if the module doesn't exist
4. Hook up your input system to ConsoleController's public API by dragging and dropping InputActionReferences that can be found on your InputAction asset dropdown

---

## Integration Guide

### Option 1: Quick Start (Example)

Just drag `ConsoleCanvas` into your scene. Done! It works immediately with keyboard input.

### Option 2: Custom Integration (Production)

1. **Add the ConsolePrefab to your Canvas**
   ```
   YourCanvas
   ├── YourExistingUI
   └── ConsolePrefab (drag here)
   ```

2. **Configure EventSystem**
    - Select EventSystem in Hierarchy
   

3. **Hook Up Your Input**

   **With New Input System (Unity 6+):**
   ```csharp
   using UnityEngine.InputSystem;
   
   public class YourInputHandler : MonoBehaviour {
       [SerializeField] private ConsoleController console;
       [SerializeField] private InputActionReference toggleConsoleAction;
       
       private void OnEnable() {
           toggleConsoleAction.action.performed += console.OnTogglePerformed;
       }
       
       private void OnDisable() {
           toggleConsoleAction.action.performed -= console.OnTogglePerformed;
       }
   }
   ```

   **With Legacy Input Manager:**
   ```csharp
   void Update() {
       if (Input.GetKeyDown(KeyCode.C))
           console.ToggleConsole();
   }
   ```

4. **Optional: Disable Player Input When Console Opens**
   ```csharp
   void Start() {
       console.OnVisibilityChanged += OnConsoleVisibilityChanged;
   }
   
   void OnConsoleVisibilityChanged(bool isVisible) {
       if (isVisible)
           // playerInput represents the ActionInputs ActionMap type
           playerInput.Disable();
       else
           playerInput.Enable();
   }
   ```

---

## ConsoleController API

The `ConsoleController` provides a clean public API - it does NOT poll input itself. This separation allows you to integrate it however you want.

### Public Methods

```csharp
// Toggle visibility
void ToggleConsole()
void OpenConsole()
void CloseConsole()

// Command history navigation
void NavigateHistoryUp()
void NavigateHistoryDown()

// Set visibility without animation hooks
void SetVisibilityImmediate(bool visible)

// Clear output
void ClearOutput()

// For Input System integration
void OnTogglePerformed(InputAction.CallbackContext context)
void OnHistoryUpPerformed(InputAction.CallbackContext context)
void OnHistoryDownPerformed(InputAction.CallbackContext context)
```

### Properties & Events

```csharp
// Check if console is visible
bool IsVisible { get; }

// Subscribe to visibility changes
event Action<bool> OnVisibilityChanged
```

### Protected Virtual Methods For Custom Overrides

```csharp
// Override for custom show/hide animations
protected virtual void ShowUI()
protected virtual void HideUI()
protected virtual void SetVisibility(bool visible)
```

---

## Creating Commands

### ICommand Classes

Best for: Standalone utility commands that don't need external context.

**Example:**
```csharp
using Spellbound.Core.Console;

[ConsoleCommandClass("mycommand", "mc", "mycmd")]  // Name + aliases
public class MyCommand : ICommand {
    public string Name => "mycommand";
    public string Description => "Does something useful";
    public string Usage => "mycommand [arg]";

    public CommandResult Execute(string[] args) {
        if (args.Length == 0)
            return CommandResult.Fail("Missing argument");

        var result = DoSomething(args[0]);
        return CommandResult.Ok($"Success: {result}");
    }

    private string DoSomething(string arg) {
        return $"Processed {arg}";
    }
}
```

**Usage in console:**
```
> mycommand test
> mc test          (alias)
> mycmd test       (alias)
```

### Utility Commands

Best for: Static helper methods, debug utilities, system commands. We like to call this "DX" for Designer Interface - where these methods typically wrap internal systems that you have written. You will find that they are easily to integrate into the console and can provide you with powerful utility and debugging in your game.

**Example:**
```csharp
using Spellbound.Core.Console;
using UnityEngine;

public static class GameUtilities {
    
    [ConsoleUtilityCommand("setgravity", "Sets physics gravity")]
    public static void SetGravity(float x, float y, float z) {
        Physics.gravity = new Vector3(x, y, z);
        ConsoleLogger.PrintToConsole($"Gravity set to ({x}, {y}, {z})");
    }

    [ConsoleUtilityCommand("timescale", "Sets time scale")]
    public static void SetTimeScale(float scale = 1f) {
        Time.timeScale = Mathf.Clamp(scale, 0f, 10f);
        ConsoleLogger.PrintToConsole($"Time scale set to {Time.timeScale}");
    }
}
```

**Usage in console:**
```
> setgravity 0 -20 0
> timescale 0.5
> timescale          (uses default: 1)
```

### Preset Commands

Best for: Commands that operate on game objects/presets. This is an advanced feature for games using the ObjectPreset system.

This is a bit more advanced because it builds on our preset object system. However, if you're already using our other packages or building off of this one you will find that it comes naturally because it utilizes the "console module" type that you can add to your object preset scriptable objects. Now you're able to easily spawn, add, or remove items from your gameworld via the console!

**Example:**
```csharp
using Spellbound.Core.Console;
using UnityEngine;

public class GameObjectSpawner : MonoBehaviour {
    
    [ConsolePresetCommand("spawn", typeof(ConsoleModule))]
    public void SpawnObject(string presetUid, Vector3 position, Quaternion rotation, float scale) {
        var preset = presetUid.ResolvePreset();
        
        // Your spawn logic here
        var obj = Instantiate(preset.prefab, position, rotation);
        obj.transform.localScale = Vector3.one * scale;
        
        ConsoleLogger.PrintToConsole($"Spawned {preset.objectName} at {position}");
    }
}
```

**Setup:**
1. Your ObjectPreset must have a `ConsoleModule` attached
2. Set `autoRegister = true` on the ConsoleModule
3. Place the preset in a Resources folder

**Usage in console:**
```
> spawn sword
> spawn sword 5     (spawns 5 swords)
```

---

## Input System Support

The console supports both Unity input systems via preprocessor directives:

### New Input System (Unity 6+)
```csharp
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

void Update() {
    if (Keyboard.current.cKey.wasPressedThisFrame)
        console.ToggleConsole();
}
#endif
```

### Legacy Input Manager
```csharp
#if ENABLE_LEGACY_INPUT_MANAGER
void Update() {
    if (Input.GetKeyDown(KeyCode.C))
        console.ToggleConsole();
}
#endif
```

### Both Systems
```csharp
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DualInputExample : MonoBehaviour {
    [SerializeField] private ConsoleController console;
    
#if ENABLE_INPUT_SYSTEM
    void Update() {
        if (Keyboard.current?.cKey.wasPressedThisFrame ?? false)
            console.ToggleConsole();
    }
#elif ENABLE_LEGACY_INPUT_MANAGER
    void Update() {
        if (Input.GetKeyDown(KeyCode.C))
            console.ToggleConsole();
    }
#endif
}
```

---

## Architecture

### Core Components

```
ConsoleController
├── UI management
├── Command execution coordination
├── Public API for external input
└── Output display

CommandRegistry
├── Manages ICommand implementations
├── Routes commands to appropriate handlers
└── Provides ExecuteCommand() entry point

AttributeCommandRegistry
├── Discovers methods with [ConsoleUtilityCommand]
├── Discovers methods with [ConsolePresetCommand]
├── Handles method invocation and parameter parsing
└── Caches method instances

PresetResolver (Optional - for preset commands)
├── Maps preset names to UIDs
├── Scans Resources for ObjectPresets with ConsoleModule
└── Provides TryResolvePresetUid()

ConsoleCommandRouter (Optional - for preset commands)
├── Routes preset commands to handler methods
├── Checks module type requirements
├── Handles quantity multiplier
└── Provides execution context (position, rotation, etc.)
```

### Command Flow

```
User Input: "help"
     ↓
ConsoleController.OnSubmitInput()
     ↓
CommandRegistry.ExecuteCommand()
     ↓
[Checks ICommand] → Found HelpCommand!
     ↓
HelpCommand.Execute()
     ↓
Returns CommandResult
     ↓
ConsoleController displays output
```

---

## Supported Parameter Types

Utility and preset commands automatically parse these types:

- `string`
- `int`
- `float`
- `double`
- `bool` (supports: true/false, 1/0)
- `byte`
- `Vector3` (consumes 3 args: x y z)
- `Vector2` (consumes 2 args: x y)
- Any `enum` type

**Example:**
```csharp
[ConsoleUtilityCommand("moveobject")]
public static void MoveObject(string name, Vector3 position, bool instant = false) {
    // Called as: moveobject player 10 5 0 true
    //   name = "player"
    //   position = Vector3(10, 5, 0)
    //   instant = true
}
```

### Default Parameters

Methods can use default parameter values:

```csharp
[ConsoleUtilityCommand("damage")]
public static void ApplyDamage(float amount, string damageType = "physical") {
    // damage 50         → amount=50, damageType="physical"
    // damage 50 fire    → amount=50, damageType="fire"
}
```

---

## ConsoleLogger API

Static utility for printing to console from anywhere in your code.

```csharp
// Print messages
ConsoleLogger.PrintToConsole("Info message");
ConsoleLogger.PrintError("Error message");

// Check if initialized
if (ConsoleLogger.IsInitialized) {
    ConsoleLogger.PrintToConsole("Console is ready");
}
```

---

## CommandResult

Result object returned by all command executions.

```csharp
// Success
return CommandResult.Ok();
return CommandResult.Ok("Success message");

// Failure
return CommandResult.Fail("Error message");
```

---

## Best Practices

### 1. Choose the Right Command Type

- **ICommand** for standalone utilities (help, clear, stats display)
- **Utility Commands** for static helper methods (debug toggles, system commands)
- **Preset Commands** for object manipulation (requires ObjectPreset system)

### 2. Keep ConsoleController Pure

Don't add input polling to ConsoleController. Use the example scripts as templates for your own input handling.

```csharp
// ✅ Good - separate input handler
public class MyInputHandler : MonoBehaviour {
    void Update() {
        if (MyInput.ConsoleToggle())
            console.ToggleConsole();
    }
}

// ❌ Bad - modifying ConsoleController
public class ConsoleController : MonoBehaviour {
    void Update() { /* input polling */ }
}
```

### 3. Provide Good Descriptions

```csharp
// ✅ Good
[ConsoleUtilityCommand("setfov", "Sets camera field of view (30-120)")]

// ❌ Bad
[ConsoleUtilityCommand("setfov", "Sets FOV")]
```

### 4. Use Default Parameters for Optional Args

```csharp
// ✅ Good - allows: damage 50  OR  damage 50 fire
public static void ApplyDamage(float amount, string type = "physical")

// ❌ Bad - always requires type
public static void ApplyDamage(float amount, string type)
```

### 5. Validate Input

```csharp
[ConsoleUtilityCommand("setvolume")]
public static void SetVolume(float volume) {
    volume = Mathf.Clamp01(volume);  // ✅ Clamp to valid range
    AudioListener.volume = volume;
}
```

### 6. Subscribe to Visibility Events

```csharp
void Start() {
    console.OnVisibilityChanged += OnConsoleVisibilityChanged;
}

void OnConsoleVisibilityChanged(bool isVisible) {
    if (isVisible) {
        // Disable gameplay input
        playerController.enabled = false;
    } else {
        // Re-enable gameplay input
        playerController.enabled = true;
    }
}
```

---

## Troubleshooting

### "Can't type in the input field"

**Problem:** Input field doesn't respond to typing  

**Solution:**
1. Ensure your scene has an EventSystem
2. Select EventSystem in Hierarchy
3. Click "Replace with InputSystemUIInputModule" (Unity 6+) or "Add Default Input Module" (older Unity)

### "Unknown command" for utility commands

**Problem:** Utility command not found  

**Solution:** Ensure method is `static` and has `[ConsoleUtilityCommand]` attribute

### "No handler found" for preset commands

**Problem:** Preset command not routing  

**Solution:**
1. Check preset has `ConsoleModule` with `autoRegister = true`
2. Verify preset is in a Resources folder
3. Ensure method has `[ConsolePresetCommand]` with correct module type

---

## Example: Creating a Custom Help Command

```csharp
using System.Linq;
using System.Text;
using Spellbound.Core.Console;

[ConsoleCommandClass("audiohelp", "ah")]
public class AudioHelpCommand : ICommand {
    public string Name => "audiohelp";
    public string Description => "Lists all audio-related commands";
    public string Usage => "audiohelp";

    public CommandResult Execute(string[] args) {
        // Get all utility commands from AudioCommands class
        var commands = AttributeCommandRegistry.GetUtilityCommandsByClass(typeof(AudioCommands));

        var sb = new StringBuilder();
        sb.AppendLine("=== Audio Commands ===");
        sb.AppendLine();

        foreach (var (commandName, description) in commands)
            sb.AppendLine($"{commandName,-25} {description}");

        return CommandResult.Ok(sb.ToString());
    }
}
```

---

## Example: Full Custom Integration

Here's a complete example of integrating the console into your game:

```csharp
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Spellbound.Core.Console;

public class GameConsoleManager : MonoBehaviour {
    [SerializeField] private ConsoleController console;
    [SerializeField] private GameObject playerController;
    
    #if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference toggleAction;
    [SerializeField] private InputActionReference historyUpAction;
    [SerializeField] private InputActionReference historyDownAction;
    #endif
    
    private void Awake() {
        console.OnVisibilityChanged += OnConsoleVisibilityChanged;
    }
    
    #if ENABLE_INPUT_SYSTEM
    private void OnEnable() {
        toggleAction.action.performed += console.OnTogglePerformed;
        historyUpAction.action.performed += console.OnHistoryUpPerformed;
        historyDownAction.action.performed += console.OnHistoryDownPerformed;
    }
    
    private void OnDisable() {
        toggleAction.action.performed -= console.OnTogglePerformed;
        historyUpAction.action.performed -= console.OnHistoryUpPerformed;
        historyDownAction.action.performed -= console.OnHistoryDownPerformed;
    }
    #else
    private void Update() {
        if (Input.GetKeyDown(KeyCode.C))
            console.ToggleConsole();
            
        if (console.IsVisible) {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                console.NavigateHistoryUp();
            if (Input.GetKeyDown(KeyCode.DownArrow))
                console.NavigateHistoryDown();
        }
    }
    #endif
    
    private void OnConsoleVisibilityChanged(bool isVisible) {
        // Disable player when console is open
        if (playerController != null)
            playerController.SetActive(!isVisible);
            
        // You could also disable input actions here
        #if ENABLE_INPUT_SYSTEM
        if (isVisible)
            DisableGameplayInput();
        else
            EnableGameplayInput();
        #endif
    }
    
    #if ENABLE_INPUT_SYSTEM
    private void DisableGameplayInput() {
        // Your gameplay input disable logic
    }
    
    private void EnableGameplayInput() {
        // Your gameplay input enable logic
    }
    #endif
}
```

---

## Support

For questions, issues, or feature requests, please contact reach out to us on Discord.

---

## License

Copyright 2025 Spellbound Studio Inc.