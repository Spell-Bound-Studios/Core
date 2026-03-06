// Copyright 2025 Spellbound Studio Inc.

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Core {
    [CustomEditor(typeof(ObjectPreset))]
    public sealed class ObjectPresetEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, 
                "damageableModule", "interactableModule", "mouseoverModule");

            EditorGUILayout.Space();
            DrawModuleField<DamageableModule>("damageableModule", "Damageable Module");
            DrawModuleField<InteractableModule>("interactableModule", "Interactable Module");
            DrawModuleField<MouseoverModule>("mouseoverModule", "Mouseover Module");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawModuleField<T>(string propertyName, string label) where T : class {
            var prop = serializedObject.FindProperty(propertyName);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            // Assign button — opens dropdown of concrete subtypes
            if (GUILayout.Button(prop.managedReferenceValue == null ? "Assign" : "Change", 
                    GUILayout.Width(60))) {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("None"), prop.managedReferenceValue == null, () => {
                    prop.managedReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                });

                foreach (var type in TypeCache.GetTypesDerivedFrom<T>()) {
                    if (type.IsAbstract) continue;

                    var captured = type;
                    var isCurrent = prop.managedReferenceValue?.GetType() == type;

                    menu.AddItem(new GUIContent(type.Name), isCurrent, () => {
                        prop.managedReferenceValue = Activator.CreateInstance(captured);
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            if (prop.managedReferenceValue != null)
                EditorGUILayout.PropertyField(prop, GUIContent.none, true);
            else
                EditorGUILayout.LabelField("None", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
    }
}
#endif