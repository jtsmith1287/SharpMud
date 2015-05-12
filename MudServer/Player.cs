using System;
using System.IO;
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
				Move (Stats.Location);
			} else {
				Move (Coordinate3.Zero);
			}

			Players.Add (ID, this);
			Conn.Send ("Welcome!");	
			State = PlayerState.Active;
		}

		bool LoadSavedState (Guid id) {

			Stats = Data.GetData (id);
			if (Stats == null) {
				return false;
			} else {
				return true;
			}
		}

		public bool Move (Coordinate3 newLocation) {
			
			Room newRoom = World.GetRoom (newLocation);
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

		public string WaitForClientReply () {
			
			try {
				string reply = Conn.Reader.ReadLine ();
				if (reply != null) {
					return reply.Trim ();
				} else {
					// null
					return reply;
				}
			} catch (IOException) {
				// Player disconnected.
				return null;
			}
		}
	}
}

