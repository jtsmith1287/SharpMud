using System;
using System.Collections.Generic;
using System.Linq;
using MudServer.Entity;
using MudServer.World;
using MudServer.Enums;
using MudServer.Actions;

namespace MudServer.World {
    public class RoomTests {
        private int _passed = 0;
        private int _failed = 0;

        public void RunTests() {
            Console.WriteLine("Starting Room Unit Tests...");

            TestNormalExit();
            TestSecretExit();
            TestHiddenExit();
            TestExitFromOtherSide();
            TestClosedDoor();

            Console.WriteLine("\nRoom Tests Finished!");
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

        private void SetupScenario(out PlayerCharacter player, out Room room1, out Room room2) {
            World.Rooms.Clear();
            
            Coordinate3 loc1 = new Coordinate3(0, 0, 0);
            Coordinate3 loc2 = new Coordinate3(1, 0, 0);
            
            room1 = new Room(loc1, "Room 1");
            room2 = new Room(loc2, "Room 2");
            
            room1.ConnectedRooms["east"] = loc2;
            room2.ConnectedRooms["west"] = loc1;
            
            Stats playerData = new Stats("TestPlayer", Guid.NewGuid());
            playerData.Location = loc1;
            player = new TestPlayer(playerData);
            player.GenerateID();
            
            World.Rooms[loc1.ToString()] = room1;
            World.Rooms[loc2.ToString()] = room2;
        }

        public void TestNormalExit() {
            SetupScenario(out PlayerCharacter player, out Room room1, out Room room2);
            
            // Test Visibility
            TestPlayer tp = (TestPlayer)player;
            tp.LastMessage = "";
            Actions.Actions.Look(player, new string[0]);
            Assert(tp.LastMessage.Contains("Exits: east"), "Normal exit should be visible in Look");
            
            // Test Functionality
            Actions.Actions.MoveRooms(player, new[] { "east" });
            Assert(player.Location.Equals(room2.Location), "Normal exit should be functional");
        }

        public void TestSecretExit() {
            SetupScenario(out PlayerCharacter player, out Room room1, out Room room2);
            
            // Make the exit secret from room1 to room2
            Exit exit = new Exit();
            exit.Path = new[] { room1.Location, room2.Location };
            exit.Secret[0] = true; // Secret from Path[0] (room1)
            room1.Exits["east"] = exit;
            
            // Test Visibility
            TestPlayer tp = (TestPlayer)player;
            tp.LastMessage = "";
            Actions.Actions.Look(player, new string[0]);
            Assert(!tp.LastMessage.Contains("east"), "Secret exit should NOT be visible in Look");
            
            // Test Functionality
            Actions.Actions.MoveRooms(player, new[] { "east" });
            Assert(player.Location.Equals(room2.Location), "Secret exit should be functional");
        }

        public void TestHiddenExit() {
            SetupScenario(out PlayerCharacter player, out Room room1, out Room room2);
            
            // Make the exit hidden from room1 to room2
            Exit exit = new Exit();
            exit.Path = new[] { room1.Location, room2.Location };
            exit.Hidden[0] = true; // Hidden from Path[0] (room1)
            room1.Exits["east"] = exit;
            
            // Test Visibility
            TestPlayer tp = (TestPlayer)player;
            tp.LastMessage = "";
            Actions.Actions.Look(player, new string[0]);
            Assert(!tp.LastMessage.Contains("east"), "Hidden exit should NOT be visible in Look");
            
            // Test Functionality
            Actions.Actions.MoveRooms(player, new[] { "east" });
            Assert(player.Location.Equals(room1.Location), "Hidden exit should NOT be functional");
            Assert(tp.LastMessage.Contains("no exit in that direction"), "Should receive error message when trying to use hidden exit");
        }

        public void TestExitFromOtherSide() {
            SetupScenario(out PlayerCharacter player, out Room room1, out Room room2);
            
            // Exit is Secret from room1 -> room2, but NOT from room2 -> room1
            Exit exit = new Exit();
            exit.Path = new[] { room1.Location, room2.Location };
            exit.Secret[0] = true;
            exit.Secret[1] = false;
            room1.Exits["east"] = exit;
            room2.Exits["west"] = exit;
            
            // From room1 (Path[0])
            TestPlayer tp = (TestPlayer)player;
            tp.LastMessage = "";
            Actions.Actions.Look(player, new string[0]);
            Assert(!tp.LastMessage.Contains("east"), "Exit should be secret from room1");
            
            // Move to room2
            Actions.Actions.MoveRooms(player, new[] { "east" });
            Assert(player.Location.Equals(room2.Location), "Move to room2 should work");
            
            // From room2 (Path[1])
            tp.LastMessage = "";
            Actions.Actions.Look(player, new string[0]);
            Assert(tp.LastMessage.Contains("west"), "Exit should NOT be secret from room2");
        }

        public void TestClosedDoor() {
            SetupScenario(out PlayerCharacter player, out Room room1, out Room room2);
            
            // Make the exit a closed door
            Exit exit = new Exit();
            exit.Path = new[] { room1.Location, room2.Location };
            exit.Open = false;
            room1.Exits["east"] = exit;
            
            // Test Visibility
            TestPlayer tp = (TestPlayer)player;
            tp.LastMessage = "";
            Actions.Actions.Look(player, new string[0]);
            Assert(tp.LastMessage.Contains("east (closed)"), "Closed exit should show as (closed) in Look");
            
            // Test Functionality
            Actions.Actions.MoveRooms(player, new[] { "east" });
            Assert(player.Location.Equals(room1.Location), "Closed door should block movement");
            Assert(tp.LastMessage.Contains("door is closed"), "Should receive error message when trying to use closed door");
            
            // Open it
            exit.Open = true;
            tp.LastMessage = "";
            Actions.Actions.MoveRooms(player, new[] { "east" });
            Assert(player.Location.Equals(room2.Location), "Opened door should allow movement");
        }

        private class TestPlayer : PlayerCharacter {
            public string LastMessage = "";
            public TestPlayer(Stats stats) : base(new MockConnection(), stats) {
                this.Stats = stats;
            }
            public override void SendToClient(string msg, string colorSequence = "") {
                LastMessage += msg;
            }
        }

        private class MockConnection : MudServer.Server.Connection {
            public MockConnection() : base(null) { }
            public override void Send(string msg) { }
        }
    }
}
