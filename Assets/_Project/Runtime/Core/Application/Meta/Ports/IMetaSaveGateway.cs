using System;

namespace Project.Core.Application.Meta.Ports
{
    public interface IMetaSaveGateway
    {
        void Save();
        void Save(Action<bool> onComplete);
    }
}
