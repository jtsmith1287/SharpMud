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
    }
}
