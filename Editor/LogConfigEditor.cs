// Copyright 2026 Spellbound Studio Inc.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Spellbound.Core.Logging.Editor {
    [CustomEditor(typeof(LogConfig))]
    public class LogConfigEditor : UnityEditor.Editor {
        private const string DefinePrefix = "SPELLBOUND_LOG_";
        private const string HeaderLabel = "Spellbound Log Configuration";
        private const string GlobalLabel = "Global Level";
        private const string UndoLabel = "Change Global Log Level";
        private const string ApplyLabel = "Apply";
        private const string HelpText = "Sets the minimum log level for all Spellbound packages. " +
                                        "Click Apply to update scripting defines and trigger recompilation.";

        public override void OnInspectorGUI() {
            var config = (LogConfig)target;

            EditorGUILayout.LabelField(HeaderLabel, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(HelpText, MessageType.Info);
            EditorGUILayout.Space();

            var newLevel = (LogLevel)EditorGUILayout.EnumPopup(GlobalLabel, config.globalLevel);
            if (newLevel != config.globalLevel) {
                Undo.RecordObject(config, UndoLabel);
                config.globalLevel = newLevel;
            }

            EditorGUILayout.Space();

            if (!GUILayout.Button(ApplyLabel))
                return;

            EditorUtility.SetDirty(config);
            ApplyDefines(config);
        }

        private static void ApplyDefines(LogConfig config) {
            var defines = new HashSet<string>();
            AddDefinesForLevel(defines, config.globalLevel);

            var target = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup
            );

            PlayerSettings.GetScriptingDefineSymbols(target, out var current);
            var preserved = current.Where(d => !d.StartsWith(DefinePrefix)).ToList();
            preserved.AddRange(defines);

            PlayerSettings.SetScriptingDefineSymbols(target, preserved.ToArray());
            Debug.Log($"[LogConfig] Applied log level: {config.globalLevel}");
        }

        private static void AddDefinesForLevel(HashSet<string> defines, LogLevel level) {
            switch (level) {
                // Verbose level prints absolutely everything.
                case LogLevel.Verbose:
                    defines.Add("SPELLBOUND_LOG_VERBOSE");
                    defines.Add("SPELLBOUND_LOG_DEBUG");
                    defines.Add("SPELLBOUND_LOG_INFO");
                    defines.Add("SPELLBOUND_LOG_WARNING");
                    break;
                case LogLevel.Debug:
                    defines.Add("SPELLBOUND_LOG_DEBUG");
                    defines.Add("SPELLBOUND_LOG_INFO");
                    defines.Add("SPELLBOUND_LOG_WARNING");
                    break;
                case LogLevel.Info:
                    defines.Add("SPELLBOUND_LOG_INFO");
                    defines.Add("SPELLBOUND_LOG_WARNING");
                    break;
                case LogLevel.Warning:
                    defines.Add("SPELLBOUND_LOG_WARNING");
                    break;
                case LogLevel.Error:
                case LogLevel.None:
                    break;
                default:
                    Debug.LogError($"[LogConfigEditor] Unhandled log level: {level}");
                    break;
            }
        }
    }
}
#endif