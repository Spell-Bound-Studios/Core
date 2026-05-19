// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Logging;
using Spellbound.Core.ObjectHandling;
using Spellbound.Core.Objects;
using Spellbound.Core.PresetContracts;

namespace Spellbound.Core.ObjectData {
    public static class DecodableDataExtensions {
        public static IDecodableData GetDefaultData<T>(
            this T data, ObjectPreset preset, int surfaceIndex, byte level = 1)
                where T : IDecodableData {
            if (preset.TryGetModule<IDefaultDataProvider<T>>(out var provider, surfaceIndex))
                return provider.GetDefaultData(preset, level);

            return data.GetEmptyData();
        }

        public static T ApplyDelta<T>(
            this T data, T delta, ObjectPreset preset, int surfaceIndex, out byte context)
                where T : IDecodableData {
            if (!preset.TryGetModule<IApplyDelta<T>>(out var module, surfaceIndex)) {
                context = 0;
                Log.Debug(
                    $"In static ApplyDelta, no IApplyDelta<{typeof(T).Name}> module on surfaceIndex " +
                    $"{surfaceIndex}.");

                return data;
            }

            return module.ApplyDelta(data, delta, preset, surfaceIndex, out context);
        }

        public static void ChangeCallback<T>(
            this T data, byte context, ObjectParent parent,
            int instanceIndex, ObjectPreset preset, int surfaceIndex, TransformData transformData)
                where T : IDecodableData {
            if (!preset.TryGetModules<IChangeHandler<T>>(out var modules, surfaceIndex))
                return;

            foreach (var module in modules)
                module.HandleChange(data, context, parent, instanceIndex, transformData);
        }

        public static void ResolveCallback<T>(
            this T data, byte context, ObjectParent parent,
            int instanceIndex, ObjectPreset preset, int surfaceIndex, TransformData transformData)
                where T : IDecodableData {
            if (!preset.TryGetModules<IChangeResolver<T>>(out var modules, surfaceIndex))
                return;

            foreach (var module in modules)
                module.ResolveChange(data, context, parent, instanceIndex, transformData);
        }
    }
}