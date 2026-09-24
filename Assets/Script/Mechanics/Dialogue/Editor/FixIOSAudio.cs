using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class FixIOSAudio
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict rootDict = plist.root;
        // Memastikan kategori audio session mengizinkan playback penuh di iOS
        rootDict.SetString("AVAudioSessionCategory", "AVAudioSessionCategoryPlayback");

        File.WriteAllText(plistPath, plist.WriteToString());
    }
}