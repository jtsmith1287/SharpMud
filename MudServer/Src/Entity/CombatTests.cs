using System;
using System.Collections.Generic;
using System.Linq;
using MudServer.Entity;
using MudServer.World;
using MudServer.Enums;

namespace MudServer.Entity {
public class CombatTests {
    private int _passed = 0;
    private int _failed = 0;

    public void RunTests() {
        Console.WriteLine("Starting Combat Unit Tests...");

        TestDamageAndHealth();
        TestDodging();
        TestSurpriseAttack();
        TestDeath();
        TestExperienceGain();
        TestDisengageWhenNotPresent();

        Console.WriteLine("\nTests Finished!");
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

    private void SetupCombatScenario(out TestMobile player, out NonPlayerCharacter enemy) {
        // Setup a dummy room so BroadCastLocal doesn't fail or so Move works
        Coordinate3 loc = new Coordinate3(999, 999, 999);
        Room room = new Room(loc, "Test Room");

        Stats playerData = new Stats("TestPlayer", Guid.NewGuid());
        playerData.Str = 20;
        playerData.Dex = 20;
        playerData.Health = 100;
        playerData.MaxHealth = 100;
        playerData.Location = loc;

        player = new TestMobile(playerData);
        player.Name = playerData.Name;
        player.GenerateID();
        player.Target = new TestMobile(new Stats("Dummy", Guid.NewGuid())); // Avoid null Target.ID in StrikeTarget
        room.EntitiesHere.Add(player.Id);

        SpawnData enemyData = new SpawnData("TestEnemy");
        enemyData.Str = 10;
        enemyData.Dex = 10;
        enemyData.Health = 50;
        enemyData.MaxHealth = 50;
        enemyData.Location = loc;

        enemy = new NonPlayerCharacter(enemyData, null);
        enemy.GenerateID();
        World.World.Mobiles[enemy.Id] = enemy;
        room.EntitiesHere.Add(enemy.Id);
    }

    public void TestDamageAndHealth() {
        TestMobile player;
        NonPlayerCharacter enemy;
        SetupCombatScenario(out player, out enemy);

        int initialHealth = enemy.Stats.Health;

        player.TestStrikeTarget(enemy);

        Assert(
            enemy.Stats.Health <= initialHealth, "Enemy health should be less than or equal to initial (might dodge)"
        );

        // To ensure we test damage, we can loop until a hit happens or force it.
        // Force hit by setting Dex low on enemy
        enemy.Stats.Dex = 1;
        player.Stats.Str = 40; // dmg = 10 to 13 approx

        int healthBeforeHit = enemy.Stats.Health;
        bool hit = false;
        for (int i = 0; i < 50; i++) {
            player.TestStrikeTarget(enemy);
            if (enemy.Stats.Health < healthBeforeHit) {
                hit = true;
                int damageDealt = healthBeforeHit - enemy.Stats.Health;
                Assert(
                    damageDealt >= 10 && damageDealt <= 13,
                    "Damage dealt should be in expected range (10-13) for Str 40. Got: " + damageDealt
                );
                break;
            }
        }

        Assert(hit, "Should eventually hit the enemy");
    }

    public void TestDodging() {
        TestMobile player;
        NonPlayerCharacter enemy;
        SetupCombatScenario(out player, out enemy);

        // Set enemy Dex very high to increase dodge chance
        enemy.Stats.Dex = 10000;

        bool dodged = false;
        int initialHealth = enemy.Stats.Health;
        for (int i = 0; i < 200; i++) {
            player.TestStrikeTarget(enemy);
            if (enemy.Stats.Health == initialHealth) {
                dodged = true;
                break;
            }
        }

        Assert(dodged, "Enemy with high Dex should eventually dodge");
    }

    public void TestSurpriseAttack() {
        TestMobile player;
        NonPlayerCharacter enemy;
        SetupCombatScenario(out player, out enemy);

        player.Hidden = true;
        player.Stats.Str = 40; // dmg range 10-13, doubled to 20-26

        enemy.Stats.Dex = 1; // Ensure hit
        int initialHealth = enemy.Stats.Health;

        // We might need to loop if it still misses (though Dex 1 makes it very unlikely to dodge)
        bool hit = false;
        for (int i = 0; i < 10; i++) {
            player.TestStrikeTarget(enemy);
            if (enemy.Stats.Health < initialHealth) {
                hit = true;
                int damageDealt = initialHealth - enemy.Stats.Health;
                Assert(
                    damageDealt >= 20 && damageDealt <= 26,
                    "Surprise attack should deal double damage. Got: " + damageDealt
                );
                Assert(player.Hidden == false, "Hidden should be set to false after surprise attack");
                break;
            }
        }

        Assert(hit, "Surprise attack should hit");
    }

    public void TestDeath() {
        TestMobile player;
        NonPlayerCharacter enemy;
        SetupCombatScenario(out player, out enemy);

        enemy.Stats.Health = 1;
        player.Stats.Str = 40;
        enemy.Stats.Dex = 1; // Ensure hit

        for (int i = 0; i < 10; i++) {
            player.TestStrikeTarget(enemy);
            if (enemy.GameState == GameState.Dead) break;
        }

        Assert(enemy.Stats.Health <= 0, "Enemy health should be 0 or less");
        Assert(enemy.GameState == GameState.Dead, "Enemy should be marked as dead");
    }

    public void TestExperienceGain() {
        TestMobile player;
        NonPlayerCharacter enemy;
        SetupCombatScenario(out player, out enemy);

        player.Target = enemy;
        player.GameState = GameState.Combat;

        enemy.Stats.Health = 1;
        player.Stats.Str = 40;
        enemy.Stats.Dex = 1; // Ensure hit

        int initialExp = player.Stats.Exp;

        player.TestStrikeTarget(enemy);

        Assert(enemy.GameState == GameState.Dead, "Enemy should be dead");
        Assert(player.Stats.Exp > initialExp, "Player should have gained experience. Exp: " + player.Stats.Exp);
        Assert(player.GameState == GameState.Idle, "Player should be idle after killing target");
        Assert(player.Target == null, "Player target should be null after killing target");
    }

    public void TestDisengageWhenNotPresent() {
        TestMobile player;
        NonPlayerCharacter enemy;
        SetupCombatScenario(out player, out enemy);

        // Put enemy in combat with player
        enemy.Target = player;
        enemy.GameState = GameState.Combat;
        enemy.LastCombatTick = World.World.CombatTick - 1;

        // Verify it's in combat
        Assert(enemy.GameState == GameState.Combat, "Enemy should be in combat");

        // Remove player from the room's EntitiesHere list, but keep location same
        Room room = World.World.GetRoom(enemy.Stats.Location);
        room.EntitiesHere.Remove(player.Id);

        // Run logic
        enemy.ExecuteLogic(World.World.CombatTick);

        // Verify disengage
        Assert(enemy.GameState == GameState.Idle, "Enemy should disengage when target is not in the room's entity list");
    }

    // Wrapper to access protected StrikeTarget
    private class TestMobile : BaseMobile {
        public TestMobile(Stats stats) {
            this.Stats = stats;
        }

        public void TestStrikeTarget(BaseMobile target) {
            this.StrikeTarget(target);
        }

        public override void SendToClient(string msg, string colorSequence = "") {
            // Do nothing in tests to avoid NullReference from PlayerCharacter.SendToClient
        }
    }
}
}
