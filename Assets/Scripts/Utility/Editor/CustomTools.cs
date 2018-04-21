using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;

public class BuildTools
{
	[System.Serializable]
	class BuildInfo
	{
		static string filepath = "Assets/Build.json";

		public int majorVersion = 0;
		public int minorVersion = 1;
		public int androidBuildNumber = 0;

		static public BuildInfo Load()
		{
			BuildInfo info = new BuildInfo();
			var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filepath);
			if (textAsset)
			{
				info = JsonUtility.FromJson<BuildInfo>(textAsset.text);
			}

			return info;
		}

		public static void Save(BuildInfo info)
		{
			var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filepath);
			string buildJson = JsonUtility.ToJson(info, true);
			File.WriteAllText(filepath, buildJson);
			if (textAsset)
			{
				EditorUtility.SetDirty(textAsset);
			}
			else
			{
				AssetDatabase.Refresh();
			}
		}
	}

	[MenuItem("Build/Android")]
	static void Build()
	{
		if(BuildPipeline.isBuildingPlayer)
		{
			Debug.LogError("Failed to Build: Build already in progress!");
			return;
		}

		{
			BuildInfo info = BuildInfo.Load();

			PlayerSettings.bundleVersion = string.Format("{0}.{1}.{2}", info.majorVersion, info.minorVersion, info.androidBuildNumber);
			PlayerSettings.Android.bundleVersionCode = info.majorVersion * 100000 + info.minorVersion * 100 + info.androidBuildNumber;

			info.androidBuildNumber++;

			BuildInfo.Save(info);
		}

		BuildPlayerOptions options = new BuildPlayerOptions();
		//options.locationPathName = Build /
		options.scenes = new[] { "Assets/GameScene.unity" };
		options.locationPathName = "Build/Android/Obelisk.apk";
		options.target = BuildTarget.Android;
		options.options = BuildOptions.StrictMode;
		string errorMsg = BuildPipeline.BuildPlayer(options);
		
		if(errorMsg.Length > 0)
		{
			Debug.LogError(errorMsg);
		}
	}
}
