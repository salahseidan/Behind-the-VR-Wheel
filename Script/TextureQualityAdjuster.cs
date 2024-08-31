// using UnityEditor;
// using UnityEngine;

// public class TextureQualityAdjuster : MonoBehaviour
// {
//     [MenuItem("Tools/Adjust Texture Quality/To medium Quality")]
//     static void SetTexturesToHighQuality()
//     {
//         // Find all textures in the project
//         string[] textureGUIDs = AssetDatabase.FindAssets("t:Texture");
//         foreach (string guid in textureGUIDs)
//         {
//             string assetPath = AssetDatabase.GUIDToAssetPath(guid);
//             TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

//             if (textureImporter != null)
//             {
//                 // Set texture compression quality here
//                 textureImporter.textureCompression = TextureImporterCompression.Compressed;
                
//                 // Apply changes
//                 AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
//             }
//         }
//         Debug.Log("All textures have been set to high quality.");
//     }
// }