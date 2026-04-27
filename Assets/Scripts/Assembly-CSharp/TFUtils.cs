#define ASSERTS_ON
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEngine;

public class TFUtils
{

	public static string DeviceID;

	public static string FacebookID;

	private static TimeSpan serverTimeDiff = new TimeSpan(0L);

	public static bool AmazonDevice { get; private set; }

	public static DateTime ServerTime
	{
		get
		{
			return DateTime.UtcNow + serverTimeDiff;
		}
	}

	public static void Init(string fbid)
	{
		DeviceID = Guid.NewGuid().ToString().Replace("-", string.Empty);
		FacebookID = ((fbid != null) ? fbid : DeviceID);
        UnityEngine.Debug.Log("This device is:" + DeviceID + " / Player ID is:" + FacebookID);
		AmazonDevice = false;
	}

	public static void UpdateServerTime(DateTime serverTime)
	{
		TimeSpan timeSpan = serverTime.Subtract(DateTime.UtcNow);
		double num = Math.Abs((timeSpan - serverTimeDiff).TotalSeconds);
		if (num > 10.0)
		{
			serverTimeDiff = timeSpan;
            UnityEngine.Debug.Log("Server time difference = " + timeSpan.TotalSeconds);
		}
	}

	public static TimeSpan GetServerTimeDiff()
	{
		return serverTimeDiff;
	}

	private static T AssertCast<T>(Dictionary<string, object> dict, string key)
	{
		return (T)dict[key];
	}
	
	public static string LoadString(Dictionary<string, object> data, string key, string defaultValue)
	{
		string text = TryLoadString(data, key);
		if (text == null || text == string.Empty)
		{
			text = defaultValue;
		}
		return text;
	}

	public static string LoadString(Dictionary<string, object> data, string key)
	{
		return AssertCast<string>(data, key);
	}

	public static string TryLoadString(Dictionary<string, object> data, string key)
	{
		if (data.ContainsKey(key))
		{
			return AssertCast<string>(data, key);
		}
		return null;
	}

	public static string LoadLocalizedString(Dictionary<string, object> data, string key, string defaultValue)
	{
		return KFFLocalization.Get(LoadString(data, key, defaultValue));
	}

	public static string LoadLocalizedString(Dictionary<string, object> data, string key)
	{
		return KFFLocalization.Get(LoadString(data, key));
	}
	
	public static bool LoadBool(Dictionary<string, object> d, string key, bool defaultValue)
	{
		//Discarded unreachable code: IL_0095
		bool result = defaultValue;
		if (d.ContainsKey(key))
		{
			object obj = d[key];
			if (obj is int)
			{
				result = (((int)obj != 0) ? true : false);
			}
			else if (obj is string)
			{
				try
				{
					return bool.Parse((string)obj);
				}
				catch (Exception)
				{
					if ((string)obj == "0")
					{
						return false;
					}
					if ((string)obj == "1")
					{
						return true;
					}
					return defaultValue;
				}
			}
		}
		return result;
	}


	public static int LoadInt(Dictionary<string, object> d, string key)
	{
		return LoadIntHelper(d, key);
	}

	public static int LoadInt(Dictionary<string, object> d, string key, int defaultValue)
	{
		int result = defaultValue;
		if (d.ContainsKey(key))
		{
			object obj = d[key];
			if (!(obj is string) || ((string)obj).Length > 0)
			{
				result = (int)Math.Floor(Convert.ToSingle(obj, CultureInfo.InvariantCulture) + 0.5f);
			}
		}
		return result;
	}

	private static int LoadIntHelper(Dictionary<string, object> d, string key)
	{
		return (int)Math.Floor(Convert.ToSingle(d[key], CultureInfo.InvariantCulture) + 0.5f);
	}

	public static uint LoadUint(Dictionary<string, object> d, string key, uint defaultValue)
	{
		uint result = defaultValue;
		if (d.ContainsKey(key))
		{
			object value = d[key];
			result = Convert.ToUInt32(value, CultureInfo.InvariantCulture);
		}
		return result;
	}

	public static float LoadFloat(Dictionary<string, object> d, string key, float defaultValue)
	{
		float result = defaultValue;
		if (d.ContainsKey(key))
		{
			object obj = d[key];
			if (!(obj is string) || ((string)obj).Length > 0)
			{
				result = Convert.ToSingle(obj, CultureInfo.InvariantCulture);
			}
		}
		return result;
	}
	
	public static string GetPersistentAssetsPath()
	{
		return Path.Combine(Application.persistentDataPath, "Contents");
	}

	public static string GetStreamingAssetsPath()
	{
		return Application.streamingAssetsPath;
	}
	
	public static string GetStreamingAssetsFile(string fileName)
	{
		string text = GetPersistentAssetsPath() + Path.DirectorySeparatorChar + fileName;
		if (File.Exists(text))
		{
			return text;
		}
		return GetStreamingAssetsPath() + Path.DirectorySeparatorChar + fileName;
	}

	public static string GetJsonFileContent(string filename)
	{
		string streamingAssetsFile = GetStreamingAssetsFile(filename);
		if (streamingAssetsFile.Contains("://"))
		{
			return GetAndroidFileContents(streamingAssetsFile);
		}
		return File.ReadAllText(streamingAssetsFile);
	}

	private static string GetAndroidFileContents(string filePath)
	{
		WWW wWW = null;
		wWW = new WWW(filePath);
		while (!wWW.isDone)
		{
		}
		return wWW.text;
	}

	[Conditional("ASSERTS_ON")]
	public static void Assert(bool condition)
	{
		if (!condition)
		{
			throw new Exception(condition.ToString());
		}
	}
	
	public static void WriteFile(string filename, string data)
	{
		File.WriteAllText(filename, data);
	}

	public static string ReadFile(string filename)
	{
		return File.ReadAllText(filename);
	}
	
}
