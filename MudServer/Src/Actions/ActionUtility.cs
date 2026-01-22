using System;
using System.Collections.Generic;
using System.Linq;
using MudServer.Entity;
using MudServer.Util;
using MudServer.World;

namespace MudServer.Actions {
public static class ActionUtility {
    public enum MessageType {
        Generic,
        Info,
        Success,
        Failure,
        PublicChat,
        PrivateChat,
    }

    public static Dictionary<MessageType, string> MessageColorMap = new Dictionary<MessageType, string>() {
        { MessageType.Generic, Color.White },
        { MessageType.Info, Color.Yellow },
        { MessageType.Success, Color.Green },
        { MessageType.Failure, Color.Red },
        { MessageType.PublicChat, Color.GreenD },
        { MessageType.PrivateChat, Color.Magenta }
        
    };
    
    public static bool TryGetRoom(PlayerCharacter player, out Room room) {
        if (World.World.TryGetRoom(player.Location, out room)) {
            return true;
        }

        player.SendToClient("Woah, you're nowhere. Try logging in again.", Color.Red);
        return false;
    }

    public static bool IsInCombat(PlayerCharacter player, bool sendError = true) {
        if (player.GameState != Enums.GameState.Combat) return false;

        if (sendError) {
            player.SendToClient("You cannot do that while in combat!", Color.Red);
        }

        return true;
    }

    public static bool IsSneaking(PlayerCharacter player, bool sendError = true) {
        if (!player.Hidden) return false;

        if (sendError) {
            player.SendToClient("You cannot do that while sneaking.", Color.Red);
        }

        return true;
    }

    public static bool TryGetDirection(string input, out string direction) {
        direction = null;
        if (string.IsNullOrEmpty(input)) return false;

        // Normalize input - handle both full names and abbreviations
        foreach (KeyValuePair<string, string> kvp in Room.DirectionNames) {
            if (!kvp.Key.Equals(input, StringComparison.OrdinalIgnoreCase) &&
                !kvp.Value.Equals(input, StringComparison.OrdinalIgnoreCase)) continue;

            direction = kvp.Value;
            return true;
        }

        // Fallback to exactly as typed if it matches a key in ConnectedRooms (shouldn't happen with standard dirs but for custom ones)
        direction = input.ToLower();
        return true;
    }

    public static bool TryGetExit(Room room, string direction, out Exit exit, PlayerCharacter player = null) {
        exit = null;
        return room.Exits.TryGetValue(direction, out exit);
    }

    public static void SendMessage(PlayerCharacter player, string localMsg, string roomMsg = null, MessageType type = MessageType.Generic) {
        player.SendToClient(localMsg, MessageColorMap[type]);
        if (!string.IsNullOrEmpty(roomMsg)) {
            player.BroadcastLocal(roomMsg, MessageColorMap[type]);
        }
    }

    public static bool RollSuccess(int statValue, double divisor = 20.0) {
        double chance = statValue / divisor;
        return new Random().NextDouble() < chance;
    }

    public static BaseMobile FindMobileInRoom(Room room, string targetName, PlayerCharacter player = null) {
        foreach (Guid id in room.EntitiesHere) {
            if (player != null && id == player.Id) continue;

            Entity.Entity entity = World.World.GetEntity(id);
            if (!(entity is BaseMobile mobile)) continue;

            if (mobile.Hidden && (player == null || id != player.Id)) continue;

            if (ArgumentHandler.TryAutoComplete(targetName, mobile.Name) ||
                (mobile.Stats != null && ArgumentHandler.TryAutoComplete(targetName, mobile.Stats.Name))) {
                return mobile;
            }
        }

        return null;
    }

    public static MudServer.Entity.Entity
        FindEntityInRoom(Room room, string targetName, PlayerCharacter player = null) {
        foreach (Entity.Entity entity in from id in room.EntitiesHere
                 let entity = World.World.GetEntity(id)
                 where entity != null
                 where !entity.Hidden || (player != null && id == player.Id)
                 select entity) {
            if (ArgumentHandler.TryAutoComplete(targetName, entity.Name) || entity is BaseMobile mobile &&
                mobile.Stats != null &&
                ArgumentHandler.TryAutoComplete(targetName, mobile.Stats.Name)) {
                return entity;
            }
        }

        return null;
    }
}
}
