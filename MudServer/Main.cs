using System;
using GameCore;

namespace ServerCore {
	public class Program {

		static void Main(string[] args) {

			try {
				Server server = new Server();
				server.Run();
			} catch (Exception) {
				Console.WriteLine("Woah woah, something happened! Saving data but it may be corrupt.");
			} finally {
				Data.SaveData();
			}
		}
	}
}

