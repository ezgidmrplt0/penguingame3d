using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector (variables, etc.)
        DrawDefaultInspector();

        LevelGenerator generator = (LevelGenerator)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Level Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Level", GUILayout.Height(40)))
        {
            generator.BuildLevel();
            // Mark scene as dirty so changes can be saved
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Clear Level", GUILayout.Height(30)))
        {
            generator.ClearLevel();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
        }
    }
}
