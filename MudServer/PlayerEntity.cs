using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ServerCore;
using GameCore.Util;

namespace GameCore {
	public class PlayerEntity : BaseMobile {

		public static Dictionary<Guid, PlayerEntity> Players = new Dictionary<Guid, PlayerEntity>();
		Connection Conn;

		public Coordinate3 Location {
			get {
				return Stats.Location;
			}
			set {
				Stats.Location = value;
			}
		}

		public bool Admin = true;
		public PlayerState State;

		public PlayerEntity(Connection conn, Data data) {

			Conn = conn;
			ID = data.ID;
			Name = data.Name;
			Stats = data;

			// The player hasn't been initialized yet.
			if (Stats.MaxHealth == 0) {
				Stats.MaxHealth = 15;
				Stats.Health = Stats.MaxHealth;
			}

			if (data.Location != null) {
				Move(data.Location);
			} else {
				Move(Coordinate3.Zero);
			}

			Players.Add(ID, this);
			Conn.Send("Welcome!");
			State = PlayerState.Active;
		}

		public bool Move(Coordinate3 newLocation) {

			Room newRoom = World.GetRoom(newLocation);
			if (newRoom != null) {
				Room oldRoom = World.GetRoom(Location);
				if (oldRoom != null)
					oldRoom.EntitiesHere.Remove(ID);
				Location = newRoom.Location;
				newRoom.EntitiesHere.Add(ID);
				Conn.Send("You have arrived at " + newRoom.Name + ".");
				Actions.Look(this);
				return true;
			}
			return false;
		}

		public void Close() {

			Conn.SendClosingMessage();
			if (Conn.socket.Connected) {
				Conn.socket.Close();
			}
			Players.Remove(ID);
			Conn.Dispose();
		}

		public void SendToClient(string msg) {

			Conn.Send(msg);
		}

		public string WaitForClientReply() {

			try {
				string reply = Conn.Reader.ReadLine();
				if (reply != null) {
					return reply.Trim();
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

