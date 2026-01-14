using System;
using System.Web.Script.Serialization;

namespace GameCore.Util {
	public class Coordinate3 {

		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		[ScriptIgnore]
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

		public static Coordinate3 Purgatory {
			get {
				return new Coordinate3(int.MaxValue, int.MaxValue, int.MaxValue);
			}
		}

		public Coordinate3() {
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
			Coordinate3 other = obj as Coordinate3;
			if (other == null) return false;
			return this == other;
		}

		public override int GetHashCode() {
			unchecked {
				int hash = 17;
				hash = hash * 23 + X.GetHashCode();
				hash = hash * 23 + Y.GetHashCode();
				hash = hash * 23 + Z.GetHashCode();
				return hash;
			}
		}
		public override string ToString() {

			return string.Format("Coordinate3({0}, {1}, {2})", X, Y, Z);
		}

		/// <summary>
		/// Find the greatest of this coordinate's X, Y or Z values;
		/// </summary>
		/// <returns>Largest coordinate value</returns>
		public int Max() {

			int max = 0;
			if (X > max)
				max = X;
			if (Y > max)
				max = Y;
			if (Z > max)
				max = Z;

			return max;
		}

		/// <summary>
		/// Find the smallest of this coordinate's X, Y or Z values;
		/// </summary>
		/// <returns>Smallest coordinate value</returns>
		public int Min() {

			int min = int.MaxValue;
			if (X < min)
				min = X;
			if (Y < min)
				min = Y;
			if (Z < min)
				min = Z;

			return min;
		}
	}
}

