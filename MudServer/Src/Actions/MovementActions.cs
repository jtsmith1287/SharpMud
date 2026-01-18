using System;
using System.Linq;
using MudServer.Entity;
using MudServer.Enums;
using MudServer.Util;
using MudServer.World;

namespace MudServer.Actions {
public static class MovementActions {
    public static void Move(PlayerCharacter player, string[] args) {
        if (!ActionUtility.TryGetRoom(player, out Room room)) {
            return;
        }

        string direction = args[0];
        if (!room.ConnectedRooms.TryGetValue(direction, out Coordinate3 locationOfNewRoom)) {
            player.SendToClient("There's no exit in that direction!", Color.Red);
            return;
        }

        Exit exit = null;
        room.Exits.TryGetValue(direction, out exit);

        bool isHidden = false;
        bool isSecret = false;
        bool isOpen = true;
        bool isLocked = false;

        if (exit != null) {
            isHidden = !player.QuestLog.DiscoveredExits.Contains(exit.GetPathId()) && exit.IsHidden(room.Location);
            isSecret = exit.IsSecret(room.Location);
            isOpen = exit.Open;
            isLocked = exit.Locked;
        }

        if (isHidden) {
            player.SendToClient("There's no exit in that direction!", Color.Red);
            return;
        }

        if (!isOpen) {
            player.SendToClient("The door is closed.", Color.Red);
            return;
        }

        if (isLocked) {
            player.SendToClient("The door is locked.", Color.Red);
            return;
        }

        // Reveal if it was a secret or hidden exit and we are not sneaking
        if (exit != null && (isSecret || exit.IsHidden(room.Location)) && !player.Hidden) {
            string exitId = exit.GetPathId();
            if (!player.QuestLog.DiscoveredExits.Contains(exitId)) {
                foreach (PlayerCharacter p in room.EntitiesHere
                             .Select(PlayerCharacter.GetPlayerByID).Where(p => p != null)) {
                    if (!p.QuestLog.DiscoveredExits.Contains(exitId)) {
                        p.QuestLog.DiscoveredExits.Add(exitId);
                    }

                    p.SendToClient(
                        $"You have discovered a secret exit to the {direction}!", Color.Cyan
                    );
                }
            }
        }

        player.Move(locationOfNewRoom);
        if (player.GameState == GameState.Resting) {
            player.GameState = GameState.Idle;
        }
    }

    public static void Sneak(PlayerCharacter player, string[] args) {
        if (player.GameState == GameState.Combat) {
            player.SendToClient("\n\tYou can't sneak! You're being stared at!\n");
            return;
        }

        //TODO: This should be chance
        player.Hidden = true;
        player.SendToClient("You think you're sneaking...", Color.Magenta);
    }
}
}
