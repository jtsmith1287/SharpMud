using System;
using System.Collections.Generic;
using GameCore.Util;

namespace GameCore {
	public static class Actions {

		public static Dictionary<string, Action<PlayerEntity, string[]>> ActionCalls =
			new Dictionary<string, Action<PlayerEntity, string[]>>{
				{"attack",		new Action<PlayerEntity, string[]>(Actions.Attack)},
				{"north",		new Action<PlayerEntity, string[]>(Actions.MoveRooms)},
				{"south",		new Action<PlayerEntity, string[]>(Actions.MoveRooms)},
				{"east",		new Action<PlayerEntity, string[]>(Actions.MoveRooms)},
				{"west",		new Action<PlayerEntity, string[]>(Actions.MoveRooms)},
				{"up",			new Action<PlayerEntity, string[]>(Actions.MoveRooms)},
				{"down",		new Action<PlayerEntity, string[]>(Actions.MoveRooms)},
				{"stats",		new Action<PlayerEntity, string[]>(Actions.ViewStats)},
				{"who",			new Action<PlayerEntity, string[]>(Actions.ViewAllPlayers)},
				{"look",		new Action<PlayerEntity, string[]>(Actions.Look)},
			};


		public static void Attack(PlayerEntity player, string[] args) {


		}

		public static void Look(PlayerEntity player, string[] args) {

			Room room = World.GetRoom(player.Location);
			string rawString = "\n " +
				Color.Green + "{0}\n" +
				Color.GreenD + "=============================\n{1}" +
				Color.Cyan + "\nPlayers: {2}" +
				Color.Magenta + "\nAlso here: {3}" +
				Color.White + "\nExits: {4}\n" +
				Color.Reset;
			string visiblePlayers = "";
			string visibleMobs = "";
			PlayerEntity playerInRoom;
			Mobile mobInRoom;

			foreach (Guid id in room.EntitiesHere) {
				if (id == player.ID) {
					continue;
				}
				if (PlayerEntity.Players.TryGetValue(id, out playerInRoom)) {
					visiblePlayers += playerInRoom.Name + ", ";
				} else {
					World.Mobiles.TryGetValue(id, out mobInRoom);
					if (mobInRoom != null)
						visibleMobs += mobInRoom.Name + ", ";
				}
			}

			if (room != null) {
				string mesage = string.Format(rawString,
											   room.Name,
											   room.Description,
											   visiblePlayers,
											   visibleMobs,
											   "");
				player.SendToClient(mesage, Color.GreenD);
			} else {
				player.SendToClient("Somehow... you're nowhere. Try logging in again.");
			}
		}

		public static void MoveRooms(PlayerEntity player, string[] args) {

			Coordinate3 location = new Coordinate3(player.Location.X, player.Location.Y, player.Location.Z);

			switch (args[0]) {
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

		public static void ViewAllPlayers(PlayerEntity player, string[] args) {

			string message = "\n";
			foreach (var entry in PlayerEntity.Players) {
				message += string.Format("Name: {0} -- Level: {1}\n", entry.Value.Name, entry.Value.Stats.Level);
			}
			player.SendToClient(message);
		}

		public static void ViewStats(PlayerEntity player, string[] args) {

			string message = "\n";
			message += "==========================================\n";
			message += string.Format(" Name: {0}\n", player.Stats.Name);
			message += string.Format(" Level: {0}\n", player.Stats.Level);
			message += "------------------------------------------\n";
			message += string.Format(" {0, -15} | {1,4} / {2,-4} \n", "Health:", player.Stats.Health, player.Stats.MaxHealth);
			message += string.Format(" {0, -15} | {1,-4} + {2,-4} \n", "Strength:", player.Stats.Str, player.Stats.BonusStr);
			message += string.Format(" {0, -15} | {1,-4} + {2,-4} \n", "Dexterity:", player.Stats.Dex, player.Stats.BonusDex);
			message += string.Format(" {0, -15} | {1,-4} + {2,-4} \n", "Intelligence:", player.Stats.Int, player.Stats.BonusInt);
			message += "==========================================\n";
			player.SendToClient(message);
		}

	}
}

