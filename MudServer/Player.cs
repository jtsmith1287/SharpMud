using System;
using System.Collections;
using System.Collections.Generic;
using ServerCore;
using GameCore.Util;

namespace GameCore {
	public class PlayerEntity: BaseMobile {

		public static Dictionary<Guid, PlayerEntity> Players = new Dictionary<Guid, PlayerEntity> ();
		Connection Conn;
		public Coordinate3 Location;
		public bool Admin = true;
		public Data Stats;
		public PlayerState State;

		public PlayerEntity (Connection conn, Guid id, string name) {

			Conn = conn;
			ID = id;
			Name = name;

			if (LoadSavedState (id)) {
				Move(Stats.Location);
			} else {
				Move (Coordinate3.Zero);
			}

			Players.Add (ID, this);
			Conn.Send ("Welcome!");	
			State = PlayerState.Active;
		}

		bool LoadSavedState (Guid id) {

			Stats = Data.GetData(id);
			if (Stats == null) {
				return false;
			} else {
				return true;
			}
		}

		public bool Move (Direction direction) {
			
			Room oldRoom = World.GetRoom (Location);
			// If this IS null, then that means the player doesn't currently have a location (like when first logging in).
			if (oldRoom != null) {
				Room newRoom = oldRoom.GetConnectedRoom (direction);
				if (newRoom != null) {
					// Remove the player from the old room and inform them.
					oldRoom.EntitiesHere.Remove (this.ID);
					Conn.Send ("You leave the room. " + oldRoom.Name);
					// Send a message to any players in the room if there are any.
					foreach (Guid id in oldRoom.EntitiesHere) {
						PlayerEntity player;
						if (Players.TryGetValue (id, out player)) {
							player.Conn.Send (this.Name + " has left."); //TODO: Add direction to this
						}
					}
					// Add the player to the new room and inform them.
					newRoom.EntitiesHere.Add (this.ID);
					Conn.Send ("You enter a new room. " + newRoom.Name);
					// Send a message to any players in the new room that this player has arrived.
					foreach (Guid id in newRoom.EntitiesHere) {
						PlayerEntity player;
						if (Players.TryGetValue (id, out player)) {
							player.Conn.Send (this.Name + " has entered the room."); //TODO: Add direction to this.
						}
					}
					
					Location = newRoom.Location;
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}			
		}

		public bool Move (Coordinate3 newLocation) {
			
			Room newRoom = World.GetRoom (newLocation);
			Console.Write (newRoom);
			if (newRoom != null) {
				Room oldRoom = World.GetRoom (Location);
				if (oldRoom != null)
					oldRoom.EntitiesHere.Remove (ID);
				Location = newRoom.Location;
				newRoom.EntitiesHere.Add (ID);
				Conn.Send ("You have arrived at " + newRoom.Name + ".");
				return true;
			}
			return false;
		}

		public void Close () {
		
			Players.Remove (ID);
		}

		public void MessageToClient (string msg) {
		
			Conn.Send (msg);
		}
	}
}

