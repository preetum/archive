using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omron.Framework.Networking
{
    public enum MessageDataType : byte
    {
        Text,
        ActorUpdate,
        ActorChangeUpdate,
        ActorPushUpdate,
        SendActors,
        SendActorsAggressive,
        EngageActor,
        SendActorsToBuild,
        ActorCreated,
        ActorDestroyed,
        RequestFaction,
        FactionRegistered,
        RequestFinalize,
        GameFinalized,
        GameStarting,
        SendClientPregameData,
        ActorMenuCommand,
        PushEffect,
        RequestBuildingConstruct,
        ResourceUpdate,
        ResearchUnlocked,
        BuildingUnlocked,
        GameOver
    }
    public enum PlayerType : byte
    {
        Human,
        Wumpus
    }
}
