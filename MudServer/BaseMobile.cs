using System;
using System.Collections.Generic;
using System.Diagnostics;
using GameCore.Util;

namespace GameCore {
	public class BaseMobile {

		public Guid ID;
		public string Name;
		public Data Stats;
		public bool InCombat = false;
		public bool IsDead = false;
		public BaseMobile Target;
		protected Stopwatch RoundTimer = new Stopwatch ();

		/// <summary>
		/// Generates and sets this instance's Guid.
		/// </summary>
		/// <returns>The generated Guid.</returns>
		public Guid GenerateID () {

			ID = Guid.NewGuid ();
			Stats.ID = ID;
			return ID;
		}

		public virtual void Move (Coordinate3 location) {

			Room oldRoom = World.GetRoom (Stats.Location);
			Room newRoom = World.GetRoom (location);

			if (oldRoom != null) {
				oldRoom.EntitiesHere.Remove (ID);
				BroadcastLocal (Name + " has left to the " + oldRoom.GetDirection (newRoom.Location),
				                Color.Yellow);
			}
			if (newRoom != null) {
				Stats.Location = newRoom.Location;
				if (oldRoom != null)
					BroadcastLocal (Name + " has arrived from the " + newRoom.GetDirection (oldRoom.Location),
					                Color.Yellow);
				else
					BroadcastLocal (Name + " has arrived.", Color.Yellow);
				newRoom.EntitiesHere.Add (ID);
				var thisPlayer = PlayerEntity.GetPlayerByID (Stats.ID);
				if (thisPlayer != null)
					Actions.Look (thisPlayer);
			}
		}

		public virtual void SendToClient (string msg, string colorSequence = "") {

			PlayerEntity player;
			if (PlayerEntity.Players.TryGetValue (ID, out player)) {
				player.SendToClient (msg, colorSequence);
			}
		}

		public void BroadcastLocal (string msg, string colorSequence = "", params Guid[] ignore) {

			PlayerEntity player;
			Room room = World.GetRoom (Stats.Location);
			if (room != null) {
				for (int i = 0; i < room.EntitiesHere.Count; i++) {
					if (PlayerEntity.Players.TryGetValue (room.EntitiesHere [i], out player)) {
						if (player.ID == ID || Contains<Guid> (ignore, player.ID)) {
							continue;
						}
						player.SendToClient (msg, colorSequence);
					}
				}
			}
		}

		protected bool Contains<T> (T[] a, T b) {

			for (int i = 0; i < a.Length; i++) {
				if (a [i].Equals (b))
					return true;
			}
			return false;
		}
	}
}

