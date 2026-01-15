using System;
using System.Collections.Generic;
using System.Linq;
using GameCore;
using GameCore.Util;

namespace MudServer {
    public class CombatTests {
        private int _passed = 0;
        private int _failed = 0;

        public void RunTests() {
            Console.WriteLine("Starting Combat Unit Tests...");

            TestDamageAndHealth();
            TestDodging();
            TestSurpriseAttack();
            TestDeath();

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

        private void SetupCombatScenario(out TestMobile player, out Mobile enemy) {
            // Setup a dummy room so BroadCastLocal doesn't fail or so Move works
            Coordinate3 loc = new Coordinate3(999, 999, 999);
            Room room = new Room(loc, "Test Room");

            Data playerData = new Data("TestPlayer", Guid.NewGuid());
            playerData.Str = 20;
            playerData.Dex = 20;
            playerData.Health = 100;
            playerData.MaxHealth = 100;
            playerData.Location = loc;
            
            player = new TestMobile(playerData);
            player.Name = playerData.Name;
            player.GenerateID();
            player.Target = new TestMobile(new Data("Dummy", Guid.NewGuid())); // Avoid null Target.ID in StrikeTarget
            room.EntitiesHere.Add(player.ID);

            SpawnData enemyData = new SpawnData("TestEnemy");
            enemyData.Str = 10;
            enemyData.Dex = 10;
            enemyData.Health = 50;
            enemyData.MaxHealth = 50;
            enemyData.Location = loc;
            
            enemy = new Mobile(enemyData, null);
            enemy.GenerateID();
            World.Mobiles[enemy.ID] = enemy;
            room.EntitiesHere.Add(enemy.ID);
        }

        public void TestDamageAndHealth() {
            TestMobile player;
            Mobile enemy;
            SetupCombatScenario(out player, out enemy);

            int initialHealth = enemy.Stats.Health;
            
            player.TestStrikeTarget(enemy);

            Assert(enemy.Stats.Health <= initialHealth, "Enemy health should be less than or equal to initial (might dodge)");
            
            // To ensure we test damage, we can loop until a hit happens or force it.
            // Force hit by setting Dex low on enemy
            enemy.Stats.Dex = 1;
            player.Stats.Str = 40; // dmg = 10 to 13 approx
            
            int healthBeforeHit = enemy.Stats.Health;
            bool hit = false;
            for(int i=0; i<50; i++) {
                player.TestStrikeTarget(enemy);
                if (enemy.Stats.Health < healthBeforeHit) {
                    hit = true;
                    int damageDealt = healthBeforeHit - enemy.Stats.Health;
                    Assert(damageDealt >= 10 && damageDealt <= 13, "Damage dealt should be in expected range (10-13) for Str 40. Got: " + damageDealt);
                    break;
                }
            }
            Assert(hit, "Should eventually hit the enemy");
        }

        public void TestDodging() {
            TestMobile player;
            Mobile enemy;
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
            Mobile enemy;
            SetupCombatScenario(out player, out enemy);

            player.Hidden = true;
            player.Stats.Str = 40; // dmg range 10-13, doubled to 20-26

            enemy.Stats.Dex = 1; // Ensure hit
            int initialHealth = enemy.Stats.Health;
            
            // We might need to loop if it still misses (though Dex 1 makes it very unlikely to dodge)
            bool hit = false;
            for(int i=0; i<10; i++) {
                player.TestStrikeTarget(enemy);
                if (enemy.Stats.Health < initialHealth) {
                    hit = true;
                    int damageDealt = initialHealth - enemy.Stats.Health;
                    Assert(damageDealt >= 20 && damageDealt <= 26, "Surprise attack should deal double damage. Got: " + damageDealt);
                    Assert(player.Hidden == false, "Hidden should be set to false after surprise attack");
                    break;
                }
            }
            Assert(hit, "Surprise attack should hit");
        }

        public void TestDeath() {
            TestMobile player;
            Mobile enemy;
            SetupCombatScenario(out player, out enemy);

            enemy.Stats.Health = 1;
            player.Stats.Str = 40;
            enemy.Stats.Dex = 1; // Ensure hit

            for(int i=0; i<10; i++) {
                player.TestStrikeTarget(enemy);
                if (enemy.Stats.Health <= 0) break;
            }
            
            Assert(enemy.Stats.Health <= 0, "Enemy health should be 0 or less");
        }

        // Wrapper to access protected StrikeTarget
        private class TestMobile : BaseMobile {
            public TestMobile(Data stats) {
                this.Stats = stats;
            }
            public void TestStrikeTarget(BaseMobile target) {
                this.StrikeTarget(target);
            }
            public override void SendToClient(string msg, string colorSequence = "") {
                // Do nothing in tests to avoid NullReference from PlayerEntity.SendToClient
            }
        }
    }
}
