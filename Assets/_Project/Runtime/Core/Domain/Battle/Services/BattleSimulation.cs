using Project.Core.Domain.Battle.Entities;
using Project.Core.Domain.Battle.ValueObjects;

namespace Project.Core.Domain.Battle.Services
{
	public class BattleSimulation
	{
		private readonly IBattleEventSink eventSink;

		private readonly IRandomService random;

		private readonly BattleState state = new BattleState();

		public BattleState State
		{
			get { return state; }
		}

		public BattleSimulation(IBattleEventSink eventSink, IRandomService random)
		{
			this.eventSink = eventSink;
			this.random = random;
		}

		public void Start(int startingActionPoints)
		{
			state.Start(startingActionPoints);
			eventSink.Publish("battle.started", "turn=1");
		}

		public void EnterActionPhase()
		{
			state.EnterActionPhase();
			eventSink.Publish("battle.phase", "actions");
		}

		public bool TrySpendActionPoints(BattlePlayer player, int amount)
		{
			bool success = state.TrySpendActionPoints(player, amount);
			if (success)
			{
				eventSink.Publish("battle.ap_spent", "player=" + player + ";amount=" + amount);
			}
			return success;
		}

		public void EndTurn(int actionPointsPerTurn)
		{
			state.EndTurn(actionPointsPerTurn);
			eventSink.Publish("battle.turn_changed", "turn=" + state.TurnIndex + ";active=" + state.ActivePlayer);
		}

		public bool RollPercent(int percent)
		{
			if (percent <= 0)
			{
				return false;
			}
			if (percent >= 100)
			{
				return true;
			}
			return random.Range(0, 100) < percent;
		}
	}
}
