using UnityEngine;
using UnityEditor;
using System.IO;

public class BackgroundRemover : EditorWindow
{
    [MenuItem("Tools/Fix White Backgrounds")]
    public static void RemoveWhiteBackgrounds()
    {
        string folderPath = "Assets/Resources/Sprites";
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                // Ensure readable so we can access pixels
                bool wasReadable = importer.isReadable;
                if (!wasReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    if (ProcessTexture(texture, path))
                    {
                        count++;
                    }
                }

                // Restore readability setting if desired, but keeping it readable is often fine for sprites
                // if (!wasReadable) { importer.isReadable = false; importer.SaveAndReimport(); }
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Complete", $"Processed {count} images. White backgrounds removed.", "OK");
    }

    private static bool ProcessTexture(Texture2D source, string path)
    {
        // Create a temporary texture to read/write
        // Note: source might be compressed, so we create a new Texture2D
        RenderTexture tmp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, tmp);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        
        Texture2D newTex = new Texture2D(source.width, source.height);
        newTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        newTex.Apply();
        
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        Color[] pixels = newTex.GetPixels();
        bool modified = false;

        for (int i = 0; i < pixels.Length; i++)
        {
            Color p = pixels[i];
            // Check for strict white or very light grey (artifacting)
            // Using a threshold of 0.85 to catch compressed whites
            if (p.r > 0.85f && p.g > 0.85f && p.b > 0.85f)
            {
                pixels[i] = Color.clear;
                modified = true;
            }
        }

        if (modified)
        {
            newTex.SetPixels(pixels);
            newTex.Apply();

            // Encode and save
            byte[] bytes = newTex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            return true;
        }

        return false;
    }
}
