using System;
using System.Collections.Generic;
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

			if (player.IsDead) {
				player.SendToClient("\n\tBut you're dead... Just relax and enjoy the ride.\n", Color.Red);
				return;
			}

			string[] args = ProcessLine(line);

			if (args.Length == 0) {
				player.DisplayVitals();
				return;
			}

			// Loop over possible commands until user input matches a command stored.
			foreach (KeyValuePair<string, Action<PlayerEntity, string[]>> entry in Actions.ActionCalls) {
				if (AutoComplete(args[0], entry.Key)) {
					args[0] = entry.Key;
					entry.Value(player, args);
					player.DisplayVitals();
					return;
				}
			}
			if (player.Admin) {
				foreach (var entry in AdminActions.ActionCalls) {
					if (AutoComplete(args[0], entry.Key)) {
						// overwite the incomplete typed command with the full command.
						args[0] = entry.Key;
						entry.Value(player, args);
						player.DisplayVitals();
						return;
					}
				}
			}

			player.SendToClient("Nope, that's not a thing, sorry!", Color.Yellow);
		}

		public static bool AutoComplete(string typed, string full) {

			if (string.IsNullOrEmpty(typed) || string.IsNullOrEmpty(full)) {
				return false;
			}

			if (typed.Length > full.Length) {
				return false;
			}

			return full.StartsWith(typed, StringComparison.OrdinalIgnoreCase);
		}
	}
}
