using ClubGamerZone.TowerDefense.Features.Gameplay;
using UnityEditor;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Editor
{
    [CustomEditor(typeof(TowerDefinitionAsset))]
    public sealed class TowerDefinitionAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "_levelDisplayNames");

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Player-Facing Level Names", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These names appear after upgrades and merges. Level 1 can remain the base class name; later levels can be names such as Sorcerer 2 or Archmage.",
                MessageType.Info);

            var names = serializedObject.FindProperty("_levelDisplayNames");
            var maxLevel = Mathf.Max(1, serializedObject.FindProperty("_maxLevel").intValue);
            if (names.arraySize != maxLevel && GUILayout.Button($"Create {maxLevel} Level Name Fields"))
            {
                names.arraySize = maxLevel;
                FillMissingDefaults(names);
            }

            for (var i = 0; i < names.arraySize; i++)
            {
                EditorGUILayout.PropertyField(names.GetArrayElementAtIndex(i), new GUIContent($"Level {i + 1} Name"));
            }

            if (GUILayout.Button("Fill Missing Names From Base Name"))
            {
                FillMissingDefaults(names);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void FillMissingDefaults(SerializedProperty names)
        {
            var baseName = serializedObject.FindProperty("_displayNameKey").stringValue;
            for (var i = 0; i < names.arraySize; i++)
            {
                var element = names.GetArrayElementAtIndex(i);
                if (string.IsNullOrWhiteSpace(element.stringValue))
                {
                    element.stringValue = i == 0 ? baseName : $"{baseName} {i + 1}";
                }
            }
        }
    }
}
