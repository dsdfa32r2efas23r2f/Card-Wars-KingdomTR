using Project.Core.Application.Battle.UseCases;
using Project.Core.Domain.Battle.Services;
using Project.Core.Domain.Battle.ValueObjects;
using Project.Core.Infrastructure.Random;

public class BattleRefactorFacade
{
	private readonly BattleTurnService turnService;

	private readonly BattleSimulation simulation;

	public BattleRefactorFacade(int actionPointsPerTurn)
	{
		simulation = new BattleSimulation(new LegacyBattleEventSink(), new UnityRandomService());
		turnService = new BattleTurnService(simulation, actionPointsPerTurn);
	}

	public void Start(int startingActionPoints)
	{
		turnService.StartBattle(startingActionPoints);
	}

	public bool TrySpendUserActionPoints(int amount)
	{
		return turnService.TrySpendActionPoints(BattlePlayer.User, amount);
	}

	public void EndTurn()
	{
		turnService.EndCurrentTurn();
	}
}
