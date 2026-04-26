# Spellbound Logging

A compile-time stripped, sink-based logging system for Unity. Log calls are removed from the binary entirely when disabled — zero IL, zero cost. When enabled, output routes to any combination of sinks: Unity Console, file, in-game console, or your own.

## Setup

1. Create the config asset: **Create > Spellbound > Log Config**.
2. Place it in any `Resources/` folder as `LogConfig`.
3. Select a log level, enable your sinks, and click **Apply**.

## Usage

```csharp
Log.Info("Player connected successfully.");
Log.Warn($"No scriptable object found - fallback to default");
Log.Error($"Failed to load chunk at {coord}");
Log.Debug($"Tick {tick}: processing {entityCount} entities");
```

Output is in the form of Source file, method name, and line number:

```
[2026-04-17 14:32:01.123] [Error] [ChunkManager.AddChunk:84] Failed to load chunk at 0, 1, 0.
```

## Log Levels

| Level | Behavior |
|-------|----------|
| **Verbose** | Compiles everything. No `Log.Verbose()` method exists — this is a config-only setting. |
| **Debug** | Compiles Debug, Info, Warning, Error. |
| **Info** | Compiles Info, Warning, Error. |
| **Warning** | Compiles Warning, Error. |
| **Error** | Error only. All other calls stripped. Error can never be stripped. |
| **None** | Equivalent to Error. |

## How Stripping Works

Each method on `Log` is decorated with `[Conditional("SPELLBOUND_LOG_X")]`. When the symbol is absent, the C# compiler removes the call — including argument evaluation and string interpolation — from the IL. The Apply button in the config inspector writes the appropriate symbols via `PlayerSettings.SetScriptingDefineSymbols`.

## Per-Sink Filtering

Each sink has its own filter level dropdown in the config. This is a runtime gate layered on top of the compile-time gate.

Example: global set to **Debug**, File sink filtered to **Debug**, Unity Console filtered to **Warning**, Game Console filtered to **Error**. All calls compile, but each sink only receives what you want it to.

## Built-In Sinks

- **Unity Console** — routes to `Debug.Log` / `LogWarning` / `LogError`.
- **File** — writes to `Application.persistentDataPath` on a background thread. File rotation keeps the last 3 sessions.
- **Game Console** — routes to `ConsoleLogger.PrintToConsole` / `PrintError`.

## Creating a Custom Sink

Implement `ILogSink` with a parameterless constructor. It will be discovered automatically via reflection and appear in the config inspector.

```csharp
public class TelemetrySink : ILogSink {
    public string DisplayName => "Example";

    public void Initialize(LogConfig config) {
        // Your initialization
    }

    public void Emit(LogLevel level, string source, string message, string member, int line) {
        // Your example code
    }
}
```

Sinks with external dependencies that require constructor arguments should skip the parameterless constructor. They won't appear in the config inspector — register them manually in your own bootstrap:

```csharp
Log.AddSink(new MyComplexSink(apiKey, endpoint), config, LogLevel.Warning);
```