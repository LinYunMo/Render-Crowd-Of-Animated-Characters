using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveToPng
{
    
    [MenuItem("Assets/Build PNG")]
    static void SavePNG()
    {
        string assetBundleDirectory = "Assets/PNG/NewOld";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        string relativePath = "Assets/AnimMapBaker/Test/Monster_Troop_1_anim";
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] {relativePath});
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string filepath =  assetBundleDirectory + "/" + Path.GetFileNameWithoutExtension(assetPath) + ".png";
            var texture = (AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath));
            var pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filepath, pngData);
        }
    }
}
