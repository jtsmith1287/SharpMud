using System;
using System.Collections.Generic;
using GameCore;
using GameCore.Util;

namespace MudServer {
    public class SpawnerTests {
        private int _passed = 0;
        private int _failed = 0;

        public void RunTests() {
            Console.WriteLine("Starting Spawner Unit Tests...");

            TestUpdateWithNullSpawnData();
            TestUpdateWithEmptySpawnData();
            TestSaveAllMapsOnShutdown();
            TestCleanupNullSpawnersOnLoad();

            Console.WriteLine("\nSpawner Tests Finished!");
            Console.WriteLine("Passed: {0}, Failed: {1}", _passed, _failed);
            
            if (_failed > 0) {
                Environment.Exit(1);
            }
        }

        private void Assert(bool condition, string message) {
            if (condition) {
                _passed++;
                Console.WriteLine("[PASS] " + message);
            } else {
                _failed++;
                Console.WriteLine("[FAIL] " + message);
            }
        }

        public void TestUpdateWithNullSpawnData() {
            try {
                Spawner spawner = new Spawner();
                spawner.SpawnData = null;
                spawner.Update();
                Assert(true, "Spawner.Update() should not throw when SpawnData is null");
            } catch (Exception e) {
                Assert(false, "Spawner.Update() threw " + e.GetType().Name + ": " + e.Message);
            }
        }

        public void TestUpdateWithEmptySpawnData() {
            try {
                Spawner spawner = new Spawner();
                spawner.SpawnData = new SpawnData[0];
                spawner.Update();
                Assert(true, "Spawner.Update() should not throw when SpawnData is empty");
            } catch (Exception e) {
                Assert(false, "Spawner.Update() threw " + e.GetType().Name + ": " + e.Message);
            }
        }

        public void TestSaveAllMapsOnShutdown() {
            try {
                // Clear rooms to avoid interference from other tests
                World.Rooms.Clear();

                // Ensure there is at least one room with a MapName
                string testMapName = "test_shutdown_map.json";
                Coordinate3 loc = new Coordinate3(999, 999, 999);
                Room testRoom = new Room(loc, "Test Shutdown Room");
                testRoom.MapName = testMapName;
                testRoom.Description = "Original Description " + DateTime.Now.Ticks;

                // Call SaveAllMaps (which is what happens on shutdown)
                Data.SaveAllMaps();

                // Verify the file exists and has the correct description
                string mapDir = System.IO.Path.GetDirectoryName(DataPaths.MapList);
                if (string.IsNullOrEmpty(mapDir)) mapDir = ".";
                string mapFile = System.IO.Path.Combine(mapDir, testMapName);

                if (!System.IO.File.Exists(mapFile)) {
                    Assert(false, "Test map file should have been created by SaveAllMaps. File not found at: " + mapFile);
                    return;
                }

                // Modify description and save again
                string newDesc = "Modified Description " + DateTime.Now.Ticks;
                testRoom.Description = newDesc;
                
                Data.SaveAllMaps();

                string json = System.IO.File.ReadAllText(mapFile);
                if (json.Contains(newDesc)) {
                    Assert(true, "SaveAllMaps correctly updated the map file with changes");
                } else {
                    Assert(false, "SaveAllMaps did not update the map file with changes");
                }

                // Cleanup
                if (System.IO.File.Exists(mapFile)) {
                    System.IO.File.Delete(mapFile);
                }
                World.Rooms.Remove("999 999 999");

            } catch (Exception e) {
                Assert(false, "TestSaveAllMapsOnShutdown threw " + e.GetType().Name + ": " + e.Message);
            }
        }

        public void TestCleanupNullSpawnersOnLoad() {
            try {
                // Prepare a room with a null spawner in the map file
                string testMapName = "test_null_spawner_cleanup.json";
                Coordinate3 loc = new Coordinate3(888, 888, 888);
                Room testRoom = new Room(loc, "Test Cleanup Room");
                testRoom.MapName = testMapName;

                // Add a valid spawner
                SpawnData validData = new SpawnData("Rat");
                validData.Health = 10;
                validData.MaxHealth = 10;
                if (!Data.NameSpawnPairs.ContainsKey("Rat")) {
                    Data.NameSpawnPairs.Add("Rat", validData);
                }

                Spawner validSpawner = new Spawner(testRoom, new List<SpawnData> { validData });

                // Add a null spawner (manual addition to list to bypass constructor logic if any)
                Spawner nullSpawner = new Spawner();
                nullSpawner.SpawnData = null;
                testRoom.SpawnersHere.Add(nullSpawner);

                // Add an empty spawner
                Spawner emptySpawner = new Spawner();
                emptySpawner.SpawnData = new SpawnData[0];
                testRoom.SpawnersHere.Add(emptySpawner);

                // Save it
                Data.SaveMap(testMapName);

                // Add to maps.json list if it's not there, so LoadData(DataPaths.World) finds it
                string mapListPath = DataPaths.MapList;
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, List<string>> mapList = new Dictionary<string, List<string>>();
                if (System.IO.File.Exists(mapListPath)) {
                    mapList = serializer.Deserialize<Dictionary<string, List<string>>>(System.IO.File.ReadAllText(mapListPath));
                }
                if (!mapList.ContainsKey("Maps")) mapList["Maps"] = new List<string>();
                bool addedMap = false;
                if (!mapList["Maps"].Contains(testMapName)) {
                    mapList["Maps"].Add(testMapName);
                    System.IO.File.WriteAllText(mapListPath, serializer.Serialize(mapList));
                    addedMap = true;
                }

                // Clear everything
                World.Rooms.Clear();
                World.Spawners.Clear();

                // Load it back
                Data.LoadData(DataPaths.World); 

                // Verify
                string stringCoord = "888 888 888";
                if (World.Rooms.TryGetValue(stringCoord, out Room loadedRoom)) {
                    Assert(loadedRoom.SpawnersHere.Count == 1, "Only valid spawners should be loaded. Found: " + loadedRoom.SpawnersHere.Count);
                    if (loadedRoom.SpawnersHere.Count > 0) {
                        Assert(loadedRoom.SpawnersHere[0].SpawnData != null && loadedRoom.SpawnersHere[0].SpawnData.Length > 0, "Loaded spawner should be the valid one");
                    }
                } else {
                    Assert(false, "Test room was not loaded back");
                }

                // Cleanup
                string mapDir = System.IO.Path.GetDirectoryName(DataPaths.MapList);
                if (string.IsNullOrEmpty(mapDir)) mapDir = ".";
                string mapFile = System.IO.Path.Combine(mapDir, testMapName);
                if (System.IO.File.Exists(mapFile)) System.IO.File.Delete(mapFile);
                
                if (addedMap) {
                    mapList["Maps"].Remove(testMapName);
                    System.IO.File.WriteAllText(mapListPath, serializer.Serialize(mapList));
                }

                World.Rooms.Remove(stringCoord);

            } catch (Exception e) {
                Assert(false, "TestCleanupNullSpawnersOnLoad threw " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
