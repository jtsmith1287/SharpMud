using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using MudServer.Entity;
using MudServer.World;
using MudServer.Enums;

namespace MudServer.Server {
    public static class DataManager {
        private static readonly object SaveLock = new object();
        public static Dictionary<string, string> UsernamePwdPairs = new Dictionary<string, string>();
        public static Dictionary<string, Guid> UsernameIdPairs = new Dictionary<string, Guid>();
        public static Dictionary<Guid, Stats> IdDataPairs = new Dictionary<Guid, Stats>();
        public static Dictionary<string, SpawnData> NameSpawnPairs = new Dictionary<string, SpawnData>();

        public static void SaveRoom(Room room) {
            if (room == null || string.IsNullOrEmpty(room.MapName)) {
                return;
            }

            SaveMap(room.MapName);
        }

        public static void SaveMap(string mapName) {
            if (string.IsNullOrEmpty(mapName)) return;

            lock (SaveLock) {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string mapDir = Path.GetDirectoryName(DataPaths.MapList);
                if (!string.IsNullOrEmpty(mapDir) && !Directory.Exists(mapDir)) {
                    Directory.CreateDirectory(mapDir);
                }

                string mapFile = Path.Combine(mapDir, mapName);

                Dictionary<string, Room> mapRooms = new Dictionary<string, Room>();
                foreach (var kvp in World.World.Rooms) {
                    if (kvp.Value.MapName == mapName) {
                        mapRooms.Add(kvp.Key, kvp.Value);
                    }
                }

                if (mapRooms.Count == 0 && File.Exists(mapFile)) {
                    return;
                }

                string updatedJson = serializer.Serialize(mapRooms);
                File.WriteAllText(mapFile, updatedJson);
                Console.WriteLine($"Saved map {mapName} ({mapRooms.Count} rooms)");
            }
        }

        public static void SaveAllMaps() {
            HashSet<string> mapNames = new HashSet<string>();
            lock (World.World.Rooms) {
                foreach (var room in World.World.Rooms.Values) {
                    if (!string.IsNullOrEmpty(room.MapName)) {
                        mapNames.Add(room.MapName);
                    }
                }
            }

            foreach (var mapName in mapNames) {
                SaveMap(mapName);
            }
        }

        public static void SaveData(params string[] paths) {
            lock (SaveLock) {
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                foreach (string path in paths) {
                    string json = "";

                    switch (path) {
                        case DataPaths.UserPwd:
                            json = serializer.Serialize(UsernamePwdPairs);
                            break;
                        case DataPaths.UserId:
                            json = serializer.Serialize(UsernameIdPairs);
                            break;
                        case DataPaths.IdData:
                            Dictionary<string, Stats> stringIdDataPairs = IdDataPairs.ToDictionary(
                                kvp => kvp.Key.ToString(),
                                kvp => kvp.Value
                            );
                            json = serializer.Serialize(stringIdDataPairs);
                            break;
                        case DataPaths.Creatures:
                            json = serializer.Serialize(NameSpawnPairs);
                            break;
                    }

                    File.WriteAllText(path, json);
                    Console.WriteLine($"Saving {path}: {json.Length / 1000f}kb");
                }
            }
        }

