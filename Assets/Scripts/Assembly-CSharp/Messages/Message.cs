using System.Collections.Generic;

namespace Messages
{
	public class Message
	{
		public static void GetMessages(Session session, MessagesCallback callback)
		{
			if (callback != null)
			{
				callback(null, ResponseFlag.None);
			}
		}

		public static void GotallMessagesCallback(List<string> messages, TFWebFileResponse response)
		{
		}

		public static void ClearMessage()
		{
			SessionManager.Instance.ClearMessages();
		}
	}
}
