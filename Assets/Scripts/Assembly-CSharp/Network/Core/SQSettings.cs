using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;

public class SQSettings
{
	private static string serverUrl;

	private static string localDataRootUrl;

	private static string photonChatAppID;

	private static string photonPUNAppID;

	private static int saveInterval;

	private static string bundleIdentifier;

	private static string bundleVersion;

	// Backward compatibility: legacy bootstrap still writes this flag.
	public static bool useLocalServer;

	public static string CDN_URL
	{
		get
		{
			return localDataRootUrl;
		}
	}

	public static string SERVER_URL
	{
		get
		{
			return serverUrl;
		}
	}

	public static string PHOTON_CHAT_APP_ID
	{
		get
		{
			return photonChatAppID;
		}
	}

	public static string PHOTON_PUN_APP_ID
	{
		get
		{ 
			return photonPUNAppID;
		}
	}

	public static string SERVER_PREFIX
	{
		get
		{
			string[] array = serverUrl.Trim().Split(new string[2] { "://", "." }, StringSplitOptions.None);
			if (array.Length > 1)
			{
				return array[1];
			}
			return string.Empty;
		}
	}

	public static int SAVE_INTERVAL
	{
		get
		{
			return saveInterval;
		}
	}

	public static string BundleIdentifier
	{
		get
		{
			return bundleIdentifier;
		}
	}

	public static string BundleVersion
	{
		get
		{
			return bundleVersion;
		}
	}

	private SQSettings()
	{
	}

	public static string getJsonPath(string filePath)
	{
		WWW wWW = null;
		wWW = new WWW(filePath);
		while (!wWW.isDone)
		{
		}
		return wWW.text;
	}

	public static void Init()
	{
		Debug.Log("Entering SQSettings Init()");
		bundleIdentifier = Application.identifier;
		string empty = string.Empty;
		string streamingAssetsFile = TFUtils.GetStreamingAssetsFile("server_settings.json");
		empty = ((!streamingAssetsFile.Contains("://")) ? File.ReadAllText(streamingAssetsFile) : getJsonPath(streamingAssetsFile));
		Dictionary<string, object> dictionary = LocalJsonUtils.DeserializeDictionary(empty);
		serverUrl = (string)dictionary["server_url"];
		photonChatAppID = (string)dictionary["photon_chat_app_id"];
		photonPUNAppID = (string)dictionary["photon_pun_app_id"];
		streamingAssetsFile = TFUtils.GetStreamingAssetsFile("global_settings.json");
		empty = ((!streamingAssetsFile.Contains("://")) ? File.ReadAllText(streamingAssetsFile) : getJsonPath(streamingAssetsFile));
		dictionary = LocalJsonUtils.DeserializeDictionary(empty);
		saveInterval = TFUtils.LoadInt(dictionary, "save_interval");
		localDataRootUrl = BuildLocalDataRootUrl();
		Debug.Log("SQSettings: local data root URL = " + localDataRootUrl);
	}

	private static string BuildLocalDataRootUrl()
	{
		string text = TFUtils.GetStreamingAssetsPath();
		text = text.Replace("\\", "/").TrimEnd('/');
		if (!text.Contains("://"))
		{
			text = "file:///" + text;
		}
		return text + "/";
	}
}
