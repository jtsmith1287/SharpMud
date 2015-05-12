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
		public static string[] ProcessLine (string line) {
			
			return line.Split (" ".ToCharArray (), StringSplitOptions.RemoveEmptyEntries);
		}

		public static void HandleLine (string line, PlayerEntity player) {
		
			string[] args = ProcessLine (line);
			
			bool processed = false;
			foreach (string arg in args) {
				if (processed) {
					break;
				}
				switch (arg) {
				default:
					player.MessageToClient ("No command found. Try again.");
					processed = true;
					break;
				case "e":
				case "w":
				case "n":
				case "s":
					processed = true;
					Actions.MoveRooms (player, arg);
					break;
				case "build":
					AdminActions.BuildRoom (player, args [1]);
					break;
				}
			}
			
		}
	}
}

