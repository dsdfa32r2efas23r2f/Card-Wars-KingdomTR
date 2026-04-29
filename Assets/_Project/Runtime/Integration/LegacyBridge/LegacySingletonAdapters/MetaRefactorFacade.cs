using Project.Core.Application.Meta.Commands;
using Project.Core.Application.Meta.UseCases;
using Project.Core.Domain.Meta.Entities;
using Project.Core.Domain.Meta.ValueObjects;
using UnityEngine;

public class MetaRefactorFacade
{
    private readonly ApplyGrantCurrencyService _applyGrant = new ApplyGrantCurrencyService();
    private readonly ApplySpendCurrencyService _applySpend = new ApplySpendCurrencyService();
    private readonly ApplyClaimMissionService _applyMissionClaim = new ApplyClaimMissionService();
    private readonly ApplyRefillStaminaService _applyStaminaRefill = new ApplyRefillStaminaService();

    private MetaWalletState _shadowWalletState = MetaWalletState.Empty;
    private bool _hasShadowState;

    public void SyncShadowFromLegacy(PlayerSaveData saveData)
    {
        if (saveData == null) return;
        _shadowWalletState = new MetaWalletState(new CurrencyAmounts(
            saveData.PaidHardCurrency,
            saveData.FreeHardCurrency,
            saveData.PvPCurrency,
            saveData.SoftCurrency,
            saveData.CustomizationCurrency));
        _hasShadowState = true;
    }

    public void ApplyStoreGrant(CurrencyPackageData packageData, string productId, PurchaseManager.ProductData purchaseData)
    {
        if (packageData == null)
        {
            Debug.LogError("[META_ECS] ApplyStoreGrant called with null packageData");
            return;
        }
        PlayerInfoScript player = Singleton<PlayerInfoScript>.Instance;
        if (player == null || player.SaveData == null)
        {
            Debug.LogError("[META_ECS] ApplyStoreGrant called without initialized PlayerInfoScript/SaveData");
            return;
        }
        PlayerSaveData saveData = player.SaveData;
        if (!_hasShadowState)
            SyncShadowFromLegacy(saveData);

        var amounts = new CurrencyAmounts(
            packageData.PaidHardCurrency,
            packageData.FreeHardCurrency,
            packageData.SocialCurrency,
            packageData.SoftCurrency,
            packageData.CustomizationCurrency);
        var command = new GrantCurrencyCommand(productId, amounts);

        MetaWalletState before = _shadowWalletState;
        _shadowWalletState = _applyGrant.Apply(_shadowWalletState, command);

        Debug.Log(string.Format("[META_ECS] Store grant source={0} paid={1} free={2} pvp={3} soft={4} custom={5}",
            command.Source,
            amounts.PaidHardCurrency, amounts.FreeHardCurrency,
            amounts.PvpCurrency, amounts.SoftCurrency, amounts.CustomizationCurrency));

        string price = purchaseData != null ? purchaseData.Price : string.Empty;
        player.AddHardCurrency2(packageData.PaidHardCurrency, packageData.FreeHardCurrency, productId,
            Singleton<PurchaseManager>.Instance.getLastHandle, price);
        saveData.PvPCurrency += packageData.SocialCurrency;
        saveData.SoftCurrency += packageData.SoftCurrency;
        saveData.CustomizationCurrency += packageData.CustomizationCurrency;

        if (saveData.PvPCurrency != _shadowWalletState.PvpCurrency
            || saveData.SoftCurrency != _shadowWalletState.SoftCurrency
            || saveData.CustomizationCurrency != _shadowWalletState.CustomizationCurrency)
        {
            Debug.LogWarning(string.Format(
                "[META_ECS] Wallet mismatch after grant. legacy(pvp={0} soft={1} custom={2}) shadow(pvp={3} soft={4} custom={5})",
                saveData.PvPCurrency, saveData.SoftCurrency, saveData.CustomizationCurrency,
                _shadowWalletState.PvpCurrency, _shadowWalletState.SoftCurrency, _shadowWalletState.CustomizationCurrency));
            SyncShadowFromLegacy(saveData);
        }

        Debug.Log(string.Format("[META_ECS] Shadow wallet: hard {0}->{1} pvp {2}->{3} soft {4}->{5} custom {6}->{7}",
            before.HardCurrency, _shadowWalletState.HardCurrency,
            before.PvpCurrency, _shadowWalletState.PvpCurrency,
            before.SoftCurrency, _shadowWalletState.SoftCurrency,
            before.CustomizationCurrency, _shadowWalletState.CustomizationCurrency));
    }

