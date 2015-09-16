using System;
using System.Windows;

namespace Rooler {
	public class ScreenCoordinates {

		static ScreenCoordinates() {
			ScreenCoordinates.ColorTolerance = 15;
		}

		public static double ColorTolerance { get; set; }

		public static IntRect ExpandPoint(IntPoint position) {

			ScreenShot screenshot = new ScreenShot();
			return ScreenCoordinates.ExpandPoint(position, screenshot);
		}

		public static IntRect ExpandPoint(IntPoint position, IScreenShot screenshot) {
			int x = position.X;
			int y = position.Y;

			int left = ScreenCoordinates.FindNearestX(x, y - 5, y + 5, -1, screenshot).X + 1;
			int right = ScreenCoordinates.FindNearestX(x, y - 5, y + 5, 1, screenshot).X;
			int top = ScreenCoordinates.FindNearestY(x - 5, x + 5, y, -1, screenshot).Y + 1;
			int bottom = ScreenCoordinates.FindNearestY(x - 5, x + 5, y, 1, screenshot).Y;

			if (right > left && bottom > top)
				return new IntRect(left, top, right - left, bottom - top);
			return IntRect.Empty;
		}

		private static IntPoint FindNearestX(int xStart, int yStart, int yEnd, int xIncrement, IScreenShot screenshot) {

			yStart = Math.Max(screenshot.Top, yStart);
			yEnd = Math.Min(screenshot.Bottom, yEnd);

			int xEdge = xIncrement < 0 ? int.MinValue : int.MaxValue;
			int yEdge = yStart;

			for (int y = yStart; y < yEnd; ++y) {
				int startPixel = screenshot.GetScreenPixel(xStart, y);

				for (int x = xStart; x >= screenshot.Left && x <= screenshot.Right; x += xIncrement) {
					if (!ScreenCoordinates.IsPixelClose(screenshot.GetScreenPixel(x, y), startPixel)) {
						if (xIncrement > 0 && xEdge > x) {
							xEdge = x;
							yEdge = y;
						}
						else if (xIncrement < 0 && xEdge < x) {
							xEdge = x;
							yEdge = y;
						}
						break;
					}
					startPixel = screenshot.GetScreenPixel(x, y);
				}
			}

			xEdge =  xEdge.Clamp(screenshot.Left, screenshot.Right);

			return new IntPoint(xEdge, yEdge);
		}

		private static IntPoint FindNearestY(int xStart, int xEnd, int yStart, int yIncrement, IScreenShot screenshot) {
			xStart = Math.Max(screenshot.Left, xStart);
			xEnd = Math.Min(screenshot.Right, xEnd);

			int xEdge = xStart;
			int yEdge = yIncrement < 0 ? int.MinValue : int.MaxValue;

			for (int x = xStart; x < xEnd; ++x) {
				int startPixel = screenshot.GetScreenPixel(x, yStart);

				for (int y = yStart; y >= screenshot.Top && y <= screenshot.Bottom; y += yIncrement) {
					if (!ScreenCoordinates.IsPixelClose(screenshot.GetScreenPixel(x, y), startPixel)) {
						if (yIncrement > 0 && yEdge > y) {
							xEdge = x;
							yEdge = y;
						}
						else if (yIncrement < 0 && yEdge < y) {
							xEdge = x;
							yEdge = y;
						}
						break;
					}
					startPixel = screenshot.GetScreenPixel(x, y);
				}
			}

			yEdge = yEdge.Clamp(screenshot.Top, screenshot.Bottom);

			return new IntPoint(xEdge, yEdge);
		}

		public static IntRect Collapse(IntRect rect) {

			return ScreenCoordinates.Collapse(rect, new ScreenShot());
		}

		public static IntRect Collapse(IntRect rect, IScreenShot screenshot) {
			int left = ScreenCoordinates.FindNearestX(rect.Left, rect.Top, rect.Bottom, 1, screenshot).X;
			int right = ScreenCoordinates.FindNearestX(rect.Right, rect.Top, rect.Bottom, -1, screenshot).X + 1;

			int top = ScreenCoordinates.FindNearestY(rect.Left, rect.Right, rect.Top, 1, screenshot).Y - 1;
			int bottom = ScreenCoordinates.FindNearestY(rect.Left, rect.Right, rect.Bottom, -1, screenshot).Y;

			if (right > left && bottom > top)
				return new IntRect(left, top + 1, right - left, bottom - top);
			return IntRect.Empty;
		}

		private static bool IsPixelClose(int a, int b) {

			int totalDifference = Math.Abs(((a >> 24) & 0xFF) - ((b >> 24) & 0xFF)) +
				Math.Abs(((a >> 16) & 0xFF) - ((b >> 16) & 0xFF)) +
				Math.Abs(((a >> 8) & 0xFF) - ((b >> 8) & 0xFF)) +
				Math.Abs(((a >> 0) & 0xFF) - ((b >> 0) & 0xFF));

			if (totalDifference > ScreenCoordinates.ColorTolerance)
				return false;

			return true;
		}

		
	}
}
