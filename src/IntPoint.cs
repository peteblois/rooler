using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rooler {
	public struct IntPoint {
		public IntPoint(int x, int y): this() {
			this.X = x;
			this.Y = y;
		}

		public int X {
			get;
			set;
		}
		public int Y {
			get;
			set;
		}
		public static bool operator ==(IntPoint point1, IntPoint point2) {
			return point1.X == point2.X && point1.Y == point2.Y;
		}
		public static bool operator !=(IntPoint point1, IntPoint point2) {
			return !(point1 == point2);
		}
		public override bool Equals(object o) {
			if (o == null || !(o is IntPoint))
				return false;

			return this.Equals((IntPoint)o);
		}

		public bool Equals(IntPoint value) {
			return this.X == value.X && this.Y == value.Y;
		}

		public override int GetHashCode() {
			return (this.X.GetHashCode() ^ this.Y.GetHashCode());
		}

		public override string ToString() {
			return this.X + "," + this.Y;
		}
	}
}
