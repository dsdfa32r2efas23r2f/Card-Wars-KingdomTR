using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using MiniJSON;

public class SQServer
{
	public delegate void JsonStringHandler(string jsonResponse, HttpStatusCode status);

	public delegate void JsonResponseHandler(Dictionary<string, object> dict, HttpStatusCode status);

	public SQServer(CookieContainer cookies)
	{
	}

	public bool IsNetworkError(Dictionary<string, object> response)
	{
		return response == null || (response.ContainsKey("error") && response["error"] != null);
	}

	public void SavePurchase(string store_id, string store, string sandbox, string partial, string bundle_id, string productId, string playerId, string receipt, SQServer.JsonResponseHandler callback)
	{
		callback(Fail("offline"), HttpStatusCode.NotFound);
	}


	public void MultiplayerPlayerInfo(string playerId, SQServer.JsonResponseHandler callback)
	{
		callback(Fail(), HttpStatusCode.OK);
	}

	public void MultiplayerNewPlayer(string name, string icon, string deck, float deckRank, string landscapes, string helpercreature, string leader, int leaderLevel, int maxLevel, int allyboxspace, SQServer.JsonResponseHandler callback)
	{
		callback(Fail(), HttpStatusCode.OK);
	}

	public void MultiplayerUpdateDeck(string name, string deck, int needUpdate, string landscapes, string helpercreature, string leader, int leaderLevel, int allyboxspace, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void MultiplayerUpdatePlayer(string name, string icon, int maxLevel, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void RedeemcodeCheck(string redeemcode, string version, string subject, string message, DateTime start_date, DateTime end_date, int soft_currency, SQServer.JsonResponseHandler callback)
	{
		callback(Fail("invalid redeemcode"), HttpStatusCode.OK);
	}

	public void MultiplayerExtendedRecord(string playerId, SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, new List<object>()), HttpStatusCode.OK);
	}

