## [1.1.4] - 8/1/2026

- Log sinks can now be unregistered with `Log.RemoveSink` or scoped to a `using` block with `Log.AddScopedSink`.
- `EntityPrefabRegistryAuthoring` takes a `PresetBakeManifest`, letting the prefab registry rebake when presets are added, removed, or edited.
- `TransformData.RotAsQuaternion` rebuilds rotation in XYZ euler order, matching how the constructors capture it. Composed rotations no longer come back skewed.
- `RecordingLogSink` captures log entries in memory for assertions, and sink discovery now ignores non-public types so test sinks no longer appear in the Log Config inspector.
- `ResourceRegistry<TEntry>` discovers registry entries under a Resources folder and adds the lazy load, name index, and per-entry validation that every registry was hand-rolling. `PresetRegistry` now sits on it; a failed load clears the registry and reports again on the next access instead of leaving it half populated.

## [2.0.0] - 4/18/2025

### Second Release

#### Major overhaul of all sections belonging to Core. The main focus for this 2.0.0 release was to manage integration across all new libraries and the necessary abstractions needed for each of them. Core remains a UnityEngine-only dependency and should be installed alongside any and all Spellbound Packages.

- Console Added
- Logger Added
- Modules Added
- Object Handling Added
- Objects Added
- Packing Added
- Tooling Added
- Editor Scripts and Tooling Added

## [1.0.0] - 4/11/2025

### First Release

- Package to git
- Package import into other Unity projects