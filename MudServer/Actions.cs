using System;
using System.Collections.Generic;
using GameCore.Util;
using ServerCore.Util;

namespace GameCore {
	public static class Actions {

		public static readonly Dictionary<string, Action<PlayerEntity, string[]>> ActionCalls =
			new Dictionary<string, Action<PlayerEntity, string[]>>{
				{"attack",		Attack},
				{"north",		MoveRooms},
				{"south",		MoveRooms},
				{"east",		MoveRooms},
				{"west",		MoveRooms},
				{"up",			MoveRooms},
				{"down",		MoveRooms},
				{"stats",		ViewStats},
				{"who",			ViewAllPlayers},
				{"look",		Look},
				{"exp",         ShowExp},
				{"sneak",		Sneak},		
			};


		public static void Attack(PlayerEntity player, string[] args) {

			if (args.Length < 2) {
				player.SendToClient("Attack what?", Color.Red);
				return;
			}
			Room room = World.GetRoom(player.Location);
			if (room == null) {
				player.SendToClient("Woah, you're nowhere. Try logging in again.", Color.Red);
				return;
			}
			string name = args[1].ToLower();
			PlayerEntity targetPlayer;
			Mobile targetMob;
			foreach (Guid id in room.EntitiesHere) {
				if (World.Mobiles.TryGetValue(id, out targetMob)) {
					if (ArgumentHandler.AutoComplete(name, targetMob.Name.ToLower())) {
						player.Target = targetMob;
						player.InCombat = true;
						break;
					}
				} else if (PlayerEntity.Players.TryGetValue(id, out targetPlayer)) {
					if (ArgumentHandler.AutoComplete(name, targetPlayer.Name.ToLower())) {
						player.Target = targetPlayer;
						player.InCombat = true;
						break;
					}
				}
			}
			if (player.InCombat) {
				player.SendToClient(string.Format(
					"* Combat engaged with {0}! *", player.Target.Name), Color.White);
			} else {
				player.SendToClient("Nothing here by that name...", Color.Red);
			}

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
			string exits = "";
			PlayerEntity playerInRoom;
			Mobile mobInRoom;

			foreach (Guid id in room.EntitiesHere) {
				if (id == player.ID) {
					continue;
				}
				if (PlayerEntity.Players.TryGetValue(id, out playerInRoom)) {
					if (!playerInRoom.Hidden) {
						visiblePlayers += playerInRoom.Name + ", ";
					}
				} else {
					World.Mobiles.TryGetValue(id, out mobInRoom);
					if (mobInRoom != null) {
						if (!mobInRoom.Hidden) {
							visibleMobs += mobInRoom.Name + ", ";
						}
					}
				}
			}

			foreach (var entry in room.ConnectedRooms) {
				exits += entry.Key + ", ";
			}

			if (room != null) {
				string mesage = string.Format(rawString,
											   room.Name,
											   room.Description,
											   visiblePlayers,
											   visibleMobs,
											   exits);
				player.SendToClient(mesage, Color.GreenD);
			} else {
				player.SendToClient("Somehow... you're nowhere. Try logging in again.");
			}
		}

		public static void MoveRooms(PlayerEntity player, string[] args) {


			Room room = World.GetRoom(player.Location);
			if (room != null) {
				Coordinate3 locationOfNewRoom;
				if (room.ConnectedRooms.TryGetValue(args[0], out locationOfNewRoom)) {
					player.Move(locationOfNewRoom);
				} else {
					Console.Write(locationOfNewRoom);
					Console.WriteLine(args[0]);
					player.SendToClient("There's no exit in that direction!", Color.Red);
				}
			} else {
				player.SendToClient(
					"Woah. Somethin' is busted. You're nowhere -- so please re-log in.", Color.Red);
			}
		}

		public static void ShowExp(PlayerEntity player, string[] args) {

			player.SendToClient(string.Format("Experience: {0}/{1}",
				player.Stats.Exp, player.Stats.ExpToNextLevel));
		}

		public static void Sneak(PlayerEntity player, string[] args) {

			if (player.InCombat) {
				player.SendToClient("\n\tYou can't sneak! You're being stared at!\n");
				return;
			}
			//TODO: This should be chance
			player.Hidden = true;
			player.SendToClient("You are sneaking...", Color.Magenta);
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

