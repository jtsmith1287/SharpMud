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

		public void Move(Coordinate3 location) {

			Room oldRoom = World.GetRoom(Stats.Location);
			if (oldRoom != null) {
				oldRoom.EntitiesHere.Remove(ID);
			}
			Room newRoom = World.GetRoom(location);
			if (newRoom != null) {
				Stats.Location = location;
				newRoom.EntitiesHere.Add(ID);
			}
		}
	}
}

