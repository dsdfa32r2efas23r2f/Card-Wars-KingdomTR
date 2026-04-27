#define ASSERTS_ON
using System;
using System.Collections.Generic;
using System.Net;
using MiniJSON;

public class TFServer
{
	public delegate void JsonStringHandler(string jsonResponse, HttpStatusCode status);

	public delegate void JsonResponseHandler(Dictionary<string, object> dict, HttpStatusCode status);

	public const string ERROR_KEY = "error";

	public const string NETWORK_ERROR = "Network error";

	public static readonly string NETWORK_ERROR_JSON = "{\"success\": false, \"error\": \"Network error\"}";

	private readonly CookieContainer cookies = new CookieContainer();

	public TFServer(CookieContainer cookies, int maxConnections)
	{
		this.cookies = cookies;
		TFWebClient.maxConnections = maxConnections;
	}

	public void ShortCircuitAllRequests()
	{
	}

	public Cookie GetCookie(Uri uri, string key)
	{
		return cookies.GetCookies(uri)[key];
	}

	public void PostToJSON(string url, Dictionary<string, object> postDict, JsonResponseHandler callback, bool ignoreEtag = false)
	{
		callback((Dictionary<string, object>)Json.Deserialize(BuildOfflineJson(url)), HttpStatusCode.OK);
	}

	public void PostToString(string url, Dictionary<string, object> postDict, JsonStringHandler callback)
	{
		callback(BuildOfflineJson(url), HttpStatusCode.OK);
	}

	public void GetToJSON(string url, JsonResponseHandler callback)
	{
		callback((Dictionary<string, object>)Json.Deserialize(BuildOfflineJson(url)), HttpStatusCode.OK);
	}

	private string BuildOfflineJson(string url)
	{
		if (url != null)
		{
			if (url.Contains("authRequest"))
			{
				return "{\"success\":true,\"data\":{\"nonce\":\"offline-nonce\"}}";
			}
			if (url.Contains("gettime"))
			{
				return "{\"success\":true,\"data\":{\"server_time\":\"Mon, 01 Jan 2024 00:00:00 GMT\"}}";
			}
		}
		return "{\"success\":true,\"data\":\"[]\"}";
	}
}
