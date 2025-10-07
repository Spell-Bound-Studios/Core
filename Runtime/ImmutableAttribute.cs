using UnityEditor;
using UnityEngine;

namespace SpellBound.Core {
    public class ImmutableAttribute : PropertyAttribute { }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ImmutableAttribute))]
    public class ImmutableDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var old = GUI.enabled;
            if (old) GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            if (old) GUI.enabled = true;
        }
    }
#endif
}