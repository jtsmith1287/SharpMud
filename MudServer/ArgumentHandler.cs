using System;
using GameCore;

namespace ServerCore.Util {
	public class ArgumentHandler {

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
				}
			}
			
		}
	}
}

