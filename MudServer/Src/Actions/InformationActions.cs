using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MudServer.Entity;
using MudServer.Util;
using MudServer.World;

namespace MudServer.Actions {
public static class InformationActions {
    public static void ViewStats(PlayerCharacter player, string[] args) {
        ViewStats(player, player);
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

    public static void ViewAllPlayers(PlayerCharacter player, string[] args) {
        string message = "\n";
        foreach (var entry in PlayerCharacter.Players) {
            message += string.Format("Name: {0} -- Level: {1}\n", entry.Value.Name, entry.Value.Stats.Level);
        }

        player.SendToClient(message);
    }

    public static void ShowExp(PlayerCharacter player, string[] args) {
        player.SendToClient(
            $"Experience: {player.Stats.Exp}/{player.Stats.ExpToNextLevel}"
        );
    }

    public static void ShowMap(PlayerCharacter player, string[] args) {
        int radius = 3;
        if (args.Length > 1) {
            int.TryParse(args[1], out radius);
        }

        player.SendToClient(AnsiMap.Display(player.Location, radius));
    }

    public static void Look(PlayerCharacter player, string[] args) {
        if (!ActionUtility.TryGetRoom(player, out Room room)) {
            return;
        }

        if (args.Length > 1) {
            string targetName = args[1];
            var entity = ActionUtility.FindEntityInRoom(room, targetName, player);

            if (entity != null) {
                if (entity is BaseMobile mobile) {
                    ViewStats(player, mobile);
                    return;
                } else if (entity is Item item) {
                    player.SendToClient($"\n{item.Name}\n{item.Description}\n", Color.Green);
                    return;
                }
            }

            // If not found or couldn't look at it
            if (args.Length > 1) {
                player.SendToClient("You don't see that here.");
                return;
            }
        }

        string visiblePlayers = "";
        string visibleMobs = "";
        string visibleItems = "";

        foreach (Guid id in room.EntitiesHere.Where(id => id != player.Id)) {
            Entity.Entity entity = World.World.GetEntity(id);
            if (entity == null || entity.Hidden) continue;

            switch (entity) {
                case PlayerCharacter p:
                    visiblePlayers += $"{p.Name}, ";
                    break;
                case NonPlayerCharacter npc:
                    visibleMobs += $"{npc.Name}, ";
                    break;
                case Item item:
                    visibleItems += $"{item.Name}, ";
                    break;
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

        string exits = "";
        foreach (KeyValuePair<string, Coordinate3> entry in room.ConnectedRooms) {
            bool isSecret = false;
            bool isHidden = false;
            bool isOpen = true;

            if (room.Exits.TryGetValue(entry.Key, out Exit exit)) {
                isSecret = exit.IsSecret(room.Location);
                isHidden = exit.IsHidden(room.Location);

                if ((isSecret || isHidden) && player.QuestLog.DiscoveredExits.Contains(exit.GetPathId())) {
                    isSecret = false;
                    isHidden = false;
                }

                isOpen = exit.Open;
            }

            if (isSecret || isHidden) continue;

            string exitName = entry.Key;
            if (!isOpen) {
                exitName += " (closed)";
            }

            exits += $"{exitName}, ";
        }

        sb.Append(Color.White + "Exits: " + exits.TrimEnd(',', ' ') + "\n" + Color.Reset);

        player.SendToClient(sb.ToString(), Color.GreenD);
    }
}
}
