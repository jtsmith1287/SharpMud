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
    public static bool UseNewSystem = true;

    public static readonly Dictionary<string, Action<PlayerCharacter, string[]>> NewActionCalls =
        new Dictionary<string, Action<PlayerCharacter, string[]>> {
            { "attack", CombatActions.Attack },
            { "north", MovementActions.Move },
            { "south", MovementActions.Move },
            { "east", MovementActions.Move },
            { "west", MovementActions.Move },
            { "up", MovementActions.Move },
            { "down", MovementActions.Move },
            { "stats", InformationActions.ViewStats },
            { "who", InformationActions.ViewAllPlayers },
            { "look", InformationActions.Look },
            { "exp", InformationActions.ShowExp },
            { "sneak", MovementActions.Sneak },
            { "map", InformationActions.ShowMap },
            { "rest", InteractionActions.Rest },
            { "picklock", InteractionActions.PickLock },
            { "unlock", InteractionActions.Unlock },
            { "bash", InteractionActions.Bash },
            { "search", InteractionActions.Search }
        };

    private static readonly Dictionary<string, Func<PlayerCharacter, string, string>> ArgumentAutocompleters =
        new Dictionary<string, Func<PlayerCharacter, string, string>> {
            { "attack", AutocompleteMobileInRoom },
            { "look", AutocompleteEntityInRoom },
            { "unlock", AutocompleteDirection },
            { "picklock", AutocompleteDirection },
            { "bash", AutocompleteDirection },
        };

    public static void DoAction(PlayerCharacter player, string line) {
        if (!UseNewSystem) {
            ArgumentHandler.HandleLine(line, player);
            return;
        }

        if (player.GameState == GameState.Dead) {
            player.SendToClient("\n\tBut you're dead... Just relax and enjoy the ride.\n", Color.Red);
            return;
        }

        string[] args = ArgumentHandler.ProcessLine(line);
        if (args.Length == 0) {
            player.DisplayVitals();
            return;
        }

        // 1. Autocomplete command
        string commandName = null;
        Action<PlayerCharacter, string[]> action = null;

        foreach (var entry in NewActionCalls) {
            if (ArgumentHandler.TryAutoComplete(args[0], entry.Key)) {
                commandName = entry.Key;
                action = entry.Value;
                break;
            }
        }

        if (commandName == null && player.Admin) {
            foreach (var entry in AdminActions.ActionCalls) {
                if (ArgumentHandler.TryAutoComplete(args[0], entry.Key)) {
                    commandName = entry.Key;
                    action = entry.Value;
                    break;
                }
            }
        }

        if (commandName == null) {
            player.SendToClient("Nope, that's not a thing, sorry!", Color.Yellow);
            return;
        }

        // 2. Autocomplete arguments if needed
        string[] expandedArgs = new string[args.Length];
        expandedArgs[0] = commandName;

        for (int i = 1; i < args.Length; i++) {
            if (i == 1 && ArgumentAutocompleters.TryGetValue(commandName, out var autocompleter)) {
                string expanded = autocompleter(player, args[i]);
                expandedArgs[i] = expanded ?? args[i];
            } else {
                expandedArgs[i] = args[i];
            }
        }

        // 3. Execute action
        action(player, expandedArgs);
        player.DisplayVitals();
    }

    private static string AutocompleteMobileInRoom(PlayerCharacter player, string input) {
        if (!World.World.TryGetRoom(player.Location, out Room room)) return null;
        var mobile = ActionUtility.FindMobileInRoom(room, input, player);
        return mobile?.Name;
    }

    private static string AutocompleteEntityInRoom(PlayerCharacter player, string input) {
        if (!World.World.TryGetRoom(player.Location, out Room room)) return null;
        var entity = ActionUtility.FindEntityInRoom(room, input, player);
        return entity?.Name;
    }

    private static string AutocompleteDirection(PlayerCharacter player, string input) {
        if (ActionUtility.TryGetDirection(input, out string direction)) {
            return direction;
        }

        return null;
    }

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

    private const bool DEFAULT_IS_HIDDEN = false;
    private const bool DEFAULT_IS_SECRET = false;
    private const bool DEFAULT_IS_LOCKED = false;
    private const bool DEFAULT_IS_OPEN = true;

    private static void Search(PlayerCharacter player, string[] args) {
        if (player.Hidden) {
            player.SendToClient("You can't search while sneaking.");
            return;
        }

        if (!World.World.TryGetRoom(player.Location, out Room room)) return;

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

    private static void Unlock(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Unlock what direction?", Color.Red);
            return;
        }

        if (!World.World.TryGetRoom(player.Location, out Room room))
            return;

        string direction = args[1];

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
        player.SendToClient($"You unlock the door to the {direction}.", Color.Green);
        player.BroadcastLocal(
            $"{player.Name} unlocks the door to the {direction}.",
            Color.Yellow
        );
    }

    private static void PickLock(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Picklock what direction?", Color.Red);
            return;
        }

        if (!World.World.TryGetRoom(player.Location, out Room room))
            return;

        string direction = args[1];

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

        // 50/50 chance at 10 Dex. Formula: Dex / 20.0
        double chance = player.Stats.Dex / 20.0;

        if (new Random().NextDouble() < chance) {
            exit.Locked = false;
            player.SendToClient(
                $"*Click* You successfully pick the lock to the {direction}!",
                Color.Green
            );
            player.BroadcastLocal(
                $"{player.Name} successfully picks the lock to the {direction}.",
                Color.Yellow
            );
            return;
        }

        player.SendToClient("You fail to pick the lock.", Color.Red);
        player.BroadcastLocal(
            $"{player.Name} attempts to pick the lock to the {direction} but fails.",
            Color.Yellow
        );
    }

    private static void Bash(PlayerCharacter player, string[] args) {
        if (args.Length < 2) {
            player.SendToClient("Bash what direction?", Color.Red);
            return;
        }

        if (!World.World.TryGetRoom(player.Location, out Room room))
            return;

        string direction = args[1];

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

        // 50/50 chance at 10 Str. Formula: Str / 20.0
        double chance = player.Stats.Str / 20.0;

        Random rnd = new Random();
        if (rnd.NextDouble() >= chance) {
            int damage = (int)(player.Stats.Str * 0.10);
            if (damage < 1) damage = 1;

            player.ApplyDamage(damage);
            player.SendToClient(
                $"You slam into the door to the {direction} but it holds firm! You take {damage} damage.",
                Color.Red
            );
            player.BroadcastLocal(
                $"{player.Name} slams into the door to the {direction} but fails to budge it.",
                Color.Yellow
            );
            return;
        }

        exit.Locked = false;
        player.SendToClient(
            $"With a heavy thud, you bash open the door to the {direction}!",
            Color.Green
        );
        player.BroadcastLocal(
            $"{player.Name} bashes open the door to the {direction}!",
            Color.Yellow
        );
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

        if (!World.World.TryGetRoom(player.Location, out Room room)) {
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
        if (!World.World.TryGetRoom(player.Location, out Room room)) {
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
        if (!World.World.TryGetRoom(player.Location, out Room room)) {
            player.SendToClient(
                "Woah. Somethin' is busted. You're nowhere -- so please re-log in.", Color.Red
            );
            return;
        }

        Exit exit = null;
        if (!room.ConnectedRooms.TryGetValue(args[0], out Coordinate3 locationOfNewRoom)) {
            player.SendToClient("There's no exit in that direction!", Color.Red);
            return;
        }

        room.Exits.TryGetValue(args[0], out exit);

        bool isHidden = false;
        bool isSecret = false;
        bool isOpen = true;
        bool isLocked = false;

        if (exit != null) {
            isHidden = player.QuestLog.DiscoveredExits.Contains(exit.GetPathId())
                ? false
                : exit.IsHidden(room.Location);
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
                        $"You have discovered a secret exit to the {args[0]}!", Color.Cyan
                    );
                }
            }
        }

        player.Move(locationOfNewRoom);
        if (player.GameState == GameState.Resting) {
            player.GameState = GameState.Idle;
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
