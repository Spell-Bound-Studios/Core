// Copyright 2026 Spellbound Studio Inc.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Spellbound.Core.Modules;
using Spellbound.Core.Objects;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Core {
    [CustomEditor(typeof(ObjectPreset))]
    public sealed class ObjectPresetEditor : Editor {
        private SerializedProperty _surfacesProp;

        // #####################################################
        // RENAME THESE IF YOU RENAME A FIELD - THEY MUST MATCH!
        // #####################################################
        private const string FieldNameOnObjectPreset = "surfaceModules";
        private const string FieldNameOnPresetSurfaceList = "presetModules";
        private const string FieldNameOnPresetSurfaceName = "surfaceName";

        private void OnEnable() => _surfacesProp = serializedObject.FindProperty(FieldNameOnObjectPreset);

        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, FieldNameOnObjectPreset);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Surface Modules", EditorStyles.boldLabel);

            for (var i = 0; i < _surfacesProp.arraySize; i++) {
                var surfaceProp = _surfacesProp.GetArrayElementAtIndex(i);
                var nameProp = surfaceProp.FindPropertyRelative(FieldNameOnPresetSurfaceName);
                var modulesProp = surfaceProp.FindPropertyRelative(FieldNameOnPresetSurfaceList);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(nameProp, new GUIContent($"Surface {i}"));

                if (GUILayout.Button("-", GUILayout.Width(25))) {
                    _surfacesProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();

                    return;
                }

                EditorGUILayout.EndHorizontal();

                DrawModules(modulesProp);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            if (GUILayout.Button("Add Surface")) {
                _surfacesProp.InsertArrayElementAtIndex(_surfacesProp.arraySize);
                var newSurface = _surfacesProp.GetArrayElementAtIndex(_surfacesProp.arraySize - 1);
                newSurface.FindPropertyRelative(FieldNameOnPresetSurfaceName).stringValue = "New Surface";
                newSurface.FindPropertyRelative(FieldNameOnPresetSurfaceList).ClearArray();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawModules(SerializedProperty modulesProp) {
            EditorGUI.indentLevel++;

            for (var i = 0; i < modulesProp.arraySize; i++) {
                var p = modulesProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();
                var label = new GUIContent(GetNiceTypeName(p));
                EditorGUILayout.PropertyField(p, label, true);

                if (GUILayout.Button("-", GUILayout.Width(25))) {
                    modulesProp.DeleteArrayElementAtIndex(i);

                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            var buttonRect = EditorGUILayout.GetControlRect();

            if (EditorGUI.DropdownButton(buttonRect, new GUIContent("Add Module"), FocusType.Passive))
                ShowModuleDropdown(buttonRect, modulesProp);

            EditorGUI.indentLevel--;
        }

        private void ShowModuleDropdown(Rect buttonRect, SerializedProperty modulesProp) {
            var existing = new HashSet<Type>();

            for (var i = 0; i < modulesProp.arraySize; i++) {
                var el = modulesProp.GetArrayElementAtIndex(i).managedReferenceValue;

                if (el != null)
                    existing.Add(el.GetType());
            }

            var menu = new GenericMenu();

            foreach (var type in TypeCache.GetTypesDerivedFrom<PresetModule>()) {
                if (type.IsAbstract)
                    continue;

                var label = new GUIContent(type.Name);

                if (existing.Contains(type))
                    menu.AddDisabledItem(label);
                else {
                    menu.AddItem(label, false, () => {
                        modulesProp.arraySize++;
                        var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                        element.managedReferenceValue = Activator.CreateInstance(type);
                        serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            menu.DropDown(buttonRect);
        }

        private static string GetNiceTypeName(SerializedProperty prop) {
            if (prop.managedReferenceValue == null)
                return "Missing";

            var full = prop.managedReferenceFullTypename;

            var lastSpace = full.LastIndexOf(' ');

            if (lastSpace >= 0)
                full = full[(lastSpace + 1)..];

            var lastDot = full.LastIndexOf('.');

            return lastDot >= 0
                    ? full[(lastDot + 1)..]
                    : full;
        }
    }
}
#endif