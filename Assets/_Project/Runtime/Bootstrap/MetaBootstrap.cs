using Project.Core.Application.Meta.Ports;
using UnityEngine;

// Composition root. Attach to a persistent GameObject in FrontEndScene.
// Owns the lifetime of all meta-layer objects and exposes them via static accessors.
public class MetaBootstrap : MonoBehaviour
{
    public static MetaRefactorFacade Facade { get; private set; }
    public static IMetaStateReader StateReader { get; private set; }
    public static IMetaSaveGateway SaveGateway { get; private set; }
    public static IMetaCommandBus CommandBus { get; private set; }

    private void Awake()
    {
        Facade = new MetaRefactorFacade();
        StateReader = new LegacyMetaStateReader();
        SaveGateway = new LegacyMetaSaveGateway();
        CommandBus = new LegacyMetaCommandBus(Facade);
    }

    private void Start()
    {
        PlayerInfoScript player = Singleton<PlayerInfoScript>.Instance;
        if (player != null && player.SaveData != null)
            Facade.SyncShadowFromLegacy(player.SaveData);
        else
            Debug.LogWarning("[META_ECS] MetaBootstrap.Start: PlayerInfoScript or SaveData not ready, shadow sync deferred.");
    }

    private void OnDestroy()
    {
        Facade = null;
        StateReader = null;
        SaveGateway = null;
        CommandBus = null;
    }
}
