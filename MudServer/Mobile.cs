using System;
using GameCore.Util;

namespace GameCore {
	public class Mobile: BaseMobile {
	
		Disposition disposition = Disposition.Neutral;

		public Mobile (Data data, Disposition disp) {
			
			Name = data.Name;
			Stats = data;
			disposition = disp;
		}
	}
}

