using System.IO;
using System.Net;

public class SQWebFileServer : TFWebFileServer
{
	private string eTagFile;

	public SQWebFileServer(CookieContainer cookies)
		: base(cookies)
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
