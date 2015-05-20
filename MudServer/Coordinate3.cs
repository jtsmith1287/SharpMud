using System;

namespace GameCore.Util {
	[Serializable]
	public class Coordinate3 {

		public int X;
		public int Y;
		public int Z;
		[NonSerialized]
		static Coordinate3 zero;

		public static Coordinate3 Zero {
			get {
				if (Coordinate3.zero == null) {
					zero = new Coordinate3(0, 0, 0);
					return (Coordinate3)zero.MemberwiseClone();
				} else {
					return (Coordinate3)zero.MemberwiseClone();
				}
			}
			private set { }
		}

		public Coordinate3(int x, int y, int z) {

			X = x;
			Y = y;
			Z = z;
		}

		public static Coordinate3 operator -(Coordinate3 one, Coordinate3 two) {

			return new Coordinate3(one.X - two.X, one.Y - two.Y, one.Z - two.Z);
		}

		public static Coordinate3 operator +(Coordinate3 one, Coordinate3 two) {

			return new Coordinate3(one.X + two.X, one.Y + two.Y, one.Z + two.Z);
		}

		public static bool operator ==(Coordinate3 one, Coordinate3 two) {

			if (System.Object.ReferenceEquals(one, two)) {
				return true;
			}
			if ((object)one == null || (object)two == null) {
				return false;
			}
			if (one.X == two.X && one.Y == two.Y && one.Z == two.Z) {
				return true;
			} else {
				return false;
			}
		}

		public static bool operator !=(Coordinate3 one, Coordinate3 two) {

			if (System.Object.ReferenceEquals(one, two)) {
				return false;
			}
			if (one == null || two == null) {
				return true;
			}
			if (one.X == two.X && one.Y == two.Y && one.Z == two.Z) {
				return false;
			} else {
				return true;
			}
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override string ToString() {

			return string.Format("Coordinate3({0}, {1}, {2})", X, Y, Z);
		}
	}
}

