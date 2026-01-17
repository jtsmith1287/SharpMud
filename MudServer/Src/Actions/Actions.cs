using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MudServer.Entity;
using MudServer.Util;
using MudServer.World;
using MudServer.Enums;

namespace MudServer.Actions {
public static class Actions {
    public static readonly Dictionary<string, Action<PlayerCharacter, string[]>> ActionCalls =
        new Dictionary<string, Action<PlayerCharacter, string[]>> {
            { "attack", Attack },
            { "north", MoveRooms },
            { "south", MoveRooms },
            { "east", MoveRooms },
            { "west", MoveRooms },
            { "up", MoveRooms },
            { "down", MoveRooms },
            { "stats", ViewStats },
            { "who", ViewAllPlayers },
            { "look", Look },
            { "exp", ShowExp },
            { "sneak", Sneak },
            { "map", ShowMap },
            { "rest", Rest },
        };


    public static void ShowMap(PlayerCharacter player, string[] args) {
        int radius = 3;
        if (args.Length > 1) {
            int.TryParse(args[1], out radius);
        }

        player.SendToClient(AnsiMap.Display(player.Location, radius));
    }

    public static void Rest(PlayerCharacter player, string[] args) {
        if (player.GameState == GameState.Combat) {
            player.SendToClient("You cannot rest while in combat!", Color.Red);
            return;
        }

        if (player.Stats.Health >= player.Stats.MaxHealth) {
            player.SendToClient("You are already at full health.", Color.Green);
            return;
        }

        player.GameState = GameState.Resting;
        player.SendToClient("You sit down and begin to rest...", Color.Cyan);
        player.BroadcastLocal(player.Name + " sits down and begins to rest.", Color.Yellow);
    }

    public static void Attack(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Attack what?", Color.Red);
            return;
        }

        Room room = World.World.GetRoom(player.Location);
        if (room == null) {
            player.SendToClient("Woah, you're nowhere. Try logging in again.", Color.Red);
            return;
        }

        string name = args[1];
        foreach (Guid id in room.EntitiesHere) {
            var entity = World.World.GetEntity(id);
            if (entity == null || entity.Id == player.Id) continue;

            if (entity is BaseMobile mobile) {
                if (ArgumentHandler.TryAutoComplete(name, mobile.Name) ||
                    (mobile.Stats != null && ArgumentHandler.TryAutoComplete(name, mobile.Stats.Name))) {
                    player.Target = mobile;
                    player.GameState = GameState.Combat;
                    break;
                }
            }
        }

        if (player.GameState == GameState.Combat) {
            player.LastCombatTick = World.World.CombatTick;
            player.SendToClient(
                string.Format(
                    "* Combat engaged with {0}! *", player.Target.Name
                ), Color.White
            );
        } else {
            player.SendToClient("Nothing here by that name...", Color.Red);
        }
    }

