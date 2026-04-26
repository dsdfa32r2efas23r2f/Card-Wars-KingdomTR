using Project.Core.Domain.Battle.Services;
using UnityEngine;

public class LegacyBattleEventSink : IBattleEventSink
{
	public void Publish(string eventName, string payload)
	{
		Debug.Log("[BattleCore] " + eventName + " " + payload);
	}
}
