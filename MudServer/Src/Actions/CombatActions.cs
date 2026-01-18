using System;
using MudServer.Entity;
using MudServer.Enums;
using MudServer.Util;
using MudServer.World;

namespace MudServer.Actions {
public static class CombatActions {
    public static void Attack(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Attack what?", Color.Red);
            return;
        }

        if (!ActionUtility.TryGetRoom(player, out Room room)) {
            return;
        }

        string name = args[1];
        BaseMobile target = ActionUtility.FindMobileInRoom(room, name, player);

        if (target != null) {
            player.Target = target;
            player.GameState = GameState.Combat;
            player.LastCombatTick = World.World.CombatTick;
            player.SendToClient($"* Combat engaged with {player.Target.Name}! *", Color.Cyan);
        } else {
            player.SendToClient("Nothing here by that name...", Color.Red);
        }
    }
}
}
