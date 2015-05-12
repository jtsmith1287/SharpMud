using System;
using GameCore.Util;

namespace GameCore {
	public static class Actions {

		public static void MoveRooms (PlayerEntity player, string direction) {
			
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
			if (room != null) {
				Room playerRoom = World.GetRoom (player.Location);
				foreach (Guid id in playerRoom.ConnectedRooms) {
					if (id == room.ID) {
						player.Move (room.Location);
						break;
					}
				}
			} else {
				player.MessageToClient ("There's no exit in that direction!");
			}
		}
	}
}

