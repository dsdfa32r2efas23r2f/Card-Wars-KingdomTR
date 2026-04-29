using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using Allies;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Session
{
	public class FramerateWatcher
	{
		public float frequency = 0.5f;

		private float accum;

		private int frames;

		private float waitTime;

		private float prevWindowsFPS;

		public float Framerate
		{
			get
			{
				return prevWindowsFPS;
			}
		}

		public void Update()
		{
			accum += Time.timeScale / Time.deltaTime;
			frames++;
			waitTime += Time.deltaTime;
			if (waitTime > frequency)
			{
				waitTime = 0f;
				prevWindowsFPS = accum / (float)frames;
				accum = 0f;
				frames = 0;
			}
		}
	}

	public class Authorizing
	{
		private bool _finishedLogin;

		private bool _isFacebookAuth;

		public void OnEnter(Session session, bool doFacebookAuth, string fbAccessToken)
		{
			Debug.Log("Starting to User login");
			_finishedLogin = false;
			_isFacebookAuth = doFacebookAuth;
			Player.LoadFromNetwork("userLogin", session, doFacebookAuth, fbAccessToken);
		}

		public void OnLeave(Session session)
		{
		}

		public void OnUpdate(Session session)
		{
			if (_finishedLogin)
			{
				return;
			}
			if (session.PlayerIsLoggedIn())
			{
				Debug.Log("User logged In");
				_finishedLogin = true;
				return;
			}
			Dictionary<string, object> dictionary = (Dictionary<string, object>)session.CheckAsyncRequest("userLogin");
			if (dictionary == null)
			{
				return;
			}
			bool flag = session.Server.IsNetworkError(dictionary);
			bool flag2 = true;
			if (dictionary.ContainsKey("success"))
			{
				flag2 = (bool)dictionary["success"];
			}
			if (flag || !flag2)
			{
				if (Session.OnSessionUserLoginFail != null)
				{
					Session.OnSessionUserLoginFail();
				}
				else
				{
					Debug.LogError("User Login failed due network error");
				}
			}
			else
			{
				session.ThePlayer = Player.LoadFromDataDict(dictionary);
			}
			if (session.ThePlayer != null)
			{
				session.TheGame.SetPlayer(session.ThePlayer);
				session.ThePlayer.SaveLocally();
				session.WebFileServer.SetPlayerInfo(session.ThePlayer);
			}
		}

		public bool IsLoggedIn()
		{
			return _finishedLogin;
		}
	}

	public delegate void GameloopAction();

	public delegate void AsyncAction();

	private const string LOAD_GAME = "loadGame";

	private const string DELETE_GAME = "deleteGame";

	private const string GET_SERVERVERSION = "getServerVersion";

	private const string TEST_CONNECTIVITY = "testConnectivity";

	private const string GET_FRIENDS_LIST = "getFriendsList";

	private const string GET_EXPLORERS_LIST = "getExplorersList";

	private const string GET_FRIEND_REQUESTS = "getFriendRequests";

	private const string CONFIRM_FRIEND_REQUEST = "confirmFriendRequest";

	private const string DENY_FRIEND_REQUEST = "denyFriendRequest";

	private const string REQUEST_FRIEND = "requestFriend";

	private const string REMOVE_FRIEND = "removeFriend";

	private SessionManager.AssignFacebookIDToUserCallback assignFacebookIDToUserCallback;

	public string manifestVersion = "0";

	private Player player;

	private SQServer server;

	private SQWebFileServer webFileServer;

	private SQAuth auth;

	private Game game;

	private Authorizing authorizing;

	private int currentVersion;

	private List<string> queuedResponses = new List<string>();

	private Dictionary<string, object> asyncRequests = new Dictionary<string, object>();

	private Dictionary<string, TFWebFileResponse> asyncFileRequests = new Dictionary<string, TFWebFileResponse>();

	private bool _finishedPatching;

	private Thread _validationThread;

	private object _validationLock = new object();

	public SQServer Server
	{
		get
		{
			return server;
		}
	}

	public SQWebFileServer WebFileServer
	{
		get
		{
			return webFileServer;
		}
	}

	public SQAuth Auth
	{
		get
		{
			return auth;
		}
	}

	public Game TheGame
	{
		get
		{
			return game;
		}
		set
		{
			game = value;
		}
	}

	public Player ThePlayer
	{
		get
		{
			return player;
		}
		set
		{
			player = value;
		}
	}

	public string UpdateUrl { get; private set; }

	public string IosUpdateUrl { get; private set; }

	public string AndroidUpdateUrl { get; private set; }

	public string AmazonUpdateUrl { get; private set; }

	public string ChatSwitch { get; private set; }

	public bool ValidatingLastPatch
	{
		get
		{
			return _validationThread != null;
		}
	}

	public static event Action OnSessionUserLoginFail;

	public Session(int currentVersion, string fbid, bool doFacebookLogin, string fbAccessToken)
	{
		TFUtils.Init(fbid);
		Debug.Log("Trying to create the session...");
		authorizing = new Authorizing();
		CookieContainer cookies = new CookieContainer();
		server = new SQServer(cookies);
		webFileServer = new SQWebFileServer(cookies);
		auth = new SQAuth();
		this.currentVersion = currentVersion;
		OnInit();
		authorizing.OnEnter(this, doFacebookLogin, fbAccessToken);
	}

	static Session()
	{
	}

	public void ProcessAsyncResponse(string key)
	{
		TFWebFileResponse tFWebFileResponse = CheckAsyncFileRequest(key);
		if (tFWebFileResponse == null)
		{
			return;
		}
		switch (key)
		{
		case LOAD_GAME:
			Debug.Log("[PROFILE_STORAGE] LOAD_GAME response status=" + tFWebFileResponse.StatusCode);
			if (tFWebFileResponse.StatusCode == HttpStatusCode.OK)
			{
				Debug.Log("Server returned success (gamedata). Loading from network response");
				Dictionary<string, object> asJSONDict = tFWebFileResponse.GetAsJSONDict();
				if (asJSONDict != null && asJSONDict.ContainsKey("PlayerName"))
				{
					object paidObj = null, freeObj = null;
					asJSONDict.TryGetValue("PaidHardCurrency", out paidObj);
					asJSONDict.TryGetValue("FreeHardCurrency", out freeObj);
					Debug.Log(string.Format("[PROFILE_STORAGE] LOAD_GAME overwriting local cache with server data: paid={0} free={1}", paidObj, freeObj));
					try
					{
						string text = tFWebFileResponse.Data.ToString();
						int num = text.IndexOf("HasAuthenticated");
						num += "HasAuthenticated".Length + 2;
						text = text.Remove(num, 1);
						text = text.Insert(num, "1");
						game.SaveLocally(text);
					}
					catch
					{
						game.SaveLocally(tFWebFileResponse.Data);
					}
					SessionManager.loginCompletedWithoutError = true;
				}
				else
				{
					Debug.Log("Server returned invalid gamedata: " + tFWebFileResponse.Data);
				}
				break;
			}
			Debug.Log(string.Concat("Server returned status ", tFWebFileResponse.StatusCode, ". Loading from local data"));
			SessionManager.loginCompletedWithoutError = tFWebFileResponse.StatusCode == HttpStatusCode.NotFound || tFWebFileResponse.StatusCode == HttpStatusCode.NotModified;
			if (game.GameExists(player))
			{
				if (tFWebFileResponse.StatusCode == HttpStatusCode.NotAcceptable)
				{
					try
					{
						string text2 = game.LoadLocally();
						int num2 = text2.IndexOf("PaidHardCurrency");
						num2 += "PaidHardCurrency".Length + 2;
						int num3 = text2.IndexOf(',', num2);
						int count = num3 - num2;
						text2 = text2.Remove(num2, count);
						text2 = text2.Insert(num2, "0");
						num2 = text2.IndexOf("FreeHardCurrency");
						num2 += "FreeHardCurrency".Length + 2;
						num3 = text2.IndexOf(',', num2);
						count = num3 - num2;
						text2 = text2.Remove(num2, count);
						text2 = text2.Insert(num2, "5");
						num2 = text2.IndexOf("Zxcvbnm");
						num2 += "Zxcvbnm".Length + 2;
						num3 = text2.IndexOf(',', num2);
						count = num3 - num2;
						text2 = text2.Remove(num2, count);
						text2 = text2.Insert(num2, "1");
						game.SaveLocally(text2);
						Debug.Log("Normal response, but it's a suspicious data - Reset HardCurrencies.");
					}
					catch
					{
						game.SaveLocally(tFWebFileResponse.Data);
					}
				}
				Debug.Log("Creating game from local file");
			}
			else if (tFWebFileResponse.StatusCode == HttpStatusCode.NotFound)
			{
				Debug.Log("Initializing new game");
				WebFileServer.DeleteETagFile();
			}
			else if (tFWebFileResponse.StatusCode == HttpStatusCode.NotModified)
			{
				Debug.Log(string.Concat("What is going on? This is not an expected outcome: response status ", tFWebFileResponse.StatusCode, " Network down: ", tFWebFileResponse.NetworkDown));
				WebFileServer.DeleteETagFile();
			}
			else
			{
				Debug.Log(string.Concat("What is going on? This is not an expected outcome: response status ", tFWebFileResponse.StatusCode, " Network down: ", tFWebFileResponse.NetworkDown));
			}
			break;
		case DELETE_GAME:
			if (tFWebFileResponse.StatusCode == HttpStatusCode.OK)
			{
				Debug.Log("Server returned success (delete game).");
			}
			else
			{
				Debug.Log(string.Concat("Server returned status ", tFWebFileResponse.StatusCode, ". Nothing we can do...."));
			}
			break;
		case GET_SERVERVERSION:
		case TEST_CONNECTIVITY:
			if (tFWebFileResponse.StatusCode == HttpStatusCode.OK)
			{
				if (game != null)
				{
					game.MyServerVersion = ProcessVersionData(tFWebFileResponse.Data);
				}
			}
			else
			{
				game.MyServerVersion = new Version(0, 0);
			}
			break;
		case GET_FRIENDS_LIST:
		case GET_EXPLORERS_LIST:
		case GET_FRIEND_REQUESTS:
		case CONFIRM_FRIEND_REQUEST:
		case DENY_FRIEND_REQUEST:
		case REQUEST_FRIEND:
		case REMOVE_FRIEND:
			if (tFWebFileResponse.StatusCode == HttpStatusCode.OK)
			{
				Debug.Log("Server returned success (" + key + "). Loading from network response");
				Debug.Log("Return = " + tFWebFileResponse.Data);
			}
			else
			{
				Debug.Log(string.Concat("Server returned status ", tFWebFileResponse.StatusCode, ". Nothing we can do...."));
			}
			switch (key)
			{
			case GET_FRIENDS_LIST:
				game.MyFriendsList = tFWebFileResponse.Data;
				Ally.AlliesListCallback(ThePlayer.playerId, tFWebFileResponse);
				break;
			case DENY_FRIEND_REQUEST:
				Ally.DenyAllyRequestCallback(tFWebFileResponse);
				break;
			case GET_FRIEND_REQUESTS:
				game.MyFriendRequests = tFWebFileResponse.Data;
				Ally.AllyRequestListCallback(ThePlayer.playerId, tFWebFileResponse);
				break;
			case REMOVE_FRIEND:
				Ally.RemoveAllyCallback(tFWebFileResponse);
				break;
			}
			break;
		}
		game.AccessDone = true;
	}

	public void OnUpdate()
	{
		authorizing.OnUpdate(this);
		ProcessAsyncResponses();
	}

	public void ReloadGame()
	{
		SceneManager.LoadScene("AppReloadScene");
	}

	public void LoadGameFromNetwork()
	{
		game.LoadFromNetwork(LOAD_GAME, this);
	}

	public void DeleteGameFromNetwork()
	{
		game.DeleteFromNetwork(DELETE_GAME, this);
	}

	public void GetServerVersion()
	{
		game.GetServerVersion(GET_SERVERVERSION, this);
	}

	public void TestConnectivity()
	{
		game.GetServerVersion(TEST_CONNECTIVITY, this);
	}

	public void GetFriendsList()
	{
		game.GetFriendsList(GET_FRIENDS_LIST, this);
	}

	public void GetExplorersList()
	{
		game.GetExplorersList(GET_EXPLORERS_LIST, this);
	}

	public void GetFriendRequests()
	{
		game.GetFriendRequests(GET_FRIEND_REQUESTS, this);
	}

	public void ConfirmFriendRequest(string id)
	{
		game.ConfirmFriendRequest(CONFIRM_FRIEND_REQUEST, id, this);
	}

	public void DenyFriendRequest(string id)
	{
		game.DenyFriendRequest(DENY_FRIEND_REQUEST, id, this);
	}

	public void RequestFriend(string id)
	{
		game.RequestFriend(REQUEST_FRIEND, id, this);
	}

	public void RemoveFriend(string id)
	{
		game.RemoveFriend(REMOVE_FRIEND, id, this);
	}

	public void GetServerTime()
	{
		TFUtils.UpdateServerTime(DateTime.UtcNow);
	}

	public bool IsLoggedIn()
	{
		return authorizing.IsLoggedIn();
	}

	public bool IsMessagelistLoaded()
	{
		return true;
	}

	public int GetLocalVersion()
	{
		return currentVersion;
	}

	public bool PlayerIsLoggedIn()
	{
		return player != null;
	}

	public void onExternalMessage(string msg)
	{
		Debug.Log("[OFFLINE_BACKEND] Ignoring external message: " + msg);
	}

	public void registerExternalCallback(string requestId, SQServer.JsonResponseHandler callback)
	{
		Debug.Log("[OFFLINE_BACKEND] registerExternalCallback ignored for requestId=" + requestId);
	}

	private Version ProcessVersionData(string response)
	{
		object obj = LocalJsonUtils.DeserializeObject(response);
		if (obj != null)
		{
			Dictionary<string, object> data = (Dictionary<string, object>)obj;
			string text = null;
			IosUpdateUrl = TFUtils.TryLoadString(data, "ios_url");
			AndroidUpdateUrl = TFUtils.TryLoadString(data, "android_url");
			AmazonUpdateUrl = TFUtils.TryLoadString(data, "amazon_url");
			ChatSwitch = TFUtils.TryLoadString(data, "chat_switch");
			if (ChatSwitch == null || ChatSwitch == string.Empty)
			{
				ChatSwitch = "1";
			}
			if (TFUtils.AmazonDevice)
			{
				text = TFUtils.TryLoadString(data, "amazon_version");
				UpdateUrl = AmazonUpdateUrl;
			}
			else
			{
				text = TFUtils.TryLoadString(data, "android_version");
				UpdateUrl = AndroidUpdateUrl;
			}
			if (text == null)
			{
				text = TFUtils.TryLoadString((Dictionary<string, object>)obj, "version");
			}
			if (text != null)
			{
				return new Version(text);
			}
		}
		return new Version(1, 0);
	}

	protected void ProcessAsyncResponses()
	{
		if (queuedResponses.Count <= 0)
		{
			return;
		}
		List<string> list = new List<string>(queuedResponses);
		foreach (string item in list)
		{
			ProcessAsyncResponse(item);
		}
	}

	protected void QueueResponse(string key)
	{
		queuedResponses.Add(key);
	}

	public void AddAsyncResponse(string key, object val)
	{
		lock (asyncRequests)
		{
			if (asyncRequests.ContainsKey(key))
			{
				Debug.Log("Warning: got second async response for " + key + "; Existing value was: " + asyncRequests[key]);
			}
			asyncRequests[key] = val;
		}
	}

	public object CheckAsyncRequest(string key)
	{
		object result = null;
		lock (asyncRequests)
		{
			if (asyncRequests.ContainsKey(key))
			{
				result = asyncRequests[key];
				asyncRequests.Remove(key);
				return result;
			}
			return result;
		}
	}

	public SQServer.JsonResponseHandler AsyncResponder(string key)
	{
		return delegate(Dictionary<string, object> response, HttpStatusCode status)
		{
			AddAsyncResponse(key, response);
		};
	}

	public void AddAsyncFileResponse(string key, TFWebFileResponse val)
	{
		lock (asyncFileRequests)
		{
			asyncFileRequests[key] = val;
			game.AccessDone = false;
			QueueResponse(key);
		}
	}

	public TFWebFileResponse CheckAsyncFileRequest(string key)
	{
		TFWebFileResponse result = null;
		lock (asyncFileRequests)
		{
			if (asyncFileRequests.ContainsKey(key))
			{
				result = asyncFileRequests[key];
				asyncFileRequests.Remove(key);
				return result;
			}
			return result;
		}
	}

	public Action<TFWebFileResponse> AsyncFileResponder(string key)
	{
		return delegate(TFWebFileResponse response)
		{
			AddAsyncFileResponse(key, response);
		};
	}

	private void OnInit()
	{
		_validationThread = null;
		_finishedPatching = false;
	}

	private void OnDispose()
	{
		lock (_validationLock)
		{
			if (_validationThread != null)
			{
				_validationThread.Abort();
				_validationThread.Join();
				_validationThread = null;
			}
		}
	}

	public bool IsPatchDone()
	{
		return _finishedPatching;
	}

	public void ValidateLastPatch()
	{
		_validationThread = null;
	}

	public void StartPatch()
	{
		_finishedPatching = true;
	}
}

