namespace Project.Core.Application.Meta.Ports
{
    public interface IMetaStateReader
    {
        int HardCurrency { get; }
        int PaidHardCurrency { get; }
        int FreeHardCurrency { get; }
        int PvpCurrency { get; }
        int SoftCurrency { get; }
        int CustomizationCurrency { get; }
    }
}
