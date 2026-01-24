using System;
using System.Collections.Generic;
using System.Linq;
using MudServer.Entity;
using MudServer.World;
using MudServer.Util;
using MudServer.Enums;
using MudServer.Server;

namespace MudServer.Actions {
public class ActionTests {
    private int _passed = 0;
    private int _failed = 0;

    public void RunTests() {
        Console.WriteLine("Starting Action Unit Tests...");

        bool originalNewSystem = Actions.UseNewSystem;
        Actions.UseNewSystem = true;

        try {
            TestCommandAutocompletion();
            TestArgumentAutocompletion();
            TestMovement();
            TestCombat();
            TestInteraction();
            TestInformation();
            TestAdmin();
            TestActionUtility();
            TestInteractionDetailed();
            TestLookItem();
            TestSharedExitInstance();
            TestExitInstanceSynchronizationAfterLoading();
            TestFullExitSynchronizationFlow();
            Console.WriteLine("All ActionTests passed!");
        } catch (Exception e) {
            Console.WriteLine("[ERROR] Exception during tests: " + e.Message);
            Console.WriteLine(e.StackTrace);
            _failed++;
        } finally {
            Actions.UseNewSystem = originalNewSystem;
        }

        Console.WriteLine("\nAction Tests Finished!");
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

    private class MockConnection : Connection {
        public List<string> SentMessages = new List<string>();

        public override void Send(string msg) {
            SentMessages.Add(msg);
        }
    }

    private PlayerCharacter CreateTestPlayer(string name, Coordinate3 loc) {
        Stats stats = new Stats(name, Guid.NewGuid()) {
            Location = loc,
            MaxHealth = 100,
            Health = 100,
            Str = 10,
            Dex = 10,
            Int = 10,
            Con = 10
        };
        return new PlayerCharacter(new MockConnection(), stats);
    }

    private Room CreateTestRoom(Coordinate3 loc, string name) {
        Room room = new Room(loc, name);
        return room;
    }

    public void TestCommandAutocompletion() {
        Coordinate3 loc = new Coordinate3(1001, 1001, 0);
        CreateTestRoom(loc, "Auto Room");
        PlayerCharacter player = CreateTestPlayer("AutoTester", loc);
        var conn = (MockConnection)player.Conn;

        Actions.DoAction(player, "l");
        Assert(conn.SentMessages.Any(m => m.Contains("Auto Room")), "Autocompleting 'l' should trigger 'look'");

        conn.SentMessages.Clear();
        Actions.DoAction(player, "st");
        Assert(conn.SentMessages.Any(m => m.Contains("AutoTester")), "Autocompleting 'st' should trigger 'stats'");
    }

    public void TestArgumentAutocompletion() {
        Coordinate3 loc = new Coordinate3(1002, 1002, 0);
        Room room = CreateTestRoom(loc, "Arg Room");
        PlayerCharacter player = CreateTestPlayer("ArgTester", loc);

        SpawnData grubData = new SpawnData("Giant Grub");
        grubData.MaxHealth = 50;
        grubData.Health = 50;
        NonPlayerCharacter grub = new NonPlayerCharacter(grubData, null);
        grub.GenerateID();
        room.EntitiesHere.Add(grub.Id);
        World.World.Mobiles[grub.Id] = grub;

        // Test 'a gr' -> 'attack Giant Grub'
        Actions.DoAction(player, "a gr");
        Assert(
            player.Target != null && player.Target.Name.Contains("Grub"),
            "Autocompleting 'a gr' should target something with 'Grub' in name. Target: " +
            (player.Target?.Name ?? "null")
        );
        Assert(player.GameState == GameState.Combat, "Should be in combat after attack");
    }

    public void TestMovement() {
        Coordinate3 loc1 = new Coordinate3(2000, 2000, 0);
        Coordinate3 loc2 = new Coordinate3(2000, 2001, 0);
        Room room1 = CreateTestRoom(loc1, "Start");
        Room room2 = CreateTestRoom(loc2, "North Room");

        room1.ConnectedRooms["north"] = loc2;
        room2.ConnectedRooms["south"] = loc1;

        PlayerCharacter player = CreateTestPlayer("Mover", loc1);

        Actions.DoAction(player, "n");
        Assert(player.Location == loc2, "Player should move north");

        Actions.DoAction(player, "s");
        Assert(player.Location == loc1, "Player should move back south");
    }

    public void TestCombat() {
        Coordinate3 loc = new Coordinate3(3000, 3000, 0);
        Room room = CreateTestRoom(loc, "Combat Room");
        PlayerCharacter player = CreateTestPlayer("Fighter", loc);

        SpawnData targetData = new SpawnData("TargetMob");
        targetData.MaxHealth = 100;
        targetData.Health = 100;
        NonPlayerCharacter target = new NonPlayerCharacter(targetData, null);
        target.GenerateID();
        room.EntitiesHere.Add(target.Id);
        World.World.Mobiles[target.Id] = target;

        Actions.DoAction(player, "attack target");
        Assert(player.Target == target, "Manual attack should set target");
        Assert(player.GameState == GameState.Combat, "Should be in combat");
    }

    public void TestInteraction() {
        Coordinate3 loc = new Coordinate3(4000, 4000, 0);
        Room room = CreateTestRoom(loc, "Interact Room");
        PlayerCharacter player = CreateTestPlayer("Interacter", loc);
        var conn = (MockConnection)player.Conn;

        Actions.DoAction(player, "search");
        Assert(conn.SentMessages.Any(m => m.Contains("nothing unusual")), "Search should find nothing in empty room");

        player.Stats.Health = 50;
        Actions.DoAction(player, "rest");
        Assert(player.GameState == GameState.Resting, "Player should be resting");

        conn.SentMessages.Clear();
        Actions.DoAction(player, "unlock n");
        Assert(conn.SentMessages.Any(m => m.Contains("no exit")), "Unlock should fail if no exit");
    }

    public void TestInformation() {
        Coordinate3 loc = new Coordinate3(5000, 5000, 0);
        CreateTestRoom(loc, "Info Room");
        PlayerCharacter player = CreateTestPlayer("Informer", loc);
        var conn = (MockConnection)player.Conn;

        conn.SentMessages.Clear();
        Actions.DoAction(player, "who");
        Assert(conn.SentMessages.Any(m => m.Contains("Informer")), "Who should list the player");

        conn.SentMessages.Clear();
        Actions.DoAction(player, "exp");
        Assert(conn.SentMessages.Any(m => m.Contains("Experience:")), "Exp should show experience");
    }

    public void TestAdmin() {
        Coordinate3 loc = new Coordinate3(6000, 6000, 0);
        CreateTestRoom(loc, "Admin Room");
        PlayerCharacter player = CreateTestPlayer("Admin", loc);
        player.Admin = true;

        Actions.DoAction(player, "god");
        Assert(player.GodMode, "God command should toggle god mode");
    }

    public void TestActionUtility() {
        Coordinate3 loc = new Coordinate3(7000, 7000, 0);
        Room room = CreateTestRoom(loc, "Utility Room");
        PlayerCharacter player = CreateTestPlayer("UtilTester", loc);

        Assert(ActionUtility.TryGetRoom(player, out Room r) && r == room, "ActionUtility.TryGetRoom should work");

        player.Hidden = true;
        Assert(ActionUtility.IsSneaking(player), "ActionUtility.IsSneaking should detect sneaking");

        player.GameState = GameState.Combat;
        Assert(ActionUtility.IsInCombat(player), "ActionUtility.IsInCombat should detect combat");

        Assert(
            ActionUtility.TryGetDirection("n", out string dir) && dir == "north",
            "ActionUtility.TryGetDirection should resolve 'n'"
        );
    }

    public void TestInteractionDetailed() {
        Coordinate3 loc1 = new Coordinate3(8000, 8000, 0);
        Coordinate3 loc2 = new Coordinate3(8001, 8000, 0);
        Room room1 = CreateTestRoom(loc1, "Room 1");
        Room room2 = CreateTestRoom(loc2, "Room 2");

        room1.ConnectedRooms["east"] = loc2;
        Exit exit = new Exit { Path = new[] { loc1, loc2 }, Locked = true };
        room1.Exits["east"] = exit;

        PlayerCharacter player = CreateTestPlayer("Interactor2", loc1);

        // Test unlock
        Actions.DoAction(player, "un e");
        Assert(!exit.Locked, "Unlock east should unlock the door");

        // Test relock and picklock
        exit.Locked = true;
        player.Stats.Dex = 100; // Guarantee success
        Actions.DoAction(player, "pick e");
        Assert(!exit.Locked, "Picklock east should unlock the door");

        // Test relock and bash
        exit.Locked = true;
        player.Stats.Str = 100; // Guarantee success
        Actions.DoAction(player, "bash e");
        Assert(!exit.Locked, "Bash east should unlock the door");

        // Test bash failure
        exit.Locked = true;
        player.Stats.Str = 0; // Guarantee failure (stat / 20.0 chance)
        int initialHealth = player.Stats.Health;
        Actions.DoAction(player, "bash e");
        Assert(exit.Locked, "Bash with 0 str should fail");
        Assert(player.Stats.Health < initialHealth, "Bash failure should deal damage");

        // Test Open
        exit.Locked = false;
        exit.Open = false;
        Actions.DoAction(player, "open e");
        Assert(exit.Open, "Open east should open the door");

        exit.Open = false;
        exit.Locked = true;
        Actions.DoAction(player, "open e");
        Assert(!exit.Open, "Open locked door should not work");
    }

    public void TestLookItem() {
        Coordinate3 loc = new Coordinate3(9000, 9000, 0);
        Room room = CreateTestRoom(loc, "Item Room");
        PlayerCharacter player = CreateTestPlayer("LookTester", loc);
        var conn = (MockConnection)player.Conn;

        Item item = new Item { Name = "A golden key", Description = "A shiny key." };
        room.EntitiesHere.Add(item.Id);
        World.World.Items[item.Id] = item;

        Actions.DoAction(player, "look key");
        Assert(conn.SentMessages.Any(m => m.Contains("shiny key")), "Looking at 'key' should show description");
    }

    public void TestSharedExitInstance() {
        Coordinate3 loc1 = new Coordinate3(10000, 10000, 0);
        Coordinate3 loc2 = new Coordinate3(10001, 10000, 0);
        Room room1 = CreateTestRoom(loc1, "Shared Room 1");
        Room room2 = CreateTestRoom(loc2, "Shared Room 2");

        // Manually link rooms as it would be in a real map
        room1.ConnectedRooms["east"] = loc2;
        room2.ConnectedRooms["west"] = loc1;

        // Create a single exit and share it
        Exit exit = new Exit { Path = new[] { loc1, loc2 }, Open = false, Locked = false };
        room1.Exits["east"] = exit;
        room2.Exits["west"] = exit;

        PlayerCharacter player1 = CreateTestPlayer("Player1", loc1);
        PlayerCharacter player2 = CreateTestPlayer("Player2", loc2);
        var conn2 = (MockConnection)player2.Conn;

        // Verify initial state
        Assert(!exit.Open, "Initial state: exit should be closed");

        // Player 1 opens the door
        Actions.DoAction(player1, "open east");

        // Verify the exit state in both rooms
        Assert(exit.Open, "Exit should be open in the shared instance");
        
        // Also verify that the exit object is the same in both rooms
        Assert(room1.Exits["east"] == room2.Exits["west"], "Exit instances MUST be shared between both rooms for consistency");

        // Player 2 should see that the door is open
        conn2.SentMessages.Clear();
        Actions.DoAction(player2, "look");
        Assert(conn2.SentMessages.Any(m => m.Contains("west") && !m.Contains("(closed)")), 
            "Anyone standing in the other room should see the door open (verified by Player 2)");
    }

    public void TestExitInstanceSynchronizationAfterLoading() {
        Coordinate3 loc1 = new Coordinate3(20000, 20000, 0);
        Coordinate3 loc2 = new Coordinate3(20001, 20000, 0);
        Room room1 = CreateTestRoom(loc1, "Sync Room 1");
        Room room2 = CreateTestRoom(loc2, "Sync Room 2");

        room1.ConnectedRooms["east"] = loc2;
        room2.ConnectedRooms["west"] = loc1;

        // Simulate independent exit instances being created (like after JSON deserialization)
        Exit exit1 = new Exit { Path = new[] { loc1, loc2 }, Open = false, Locked = false };
        Exit exit2 = new Exit { Path = new[] { loc1, loc2 }, Open = false, Locked = false };
        
        room1.Exits["east"] = exit1;
        room2.Exits["west"] = exit2;

        Assert(room1.Exits["east"] != room2.Exits["west"], "Initially, they are different objects");

        // Use the production unification logic
        DataManager.UnifyExits();

        Assert(room1.Exits["east"] == room2.Exits["west"], "After unification, they MUST be the same object");
        
        // Verify state sharing
        room1.Exits["east"].Open = true;
        Assert(room2.Exits["west"].Open, "State change in one should be reflected in the other after unification");
    }

    public void TestFullExitSynchronizationFlow() {
        Coordinate3 loc1 = new Coordinate3(30000, 30000, 0);
        Coordinate3 loc2 = new Coordinate3(30001, 30000, 0);
        Room room1 = CreateTestRoom(loc1, "Flow Room 1");
        Room room2 = CreateTestRoom(loc2, "Flow Room 2");

        room1.ConnectedRooms["east"] = loc2;
        room2.ConnectedRooms["west"] = loc1;

        // 1. Setup separate instances
        Exit exit1 = new Exit { Path = new[] { loc1, loc2 }, Open = false, Locked = false };
        Exit exit2 = new Exit { Path = new[] { loc1, loc2 }, Open = false, Locked = false };
        room1.Exits["east"] = exit1;
        room2.Exits["west"] = exit2;

        PlayerCharacter player1 = CreateTestPlayer("P1", loc1);
        PlayerCharacter player2 = CreateTestPlayer("P2", loc2);

        // 2. Demonstrate the BUG: inconsistency when not unified
        Actions.DoAction(player1, "open east");
        Assert(room1.Exits["east"].Open, "Room 1 door is open");
        Assert(!room2.Exits["west"].Open, "BUG DEMONSTRATED: Room 2 door is still closed because instances are not shared");

        // 3. Apply the FIX: Unify
        DataManager.UnifyExits();
        Assert(room1.Exits["east"] == room2.Exits["west"], "Now they share the same instance");

        // 4. Demonstrate the FIX: consistency after unification
        // Note: UnifyExits currently picks one of the existing exits. 
        // In our case it might pick exit1 (open) or exit2 (closed) depending on room order.
        // Let's ensure it's closed first to test the opening.
        room1.Exits["east"].Open = false; 
        Assert(!room2.Exits["west"].Open, "Both should be closed now");

        Actions.DoAction(player1, "open east");
        Assert(room1.Exits["east"].Open, "Room 1 door opened");
        Assert(room2.Exits["west"].Open, "FIX VERIFIED: Room 2 door is also open because they share the same instance");
    }
}
}
