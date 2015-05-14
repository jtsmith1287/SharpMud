using System;
using GameCore;
using GameCore.Util;

namespace ServerCore.Util {
	public class ArgumentHandler {

		/// <summary>
		/// Creates a white space delimited array of zero whitespace strings, usually words."
		/// </summary>
		/// <returns>A whitespace delimited array of strings.</returns>
		/// <param name="line">A string.</param>
		public static string[] ProcessLine(string line) {

			return line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		}

		public static void HandleLine(string line, PlayerEntity player) {

			string[] args = ProcessLine(line);
			string arg;

			if (args.Length > 0) {
				arg = args[0];
			} else {
				player.SendToClient("Yes?");
				return;
			}

			switch (arg) {
				default:
					if (player.Admin) {
						HandleLineAdmin(args, player);
					} else {
						player.SendToClient("No command found. Try again.");
					}
					break;
				// Movement
				case "e":
				case "w":
				case "n":
				case "s":
					Actions.MoveRooms(player, arg);
					break;
				case "look":
					Actions.Look(player);
					break;
				case "stats":
					Actions.ViewStats(player);
					break;
				case "who":
					Actions.ViewAllPlayers(player);
					break;
			}
		}

		private static void HandleLineAdmin(string[] args, PlayerEntity player) {


			switch (args[0]) {
				default:
					player.SendToClient("No command found. Try again.");
					break;
				case "build":
					AdminActions.BuildRoom(player, args[1]);
					break;
				case "spawn":
					AdminActions.CreateSpawner(player, args);
					break;
			}
		}
	}
}
