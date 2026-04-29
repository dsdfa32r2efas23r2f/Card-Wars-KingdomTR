namespace Project.Core.Domain.Meta.Entities
{
    public class MissionStateModel
    {
        public string MissionId { get; }
        public bool IsCompleted { get; }
        public bool IsClaimed { get; }

        public MissionStateModel(string missionId, bool isCompleted, bool isClaimed)
        {
            MissionId = missionId;
            IsCompleted = isCompleted;
            IsClaimed = isClaimed;
        }

        public MissionStateModel MarkClaimed() => new MissionStateModel(MissionId, IsCompleted, true);
    }
}
