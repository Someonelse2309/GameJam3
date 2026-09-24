using UnityEngine;
using UnityEditor;

public class FixAudioImportSettings
{
    [MenuItem("Tools/Fix Audio Import Settings")]
    public static void FixAllAudio()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
        int changed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) continue;

            SerializedObject so = new SerializedObject(importer);
            SerializedProperty defaultSettings = so.FindProperty("m_DefaultSettings");

            SerializedProperty loadType = defaultSettings.FindPropertyRelative("m_LoadType");

            bool modified = false;

            // Change Decompress on Load (0) -> Streaming (2) to reduce memory usage
            if (loadType != null && loadType.intValue == 0)
            {
                loadType.intValue = 2;
                modified = true;
            }

            if (modified)
            {
                so.ApplyModifiedProperties();
                changed++;
                Debug.Log($"[FixAudio] Updated: {path}");
            }
        }

        Debug.Log($"[FixAudio] Done. Changed {changed} audio files.");
        EditorUtility.DisplayDialog("FixAudio", $"Updated {changed} audio files.\nLoad Type set to Streaming.", "OK");
    }
}
