using System;
using System.Text;

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
}
