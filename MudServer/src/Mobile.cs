using System;
using System.Linq;
using System.Collections;
using GameCore.Util;

namespace GameCore {
	public class Mobile : BaseMobile {

		public Spawner SpawnerParent;
		public bool RoomEmpty = true;
		// an average rate the mob will wander. Greater is less often.
		int WanderFrequency = 300;

		public Mobile(SpawnData data, Spawner spawner) {

			Name = data.Name + Rnd.Next(1, 1000).ToString();
			Stats = (SpawnData)data.ShallowCopy();
			Stats.Health = Stats.MaxHealth;
			Stats.thisBaseMobile = this;
			SpawnerParent = spawner;
			GenerateID();
		}

		public void ExecuteLogic(int currentTick) {

			if (InCombat) {
				if (LastCombatTick < currentTick) {
					int ticksPassed = currentTick - (LastCombatTick == -1 ? currentTick : LastCombatTick);
					if (LastCombatTick == -1) ticksPassed = 1;

					float speedModifier = 1.0f + (Stats.GetTickModifier() / 3000f);
					CombatEnergy += ticksPassed * speedModifier;

					while (CombatEnergy >= 1.0f) {
						if (Target == null || Target.IsDead || Target.Stats == null || Target.Stats.Health <= 0) {
							BreakCombat();
							break;
						}
						Attack(Target);
						if (!InCombat) {
							CombatEnergy = 0;
							break;
						}
						CombatEnergy -= 1.0f;
					}
					LastCombatTick = currentTick;
				}
			} else {
				CombatEnergy = 0;
				NonCombatAction();
			}
		}

		public void Attack(BaseMobile target) {

			if (target.IsDead) {
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

			StrikeTarget(target);
		}

		bool TryFollow(BaseMobile target) {
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

		public void BreakCombat() {

			Target = null;
			InCombat = false;
			CombatEnergy = 0;
			RoundTimer.Reset();
			RoundTimer.Stop();
		}

		void NonCombatAction() {

			SpawnData stats = (SpawnData)Stats;

			if (stats.Behaviour == Disposition.Hostile)
				SeekTarget();

			if (Rnd.Next(1, WanderFrequency) == 1) {
				Wander();
			}

		}

		void SeekTarget() {
			Room room = World.GetRoom(Stats.Location);
			if (room == null) return;

			PlayerEntity player;
			Mobile mob;
			SpawnData thisData = (SpawnData)Stats;
			foreach (Guid id in room.EntitiesHere) {
				if (id == Stats.ID) { continue; }
				if (PlayerEntity.Players.TryGetValue(id, out player)) {
					if (player.Hidden || player.IsDead) { continue; }
					Target = player;
					player.SendToClient(Name + " sees you!", Color.Red);
					player.InCombat = true;
					break;
				} else if (World.Mobiles.TryGetValue(id, out mob)) {
					if (!mob.IsDead && !mob.Hidden) {
						SpawnData data = (SpawnData)mob.Stats;
						if (data.Faction == thisData.Faction) { continue; }
						Target = mob;
						break;
					}
				}
			}

			if (Target != null) {
				InCombat = true;
				LastCombatTick = World.CombatTick;
				CombatEnergy = 0;
			}
		}

		void Wander() {
			Room room = World.GetRoom(Stats.Location);
			if (room != null) {
				lock (room.ConnectedRooms) {
					if (room.ConnectedRooms.Count > 0) {
						// Randomly choose a <string, coordinate3> element from connected rooms.
						var element = room.ConnectedRooms.ElementAt(Rnd.Next(0, room.ConnectedRooms.Count));
						Move(element.Value);
					}
				}
			}
		}
	}
}

