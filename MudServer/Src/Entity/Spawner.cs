using System.Linq;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using MudServer.World;
using MudServer.Util;
using MudServer.Enums;
using MudServer.Server;

namespace MudServer.Entity {
	public class Spawner {

		public Guid ID;
		public List<Guid> SpawnDataIds = new List<Guid>();
		public List<SpawnData> SpawnData {
			set {
				if (value == null) return;
				foreach (var data in value) {
					if (data == null) continue;
					if (data.Id == Guid.Empty) data.Id = Guid.NewGuid();
					if (!SpawnDataIds.Contains(data.Id)) {
						SpawnDataIds.Add(data.Id);
					}
					// Ensure it's in the global list so it can be resolved
					if (!string.IsNullOrEmpty(data.Name) && !DataManager.NameSpawnPairs.ContainsKey(data.Name)) {
						DataManager.NameSpawnPairs[data.Name] = data;
					}
				}
			}
		}
		[ScriptIgnore]
		public List<NonPlayerCharacter> Spawns = new List<NonPlayerCharacter>();
		[ScriptIgnore]
		public List<NonPlayerCharacter> DeadSpawn = new List<NonPlayerCharacter>();
		public int MaxNumberOfSpawn = 1;
		public bool Spawning = false;
		public Coordinate3 Location;
		[ScriptIgnore]
		public Random Rand;
		[ScriptIgnore]
		public DateTime SpawnTime;

		public Spawner() {
			Spawning = true;
			Spawns = new List<NonPlayerCharacter>();
			DeadSpawn = new List<NonPlayerCharacter>();
			Rand = new Random();
			SpawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));
		}

		public Spawner(Room room, List<Guid> spawnDataIds) : this() {
			if (room == null) return;

			SpawnDataIds = spawnDataIds;

			if (room.SpawnersHere == null)
				room.SpawnersHere = new List<Spawner>();

			room.SpawnersHere.Add(this);
			Location = room.Location;
			ID = Guid.NewGuid();

			World.World.Spawners.Add(this);
			World.World.StartAiThread();
		}

		public void Update() {

			if (!Spawning || SpawnDataIds == null || SpawnDataIds.Count == 0) return;

			if (Spawns.Count < MaxNumberOfSpawn) {
				if (SpawnTime < DateTime.Now) {
					Guid randomSpawnId = SpawnDataIds[Rand.Next(0, SpawnDataIds.Count)];
					SpawnData data = DataManager.NameSpawnPairs.Values.FirstOrDefault(s => s.Id == randomSpawnId);
					if (data != null) {
						NonPlayerCharacter newMob = new NonPlayerCharacter(data, this);
						Spawns.Add(newMob);
						newMob.Stats.OnZeroHealth += QueueDestroyMob;
						newMob.Move(Location);
						World.World.Mobiles.Add(newMob.Id, newMob);
					}

					SpawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));
				}
			}
			int currentTick = World.World.CombatTick;
			foreach (NonPlayerCharacter mob in Spawns.ToArray()) {
				if (mob.GameState != GameState.Dead)
					mob.ExecuteLogic(currentTick);
			}

			if (DeadSpawn.Count <= 0) return;
			
			lock (DeadSpawn) {
				foreach (NonPlayerCharacter t in DeadSpawn) {
					DestroyMob(t);
				}

				DeadSpawn.Clear();
			}
		}

		private void DestroyMob(NonPlayerCharacter mob) {
			lock (Spawns) {
				Spawns.Remove(mob);
			}

			lock (World.World.Mobiles) {
				World.World.Mobiles.Remove(mob.Id);
			}

			mob.BroadcastLocal(mob.Name + " has died!", Color.Yellow);

			if (!World.World.TryGetRoom(mob.Stats.Location, out Room room)) {
				mob.Stats.Location = null;
				return;
			}

			lock (room.EntitiesHere) {
				room.EntitiesHere.Remove(mob.Id);
			}
			mob.Stats.Location = null;
		}

		private void QueueDestroyMob(Stats data) {

			foreach (NonPlayerCharacter spawn in Spawns) {
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

