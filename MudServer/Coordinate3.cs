using System;

namespace GameCore.Util {
	public class Coordinate3 {
		
		public int X;
		public int Y;
		public int Z;
		static Coordinate3 zero;

		public static Coordinate3 Zero {
			get {
				if (Coordinate3.zero == null) {
					zero = new Coordinate3 (0, 0, 0);
				}
				return zero;
			}
			private set {}
		}

		public Coordinate3 (int x, int y, int z) {
			
 X = x;
			Y = y;
			Z = z;
		}
	}
}

