using System.Linq;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace GameCore.Util {
	public class Spawner {

		public Guid ID;
		public SpawnData[] SpawnData;
		[ScriptIgnore]
		public List<Mobile> Spawns = new List<Mobile>();
		[ScriptIgnore]
		public List<Mobile> DeadSpawn = new List<Mobile>();
		public int MaxNumberOfSpawn = 1;
		public bool Spawning = false;
		public Coordinate3 Location;
		[ScriptIgnore]
		public Random Rand;
		[ScriptIgnore]
		public DateTime SpawnTime;

		public Spawner() {
			Spawning = true;
			Spawns = new List<Mobile>();
			DeadSpawn = new List<Mobile>();
			Rand = new Random();
			SpawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));
		}

		public Spawner(Room room, List<SpawnData> spawnList) : this() {
			if (room == null) return;

			SpawnData = spawnList.ToArray();

			if (room.SpawnersHere == null)
				room.SpawnersHere = new List<Spawner>();

			room.SpawnersHere.Add(this);
			Location = room.Location;
			ID = Guid.NewGuid();

			World.Spawners.Add(this);
			World.StartAiThread();
		}

		public void Update() {

			if (!Spawning || SpawnData == null || SpawnData.Length == 0) return;

			if (Spawns.Count < MaxNumberOfSpawn) {
				if (SpawnTime < DateTime.Now) {
					Mobile newMob = new Mobile(SpawnData[Rand.Next(0, SpawnData.Length)], this);
					Spawns.Add(newMob);
					newMob.Stats.OnZeroHealth += QueueDestroyMob;
					newMob.Move(Location);
					World.Mobiles.Add(newMob.Id, newMob);

					SpawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));
				}
			}
			int currentTick = World.CombatTick;
			foreach (Mobile mob in Spawns.ToArray()) {
				if (mob.GameState != GameState.Dead)
					mob.ExecuteLogic(currentTick);
			}

			if (DeadSpawn.Count <= 0) return;
			
			lock (DeadSpawn) {
				foreach (Mobile t in DeadSpawn) {
					DestroyMob(t);
				}

				DeadSpawn.Clear();
			}
		}

		private void DestroyMob(Mobile mob) {

			lock (Spawns) {
				Spawns.Remove(mob);
			}
			Room room = World.GetRoom(mob.Stats.Location);
			if (room != null) {
				lock (room.EntitiesHere) {
					room.EntitiesHere.Remove(mob.Id);
				}
			}
			lock (World.Mobiles) {
				World.Mobiles.Remove(mob.Id);
			}
			mob.BroadcastLocal(mob.Name + " has died!", Color.Yellow);
			mob.Stats.Location = null;
		}

		private void QueueDestroyMob(Stats data) {

			foreach (Mobile spawn in Spawns) {
				if (spawn.Id == data.Id) {
					spawn.GameState = GameState.Dead;
					spawn.Target = null;
					DeadSpawn.Add(spawn);
					break;
				}
			}
		}
	}
}

