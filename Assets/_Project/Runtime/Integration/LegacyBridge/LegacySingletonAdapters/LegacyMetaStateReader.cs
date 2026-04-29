using Project.Core.Application.Meta.Ports;
using UnityEngine;

public class LegacyMetaStateReader : IMetaStateReader
{
    private PlayerSaveData Data => Singleton<PlayerInfoScript>.Instance?.SaveData;

    public int HardCurrency => Data?.HardCurrency ?? 0;
    public int PaidHardCurrency => Data?.PaidHardCurrency ?? 0;
    public int FreeHardCurrency => Data?.FreeHardCurrency ?? 0;
    public int PvpCurrency => Data?.PvPCurrency ?? 0;
    public int SoftCurrency => Data?.SoftCurrency ?? 0;
    public int CustomizationCurrency => Data?.CustomizationCurrency ?? 0;
}
