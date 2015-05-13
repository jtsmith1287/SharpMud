using System;

namespace GameCore.Util {
	public static class AdminActions {

		public static void BuildRoom (PlayerEntity player, string direction) {
			
			Coordinate3 location = new Coordinate3 (player.Location.X, player.Location.Y, player.Location.Z);

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

			Room room = World.GetRoom (location);
			// A room already exists in this location
			if (room != null) {
				player.SendToClient ("You're trying to build a room that already exists. Would you like to link them? (y|n)");
				string reply = player.WaitForClientReply ();
				if (reply != null && reply == "y") {
					Room playerRoom = World.GetRoom (player.Location);
					playerRoom.ConnectedRooms.Add (room.ID);
					room.ConnectedRooms.Add (playerRoom.ID);
					player.SendToClient (string.Format ("{0} and {1} have been connected.", room.Name, playerRoom.Name));
				} else {
					player.SendToClient ("Aborting build");
				}
				// Clear to build a new room at this location.
			} else {
				room = new Room (location, "###");
				Room playerRoom = World.GetRoom (player.Location);
				room.ConnectedRooms.Add (playerRoom.ID);
				playerRoom.ConnectedRooms.Add (room.ID);
				player.SendToClient (string.Format ("{0} has been created @ ({1}, {2}. {3}).", room.Name, room.Location.X, room.Location.Y, room.Location.Z));
			}
			
		}
	}
}

