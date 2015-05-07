using System;

namespace GameCore {
	public class BaseMobile {
		
		public Guid ID;
		public string Name;

		public void GenerateID () {
			
			ID = Guid.NewGuid ();
		}
	}
}

