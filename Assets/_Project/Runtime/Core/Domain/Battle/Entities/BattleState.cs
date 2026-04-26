using Project.Core.Domain.Battle.ValueObjects;

namespace Project.Core.Domain.Battle.Entities
{
	public class BattleState
	{
		public BattlePhase Phase { get; private set; }

		public BattlePlayer ActivePlayer { get; private set; }

		public int TurnIndex { get; private set; }

		public int UserActionPoints { get; private set; }

		public int OpponentActionPoints { get; private set; }

		public bool IsFinished
		{
			get { return Phase == BattlePhase.Finished; }
		}

		public void Start(int startingActionPoints)
		{
			Phase = BattlePhase.Deployment;
			ActivePlayer = BattlePlayer.User;
			TurnIndex = 1;
			UserActionPoints = startingActionPoints;
			OpponentActionPoints = startingActionPoints;
		}

		public void EnterActionPhase()
		{
			if (Phase == BattlePhase.Deployment)
			{
				Phase = BattlePhase.Actions;
			}
		}

		public bool TrySpendActionPoints(BattlePlayer player, int amount)
		{
			if (amount <= 0 || Phase != BattlePhase.Actions || IsFinished)
			{
				return false;
			}
			if (player == BattlePlayer.User)
			{
				if (UserActionPoints < amount)
				{
					return false;
				}
				UserActionPoints -= amount;
				return true;
			}
			if (OpponentActionPoints < amount)
			{
				return false;
			}
			OpponentActionPoints -= amount;
			return true;
		}

		public void EndTurn(int actionPointsPerTurn)
		{
			if (IsFinished)
			{
				return;
			}

			ActivePlayer = ActivePlayer == BattlePlayer.User ? BattlePlayer.Opponent : BattlePlayer.User;
			TurnIndex++;
			if (ActivePlayer == BattlePlayer.User)
			{
				UserActionPoints += actionPointsPerTurn;
			}
			else
			{
				OpponentActionPoints += actionPointsPerTurn;
			}
		}

		public void Finish()
		{
			Phase = BattlePhase.Finished;
		}
	}
}