        public static void LoadData(params string[] paths) {
            if (paths.Length == 0) {
                paths = new[] {
                    DataPaths.UserPwd,
                    DataPaths.UserId,
                    DataPaths.IdData,
                    DataPaths.Creatures,
                    DataPaths.World
                };
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            long bytes = 0;
            foreach (string path in paths) {
                if (!File.Exists(path) && path != DataPaths.World) {
                    SaveData(path);
                    continue;
                }

                try {
                    string json = "";
                    if (path != DataPaths.World) {
                        json = File.ReadAllText(path);
                        bytes += json.Length;
                    }

                    switch (path) {
                        case DataPaths.UserPwd:
                            UsernamePwdPairs = serializer.Deserialize<Dictionary<string, string>>(json) ??
                                             new Dictionary<string, string>();
                            break;
                        case DataPaths.UserId:
                            UsernameIdPairs = serializer.Deserialize<Dictionary<string, Guid>>(json) ??
                                             new Dictionary<string, Guid>();
                            break;
                        case DataPaths.IdData:
                            Dictionary<string, Stats> stringStats =
                                serializer.Deserialize<Dictionary<string, Stats>>(json) ??
                                new Dictionary<string, Stats>();
                            IdDataPairs = stringStats.ToDictionary(
                                kvp => Guid.Parse(kvp.Key),
                                kvp => kvp.Value
                            );
                            break;
                        case DataPaths.World:
                            string mapsDir = Path.GetDirectoryName(DataPaths.MapList);
                            if (Directory.Exists(mapsDir)) {
                                string[] mapFiles = Directory.GetFiles(mapsDir, "*.json");
                                foreach (string mapFile in mapFiles) {
                                    string mapFileName = Path.GetFileName(mapFile);
                                    if (mapFileName == "maps.json") continue;

                                    string mapJson = File.ReadAllText(mapFile);
                                    bytes += mapJson.Length;
                                    Dictionary<string, Room> rooms
                                        = serializer.Deserialize<Dictionary<string, Room>>(mapJson);
                                    if (rooms == null) continue;
                                    foreach (KeyValuePair<string, Room> room in rooms) {
                                        room.Value.MapName = mapFileName;
                                        if (room.Value.Location == null || (room.Value.Location.X == 0 &&
                                                                            room.Value.Location.Y == 0 &&
                                                                            room.Value.Location.Z == 0)) {
                                            string[] coords = room.Key.Split(' ');
                                            if (coords.Length == 3) {
                                                if (int.TryParse(coords[0], out int x) &&
                                                    int.TryParse(coords[1], out int y) &&
                                                    int.TryParse(coords[2], out int z)) {
                                                    room.Value.Location = new Coordinate3(x, y, z);
                                                }
                                            }
                                        }

                                        if (room.Value.SpawnersHere != null) {
                                            foreach (var spawner in room.Value.SpawnersHere) {
                                                spawner.Location = room.Value.Location;
                                                if (spawner.SpawnDataIds != null && spawner.SpawnDataIds.Count > 0) {
                                                    if (!World.World.Spawners.Contains(spawner)) {
                                                        World.World.Spawners.Add(spawner);
                                                    }
                                                }
                                            }

                                            room.Value.SpawnersHere.RemoveAll(
                                                s => (s.SpawnDataIds == null || s.SpawnDataIds.Count == 0)
                                            );

                                            if (room.Value.SpawnersHere.Count > 0) {
                                                World.World.StartAiThread();
                                            }
                                        }

                                        World.World.Rooms[room.Key] = room.Value;
                                    }
                                }
                            } else {
                                if (File.Exists(DataPaths.World)) {
                                    string worldJson = File.ReadAllText(DataPaths.World);
                                    bytes += worldJson.Length;
                                    World.World.Rooms = serializer.Deserialize<Dictionary<string, Room>>(worldJson) ??
                                                  new Dictionary<string, Room>();
                                    foreach (KeyValuePair<string, Room> room in World.World.Rooms) {
                                        if (room.Value.Location == null || (room.Value.Location.X == 0 &&
                                                                            room.Value.Location.Y == 0 &&
                                                                            room.Value.Location.Z == 0)) {
                                            string[] coords = room.Key.Split(' ');
                                            if (coords.Length == 3) {
                                                if (int.TryParse(coords[0], out int x) &&
                                                    int.TryParse(coords[1], out int y) &&
                                                    int.TryParse(coords[2], out int z)) {
                                                    room.Value.Location = new Coordinate3(x, y, z);
                                                }
                                            }
                                        }

                                        if (room.Value.SpawnersHere != null) {
                                            foreach (var spawner in room.Value.SpawnersHere) {
                                                spawner.Location = room.Value.Location;
                                                if (spawner.SpawnDataIds != null && spawner.SpawnDataIds.Count > 0) {
                                                    if (!World.World.Spawners.Contains(spawner)) {
                                                        World.World.Spawners.Add(spawner);
                                                    }
                                                }
                                            }

                                            room.Value.SpawnersHere.RemoveAll(
                                                s => (s.SpawnDataIds == null || s.SpawnDataIds.Count == 0)
                                            );

                                            if (room.Value.SpawnersHere.Count > 0) {
                                                World.World.StartAiThread();
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (Room room in World.World.Rooms.Values.Where(room => room.EntitiesHere == null)) {
                                room.EntitiesHere = new List<Guid>();
                            }

                            Console.WriteLine($"Loaded {World.World.Rooms.Count} rooms.");
                            break;
                        case DataPaths.Creatures:
                            NameSpawnPairs = serializer.Deserialize<Dictionary<string, SpawnData>>(json) ??
                                             new Dictionary<string, SpawnData>();
                            foreach (KeyValuePair<string, SpawnData> entry in NameSpawnPairs.Where(
                                         entry => entry.Value.Id == Guid.Empty
                                     )) {
                                entry.Value.Id = Guid.NewGuid();
                            }

                            break;
                    }
                } catch (Exception e) {
                    Console.WriteLine($"[ERROR] Failed to load {path}: {e.Message}");
                    Console.WriteLine(e.StackTrace);
                }
            }

            Console.WriteLine($"Loaded {bytes / 1000f}kb of data into memory.");
        }

        public static void SaveSpawnTemplates() {
            lock (SaveLock) {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(NameSpawnPairs);
                File.WriteAllText(DataPaths.Creatures, json);
                Console.WriteLine($"Saving {DataPaths.Creatures}: {json.Length / 1000f}kb");
            }
        }
    }
}
