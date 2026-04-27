using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

public static class LocalJsonUtils
{
	public static string NormalizeLikelyJson(string raw)
	{
		if (string.IsNullOrEmpty(raw))
		{
			return raw;
		}
		return RemoveTrailingCommas(raw);
	}

	private static string RemoveTrailingCommas(string input)
	{
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (flag)
			{
				stringBuilder.Append(c);
				if (flag2)
				{
					flag2 = false;
				}
				else if (c == '\\')
				{
					flag2 = true;
				}
				else if (c == '"')
				{
					flag = false;
				}
				continue;
			}
			if (c == '"')
			{
				flag = true;
				stringBuilder.Append(c);
				continue;
			}
			if (c == ',')
			{
				int num = i + 1;
				while (num < input.Length && char.IsWhiteSpace(input[num]))
				{
					num++;
				}
				if (num < input.Length && (input[num] == '}' || input[num] == ']'))
				{
					continue;
				}
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	public static object DeserializeObject(string raw)
	{
		if (string.IsNullOrEmpty(raw))
		{
			return null;
		}
		string text = NormalizeLikelyJson(raw);
		JToken jToken = JToken.Parse(text);
		return ConvertToken(jToken);
	}

	public static Dictionary<string, object> DeserializeDictionary(string raw)
	{
		return DeserializeObject(raw) as Dictionary<string, object>;
	}

	public static Dictionary<string, object>[] DeserializeDictionaryArray(string raw)
	{
		object obj = DeserializeObject(raw);
		object[] array = obj as object[];
		if (array == null)
		{
			return new Dictionary<string, object>[0];
		}
		return array.Cast<Dictionary<string, object>>().ToArray();
	}

	public static List<object> DeserializeList(string raw)
	{
		object obj = DeserializeObject(raw);
		object[] array = obj as object[];
		if (array != null)
		{
			return array.ToList();
		}
		List<object> list = obj as List<object>;
		if (list != null)
		{
			return list;
		}
		return new List<object>();
	}

	private static object ConvertToken(JToken token)
	{
		if (token == null)
		{
			return null;
		}
		switch (token.Type)
		{
		case JTokenType.Object:
		{
			JObject jObject = (JObject)token;
			Dictionary<string, object> dictionary = new Dictionary<string, object>(jObject.Count);
			foreach (JProperty item in jObject.Properties())
			{
				dictionary[item.Name] = ConvertToken(item.Value);
			}
			return dictionary;
		}
		case JTokenType.Array:
		{
			JArray jArray = (JArray)token;
			object[] array = new object[jArray.Count];
			for (int i = 0; i < jArray.Count; i++)
			{
				array[i] = ConvertToken(jArray[i]);
			}
			return array;
		}
		case JTokenType.Integer:
			return token.Value<long>();
		case JTokenType.Float:
			return token.Value<double>();
		case JTokenType.Boolean:
			return token.Value<bool>();
		case JTokenType.String:
			return token.Value<string>();
		case JTokenType.Null:
		case JTokenType.Undefined:
			return null;
		default:
		{
			JValue jValue = token as JValue;
			return (jValue != null) ? jValue.Value : token.ToString();
		}
		}
	}
}
