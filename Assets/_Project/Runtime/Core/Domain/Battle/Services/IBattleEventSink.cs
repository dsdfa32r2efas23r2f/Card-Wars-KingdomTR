namespace Project.Core.Domain.Battle.Services
{
	public interface IBattleEventSink
	{
		void Publish(string eventName, string payload);
	}
}
