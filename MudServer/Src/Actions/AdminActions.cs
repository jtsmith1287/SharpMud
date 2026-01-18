using System;
using System.Collections.Generic;
using System.Linq;
using MudServer.Entity;
using MudServer.Util;
using MudServer.World;
using MudServer.Enums;
using MudServer.Server;

namespace MudServer.Actions {
public static class AdminActions {
    public static readonly Dictionary<string, Action<PlayerCharacter, string[]>> ActionCalls =
        new Dictionary<string, Action<PlayerCharacter, string[]>> {
            { "build", BuildRoom },
            { "spawn", CreateSpawner },
            { "view", ViewEntity },
            { "save", SaveAll },
            { "god", GodMode }
        };

    public static void GodMode(PlayerCharacter player, string[] args) {
        player.GodMode = !player.GodMode;
        player.SendToClient($"God mode {(player.GodMode ? "enabled" : "disabled")}", Color.Yellow);
    }

    public static void SaveAll(PlayerCharacter player, string[] args) {
        DataManager.SaveData(
            DataPaths.IdData,
            DataPaths.Creatures,
            DataPaths.UserId,
            DataPaths.UserPwd
        );

        // Save all maps
        HashSet<string> savedMaps = new HashSet<string>();
        foreach (var room in World.World.Rooms.Values) {
            if (!string.IsNullOrEmpty(room.MapName) && !savedMaps.Contains(room.MapName)) {
                DataManager.SaveMap(room.MapName);
                savedMaps.Add(room.MapName);
            }
        }

        player.SendToClient("Saved all data.", Color.White);
    }

    public static void ViewEntity(PlayerCharacter player, string[] args) {
        // TODO: Implement viewing entity details
    }

    public static void BuildRoom(PlayerCharacter player, string[] args) {
        if (!ActionUtility.TryGetRoom(player, out Room playerRoom)) {
            return;
        }

        if (args.Length < 2) {
            player.SendToClient("Please provide a direction!", Color.Red);
            return;
        }

        if (!ActionUtility.TryGetDirection(args[1], out string direction)) {
            player.SendToClient(
                "That's not a valid direction for this command. Try using 'e' or 'n'.", Color.Red
            );
            return;
        }

        Coordinate3 location = new Coordinate3(player.Location.X, player.Location.Y, player.Location.Z);
        Coordinate3 newRoomLocation = location + Room.DirectionMap[args[1]];

        // A room already exists in this location
        if (!World.World.TryGetRoom(newRoomLocation, out Room newRoom)) {
            newRoom = new Room(newRoomLocation, "###");
            newRoom.MapName = playerRoom.MapName; // Inherit MapName from current room

            lock (playerRoom.ConnectedRooms) {
                playerRoom.ConnectedRooms.Add(direction, newRoom.Location);
            }

            lock (newRoom.ConnectedRooms) {
                newRoom.ConnectedRooms.Add(newRoom.GetDirection(playerRoom.Location), playerRoom.Location);
            }

            DataManager.SaveRoom(playerRoom);
            DataManager.SaveRoom(newRoom);

            player.SendToClient(
                string.Format(
                    "{0} has been created to the {1}!).", newRoom.Name, direction
                ), Color.White
            );
            return;
        }

        player.SendToClient(
            string.Format(
                "You're trying to build a room that already exists. ({0}, {1}, {2})\n" +
                "Would you like to link them? (y|n)",
                newRoom.Location.X,
                newRoom.Location.Y,
                newRoom.Location.Z
            ), Color.Yellow
        );

        string reply = player.WaitForClientReply();

        if (reply != null && (reply == "y" || reply == "yes")) {
            lock (playerRoom.ConnectedRooms) {
                playerRoom.ConnectedRooms.Add(direction, newRoom.Location);
            }

            lock (newRoom.ConnectedRooms) {
                newRoom.ConnectedRooms.Add(newRoom.GetDirection(playerRoom.Location), playerRoom.Location);
            }

            DataManager.SaveRoom(playerRoom);
            DataManager.SaveRoom(newRoom);

            player.SendToClient(
                string.Format(
                    "{0} and {1} have been connected.", newRoom.Name, playerRoom.Name
                )
            );
        } else {
            player.SendToClient("Aborting build");
        }
    }

    public static void CreateSpawner(PlayerCharacter player, string[] args) {
        if (!ActionUtility.TryGetRoom(player, out Room room)) {
            return;
        }

        //TODO: The spawn data should be generated based on the args
        // Create the creatures that will be spawning here.

        int spawnCount = 0;
        List<SpawnData> spawnList = new List<SpawnData>();
        for (int i = 1; i < args.Length; i++) {
            player.SendToClient("Trying to find some " + args[i] + " DNA ...");
            SpawnData spawn
                = (from kvp in DataManager.NameSpawnPairs
                    where kvp.Key.Equals(args[i], StringComparison.OrdinalIgnoreCase)
                    select kvp.Value).FirstOrDefault();

            if (spawn == null) continue;

            spawnList.Add(spawn);
            spawnCount += 1;
            player.SendToClient(spawn.Name + " created!");
        }

        if (spawnCount > 0) {
            new Spawner(room, spawnList.Select(s => s.Id).ToList());
            DataManager.SaveRoom(room);
            player.SendToClient("Spawner created.");
        } else {
            player.SendToClient("Nope. Nothin'.");
        }
    }
}
}
