using Project.Core.Domain.Battle.Services;
using UnityEngine;

namespace Project.Core.Infrastructure.Random
{
	public class UnityRandomService : IRandomService
	{
		public int Range(int minInclusive, int maxExclusive)
		{
			return UnityEngine.Random.Range(minInclusive, maxExclusive);
		}
	}
}
