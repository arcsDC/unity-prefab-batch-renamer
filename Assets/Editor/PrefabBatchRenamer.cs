using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class PrefabBatchRenamer : EditorWindow
{
    private string prefix = "PFX_";
    private string suffix = "";
    private bool applyPrefix = true;
    private bool applySuffix = true;

    [MenuItem("Tools/Prefab Batch Renamer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabBatchRenamer>("Prefab Renamer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Rename selected Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        applyPrefix = EditorGUILayout.Toggle("Apply Prefix", applyPrefix);
        if (applyPrefix)
            prefix = EditorGUILayout.TextField("Prefix", prefix);

        applySuffix = EditorGUILayout.Toggle("Apply Suffix", applySuffix);
        if (applySuffix)
            suffix = EditorGUILayout.TextField("Suffix", suffix);

        EditorGUILayout.Space();

        if (GUILayout.Button("Rename Selected Prefabs"))
        {
            RenameSelectedPrefabs();
        }
    }

    private void RenameSelectedPrefabs()
    {
        var selectedAssets = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        var prefabs = selectedAssets.Where(go => AssetDatabase.GetMainAssetPath(go).EndsWith(".prefab")).ToList();

        if (prefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("No Prefabs", "Select one or more Prefab assets in the Project window.", "OK");
            return;
        }

        var renames = new List<(string oldPath, string newPath)>();
        foreach (var prefab in prefabs)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            string dir = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            string ext = System.IO.Path.GetExtension(path);

            string newName = name;
            if (applyPrefix && !name.StartsWith(prefix))
                newName = prefix + name;
            if (applySuffix && !name.EndsWith(suffix))
                newName = newName + suffix;

            if (newName != name)
            {
                string newPath = System.IO.Path.Combine(dir, newName + ext);
                renames.Add((path, newPath));
            }
        }

        if (renames.Count == 0)
        {
            EditorUtility.DisplayDialog("No Changes", "No prefabs needed renaming.", "OK");
            return;
        }

        foreach (var (oldPath, newPath) in renames)
        {
            AssetDatabase.RenameAsset(oldPath, System.IO.Path.GetFileName(newPath));
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", $"Renamed {renames.Count} prefab(s).", "OK");
    }
}
