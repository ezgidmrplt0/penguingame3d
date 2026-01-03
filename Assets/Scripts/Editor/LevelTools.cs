using UnityEngine;
using UnityEditor;

public class LevelTools : EditorWindow
{
    [MenuItem("Tools/Level Generator/Select Generator")]
    public static void SelectGenerator()
    {
        LevelGenerator generator = Object.FindObjectOfType<LevelGenerator>();
        if (generator != null)
        {
            Selection.activeGameObject = generator.gameObject;
            EditorGUIUtility.PingObject(generator.gameObject);
        }
        else
        {
            if (EditorUtility.DisplayDialog("Generator Not Found", "No LevelGenerator found in the scene. Create one?", "Yes", "No"))
            {
                GameObject go = new GameObject("LevelGenerator");
                go.AddComponent<LevelGenerator>();
                Selection.activeGameObject = go;
            }
        }
    }

    [MenuItem("Tools/Level Generator/Generate New Level")]
    public static void GenerateLevel()
    {
        LevelGenerator generator = Object.FindObjectOfType<LevelGenerator>();
        if (generator != null)
        {
            generator.BuildLevel();
            Debug.Log("Level Generated via Tools Menu!");
        }
        else
        {
            SelectGenerator(); // Prompt to create
        }
    }
}
