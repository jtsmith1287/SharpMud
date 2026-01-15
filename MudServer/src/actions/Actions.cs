using System;
using System.Collections.Generic;
using GameCore.Util;
using ServerCore.Util;

namespace GameCore {
public static class Actions {
    public static readonly Dictionary<string, Action<PlayerEntity, string[]>> ActionCalls =
        new Dictionary<string, Action<PlayerEntity, string[]>> {
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


    public static void ShowMap(PlayerEntity player, string[] args) {
        int radius = 3;
        if (args.Length > 1) {
            int.TryParse(args[1], out radius);
        }

        player.SendToClient(AnsiMap.Display(player.Location, radius));
    }

    public static void Rest(PlayerEntity player, string[] args) {
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

    public static void Attack(PlayerEntity player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Attack what?", Color.Red);
            return;
        }

        Room room = World.GetRoom(player.Location);
        if (room == null) {
            player.SendToClient("Woah, you're nowhere. Try logging in again.", Color.Red);
            return;
        }

        string name = args[1];
        PlayerEntity targetPlayer;
        Mobile targetMob;
        foreach (Guid id in room.EntitiesHere) {
            if (World.Mobiles.TryGetValue(id, out targetMob)) {
                if (ArgumentHandler.AutoComplete(name, targetMob.Name) ||
                    ArgumentHandler.AutoComplete(name, targetMob.Stats.Name)) {
                    player.Target = targetMob;
                    player.GameState = GameState.Combat;
                    break;
                }
            } else if (PlayerEntity.Players.TryGetValue(id, out targetPlayer)) {
                if (ArgumentHandler.AutoComplete(name, targetPlayer.Name)) {
                    player.Target = targetPlayer;
                    player.GameState = GameState.Combat;
                    break;
                }
            }
        }

        if (player.GameState == GameState.Combat) {
            player.LastCombatTick = World.CombatTick;
            player.CombatEnergy = 0;
            player.SendToClient(
                string.Format(
                    "* Combat engaged with {0}! *", player.Target.Name
                ), Color.White
            );
        } else {
            player.SendToClient("Nothing here by that name...", Color.Red);
        }
    }

    public static void Look(PlayerEntity player, string[] args) {
        Room room = World.GetRoom(player.Location);
        if (room == null) {
            player.SendToClient("Somehow... you're nowhere. Try logging in again.");
            return;
        }

        if (args.Length > 1) {
            string targetName = args[1];
            PlayerEntity targetPlayer;
            Mobile targetMob;

            foreach (Guid id in room.EntitiesHere) {
                BaseMobile target = null;

                if (id == player.ID) {
                    target = player;
                } else if (PlayerEntity.Players.TryGetValue(id, out targetPlayer)) {
                    if (!targetPlayer.Hidden) {
                        target = targetPlayer;
                    }
                } else if (World.Mobiles.TryGetValue(id, out targetMob)) {
                    if (!targetMob.Hidden) {
                        target = targetMob;
                    }
                }

                if (target != null && (ArgumentHandler.AutoComplete(targetName, target.Name) ||
                                       (target is Mobile && ArgumentHandler.AutoComplete(
                                           targetName, target.Stats.Name
                                       )))) {
                    ViewStats(player, target);
                    return;
                }
            }
            // If we got here, we didn't find the entity. 
            // The requirement says "stub the entity search results to return the default look behavior"
        }

        string rawString = "\n " +
                           Color.Green + "{0}\n" +
                           Color.GreenD + "=============================\n{1}" +
                           Color.Cyan + "\nPlayers: {2}" +
                           Color.Magenta + "\nAlso here: {3}" +
                           Color.White + "\nExits: {4}\n" +
                           Color.Reset;
        string visiblePlayers = "";
        string visibleMobs = "";
        string exits = "";

        foreach (Guid id in room.EntitiesHere) {
            if (id == player.ID) {
                continue;
            }

            if (PlayerEntity.Players.TryGetValue(id, out var playerInRoom)) {
                if (!playerInRoom.Hidden) {
                    visiblePlayers += $"{playerInRoom.Name}, ";
                }
            } else if (World.Mobiles.TryGetValue(id, out var mobInRoom)) {
                if (!mobInRoom.Hidden) {
                    visibleMobs += $"{mobInRoom.Name}, ";
                }
            }
        }

        foreach (var entry in room.ConnectedRooms) {
            exits += $"{entry.Key}, ";
        }

        string mesage = string.Format(
            rawString,
            room.Name,
            room.Description,
            visiblePlayers,
            visibleMobs,
            exits
        );
        player.SendToClient(mesage, Color.GreenD);
    }

    public static void MoveRooms(PlayerEntity player, string[] args) {
        Room room = World.GetRoom(player.Location);
        if (room != null) {
            Coordinate3 locationOfNewRoom;
            if (room.ConnectedRooms.TryGetValue(args[0], out locationOfNewRoom) ||
                room.InvisibleConnections.TryGetValue(args[0], out locationOfNewRoom)) {
                player.Move(locationOfNewRoom);
            } else {
                Console.Write(locationOfNewRoom);
                Console.WriteLine(args[0]);
                player.SendToClient("There's no exit in that direction!", Color.Red);
            }
        } else {
            player.SendToClient(
                "Woah. Somethin' is busted. You're nowhere -- so please re-log in.", Color.Red
            );
        }
    }

    public static void ShowExp(PlayerEntity player, string[] args) {
        player.SendToClient(
            string.Format(
                "Experience: {0}/{1}",
                player.Stats.Exp, player.Stats.ExpToNextLevel
            )
        );
    }

    public static void Sneak(PlayerEntity player, string[] args) {
        if (player.GameState == GameState.Combat) {
            player.SendToClient("\n\tYou can't sneak! You're being stared at!\n");
            return;
        }

        //TODO: This should be chance
        player.Hidden = true;
        player.SendToClient("You are sneaking...", Color.Magenta);
    }

    public static void ViewAllPlayers(PlayerEntity player, string[] args) {
        string message = "\n";
        foreach (var entry in PlayerEntity.Players) {
            message += string.Format("Name: {0} -- Level: {1}\n", entry.Value.Name, entry.Value.Stats.Level);
        }

        player.SendToClient(message);
    }

    public static void ViewStats(PlayerEntity viewer, string[] args) {
        ViewStats(viewer, viewer);
    }

    public static void ViewStats(PlayerEntity viewer, BaseMobile entity) {
        string message = "\n";
        message += "==========================================\n";
        message += string.Format(" Name: {0}\n", entity.Stats.Name);
        message += string.Format(" Level: {0}\n", entity.Stats.Level);
        message += string.Format(" TNL: {0}\n", entity.Stats.ExpToNextLevel - entity.Stats.Exp);
        message += "------------------------------------------\n";
        message += string.Format(
            " {0,-15} | {1,4} / {2,-4} \n", "Health:", entity.Stats.Health, entity.Stats.MaxHealth
        );
        message += string.Format(" {0,-15} | {1,-4} + {2,-4} \n", "Strength:", entity.Stats.Str, entity.Stats.BonusStr);
        message += string.Format(
            " {0,-15} | {1,-4} + {2,-4} \n", "Dexterity:", entity.Stats.Dex, entity.Stats.BonusDex
        );
        message += string.Format(
            " {0,-15} | {1,-4} + {2,-4} \n", "Intelligence:", entity.Stats.Int, entity.Stats.BonusInt
        );
        message += "==========================================\n";
        viewer.SendToClient(message);
    }
}
}
