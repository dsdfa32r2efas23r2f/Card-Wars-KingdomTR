using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SQAuth
{
	public delegate void OnUserDataFn(Session session, int userID);

	public delegate void KFFWWWRequestCallback(object wwwinfo, object obj, string str, object param);

	public delegate object KFFSendWWWRequestWithFormCallback(WWWForm form, string scriptNameAndParams, KFFWWWRequestCallback cb, object callbackParam);

	public delegate string LoadPlayerNameCallback();

	public static bool g_reassignID;

	public bool loggedIn;

	public static KFFSendWWWRequestWithFormCallback KFFSendWWWRequestWithFormFunction;

	public static LoadPlayerNameCallback LoadPlayerNameFunction;

	public void AuthUser(Session session, TFServer.JsonResponseHandler callback, bool doFacebookAuth, string fbAccessToken)
	{
		Player player = Player.LoadFromFilesystem();
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2["user_id"] = player.playerId;
		dictionary2["is_new"] = player.isNew;
		dictionary["success"] = true;
		dictionary["data"] = dictionary2;
		callback(dictionary, HttpStatusCode.OK);
	}
}
