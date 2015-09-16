using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Rooler {

	public interface IScreenShot {

		int Left { get; }
		int Top { get; }
		int Bottom { get; }
		int Right { get; }

		int GetLocalPixel(int x, int y);
		int GetScreenPixel(int x, int y);
	}


	public class ScreenShot: IScreenShot {
		private BitmapSource bitmap;
		private int[] pixels;
		private IntRect bounds;

		public ScreenShot() {
			this.Capture(ScreenShot.FullScreenBounds);
		}

		public ScreenShot(IntRect screenBounds): this(screenBounds, true) {
		}

		public ScreenShot(IntRect screenBounds, bool keepBitmap) {
			this.Capture(screenBounds);

			if (!keepBitmap)
				this.bitmap = null;
		}

		public ScreenShot(BitmapSource bitmap) {
			this.Init(bitmap);
		}

		private void Capture(IntRect bounds) {

			this.bounds = bounds;

			BitmapSource bitmap = null;

			using (NativeMethods.DC hdcScreen = NativeMethods.CreateDC("Display", null, null, IntPtr.Zero)) {
				using (NativeMethods.DC hdcDest = hdcScreen.CreateCompatibleDC()) {
					using (NativeMethods.GdiObject hBitmap = hdcScreen.CreateCompatibleBitmap(bounds.Width, bounds.Height)) {
						NativeMethods.GdiObject oldBitmap = hdcDest.SelectObject(hBitmap);
						hdcDest.BitBlt(0, 0, bounds.Width, bounds.Height, hdcScreen, bounds.Left, bounds.Top, 0xcc0020);


						bitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap.IntPtr, IntPtr.Zero, new Int32Rect(0, 0, bounds.Width, bounds.Height), null);
						hdcDest.SelectObject(oldBitmap);
					}
				}
			}

			//IntPtr hdcScreen = NativeMethods.CreateDC("Display", null, null, IntPtr.Zero);
			//if (hdcScreen != IntPtr.Zero) {
			//    IntPtr hdcDest = NativeMethods.CreateCompatibleDC(hdcScreen);
			//    if (hdcDest != IntPtr.Zero) {
			//        IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(hdcScreen, bounds.Width, bounds.Height);
			//        if (hBitmap != IntPtr.Zero) {
			//            IntPtr oldBitmap = NativeMethods.SelectObject(hdcDest, hBitmap);
			//            NativeMethods.BitBlt(hdcDest, 0, 0, bounds.Width, bounds.Height, hdcScreen, bounds.X, bounds.Y, 0xcc0020);


			//            bitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, new Int32Rect(0, 0, bounds.Width, bounds.Height), null);
			//            NativeMethods.SelectObject(hdcDest, oldBitmap);
			//            NativeMethods.DeleteObject(hBitmap);
			//        }
			//        NativeMethods.DeleteDC(hdcDest);
			//    }
			//    NativeMethods.DeleteDC(hdcScreen);
			//}

			if (bitmap != null) {
				this.Init(bitmap);
			}
		}

		private static IntRect fullScreenBounds = new IntRect();
		public static IntRect FullScreenBounds {
			get {
				if (fullScreenBounds.IsEmpty) {
					IntRect fullBounds = new IntRect();
					foreach (Screen screen in Screen.AllScreens) {
						fullBounds.Union(new IntRect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height));
					}
					fullScreenBounds = fullBounds;
				}

				return fullScreenBounds;
			}
		}

		private void Init(BitmapSource bitmap) {
			this.bitmap = bitmap;
			this.pixels = new int[this.bitmap.PixelWidth * this.bitmap.PixelHeight];
			this.bitmap.CopyPixels(pixels, this.bitmap.PixelWidth * 4, 0);
		}

		public int Left { 
			get { return this.bounds.Left; } 
		}

		public int Top {
			get { return this.bounds.Top; }
		}

		public int Right {
			get { return this.bounds.Right; }
		}

		public int Bottom {
			get { return this.bounds.Bottom; }
		}

		//public int this[int x, int y] {
		//    get {
		//        return this.GetPixel(x, y);
		//    }
		//}

		public int GetScreenPixel(int x, int y) {
			x -= this.bounds.Left;
			y -= this.bounds.Top;

			return this.GetLocalPixel(x, y);
		}

		public int GetLocalPixel(int x, int y) {

			

			int newX = x.Clamp(0, this.bounds.Width - 1);
			int newY = y.Clamp(0, this.bounds.Height - 1);

			//Debug.Assert(newX == x && newY == y);



			return this.pixels[newY * this.bounds.Width + newX];
		}

		public BitmapSource Image {
			get { return this.bitmap; }
		}
	}
}