    public static void Look(PlayerCharacter player, string[] args) {
        Room room = World.World.GetRoom(player.Location);
        if (room == null) {
            player.SendToClient("Somehow... you're nowhere. Try logging in again.");
            return;
        }

        if (args.Length > 1) {
            string targetName = args[1];

            foreach (Guid id in room.EntitiesHere) {
                var entity = World.World.GetEntity(id);
                if (entity == null) continue;

                if (entity is BaseMobile target) {
                    if (!target.Hidden || id == player.Id) {
                        if (ArgumentHandler.TryAutoComplete(targetName, target.Name) ||
                            (target.Stats != null && ArgumentHandler.TryAutoComplete(targetName, target.Stats.Name))) {
                            ViewStats(player, target);
                            return;
                        }
                    }
                } else if (entity is Item item) {
                    if (!item.Hidden) {
                        if (ArgumentHandler.TryAutoComplete(targetName, item.Name)) {
                            player.SendToClient($"\n{item.Name}\n{item.Description}\n", Color.Green);
                            return;
                        }
                    }
                }
            }
        }

        string visiblePlayers = "";
        string visibleMobs = "";
        string visibleItems = "";

        foreach (Guid id in room.EntitiesHere.Where(id => id != player.Id)) {
            var entity = World.World.GetEntity(id);
            if (entity == null || entity.Hidden) continue;

            if (entity is PlayerCharacter p) {
                visiblePlayers += $"{p.Name}, ";
            } else if (entity is NonPlayerCharacter npc) {
                visibleMobs += $"{npc.Name}, ";
            } else if (entity is Item item) {
                visibleItems += $"{item.Name}, ";
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("\n " + Color.Green + room.Name + "\n");
        sb.Append(Color.GreenD + "=============================\n" + room.Description + "\n" + Color.Reset);

        if (!string.IsNullOrEmpty(visiblePlayers)) {
            sb.Append(Color.Cyan + "Players: " + visiblePlayers.TrimEnd(',', ' ') + "\n");
        }

        if (!string.IsNullOrEmpty(visibleMobs)) {
            sb.Append(Color.Magenta + "Also here: " + visibleMobs.TrimEnd(',', ' ') + "\n");
        }

        if (!string.IsNullOrEmpty(visibleItems)) {
            sb.Append(Color.Yellow + "Items: " + visibleItems.TrimEnd(',', ' ') + "\n");
        }

        string exits = room.ConnectedRooms.Aggregate("", (current, entry) => current + $"{entry.Key}, ");
        sb.Append(Color.White + "Exits: " + exits.TrimEnd(',', ' ') + "\n" + Color.Reset);

        player.SendToClient(sb.ToString(), Color.GreenD);
    }

    public static void MoveRooms(PlayerCharacter player, string[] args) {
        Room room = World.World.GetRoom(player.Location);
        if (room != null) {
            Coordinate3 locationOfNewRoom;
            if (room.ConnectedRooms.TryGetValue(args[0], out locationOfNewRoom) ||
                room.InvisibleConnections.TryGetValue(args[0], out locationOfNewRoom)) {
                player.Move(locationOfNewRoom);
                if (player.GameState == GameState.Resting) {
                    player.GameState = GameState.Idle;
                }
            } else {
                player.SendToClient("There's no exit in that direction!", Color.Red);
            }
        } else {
            player.SendToClient(
                "Woah. Somethin' is busted. You're nowhere -- so please re-log in.", Color.Red
            );
        }
    }

    public static void ShowExp(PlayerCharacter player, string[] args) {
        player.SendToClient(
            $"Experience: {player.Stats.Exp}/{player.Stats.ExpToNextLevel}"
        );
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

    public static void ViewAllPlayers(PlayerCharacter player, string[] args) {
        string message = "\n";
        foreach (var entry in PlayerCharacter.Players) {
            message += string.Format("Name: {0} -- Level: {1}\n", entry.Value.Name, entry.Value.Stats.Level);
        }

        player.SendToClient(message);
    }

    public static void ViewStats(PlayerCharacter viewer, string[] args) {
        ViewStats(viewer, viewer);
    }

    public static void ViewStats(PlayerCharacter viewer, BaseMobile entity) {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("==========================================");
        sb.AppendLine($" Name: {entity.Stats.Name}");
        sb.AppendLine($" Level: {entity.Stats.Level}");
        sb.AppendLine($" TNL: {entity.Stats.ExpToNextLevel - entity.Stats.Exp}");
        sb.AppendLine("------------------------------------------");

        AppendStat(sb, "Health", entity.Stats.Health, entity.Stats.MaxHealth);
        sb.AppendLine("------------------------------------------");
        AppendStat(sb, "Strength", entity.Stats.Str, entity.Stats.BonusStr);
        AppendStat(sb, "Dexterity", entity.Stats.Dex, entity.Stats.BonusDex);
        AppendStat(sb, "Intelligence", entity.Stats.Int, entity.Stats.BonusInt);
        AppendStat(sb, "Constitution", entity.Stats.Con, entity.Stats.BonusCon);
        AppendStat(sb, "Speed", entity.Stats.Speed, entity.Stats.BonusSpeed);

        sb.AppendLine("==========================================");

        viewer.SendToClient(sb.ToString());
    }

    private static void AppendStat(StringBuilder sb, string label, int baseValue, int bonusValue, string suffix = "") {
        sb.AppendLine(
            $" {label + ":",-15} | {baseValue,-4} + {bonusValue,-4}{suffix}"
        );
    }
}
}
