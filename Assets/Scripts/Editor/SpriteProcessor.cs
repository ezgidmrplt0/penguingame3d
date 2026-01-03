using UnityEngine;
using UnityEditor;

public class SpriteProcessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Check if the asset is in the specific folder we created
        if (assetPath.Contains("Resources/Sprites"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true; // Assumes alpha channel or handles colors
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            
            // Optional: Remove white background if the generator made it white
            // This is tricky without custom shader or texture processing, 
            // but setting alphaIsTransparency is a good start. 
            // Often AI images are just opaque PNGs. 
            // We might default to standard settings.
        }
    }
}
