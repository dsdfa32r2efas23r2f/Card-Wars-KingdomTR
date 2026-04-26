namespace Project.Core.Domain.Battle.Services
{
	public interface IRandomService
	{
		int Range(int minInclusive, int maxExclusive);
	}
}
