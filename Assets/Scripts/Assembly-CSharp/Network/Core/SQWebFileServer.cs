using System.IO;
using System.Net;

public class SQWebFileServer
{
	private string eTagFile;

	public SQWebFileServer(CookieContainer cookies)
	{
	}

	public void SetPlayerInfo(Player player)
	{
		if (player != null)
		{
			eTagFile = player.CacheFile("lastETag");
		}
	}

	public void DeleteETagFile()
	{
		if (!string.IsNullOrEmpty(eTagFile) && File.Exists(eTagFile))
		{
			File.Delete(eTagFile);
		}
	}
}
