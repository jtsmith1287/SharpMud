using System.Collections.Generic;
using System.Linq;
using System.Text;
using MudServer.Entity;
using MudServer.Enums;
using MudServer.Util;
using MudServer.World;

namespace MudServer.Actions {
public static class InteractionActions {
    public static void Search(PlayerCharacter player, string[] args) {
        if (ActionUtility.IsSneaking(player)) {
            return;
        }

        if (!ActionUtility.TryGetRoom(player, out Room room)) return;

        bool foundSomething = false;
        foreach (KeyValuePair<string, Exit> exitEntry in room.Exits) {
            Exit exit = exitEntry.Value;
            if (!exit.IsSecret(room.Location) && !exit.IsHidden(room.Location)) continue;
            string exitId = exit.GetPathId();
            if (player.QuestLog.DiscoveredExits.Contains(exitId)) continue;
            // Reveal it to everyone in the room
            foreach (PlayerCharacter p in room.EntitiesHere.Select(PlayerCharacter.GetPlayerByID)
                         .Where(p => p != null)) {
                if (!p.QuestLog.DiscoveredExits.Contains(exitId)) {
                    p.QuestLog.DiscoveredExits.Add(exitId);
                }

                p.SendToClient($"You have discovered a secret exit to the {exitEntry.Key}!", Color.Cyan);
            }

            foundSomething = true;
        }

        foreach (Entity.Entity entity in room.EntitiesHere.Select(World.World.GetEntity)
                     .Where(entity => entity != null && entity.Hidden && entity.Id != player.Id)) {
            entity.Hidden = false;
            foundSomething = true;
            player.SendToClient($"You have revealed {entity.Name}!", Color.Cyan);
            player.BroadcastLocal($"{player.Name} has revealed {entity.Name}!", Color.Yellow);
        }

        if (foundSomething) return;

        player.SendToClient("You search around but find nothing unusual.");
        player.BroadcastLocal(player.Name + " searches the area thoroughly.", Color.Yellow);
    }

    public static void Rest(PlayerCharacter player, string[] args) {
        if (ActionUtility.IsInCombat(player)) {
            return;
        }

        if (player.Stats.Health >= player.Stats.MaxHealth) {
            player.SendToClient("You are already at full health.", Color.Green);
            return;
        }

        player.GameState = GameState.Resting;
        ActionUtility.SendMessage(
            player,
            "You sit down and begin to rest...", player.Name + " sits down and begins to rest."
        );
    }

    public static void Unlock(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Unlock what direction?", Color.Red);
            return;
        }

        if (!ActionUtility.TryGetRoom(player, out Room room)) return;

        if (!ActionUtility.TryGetDirection(args[1], out string direction)) {
            player.SendToClient("That's not a valid direction.");
            return;
        }

        if (!room.ConnectedRooms.ContainsKey(direction)) {
            player.SendToClient("There's no exit in that direction.");
            return;
        }

        if (!room.Exits.TryGetValue(direction, out Exit exit)) {
            player.SendToClient("There's nothing to unlock that way.");
            return;
        }

        if (!exit.Locked) {
            player.SendToClient("It's already unlocked.");
            return;
        }

        // Stub: Assume key is always present for now
        bool hasKey = true;
        if (!hasKey) {
            player.SendToClient("You don't have the key.", Color.Red);
            return;
        }

        exit.Locked = false;
        ActionUtility.SendMessage(
            player,
            $"You unlock the door to the {direction}.",
            $"{player.Name} unlocks the door to the {direction}.",
            ActionUtility.MessageType.Success
        );
    }

    public static void PickLock(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Picklock what direction?", Color.Red);
            return;
        }

        if (!ActionUtility.TryGetRoom(player, out Room room)) return;

        if (!ActionUtility.TryGetDirection(args[1], out string direction)) {
            player.SendToClient("That's not a valid direction.");
            return;
        }

        if (!room.ConnectedRooms.ContainsKey(direction)) {
            player.SendToClient("There's no exit in that direction.");
            return;
        }

        if (!room.Exits.TryGetValue(direction, out Exit exit)) {
            player.SendToClient("There's nothing to picklock that way.");
            return;
        }

        if (!exit.Locked) {
            player.SendToClient("It's already unlocked.");
            return;
        }

        // Stub: Assume lockpick is always present for now
        bool hasLockpick = true;
        if (!hasLockpick) {
            player.SendToClient("You need a lockpick to do that.", Color.Red);
            return;
        }

        if (ActionUtility.RollSuccess(player.Stats.Dex)) {
            exit.Locked = false;
            ActionUtility.SendMessage(
                player,
                $"*Click* You successfully pick the lock to the {direction}!",
                $"{player.Name} successfully picks the lock to the {direction}.",
                ActionUtility.MessageType.Success
            );
        } else {
            ActionUtility.SendMessage(
                player,
                "You fail to pick the lock.",
                $"{player.Name} attempts to pick the lock to the {direction} but fails.",
                ActionUtility.MessageType.Failure
            );
        }
    }

    public static void Bash(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Bash what direction?", Color.Red);
            return;
        }

        if (!ActionUtility.TryGetRoom(player, out Room room)) return;

        if (!ActionUtility.TryGetDirection(args[1], out string direction)) {
            player.SendToClient("That's not a valid direction.");
            return;
        }

        if (!room.ConnectedRooms.ContainsKey(direction)) {
            player.SendToClient("There's no exit in that direction.");
            return;
        }

        if (!room.Exits.TryGetValue(direction, out Exit exit)) {
            player.SendToClient("There's nothing to bash that way.");
            return;
        }

        if (!exit.Locked) {
            player.SendToClient("It's already unlocked.");
            return;
        }

        if (ActionUtility.RollSuccess(player.Stats.Str)) {
            exit.Locked = false;
            exit.Open = true;
            ActionUtility.SendMessage(
                player,
                $"With a heavy thud, you bash open the door to the {direction}!",
                $"{player.Name} bashes open the door to the {direction}!",
                ActionUtility.MessageType.Success
            );
        } else {
            int damage = (int)(player.Stats.Str * 0.10);
            if (damage < 1) damage = 1;
            player.ApplyDamage(damage);
            ActionUtility.SendMessage(
                player,
                $"You slam into the door to the {direction} but it holds firm! You take {damage} damage.",
                $"{player.Name} slams into the door to the {direction} but fails to budge it.",
                ActionUtility.MessageType.Failure
            );
        }
    }

    public static void Say(PlayerCharacter player, string[] args) {
        if (args.Length == 0) {
            player.SendToClient("Say what?");
            return;
        }

        string message = string.Join(" ", args.Skip(1));
        ActionUtility.SendMessage(
            player,
            $"You say, \"{message}\"",
            $"{player.Name} says, \"{message}\"",
            ActionUtility.MessageType.PublicChat
        );
    }

    public static void Inventory(PlayerCharacter player, string[] args) {
        StringBuilder message = new StringBuilder();

        foreach (Item item in player.Inventory.GetItems()
                     .OrderBy(i => i.Name)) {
            message.AppendLine(item.Name);
        }

        player.SendToClient(message.ToString());
    }
}
}
