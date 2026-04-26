using Project.Core.Domain.Battle.Services;
using Project.Core.Domain.Battle.ValueObjects;

namespace Project.Core.Application.Battle.UseCases
{
	public class BattleTurnService
	{
		private readonly BattleSimulation simulation;

		private readonly int actionPointsPerTurn;

		public BattleTurnService(BattleSimulation simulation, int actionPointsPerTurn)
		{
			this.simulation = simulation;
			this.actionPointsPerTurn = actionPointsPerTurn;
		}

		public void StartBattle(int startingActionPoints)
		{
			simulation.Start(startingActionPoints);
			simulation.EnterActionPhase();
		}

		public bool TrySpendActionPoints(BattlePlayer player, int amount)
		{
			return simulation.TrySpendActionPoints(player, amount);
		}

		public void EndCurrentTurn()
		{
			simulation.EndTurn(actionPointsPerTurn);
		}
	}
}
