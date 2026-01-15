using System.Linq;
using System;
using System.Threading;
using System.Collections.Generic;
using GameCore.Util;

namespace GameCore {
	public static class World {

		public static Dictionary<string, Room> Rooms = new Dictionary<string, Room>();
		public static readonly Dictionary<Guid, Mobile> Mobiles = new Dictionary<Guid, Mobile>();
		public static Dictionary<string, Guid> NameToPlayerPairs = new Dictionary<string, Guid>();
		public static readonly List<Spawner> Spawners = new List<Spawner>();
		public static readonly List<Thread> SpawnThreads = new List<Thread>();
		public static int CombatTick { get; private set; }
		
		private static Thread _aiThread;
		private static long _lastCombatTickTime;
		private const int CombatTickInterval = 3000;

		public static bool AddRoom(Room room) {

			string stringCoord = $"{room.Location.X} {room.Location.Y} {room.Location.Z}";

			// Check if the room exists already and return false if it does.
			Room maybeRoom;
			if (Rooms.TryGetValue(stringCoord, out maybeRoom)) {
				return false;
			}
			Rooms.Add(stringCoord, room);
			return true;
		}

		public static Room GetRoom(Coordinate3 location) {

			if (location == null) {
				return null;
			}

			string stringCoord = $"{location.X} {location.Y} {location.Z}";
			Rooms.TryGetValue(stringCoord, out Room room);
			return room;
		}

		/// <summary>
		/// Gets the room by ID. Incredibly slow, avoid using.
		/// </summary>
		/// <returns>The room by ID.</returns>
		/// <param name="id">Identifier.</param>
		public static Room GetRoomById(Guid id) {
			return (from entry in Rooms where entry.Value.ID == id select entry.Value).FirstOrDefault();
		}

		internal static void StopAllSpawnThreads() {
			_aiThread?.Abort();

			foreach (Thread thread in SpawnThreads) {
				thread.Abort();
			}
		}

		public static void StartAiThread() {
			if (_aiThread != null) return;
			
			_lastCombatTickTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			_aiThread = new Thread(RunAiUpdate) {
				IsBackground = true
			};
			_aiThread.Start();
		}

		private static void RunAiUpdate() {
			try {
				while (true) {
					long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
					if (currentTime - _lastCombatTickTime >= CombatTickInterval) {
						CombatTick++;
						_lastCombatTickTime = currentTime;
					}

					lock (Spawners) {
						foreach (var spawner in Spawners.ToArray()) {
							spawner.Update();
						}
					}
					Thread.Sleep(33);
				}
			} catch (ThreadAbortException) {
				Console.WriteLine("AI Thread aborted.");
			}
		}
	}
}

