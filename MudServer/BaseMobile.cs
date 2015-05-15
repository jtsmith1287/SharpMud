using System;
using GameCore.Util;

namespace GameCore {
	public class BaseMobile {

		public Guid ID;
		public string Name;
		public Data Stats;

		public void GenerateID() {

			ID = Guid.NewGuid();
		}

		public virtual void Move(Coordinate3 location) {

			Room oldRoom = World.GetRoom(Stats.Location);
			Room newRoom = World.GetRoom(location);

			if (oldRoom != null) {
				oldRoom.EntitiesHere.Remove(ID);
				Stats.Location = null;

				foreach (Guid id in oldRoom.EntitiesHere) {
					var player = PlayerEntity.GetPlayerByID(id);
					if (player != null) {
						player.SendToClient(Name + " has left to the " + oldRoom.GetDirection(newRoom.Location));
					}
				}
			}
			if (newRoom != null) {
				foreach (Guid id in newRoom.EntitiesHere) {
					var player = PlayerEntity.GetPlayerByID(id);
					if (player != null) {
						if (oldRoom != null)
							player.SendToClient(
								Name + " has arrived from the " + newRoom.GetDirection(oldRoom.Location));
						else
							player.SendToClient(Name + " has arrived.");
					}
				}
				newRoom.EntitiesHere.Add(ID);
				Stats.Location = newRoom.Location;
				var thisPlayer = PlayerEntity.GetPlayerByID(Stats.ID);
				if (thisPlayer != null)
					Actions.Look(thisPlayer);
			}
		}

		public virtual void SendToClient(string msg) {
		}
	}
}

