using System;
using System.Linq;
using System.Collections.Generic;
using MudServer.Util;
using MudServer.World;
using MudServer.Enums;

namespace MudServer.Entity {
    public class NonPlayerCharacter : BaseMobile {
        public Spawner SpawnerParent;
        public bool RoomEmpty = true;
        // an average rate the mob will wander. Greater is less often.
        readonly int _wanderFrequency = 300;

        public NonPlayerCharacter(SpawnData data, Spawner spawner) {
            Name = data.Name + Rnd.Next(1, 1000).ToString();
            Stats = (SpawnData)data.ShallowCopy();
            Stats.Health = Stats.MaxHealth;
            Stats.OnZeroHealth += HandleDeath;
            // Removed coupling to ThisBaseMobile
            SpawnerParent = spawner;
            GenerateID();
        }

        private void HandleDeath(Stats data) {
            GameState = GameState.Dead;
            TriggerOnDeath(data);
        }

        public void ExecuteLogic(int currentTick) {
            if (GameState == GameState.Combat) {
                ExecuteCombatLogic(currentTick);
            } else {
                ExecuteNonCombatLogic(currentTick);
            }
        }

        private void ExecuteCombatLogic(int currentTick) {
            if (LastCombatTick >= currentTick) return;

            int attacksPerTick = Stats.GetNumberOfAttacks();
            int ticksPassed = currentTick - (LastCombatTick == -1 ? currentTick - 1 : LastCombatTick);

            for (int t = 0; t < ticksPassed; t++) {
                for (int i = 0; i < attacksPerTick; i++) {
                    if (Target == null || Target.GameState == GameState.Dead || Target.Stats == null ||
                        Target.Stats.Health <= 0) {
                        BreakCombat();
                        goto End;
                    }

                    Attack(Target);
                    if (GameState != GameState.Combat) {
                        goto End;
                    }
                }
            }

            End:
            LastCombatTick = currentTick;
        }

        private void ExecuteNonCombatLogic(int currentTick) {
            NonCombatAction();
        }

        private void Attack(BaseMobile target) {
            if (target == null || target.Stats == null || target.GameState == GameState.Dead || target.Stats.Health <= 0) {
                BreakCombat();
                return;
            }

            if (target.Stats.Location != this.Stats.Location) {
                bool followed = TryFollow(target);
                if (!followed) {
                    BreakCombat();
                    return;
                } else {
                    return;
                }
            }

            if (!IsTargetPresent()) {
                BreakCombat();
                return;
            }

            StrikeTarget(target);
        }

        private bool TryFollow(BaseMobile target) {
            Room targetsRoom = World.World.GetRoom(target.Stats.Location);
            Room thisRoom = World.World.GetRoom(this.Stats.Location);
            if (thisRoom != null && targetsRoom != null) {
                lock (thisRoom.ConnectedRooms) {
                    foreach (var entry in thisRoom.ConnectedRooms) {
                        if (entry.Value == targetsRoom.Location) {
                            Move(entry.Value);
                            return true;
                        }
                    }
                }
            }

            // Unable to follow
            return false;
        }

        private void BreakCombat() {
            Target = null;
            GameState = GameState.Idle;
        }

        private void NonCombatAction() {
            SpawnData stats = (SpawnData)Stats;

            if (stats.Behaviour == Disposition.Hostile)
                SeekTarget();

            if (Rnd.Next(1, _wanderFrequency) == 1) {
                Wander();
            }
        }

        private void SeekTarget() {
            Room room = World.World.GetRoom(Stats.Location);
            if (room == null) return;

            SpawnData thisData = (SpawnData)Stats;
            foreach (Guid id in room.EntitiesHere) {
                if (id == Stats.Id) continue;

                var entity = World.World.GetEntity(id);
                if (entity == null || entity.Hidden) continue;

                if (entity is PlayerCharacter player) {
                    if (player.GameState == GameState.Dead || player.GodMode) {
                        continue;
                    }

                    Target = player;
                    player.SendToClient(Name + " sees you!", Color.Red);
                    break;
                } else if (entity is NonPlayerCharacter mob) {
                    if (mob.GameState == GameState.Dead) continue;
                    SpawnData data = (SpawnData)mob.Stats;
                    if (data.Faction == thisData.Faction) {
                        continue;
                    }

                    Target = mob;
                    break;
                }
            }

            if (Target == null) return;

            GameState = GameState.Combat;
            LastCombatTick = World.World.CombatTick;
        }

        private void Wander() {
            Room room = World.World.GetRoom(Stats.Location);
            if (room == null) return;

            lock (room.ConnectedRooms) {
                if (room.ConnectedRooms.Count <= 0) return;

                // Randomly choose a <string, coordinate3> element from connected rooms.
                KeyValuePair<string, Coordinate3> exitStringCoordPair
                    = room.ConnectedRooms.ElementAt(Rnd.Next(0, room.ConnectedRooms.Count));
                Move(exitStringCoordPair.Value);
            }
        }
    }
}
