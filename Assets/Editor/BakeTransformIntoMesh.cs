using UnityEngine;
using UnityEditor;
using System.IO;

// Editor tool: bakes each selected GameObject's current rotation & scale
// directly into its mesh vertices/normals, recenters the mesh so its
// lowest point sits at local Y = 0 (so it sits ON the terrain instead of
// floating/sinking), then resets that object's Transform (position,
// rotation, scale) to identity — which is what Terrain's Tree Prototype
// system expects.
//
// Usage:
// 1. Put this script in a folder named "Editor" anywhere under Assets
//    (e.g. Assets/Editor/BakeTransformIntoMesh.cs)
// 2. In the Hierarchy, select ALL the tree/rock variants you want fixed
//    at once (click first, Shift+click last, or Ctrl+click individually)
// 3. Menu: Tools > Bake Transform Into Mesh
// 4. Each selected object gets its own new "_Baked" mesh asset saved,
//    and its Transform reset to 0 position / 0 rotation / 1 scale.
//    A summary dialog shows how many were processed.
// 5. Drag each corrected object into your Prefabs folder as usual.
//
// Safe to re-run on objects that were already baked with an older
// version of this script (e.g. before the recenter-to-base fix existed)
// — it just bakes again from their current state.

public class BakeTransformIntoMesh
{
    [MenuItem("Tools/Bake Transform Into Mesh")]
    private static void BakeAll()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            EditorUtility.DisplayDialog("Bake Transform Into Mesh",
                "Select one or more GameObjects with a MeshFilter first " +
                "(you can select all your tree variants together in the " +
                "Hierarchy and run this once).", "OK");
            return;
        }

        int done = 0;
        System.Text.StringBuilder log = new System.Text.StringBuilder();

        foreach (var go in selection)
        {
            string result = BakeOne(go);
            if (result != null)
            {
                done++;
                log.AppendLine($"{go.name}: {result}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Bake Transform Into Mesh",
            $"Baked {done} of {selection.Length} selected object(s).\n\n{log}",
            "OK");
    }

    // Returns the saved asset path on success, or null if this object
    // had no usable MeshFilter and was skipped.
    private static string BakeOne(GameObject go)
    {
        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            return null; // skip silently, e.g. an empty parent got selected too
        }

        Mesh original = mf.sharedMesh;
        Mesh baked = Object.Instantiate(original);
        baked.name = original.name + "_Baked";

        // Bake rotation + scale (not position) into vertices & normals,
        // since we want the object's pivot to stay at local (0,0,0)
        // but its *orientation/size* corrected permanently.
        Quaternion rot = go.transform.localRotation;
        Vector3 scale = go.transform.localScale;

        // Build the full vertex transform matrix (rotation * scale),
        // and the correct normal matrix (inverse-transpose) so normals
        // stay correct even with non-uniform scale.
        Matrix4x4 vertexMatrix = Matrix4x4.Rotate(rot) * Matrix4x4.Scale(scale);
        Matrix4x4 normalMatrix = vertexMatrix.inverse.transpose;

        Vector3[] verts = baked.vertices;
        for (int i = 0; i < verts.Length; i++)
            verts[i] = vertexMatrix.MultiplyPoint3x4(verts[i]);

        // Recenter so the mesh's lowest point sits at local Y = 0 —
        // this is what makes it sit ON the terrain surface instead of
        // floating or sinking, regardless of where the original pivot was.
        float minY = float.MaxValue;
        for (int i = 0; i < verts.Length; i++)
            if (verts[i].y < minY) minY = verts[i].y;
        for (int i = 0; i < verts.Length; i++)
            verts[i].y -= minY;

        baked.vertices = verts;

        Vector3[] normals = baked.normals;
        if (normals != null && normals.Length == verts.Length)
        {
            for (int i = 0; i < normals.Length; i++)
                normals[i] = normalMatrix.MultiplyVector(normals[i]).normalized;
            baked.normals = normals;
        }

        baked.RecalculateBounds();

        // Save the new mesh as its own asset, next to the original mesh
        // if we can locate it, otherwise under Assets/BakedMeshes/.
        string origPath = AssetDatabase.GetAssetPath(original);
        string folder = "Assets/BakedMeshes";
        if (!string.IsNullOrEmpty(origPath))
        {
            string dir = Path.GetDirectoryName(origPath);
            if (!string.IsNullOrEmpty(dir))
                folder = dir;
        }
        if (!AssetDatabase.IsValidFolder(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(
            Path.Combine(folder, baked.name + ".asset"));
        AssetDatabase.CreateAsset(baked, assetPath);
        AssetDatabase.SaveAssets();

        // Point the object at the new baked mesh, then zero out its transform.
        Undo.RecordObject(mf, "Bake Transform Into Mesh");
        mf.sharedMesh = baked;

        Undo.RecordObject(go.transform, "Reset Transform After Bake");
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero; // fully reset so Terrain has no hidden offset

        EditorUtility.SetDirty(mf);

        return assetPath;
    }
}