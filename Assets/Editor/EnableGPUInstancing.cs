using UnityEngine;
using UnityEditor;

public class EnableGPUInstancing
{
    [MenuItem("Tools/Enable GPU Instancing On All Materials")]
    static void EnableAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && !mat.enableInstancing)
            {
                mat.enableInstancing = true;
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"GPU Instancing enabled on {count} materials.");
    }
}