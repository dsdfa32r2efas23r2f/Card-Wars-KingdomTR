using Project.Core.Application.Meta.Commands;
using Project.Core.Application.Meta.UseCases;

namespace Project.Core.Application.Meta.Ports
{
    public interface IMetaCommandBus
    {
        void Execute(GrantCurrencyCommand command);
        SpendResult Execute(SpendCurrencyCommand command);
    }
}
