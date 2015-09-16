using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rooler {
	public struct IntRect {

		public IntRect(int left, int top, int width, int height): this() {
			this.Left = left;
			this.Top = top;
			this.Width = width;
			this.Height = height;
		}

		public static bool operator ==(IntRect a, IntRect b) {
			return a.Equals(b);
		}

		public static bool operator !=(IntRect a, IntRect b) {
			return !a.Equals(b);
		}

		public int Height { get; set; }
		public bool IsEmpty {
			get {
				return this.Left == 0 && this.Top == 0 && this.Width == 0 && this.Height == 0;
			}
		}

		public int Width { get; set; }

		public int Left { get; set; }
		public int Top { get; set; }

		public IntPoint TopLeft {
			get { return new IntPoint(this.Left, this.Top); }
		}

		public IntPoint BottomRight {
			get { return new IntPoint(this.Right, this.Bottom); }
		}

		public int Right {
			get { return this.Left + this.Width; }
		}

		public int Bottom {
			get { return this.Top + this.Height; }
		}

		public bool Equals(IntRect value) {
			return this.Left == value.Left && this.Width == value.Width && this.Top == value.Top && this.Height == value.Height;
		}

		public override bool Equals(object o) {
			if (o == null || !(o is IntRect))
				return false;
			return this.Equals((IntRect)o);
		}

		public override int GetHashCode() {
			if (this.IsEmpty)
				return 0;

			return (((this.Left.GetHashCode() ^ this.Top.GetHashCode()) ^ this.Width.GetHashCode()) ^ this.Height.GetHashCode());
		}


		public override string ToString() {
			return this.Left + "," + this.Top + "," + this.Width + "," + this.Height;
		}

		public void Union(IntRect rect) {
			if (this.IsEmpty)
				this = rect;

			else if (!rect.IsEmpty) {
				int left = Math.Min(this.Left, rect.Left);
				int top = Math.Min(this.Top, rect.Top);

				int right = Math.Max(this.Right, rect.Right);
				this.Width = Math.Max(right - left, 0);

				int bottom = Math.Max(this.Bottom, rect.Bottom);
				this.Height = Math.Max(bottom - top, 0);

				this.Left = left;
				this.Top = top;
			}
		}

		public static IntRect Empty {
			get { return new IntRect(); }
		}
	}
}
