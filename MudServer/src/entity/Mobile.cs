using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GameCore.Util;

namespace GameCore {
public class Mobile : BaseMobile {
    public Spawner SpawnerParent;
    public bool RoomEmpty = true;
    // an average rate the mob will wander. Greater is less often.
    readonly int _wanderFrequency = 300;

    public Mobile(SpawnData data, Spawner spawner) {
        Name = data.Name + Rnd.Next(1, 1000).ToString();
        Stats = (SpawnData)data.ShallowCopy();
        Stats.Health = Stats.MaxHealth;
        Stats.ThisBaseMobile = this;
        SpawnerParent = spawner;
        GenerateID();
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
        Room targetsRoom = World.GetRoom(target.Stats.Location);
        Room thisRoom = World.GetRoom(this.Stats.Location);
        if (thisRoom != null && targetsRoom != null) {
            lock (thisRoom.ConnectedRooms) {
                foreach (var entry in thisRoom.ConnectedRooms) {
                    if (entry.Value == targetsRoom.Location) {
                        Move(entry.Value);
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
        RoundTimer.Reset();
        RoundTimer.Stop();
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
        Room room = World.GetRoom(Stats.Location);
        if (room == null) return;

        SpawnData thisData = (SpawnData)Stats;
        foreach (Guid id in room.EntitiesHere.Where(id => id != Stats.Id)) {
            if (PlayerEntity.Players.TryGetValue(id, out PlayerEntity player)) {
                if (player.Hidden || player.GameState == GameState.Dead || player.GodMode) {
                    continue;
                }

                Target = player;
                player.SendToClient(Name + " sees you!", Color.Red);
                break;
            } else {
                if (!World.Mobiles.TryGetValue(id, out Mobile mob)) continue;
                if (mob.GameState == GameState.Dead || mob.Hidden) continue;
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
        LastCombatTick = World.CombatTick;
    }

    private void Wander() {
        Room room = World.GetRoom(Stats.Location);
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
