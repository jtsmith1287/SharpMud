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

		public void ExecuteLogic() {

			if (InCombat) {
				if (RoundTimer.ElapsedMilliseconds > (long)3000) {
					Attack(Target);
				}
			} else {
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
			RoundTimer.Restart();
		}

		bool TryFollow(BaseMobile target) {

			Room targetsRoom = World.GetRoom(target.Stats.Location);
			Room thisRoom = World.GetRoom(this.Stats.Location);
			if (thisRoom != null && targetsRoom != null) {
				foreach (var entry in thisRoom.ConnectedRooms) {
					if (entry.Value == targetsRoom.Location) {
						Move(entry.Value);
					}
				}
			}
			// Unable to follow
			return false;
		}

		public void BreakCombat() {

			Target = null;
			InCombat = false;
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

			PlayerEntity player;
			Mobile mob;
			SpawnData thisData = (SpawnData)Stats;
			foreach (Guid id in World.GetRoom(Stats.Location).EntitiesHere) {
				if (id == Stats.ID) { continue; }
				if (PlayerEntity.Players.TryGetValue(id, out player)) {
					if (player.Hidden) { continue; }
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
				RoundTimer.Start();
			}
		}

		void Wander() {

			Room room = World.GetRoom(Stats.Location);
			if (room != null) {
				// Randomly choose a <string, coordinate3> element from connected rooms.
				var element = room.ConnectedRooms.ElementAt(Rnd.Next(0, room.ConnectedRooms.Count));
				Move(element.Value);
			}

		}
	}
}

