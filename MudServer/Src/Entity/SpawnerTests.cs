using System;
using System.Collections.Generic;
using MudServer.Entity;
using MudServer.World;
using MudServer.Server;
using MudServer.Enums;

namespace MudServer.Entity {
    public class SpawnerTests {
        private int _passed = 0;
        private int _failed = 0;

        public void RunTests() {
            Console.WriteLine("Starting Spawner Unit Tests...");

            TestUpdateWithNullSpawnData();
            TestUpdateWithEmptySpawnData();
            TestSaveAllMapsOnShutdown();
            TestCleanupNullSpawnersOnLoad();
            TestIdDataSerialization();
            TestCreateSpawnerInRoomWithNullSpawnersList();
            TestCreateSpawnerWithNullRoom();

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
                World.World.Rooms.Clear();

                // Ensure there is at least one room with a MapName
                string testMapName = "test_shutdown_map.json";
                Coordinate3 loc = new Coordinate3(999, 999, 999);
                Room testRoom = new Room(loc, "Test Shutdown Room");
                testRoom.MapName = testMapName;
                testRoom.Description = "Original Description " + DateTime.Now.Ticks;

                // Call SaveAllMaps (which is what happens on shutdown)
                DataManager.SaveAllMaps();

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
                
                DataManager.SaveAllMaps();

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
                World.World.Rooms.Remove("999 999 999");

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
                if (!DataManager.NameSpawnPairs.ContainsKey("Rat")) {
                    DataManager.NameSpawnPairs.Add("Rat", validData);
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
                DataManager.SaveMap(testMapName);

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
                World.World.Rooms.Clear();
                World.World.Spawners.Clear();

                // Load it back
                DataManager.LoadData(DataPaths.World); 

                // Verify
                string stringCoord = "888 888 888";
                if (World.World.Rooms.TryGetValue(stringCoord, out Room loadedRoom)) {
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

                World.World.Rooms.Remove(stringCoord);

            } catch (Exception e) {
                Assert(false, "TestCleanupNullSpawnersOnLoad threw " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
            }
        }
        public void TestIdDataSerialization() {
            try {
                // Clear existing data
                DataManager.IdDataPairs.Clear();

                // Add some test data
                Guid id1 = Guid.NewGuid();
                Stats stats1 = new Stats("TestPlayer1", id1);

                Guid id2 = Guid.NewGuid();
                Stats stats2 = new Stats("TestPlayer2", id2);

                // Try to save
                DataManager.SaveData(DataPaths.IdData);
                Assert(true, "DataManager.SaveData(DataPaths.IdData) should not throw");

                // Clear memory
                DataManager.IdDataPairs.Clear();

                // Try to load
                DataManager.LoadData(DataPaths.IdData);
                Assert(DataManager.IdDataPairs.Count == 2, "Should load 2 player data entries");
                Assert(DataManager.IdDataPairs.ContainsKey(id1), "Should contain id1");
                Assert(DataManager.IdDataPairs[id1].Name == "TestPlayer1", "id1 name should match");

            } catch (Exception e) {
                Assert(false, "TestIdDataSerialization failed: " + e.Message + "\n" + e.StackTrace);
            }
        }

        public void TestCreateSpawnerInRoomWithNullSpawnersList() {
            try {
                Room room = new Room(new Coordinate3(777, 777, 777), "Null Spawners List Room");
                room.SpawnersHere = null; // Simulate deserialization issue where it might be null

                List<SpawnData> spawnList = new List<SpawnData> { new SpawnData("Test") };
                new Spawner(room, spawnList);

                Assert(room.SpawnersHere != null, "Spawner constructor should initialize SpawnersHere if it is null");
                Assert(room.SpawnersHere.Count == 1, "Spawner should be added to SpawnersHere list");
            } catch (NullReferenceException) {
                Assert(false, "Spawner constructor threw NullReferenceException when room.SpawnersHere was null");
            } catch (Exception e) {
                Assert(false, "TestCreateSpawnerInRoomWithNullSpawnersList threw " + e.GetType().Name + ": " + e.Message);
            } finally {
                World.World.Rooms.Remove("777 777 777");
            }
        }

        public void TestCreateSpawnerWithNullRoom() {
            try {
                List<SpawnData> spawnList = new List<SpawnData> { new SpawnData("Test") };
                Spawner spawner = new Spawner(null, spawnList);
                Assert(true, "Spawner constructor should not throw when room is null");
                Assert(!World.World.Spawners.Contains(spawner), "Spawner should not be added to World.World.Spawners if room is null");
            } catch (Exception e) {
                Assert(false, "TestCreateSpawnerWithNullRoom threw " + e.GetType().Name + ": " + e.Message);
            }
        }
    }
}