	public void MultiplayerLeaderboardPlayer(string playerId, SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, new List<object>()), HttpStatusCode.OK);
	}

	public void MultiplayerLeaderboardTop(SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, new List<object>()), HttpStatusCode.OK);
	}

	public void MultiplayerGetRank(bool global, SQServer.JsonResponseHandler callback)
	{
		callback(Fail(), HttpStatusCode.OK);
	}

	public void MultiplayerPersonalRecord(string target, SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, new List<object>()), HttpStatusCode.OK);
	}

	public void MultiplayerNotification(SQServer.JsonResponseHandler callback)
	{
		callback(Fail(), HttpStatusCode.OK);
	}

	public void MultiplayerTournamentPlayerResult(SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, new List<object>()), HttpStatusCode.OK);
	}

	public void MultiplayerFindMatch(int maxLevel, SQServer.JsonResponseHandler callback)
	{
		callback(Fail(), HttpStatusCode.OK);
	}

	public void MultiplayerStartMatch(string matchId, float deckRank, string leader, int leaderLevel, SQServer.JsonStringHandler callback)
	{
		callback(string.Empty, HttpStatusCode.NotFound);
	}

	public void MultiplayerEndMatch(string matchId, bool loss, SQServer.JsonResponseHandler callback)
	{
		callback(Fail(), HttpStatusCode.OK);
	}

	public void MultiplayerCheaterTournamentEnd(SQServer.JsonResponseHandler callback)
	{
		callback(Fail(), HttpStatusCode.OK);
	}

	public void MultiplayerGetTournamentEnd(SQServer.JsonResponseHandler callback)
	{
		callback(Fail(), HttpStatusCode.OK);
	}

	public void MultiplayerRedeemReward(int tournamentId, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void Friend_update_myinfo(bool helpcount, bool anonymoushelpcount, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void Friend_use_friend(string friend_id, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void Friend_use_player(string user_id, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void Friend_request_with_myinfo(string playerId, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void Friend_fake_request_with_myinfo(SQServer.JsonResponseHandler handler)
	{
		handler(Ok(), HttpStatusCode.OK);
	}

	public void Friend_confirm_with_myinfo(string playerId, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void Friend_get_userinfo(string playerId, SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, BuildOfflineAllyUserInfo(playerId)), HttpStatusCode.OK);
	}

	public void Friend_get_helpers(string optionTarget, List<string> excludeIDs, SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, "[]"), HttpStatusCode.OK);
	}

	public void User_currency_history2(int num, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void User_currency_history(string country, int transaction, int tier, int paid, int free, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void User_action(int pd, int fr, int cu, int dp, int df, int dc, string us, int hd, int misc, string evt, string cc, SQServer.JsonResponseHandler callback)
	{
		int num = Math.Max(0, pd + dp);
		int num2 = Math.Max(0, fr + df);
		int num3 = Math.Max(0, cu + dc);
		string text = GetPlayerIdForHash();
		string key = "5424493204pemhi3148ifmanseu4iksdf4_4" + text + Convert.ToString(misc);
		string hashString = GetHashString(text, key);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["level1"] = num;
		dictionary["level2"] = num2;
		dictionary["level3"] = num3;
		dictionary["handle"] = hashString;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2["fields"] = dictionary;
		string data = "[" + MiniJSON.Json.Serialize(dictionary2) + "]";
		callback(Json(true, data), HttpStatusCode.OK);
	}

	public void GetCC(SQServer.JsonResponseHandler handler)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["ip"] = string.Empty;
		dictionary["country"] = "US";
		handler(dictionary, HttpStatusCode.OK);
	}

	public void PlaceMeOnLeaderboard(string user_id, string currentSeasonID, SQServer.JsonResponseHandler callback)
	{
		callback(Ok(), HttpStatusCode.OK);
	}

	public void RegisterMatchResult(string user_id, string currentSeasonID, string opnentID, bool didIWin, SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, 0), HttpStatusCode.OK);
	}

	public void FetchLeaderboardsEntries(int startPosition, int endPosition, SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, "[]"), HttpStatusCode.OK);
	}

	public void HasSeasonEnded(string user_id, SQServer.JsonResponseHandler callback)
	{
		callback(Json(true, 0), HttpStatusCode.OK);
	}

	public void CompassSupportLogin(string project_id, string support_id, string p, string user_id, string checkkey, SQServer.JsonStringHandler callback)
	{
		callback("{\"result\":\"0\"}", HttpStatusCode.OK);
	}

	private static Dictionary<string, object> Json(bool success, object data)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["success"] = success;
		dictionary["data"] = data;
		return dictionary;
	}

	private static Dictionary<string, object> Ok()
	{
		return Json(true, 1);
	}

	private static Dictionary<string, object> Fail()
	{
		return Fail(null);
	}

	private static Dictionary<string, object> Fail(string error)
	{
		Dictionary<string, object> dictionary = Json(false, 0);
		if (!string.IsNullOrEmpty(error))
		{
			dictionary["error"] = error;
		}
		return dictionary;
	}

	private static string GetPlayerIdForHash()
	{
		string text = null;
		if (SessionManager.Instance != null)
		{
			text = SessionManager.Instance.PlayerID;
		}
		return string.IsNullOrEmpty(text) ? "ua" : text;
	}

	private static string GetHashString(string sourcevalue, string key)
	{
		using (HMACSHA256 hMACSHA = new HMACSHA256())
		{
			hMACSHA.Key = Encoding.UTF8.GetBytes(key);
			byte[] array = hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(sourcevalue));
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.AppendFormat("{0:X2}", array[i]);
			}
			return stringBuilder.ToString();
		}
	}

	private static string BuildOfflineAllyUserInfo(string playerId)
	{
		string text = string.IsNullOrEmpty(playerId) ? GetPlayerIdForHash() : playerId;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["user_id"] = text;
		dictionary["name"] = "Offline Ally";
		dictionary["icon"] = "0";
		dictionary["rankxp"] = 1;
		dictionary["ally"] = 1;
		dictionary["helpcount"] = 0;
		dictionary["anonymoushelpcount"] = 0;
		dictionary["helpercreatureid"] = -1;
		dictionary["helpercreature"] = string.Empty;
		dictionary["landscapes"] = string.Empty;
		dictionary["sincelastactivedate"] = 0;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2["fields"] = dictionary;
		List<object> list = new List<object> { dictionary2 };
		return MiniJSON.Json.Serialize(list);
	}
}

