using System;
using GameCore.Util;

namespace GameCore {
	public static class Actions {

		public static void Look(PlayerEntity player) {

			Room room = World.GetRoom(player.Location);
			string rawString = "{0}\n=============================\n{1}\nPlayers: {2}";
			string visiblePlayers = "";
			PlayerEntity entityInRoom;

			foreach (Guid id in room.EntitiesHere) {
				if (id == player.ID) {
					continue;
				}
				if (PlayerEntity.Players.TryGetValue(id, out entityInRoom)) {
					visiblePlayers += entityInRoom.Name + ", ";
				}
			}

			if (room != null) {
				string mesage = string.Format(rawString,
											   room.Name,
											   room.Description,
											   visiblePlayers);
				player.SendToClient(mesage);
			} else {
				player.SendToClient("Somehow... you're nowhere. Try logging in again.");
			}
		}

		public static void MoveRooms(PlayerEntity player, string direction) {

			Coordinate3 location = new Coordinate3(player.Location.X, player.Location.Y, player.Location.Z);

			switch (direction) {
				case "n":
					location.Y += 1;
					break;
				case "s":
					location.Y -= 1;
					break;
				case "e":
					location.X += 1;
					break;
				case "w":
					location.X -= 1;
					break;
				case "u":
					location.Z += 1;
					break;
				case "d":
					location.Z -= 1;
					break;
			}

			Room room = World.GetRoom(location);
			if (room != null) {
				Room playerRoom = World.GetRoom(player.Location);
				foreach (Guid id in playerRoom.ConnectedRooms) {
					if (id == room.ID) {
						player.Move(room.Location);
						break;
					}
				}
			} else {
				player.SendToClient("There's no exit in that direction!");
			}
		}

		public static void ViewStats(PlayerEntity player) {

			string message = "\n";
			message += string.Format("Name: {0}\n", player.Stats.Name);
			message += string.Format("Level: {0}\n", player.Stats.Level);
			message += string.Format("Health: {0}/{1}", player.Stats.Health, player.Stats.MaxHealth);
			player.SendToClient(message);
		}

		public static void ViewAllPlayers(PlayerEntity player) {

			string message = "\n";
			foreach (var entry in PlayerEntity.Players) {
				message += string.Format("Name: {0} -- Level: {1}\n", entry.Value.Name, entry.Value.Stats.Level);
			}
			player.SendToClient(message);
		}
	}
}

