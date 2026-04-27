#define ASSERTS_ON
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MiniJSON;
using UnityEngine;

public class Game
{
	private const string GAME_FILE = "game.json";

	private string gameFile;

	private string myfriendslist;

	private string myexplorerslist;

	private string myfriendrequests;

	private Version myserverversion;

	private bool _finishedAccess;

	public volatile bool needsSaveSuccessfulDialog;

	public volatile bool needsSaveFailedDialog;

	public volatile bool needsReloadErrorDialog;

	public volatile bool needsNetworkDownErrorDialog;

	public Player player;

	public bool AccessDone
	{
		get
		{
			return _finishedAccess;
		}
		set
		{
			_finishedAccess = value;
		}
	}

	public Version MyServerVersion
	{
		get
		{
			return myserverversion;
		}
		set
		{
			myserverversion = value;
		}
	}

	public string MyFriendsList
	{
		get
		{
			return myfriendslist;
		}
		set
		{
			myfriendslist = value;
		}
	}

	public string MyExplorersList
	{
		get
		{
			return myexplorerslist;
		}
		set
		{
			myexplorerslist = value;
		}
	}

	public string MyFriendRequests
	{
		get
		{
			return myfriendrequests;
		}
		set
		{
			myfriendrequests = value;
		}
	}

	public Game()
	{
		_finishedAccess = false;
	}

	public void SetPlayer(Player p)
	{
		gameFile = p.CacheFile("game.json");
		player = p;
	}

	public void LoadFromNetwork(string key, Session session)
	{
		_finishedAccess = false;
		HttpStatusCode statusCode = ((!File.Exists(gameFile)) ? HttpStatusCode.NotFound : HttpStatusCode.NotModified);
		session.AddAsyncFileResponse(key, CreateOfflineResponse(statusCode, string.Empty, session));
	}

	public void DeleteFromNetwork(string key, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "{\"success\":true}", session));
	}

	public void AssignFacebookIDToUser(string key, string facebookID, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "{\"success\":true}", session));
	}

	public void GetServerVersion(string key, Session session)
	{
		_finishedAccess = false;
		string text = string.IsNullOrEmpty(SQSettings.BundleVersion) ? "1.0" : SQSettings.BundleVersion;
		string data = "{\"version\":\"" + text + "\",\"android_version\":\"" + text + "\",\"ios_version\":\"" + text + "\",\"amazon_version\":\"" + text + "\",\"chat_switch\":\"1\"}";
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, data, session));
	}

	public void GetFriendsList(string key, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "[]", session));
	}

	public void GetExplorersList(string key, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "[]", session));
	}

	public void GetFriendRequests(string key, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "[]", session));
	}

	public void ConfirmFriendRequest(string key, string id, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "{\"success\":true}", session));
	}

	public void DenyFriendRequest(string key, string id, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "{\"success\":true}", session));
	}

	public void RequestFriend(string key, string id, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "{\"success\":true}", session));
	}

	public void RemoveFriend(string key, string id, Session session)
	{
		_finishedAccess = false;
		session.AddAsyncFileResponse(key, CreateOfflineResponse(HttpStatusCode.OK, "{\"success\":true}", session));
	}

	public void SaveToServer(Session session, string gameData, Action<TFWebFileResponse> callback = null)
	{
		if (gameData == null)
		{
			Debug.Log("Null gameData, not saving");
			return;
		}
		_finishedAccess = false;
		Debug.Log("Saving gamedata to server");
		Action<TFWebFileResponse> callback2 = saveCBHandler;
		if (callback != null)
		{
			callback2 = delegate(TFWebFileResponse response)
			{
				saveCBHandler(response);
				callback(response);
			};
		}
		callback2(CreateOfflineResponse(HttpStatusCode.OK, "{\"success\":true}", session));
	}

	public void saveCBHandler(TFWebFileResponse response)
	{
		if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
		{
			if (response.StatusCode == HttpStatusCode.PreconditionFailed)
			{
				Debug.Log("Reloading game from server " + response.StatusCode);
				needsReloadErrorDialog = true;
			}
			else
			{
				Debug.Log("Network down. continuing offline play without saving to server.");
				needsNetworkDownErrorDialog = true;
			}
		}
		else
		{
			Debug.Log("Game saved to server " + response.StatusCode);
			if (response.StatusCode == HttpStatusCode.OK)
			{
				needsReloadErrorDialog = false;
				needsSaveSuccessfulDialog = true;
			}
			else
			{
				needsSaveFailedDialog = true;
			}
		}
		_finishedAccess = true;
	}

	public void SaveLocally(string json_gamestate)
	{
		if (gameFile != null)
		{
			string text = LocalJsonUtils.NormalizeLikelyJson(json_gamestate);
			Debug.Log("Game.SaveLocally: " + text);
			TFUtils.WriteFile(gameFile, text);
		}
	}

	public string LoadLocally()
	{
		string text = gameFile;
		Debug.Log("Gamefile location: " + text);
		string text2 = TFUtils.ReadFile(text);
		string contents = LocalJsonUtils.NormalizeLikelyJson(text2);
		if (!string.Equals(text2, contents, StringComparison.Ordinal))
		{
			TFUtils.WriteFile(text, contents);
		}
		if (LooksLikeValidJson(contents))
		{
			return contents;
		}
		Debug.LogWarning("[PROFILE_STORAGE] Local save is not valid JSON.");
		return contents;
	}

	private static bool LooksLikeValidJson(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		try
		{
			return Json.Deserialize(json) != null;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public bool GameExists(Player p)
	{
		return p != null && File.Exists(p.CacheFile("game.json"));
	}

	public void DestroyCache(Player p)
	{
		string text = p.CacheDir();
		if (Directory.Exists(text))
		{
			Debug.Log("Removing directory: " + text);
			Directory.Delete(text, true);
			Debug.Log("Removing file: " + p.LastPlayedFile());
			File.Delete(p.LastPlayedFile());
			if (Directory.Exists(Player.LOCAL_TEXTURE_CACHE_DIRECTORY))
			{
				Debug.Log("Removing directory: " + Player.LOCAL_TEXTURE_CACHE_DIRECTORY);
				Directory.Delete(Player.LOCAL_TEXTURE_CACHE_DIRECTORY, true);
			}
		}
	}

	public bool IsDoneServerAccess()
	{
		return _finishedAccess;
	}

	private static TFWebFileResponse CreateOfflineResponse(HttpStatusCode statusCode, string data, Session session)
	{
		return new TFWebFileResponse
		{
			StatusCode = statusCode,
			Data = data,
			NetworkDown = false,
			UserData = session
		};
	}
}
