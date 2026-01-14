using System;
using GameCore;
using GameCore.Util;

namespace ServerCore {
	public class Program {
		static void Main(string[] args) {
			if (args.Length > 0 && args[0] == "--test") {
				new MudServer.CombatTests().RunTests();
				return;
			}
			try {
				Server server = new Server();
				server.Run();
			} catch (Exception e) {
				Console.WriteLine("Woah woah, something happened! Saving data but it may be corrupt.");
				Console.WriteLine(e.Message);
				Console.WriteLine(e.GetType().ToString());
				Console.WriteLine(e.StackTrace);
			} finally {
				Data.SaveData(
					DataPaths.IdData,
					DataPaths.Spawn,
					DataPaths.UserId,
					DataPaths.UserPwd,
					DataPaths.World);
				Console.ReadLine();
			}
		}
	}
}

