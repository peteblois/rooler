using System;

namespace Rooler
{
	/// <summary>
	/// On large screens we occasionally run out of memory when trying to take a screenshot
	/// of the entire area. There are a couple of scenarios where we need to access large
	/// areas of the screen, so this virtualizes the loading of that and only captures areas
	/// on demand.
	/// </summary>
	public class VirtualizedScreenShot : IScreenShot
	{
		private ScreenShot[,] screenShots;
		private IntRect bounds;
		private int tileWidth = 200;
		private int tileHeight = 200;

		public VirtualizedScreenShot()
		{
			this.bounds = ScreenShot.FullScreenBounds;

			var countX = (int)Math.Ceiling(this.bounds.Width / (double)tileWidth) + 1;
			var countY = (int)Math.Ceiling(this.bounds.Height / (double)tileHeight) + 1;

			this.screenShots = new ScreenShot[countX, countY];
		}

		public int Left
		{
			get { return this.bounds.Left; }
		}

		public int Top
		{
			get { return this.bounds.Top; }
		}

		public int Right
		{
			get { return this.bounds.Right; }
		}

		public int Bottom
		{
			get { return this.bounds.Bottom; }
		}

		public int GetScreenPixel(int x, int y)
		{
			x = x - this.bounds.Left;
			y = y - this.bounds.Top;

			return this.GetLocalPixel(x, y);
		}

		public int GetLocalPixel(int x, int y)
		{

			int xIndex = x / this.tileWidth;
			int yIndex = y / this.tileHeight;

			ScreenShot ss = this.screenShots[xIndex, yIndex];
			if (ss == null)
			{
				try
				{
					ss = new ScreenShot(new IntRect(xIndex * this.tileWidth + this.bounds.Left, yIndex * tileHeight + this.bounds.Top, tileWidth + 1, tileHeight + 1), false);
					this.screenShots[xIndex, yIndex] = ss;
				}
				catch (Exception) { }
			}

			if (ss != null)
			{
				return ss.GetLocalPixel(x - xIndex * this.tileWidth, y - yIndex * this.tileHeight);
			}
			return xIndex ^ yIndex;
		}
	}
}
