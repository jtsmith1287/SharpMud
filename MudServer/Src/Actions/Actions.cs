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
            { "picklock", PickLock },
            { "unlock", Unlock },
            { "bash", Bash },
            { "search", Search }
        };

    private static void Search(PlayerCharacter player, string[] args) {
        if (player.Hidden) {
            player.SendToClient("You can't search while sneaking.");
            return;
        }

        Room room = World.World.GetRoom(player.Location);
        if (room == null) return;

        bool foundSomething = false;
        foreach (var exitEntry in room.Exits) {
            Exit exit = exitEntry.Value;
            if (exit.IsSecret(room.Location) || exit.IsHidden(room.Location)) {
                string exitId = exit.GetPathId();
                if (!player.QuestLog.DiscoveredExits.Contains(exitId)) {
                    // Reveal it to everyone in the room
                    foreach (Guid id in room.EntitiesHere) {
                        PlayerCharacter p = PlayerCharacter.GetPlayerByID(id);
                        if (p != null) {
                            if (!p.QuestLog.DiscoveredExits.Contains(exitId)) {
                                p.QuestLog.DiscoveredExits.Add(exitId);
                            }
                            p.SendToClient($"You have discovered a secret exit to the {exitEntry.Key}!", Color.Cyan);
                        }
                    }
                    foundSomething = true;
                }
            }
        }

        foreach (Guid id in room.EntitiesHere) {
            Entity.Entity entity = World.World.GetEntity(id);
            if (entity != null && entity.Hidden && entity.Id != player.Id) {
                entity.Hidden = false;
                foundSomething = true;
                player.SendToClient($"You have revealed {entity.Name}!", Color.Cyan);
                player.BroadcastLocal($"{player.Name} has revealed {entity.Name}!", Color.Yellow);
            }
        }

        if (!foundSomething) {
            player.SendToClient("You search around but find nothing unusual.");
            player.BroadcastLocal(player.Name + " searches the area thoroughly.", Color.Yellow);
        }
    }

    private static void Unlock(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Unlock what direction?", Color.Red);
            return;
        }

        string direction = args[1];
        Room room = World.World.GetRoom(player.Location);
        if (room == null) return;

        if (room.ConnectedRooms.TryGetValue(direction, out _)) {
            if (room.Exits.TryGetValue(direction, out Exit exit)) {
                if (!exit.Locked) {
                    player.SendToClient("It's already unlocked.");
                    return;
                }

                // Stub: Assume key is always present for now
                bool hasKey = true; 

                if (hasKey) {
                    exit.Locked = false;
                    player.SendToClient($"You unlock the door to the {direction}.", Color.Green);
                    player.BroadcastLocal($"{player.Name} unlocks the door to the {direction}.", Color.Yellow);
                } else {
                    player.SendToClient("You don't have the key.", Color.Red);
                }
            } else {
                player.SendToClient("There's nothing to unlock that way.");
            }
        } else {
            player.SendToClient("There's no exit in that direction.");
        }
    }

    private static void PickLock(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Picklock what direction?", Color.Red);
            return;
        }

        string direction = args[1];
        Room room = World.World.GetRoom(player.Location);
        if (room == null) return;

        if (room.ConnectedRooms.TryGetValue(direction, out _)) {
            if (room.Exits.TryGetValue(direction, out Exit exit)) {
                if (!exit.Locked) {
                    player.SendToClient("It's already unlocked.");
                    return;
                }

                // Stub: Assume lockpick is always present for now
                bool hasLockpick = true;

                if (hasLockpick) {
                    // 50/50 chance at 10 Dex. formula: Dex / 20.0
                    double chance = player.Stats.Dex / 20.0;
                    Random rnd = new Random();
                    if (rnd.NextDouble() < chance) {
                        exit.Locked = false;
                        player.SendToClient($"*Click* You successfully pick the lock to the {direction}!", Color.Green);
                        player.BroadcastLocal($"{player.Name} successfully picks the lock to the {direction}.", Color.Yellow);
                    } else {
                        player.SendToClient("You fail to pick the lock.", Color.Red);
                        player.BroadcastLocal($"{player.Name} attempts to pick the lock to the {direction} but fails.", Color.Yellow);
                    }
                } else {
                    player.SendToClient("You need a lockpick to do that.", Color.Red);
                }
            } else {
                player.SendToClient("There's nothing to picklock that way.");
            }
        } else {
            player.SendToClient("There's no exit in that direction.");
        }
    }

    private static void Bash(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Bash what direction?", Color.Red);
            return;
        }

        string direction = args[1];
        Room room = World.World.GetRoom(player.Location);
        if (room == null) return;

        if (room.ConnectedRooms.TryGetValue(direction, out _)) {
            if (room.Exits.TryGetValue(direction, out Exit exit)) {
                if (!exit.Locked) {
                    player.SendToClient("It's already unlocked.");
                    return;
                }

                // 50/50 chance at 10 Str. formula: Str / 20.0
                double chance = player.Stats.Str / 20.0;
                Random rnd = new Random();
                if (rnd.NextDouble() < chance) {
                    exit.Locked = false;
                    player.SendToClient($"With a heavy thud, you bash open the door to the {direction}!", Color.Green);
                    player.BroadcastLocal($"{player.Name} bashes open the door to the {direction}!", Color.Yellow);
                } else {
                    int damage = (int)(player.Stats.Str * 0.10);
                    if (damage < 1) damage = 1;
                    player.ApplyDamage(damage);
                    player.SendToClient($"You slam into the door to the {direction} but it holds firm! You take {damage} damage.", Color.Red);
                    player.BroadcastLocal($"{player.Name} slams into the door to the {direction} but fails to budge it.", Color.Yellow);
                }
            } else {
                player.SendToClient("There's nothing to bash that way.");
            }
        } else {
            player.SendToClient("There's no exit in that direction.");
        }
    }


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
        foreach (Entity.Entity entity in room.EntitiesHere.Select(World.World.GetEntity)
                     .Where(entity => entity != null && entity.Id != player.Id)) {
            if (!(entity is BaseMobile mobile)) continue;

            if (!ArgumentHandler.TryAutoComplete(name, mobile.Name) &&
                (mobile.Stats == null || !ArgumentHandler.TryAutoComplete(name, mobile.Stats.Name))) continue;
            player.Target = mobile;
            player.GameState = GameState.Combat;
            break;
        }

        if (player.GameState == GameState.Combat) {
            player.LastCombatTick = World.World.CombatTick;
            player.SendToClient(
                $"* Combat engaged with {player.Target.Name}! *", Color.Cyan
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
                Entity.Entity entity = World.World.GetEntity(id);
                switch (entity) {
                    case null:
                    case BaseMobile target when target.Hidden && id != player.Id:
                        continue;
                    case BaseMobile target when !ArgumentHandler.TryAutoComplete(targetName, target.Name) &&
                                                (target.Stats == null || !ArgumentHandler.TryAutoComplete(
                                                    targetName, target.Stats.Name
                                                )):
                        continue;
                    case BaseMobile target:
                        ViewStats(player, target);
                        return;
                }

                if (!(entity is Item item)) continue;
                if (item.Hidden) continue;
                if (!ArgumentHandler.TryAutoComplete(targetName, item.Name)) continue;

                player.SendToClient($"\n{item.Name}\n{item.Description}\n", Color.Green);
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

    public static void MoveRooms(PlayerCharacter player, string[] args) {
        Room room = World.World.GetRoom(player.Location);
        if (room != null) {
            Coordinate3 locationOfNewRoom;
            if (room.ConnectedRooms.TryGetValue(args[0], out locationOfNewRoom)) {
                bool isHidden = false;
                bool isOpen = true;
                bool isSecret = false;
                Exit exit = null;
                if (room.Exits.TryGetValue(args[0], out exit)) {
                    isHidden = exit.IsHidden(room.Location);
                    isSecret = exit.IsSecret(room.Location);

                    if (isHidden && player.QuestLog.DiscoveredExits.Contains(exit.GetPathId())) {
                        isHidden = false;
                    }

                    isOpen = exit.Open;
                }

                if (!isHidden) {
                    if (isOpen) {
                        if (exit != null && exit.Locked) {
                            player.SendToClient("The door is locked.", Color.Red);
                            return;
                        }

                        // Reveal if it was a secret or hidden exit and we are not sneaking
                        if (exit != null && (isSecret || exit.IsHidden(room.Location)) && !player.Hidden) {
                            string exitId = exit.GetPathId();
                            if (!player.QuestLog.DiscoveredExits.Contains(exitId)) {
                                foreach (Guid id in room.EntitiesHere) {
                                    PlayerCharacter p = PlayerCharacter.GetPlayerByID(id);
                                    if (p != null) {
                                        if (!p.QuestLog.DiscoveredExits.Contains(exitId)) {
                                            p.QuestLog.DiscoveredExits.Add(exitId);
                                        }
                                        p.SendToClient($"You have discovered a secret exit to the {args[0]}!", Color.Cyan);
                                    }
                                }
                            }
                        }

                        player.Move(locationOfNewRoom);
                        if (player.GameState == GameState.Resting) {
                            player.GameState = GameState.Idle;
                        }
                    } else {
                        player.SendToClient("The door is closed.", Color.Red);
                    }
                } else {
                    player.SendToClient("There's no exit in that direction!", Color.Red);
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
