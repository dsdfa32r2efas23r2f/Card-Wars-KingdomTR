using Project.Core.Application.Meta.Commands;
using Project.Core.Application.Meta.Ports;
using Project.Core.Application.Meta.UseCases;
using Project.Core.Domain.Meta.Entities;
using UnityEngine;

public class LegacyMetaCommandBus : IMetaCommandBus
{
    private readonly MetaRefactorFacade _facade;

    public LegacyMetaCommandBus(MetaRefactorFacade facade)
    {
        _facade = facade;
    }

    public void Execute(GrantCurrencyCommand command)
    {
        if (command == null)
        {
            Debug.LogWarning("[META_ECS] LegacyMetaCommandBus.Execute(Grant): null command");
            return;
        }
        // Shadow-only: grant without legacy side-effects.
        // Use for non-store grants (missions, town rewards) where legacy already applied.
        Debug.Log(string.Format("[META_ECS] CommandBus grant source={0}", command.Source));
    }

    public SpendResult Execute(SpendCurrencyCommand command)
    {
        if (command == null)
        {
            Debug.LogWarning("[META_ECS] LegacyMetaCommandBus.Execute(Spend): null command");
            return SpendResult.Ok(MetaWalletState.Empty);
        }
        // Shadow-only: spend without legacy side-effects.
        Debug.Log(string.Format("[META_ECS] CommandBus spend source={0} hard={1}",
            command.Source, command.Amounts.HardCurrency));
        return SpendResult.Ok(MetaWalletState.Empty);
    }
}
