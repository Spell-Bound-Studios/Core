// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Spellbound.Core {
    [CustomEditor(typeof(ObjectPreset))]
    public sealed class ObjectPresetEditor : Editor {
        private ReorderableList _modulesList;

        private void OnEnable() {
            var modulesProp = serializedObject.FindProperty("modules");

            _modulesList = new ReorderableList(
                serializedObject,
                modulesProp,
                true,
                true,
                true,
                true) {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Preset Modules"),
                elementHeightCallback = index => {
                    var p = modulesProp.GetArrayElementAtIndex(index);
                    var h = EditorGUI.GetPropertyHeight(p, true);
                    var min = EditorGUIUtility.singleLineHeight + 4f;

                    return Mathf.Max(h, min);
                },

                drawElementCallback = (rect, index, _, _) => {
                    var p = modulesProp.GetArrayElementAtIndex(index);

                    string GetNiceTypeName(SerializedProperty prop) {
                        if (prop.managedReferenceValue == null)
                            return "Missing";

                        var full = prop.managedReferenceFullTypename;
                        var lastSpace = full.LastIndexOf(' ');

                        if (lastSpace >= 0)
                            full = full[(lastSpace + 1)..];
                        var lastDot = full.LastIndexOf('.');

                        return lastDot >= 0 ? full[(lastDot + 1)..] : full;
                    }

                    var label = new GUIContent(GetNiceTypeName(p));
                    EditorGUI.PropertyField(rect, p, label, true);
                },

                onAddDropdownCallback = (buttonRect, list) => {
                    var existing = new HashSet<Type>();

                    for (var i = 0; i < list.serializedProperty.arraySize; i++) {
                        var el = list.serializedProperty.GetArrayElementAtIndex(i)
                                .managedReferenceValue;
                        if (el != null) existing.Add(el.GetType());
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
                                list.serializedProperty.arraySize++;

                                var element = list.serializedProperty
                                        .GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                                element.managedReferenceValue = Activator.CreateInstance(type);
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                    }

                    menu.DropDown(buttonRect);
                }
            };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "modules");
            _modulesList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif