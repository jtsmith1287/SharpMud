using System;
using System.Collections.Generic;
using System.Linq;
using ServerCore.Util;

namespace GameCore.Util {
public static class AdminActions {
    public static readonly Dictionary<string, Action<PlayerEntity, string[]>> ActionCalls =
        new Dictionary<string, Action<PlayerEntity, string[]>> {
            { "build", BuildRoom },
            { "spawn", CreateSpawner },
            { "view", ViewEntity },
            { "save", SaveAll },
            { "god", GodMode }
        };

    private static void GodMode(PlayerEntity player, string[] arg2) {
        player.GodMode = !player.GodMode;
        player.SendToClient($"God mode {(player.GodMode ? "enabled" : "disabled")}", Color.Yellow);
    }

    private static void SaveAll(PlayerEntity player, string[] arg2) {
        Data.SaveData(
            DataPaths.IdData,
            DataPaths.Creatures,
            DataPaths.UserId,
            DataPaths.UserPwd
        );

        // Save all maps
        HashSet<string> savedMaps = new HashSet<string>();
        foreach (var room in World.Rooms.Values) {
            if (!string.IsNullOrEmpty(room.MapName) && !savedMaps.Contains(room.MapName)) {
                Data.SaveMap(room.MapName);
                savedMaps.Add(room.MapName);
            }
        }

        player.SendToClient("Saved all data.", Color.White);
    }

    private static void ViewEntity(PlayerEntity arg1, string[] arg2) { }

    public static void BuildRoom(PlayerEntity player, string[] args) {
        string direction;
        Coordinate3 location = new Coordinate3(player.Location.X, player.Location.Y, player.Location.Z);

        if (args.Length < 2) {
            player.SendToClient("Please provide a direction!", Color.Red);
            return;
        }

        if (!Room.DirectionNames.TryGetValue(args[1], out direction)) {
            player.SendToClient(
                "That's not a valid direction for this command. Try using 'e' or 'n'.", Color.Red
            );
            return;
        }

        Room newRoom = World.GetRoom(location + Room.DirectionMap[args[1]]);

        // A room already exists in this location
        if (newRoom != null) {
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
                Room playerRoom = World.GetRoom(player.Location);
                lock (playerRoom.ConnectedRooms) {
                    playerRoom.ConnectedRooms.Add(direction, newRoom.Location);
                }

                lock (newRoom.ConnectedRooms) {
                    newRoom.ConnectedRooms.Add(newRoom.GetDirection(playerRoom.Location), playerRoom.Location);
                }

                Data.SaveRoom(playerRoom);
                Data.SaveRoom(newRoom);

                player.SendToClient(
                    string.Format(
                        "{0} and {1} have been connected.", newRoom.Name, playerRoom.Name
                    )
                );
            } else {
                player.SendToClient("Aborting build");
            }
            // Clear to build a new room at this location.
        } else {
            Room playerRoom = World.GetRoom(player.Location);
            newRoom = new Room(location + Room.DirectionMap[args[1]], "###");
            newRoom.MapName = playerRoom.MapName; // Inherit MapName from current room

            lock (playerRoom.ConnectedRooms) {
                playerRoom.ConnectedRooms.Add(direction, newRoom.Location);
            }

            lock (newRoom.ConnectedRooms) {
                newRoom.ConnectedRooms.Add(newRoom.GetDirection(playerRoom.Location), playerRoom.Location);
            }

            Data.SaveRoom(playerRoom);
            Data.SaveRoom(newRoom);

            player.SendToClient(
                string.Format(
                    "{0} has been created to the {1}!).", newRoom.Name, direction
                ), Color.White
            );
        }
    }

    public static void CreateSpawner(PlayerEntity player, string[] args) {
        Room room = World.GetRoom(player.Location);
        //TODO: The spawn data should be generated based on the args
        // Create the creatures that will be spawning here.

        int spawnCount = 0;
        List<SpawnData> spawnList = new List<SpawnData>();
        for (int i = 1; i < args.Length; i++) {
            player.SendToClient("Trying to find some " + args[i] + " DNA ...");
            SpawnData spawn
                = (from kvp in Data.NameSpawnPairs
                    where kvp.Key.Equals(args[i], StringComparison.OrdinalIgnoreCase)
                    select kvp.Value).FirstOrDefault();

            if (spawn == null) continue;
            
            spawnList.Add(spawn);
            spawnCount += 1;
            player.SendToClient(spawn.Name + " created!");
        }

        if (spawnCount > 0) {
            new Spawner(room, spawnList);
            Data.SaveRoom(room);
            player.SendToClient("Spawner created.");
        } else {
            player.SendToClient("Nope. Nothin'.");
        }
    }
}
}
