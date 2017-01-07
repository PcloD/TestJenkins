using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class BuildTool : ScriptableObject 
{
	private static string buildNumber = "0";
    private static string gameName = "TestJenkins";
	private static string buildsPath = ".";
	
	static string[] GetBuildScenes()
	{
		List<string> names = new List<string>();
		
		foreach(EditorBuildSettingsScene e in EditorBuildSettings.scenes)
		{
			if(e==null)
				continue;
			
			if(e.enabled)
				names.Add(e.path);
		}
		return names.ToArray();
	}
	
	static string GetBuildPathAndroid()
	{
        return string.Format(buildsPath + "/{0}_{1}.apk", gameName, DateTime.Now.ToString("yyyyMMddhhmm"));
	}

	static void SetupAndroid()
	{
		Dictionary<string, Action<string>> argHandlers = new Dictionary<string, Action<string>>
		{
			{"-dest", delegate(string value)
				{
					buildsPath = value;
				}
			}
		};

		string[] cmdArgs = Environment.GetCommandLineArgs();
		Action<string> handler;
		for(int i = 0;i < cmdArgs.Length; i++)
		{
			if(argHandlers.ContainsKey(cmdArgs[i]))
			{
				handler = argHandlers[cmdArgs[i]];
				handler(cmdArgs[i + 1]);
			}
		}

		buildNumber = (PlayerSettings.Android.bundleVersionCode + 1).ToString();
	}


	[MenuItem("TEDTools/DefineSymbol/Develop")]
	static void SetDefineSymbolDevelop()
	{
		EditorPrefs.SetBool("develop", true);
		PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android, "TED_DEV");
	}


	[MenuItem("TEDTools/DefineSymbol/Develop", true)]
	static bool SetDefineSymbolDevelopValidate()
	{
		Menu.SetChecked("TEDTools/DefineSymbol/Develop", EditorPrefs.GetBool("develop", true));
		Menu.SetChecked("TEDTools/DefineSymbol/Release", !EditorPrefs.GetBool("develop", true));
		return !EditorPrefs.GetBool("develop", true);
	}


	[MenuItem("TEDTools/DefineSymbol/Release")]
	static void SetDefineSymbolRelease()
	{
		EditorPrefs.SetBool("develop", false);
		PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android, "");
	}


	[MenuItem("TEDTools/DefineSymbol/Release", true)]
	static bool SetDefineSymbolReleaseValidate()
	{
		Menu.SetChecked("TEDTools/DefineSymbol/Develop", EditorPrefs.GetBool("develop", true));
		Menu.SetChecked("TEDTools/DefineSymbol/Release", !EditorPrefs.GetBool("develop", true));
		return EditorPrefs.GetBool("develop", true);
	}
	
	
	[MenuItem("TEDTools/Build/Build Android Develop")]
	static void BuildAndroidDevelop ()
	{
		SetDefineSymbolDevelop();

		SetupAndroid();
		string[] scenes = GetBuildScenes();
		string path = GetBuildPathAndroid();
		
		if(scenes == null || scenes.Length == 0 || path == null)
		{
			return;
		}
		
		AssetDatabase.Refresh();
		BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, BuildOptions.Development | BuildOptions.AllowDebugging);
	}

	[MenuItem("TEDTools/Build/Build Android Release")]
	static void BuildAndroidRelease ()
	{
		SetDefineSymbolRelease();

		SetupAndroid();
		string[] scenes = GetBuildScenes();
		string path = GetBuildPathAndroid();
		
		if(scenes == null || scenes.Length == 0 || path == null)
		{
			return;
		}
		
		AssetDatabase.Refresh();
		BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, BuildOptions.None);
	}
}