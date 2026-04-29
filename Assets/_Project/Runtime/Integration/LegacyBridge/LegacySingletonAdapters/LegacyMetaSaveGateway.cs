using System;
using Project.Core.Application.Meta.Ports;
using UnityEngine;

public class LegacyMetaSaveGateway : IMetaSaveGateway
{
    public void Save()
    {
        PlayerInfoScript player = Singleton<PlayerInfoScript>.Instance;
        if (player != null)
            player.Save();
        else
            Debug.LogWarning("[META_ECS] LegacyMetaSaveGateway.Save: PlayerInfoScript not available");
    }

    public void Save(Action<bool> onComplete)
    {
        PlayerInfoScript player = Singleton<PlayerInfoScript>.Instance;
        if (player != null)
            player.Save(onComplete != null ? (SessionManager.OnSaveDelegate)(success => onComplete(success)) : null);
        else
        {
            Debug.LogWarning("[META_ECS] LegacyMetaSaveGateway.Save: PlayerInfoScript not available");
            onComplete?.Invoke(false);
        }
    }
}