    public void ApplyStoreSpend(int hardCurrency, string source)
    {
        PlayerInfoScript player = Singleton<PlayerInfoScript>.Instance;
        if (player == null || player.SaveData == null)
        {
            Debug.LogError("[META_ECS] ApplyStoreSpend called without initialized PlayerInfoScript/SaveData");
            return;
        }
        if (!_hasShadowState)
            SyncShadowFromLegacy(player.SaveData);

        var amounts = new SpendAmounts(hardCurrency, 0, 0, 0);
        var command = new SpendCurrencyCommand(source, amounts);
        MetaWalletState before = _shadowWalletState;
        SpendResult result = _applySpend.Apply(_shadowWalletState, command);

        Debug.Log(string.Format("[META_ECS] Store spend source={0} hard={1}", source, hardCurrency));

        if (!result.Succeeded)
        {
            Debug.LogWarning(string.Format("[META_ECS] Shadow spend insufficient funds source={0} available={1} needed={2}",
                source, before.HardCurrency, hardCurrency));
            SyncShadowFromLegacy(player.SaveData);
            return;
        }
        _shadowWalletState = result.NewState;

        Debug.Log(string.Format("[META_ECS] Shadow wallet: hard {0}->{1}",
            before.HardCurrency, _shadowWalletState.HardCurrency));
    }

    public void ApplyStaminaRefill(int hardCurrencyCost)
    {
        PlayerInfoScript player = Singleton<PlayerInfoScript>.Instance;
        if (player == null || player.SaveData == null)
        {
            Debug.LogError("[META_ECS] ApplyStaminaRefill called without initialized PlayerInfoScript/SaveData");
            return;
        }
        if (!_hasShadowState)
            SyncShadowFromLegacy(player.SaveData);

        var command = new RefillStaminaCommand("stamina_refill", hardCurrencyCost);
        MetaWalletState before = _shadowWalletState;
        SpendResult result = _applyStaminaRefill.Apply(_shadowWalletState, command);

        Debug.Log(string.Format("[META_ECS] Stamina refill cost={0}", hardCurrencyCost));

        if (!result.Succeeded)
        {
            Debug.LogWarning(string.Format("[META_ECS] Shadow stamina refill insufficient funds available={0} needed={1}",
                before.HardCurrency, hardCurrencyCost));
            SyncShadowFromLegacy(player.SaveData);
            return;
        }
        _shadowWalletState = result.NewState;

        Debug.Log(string.Format("[META_ECS] Shadow wallet: hard {0}->{1}",
            before.HardCurrency, _shadowWalletState.HardCurrency));
    }

    public void ApplyMissionRewardGrant(string missionId, int freeHardCurrency, int pvpCurrency, int softCurrency)
    {
        PlayerInfoScript player = Singleton<PlayerInfoScript>.Instance;
        if (player == null || player.SaveData == null)
        {
            Debug.LogError("[META_ECS] ApplyMissionRewardGrant called without initialized PlayerInfoScript/SaveData");
            return;
        }
        if (!_hasShadowState)
            SyncShadowFromLegacy(player.SaveData);

        var rewards = new CurrencyAmounts(0, freeHardCurrency, pvpCurrency, softCurrency, 0);
        var command = new ClaimMissionCommand(missionId, rewards);
        MetaWalletState before = _shadowWalletState;
        _shadowWalletState = _applyMissionClaim.Apply(_shadowWalletState, command);

        Debug.Log(string.Format("[META_ECS] Mission reward source={0} free={1} pvp={2} soft={3}",
            missionId, freeHardCurrency, pvpCurrency, softCurrency));
        Debug.Log(string.Format("[META_ECS] Shadow wallet: hard {0}->{1} pvp {2}->{3} soft {4}->{5}",
            before.HardCurrency, _shadowWalletState.HardCurrency,
            before.PvpCurrency, _shadowWalletState.PvpCurrency,
            before.SoftCurrency, _shadowWalletState.SoftCurrency));
    }
}
