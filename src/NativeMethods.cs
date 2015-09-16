using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Rooler {
	public class NativeMethods {

		public static IntPoint GetCursorPos() {
			Win32Point position = new Win32Point(0, 0);
			NativeMethods.GetCursorPos(ref position);

			return new IntPoint(position.x, position.y);
		}

		public static void SetCursorPos(IntPoint point) {
			NativeMethods.SetCursorPos(point.X, point.Y);
		}

		public static Point ScreenToClient(FrameworkElement element, IntPoint point) {
			PresentationSource source = PresentationSource.FromVisual(element);

			Win32Point winPt = new Win32Point(point.X, point.Y);
			NativeMethods.ScreenToClient(((HwndSource)source).Handle, ref winPt);

			Point offset = source.CompositionTarget.TransformFromDevice.Transform(new Point(winPt.x, winPt.y));

			return source.RootVisual.TransformToDescendant(element).Transform(offset);
		}

		public static Rect ScreenToClient(FrameworkElement element, IntRect rect) {
			Point topLeft = NativeMethods.ScreenToClient(element, rect.TopLeft);
			Point bottomRight = NativeMethods.ScreenToClient(element, rect.BottomRight);

			return new Rect(topLeft, bottomRight);
		}

		public static IntRect ClientToScreen(FrameworkElement element, Rect rect) {
			IntPoint topLeft = NativeMethods.ClientToScreen(element, new Point(rect.X, rect.Y));
			IntPoint bottomRight = NativeMethods.ClientToScreen(element, new Point(rect.X + rect.Width, rect.Y + rect.Height));

			return new IntRect(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
		}

		public static IntPoint ClientToScreen(FrameworkElement element, Point point) {
			PresentationSource source = PresentationSource.FromVisual(element);
			point = element.TransformToAncestor(source.RootVisual).Transform(point);

			Point offset = source.CompositionTarget.TransformToDevice.Transform(new Point(point.X, point.Y));

			Win32Point winPt = new Win32Point((int)offset.X, (int)offset.Y);
			NativeMethods.ClientToScreen(((HwndSource)source).Handle, ref winPt);

			return new IntPoint(winPt.x, winPt.y);
		}

		public static DC CreateDC(string strDriver, string strDevice, string strOutput, IntPtr pData) {
			return new DC(NativeMethods.CreateDCNative(strDriver, strDevice, strOutput, pData));
		}

		[DllImport("user32.dll")]
		static extern bool SetCursorPos(int X, int Y);

		[DllImport("user32.dll")]
		static extern bool ScreenToClient(IntPtr hWnd, ref Win32Point lpPoint);

		[DllImport("user32.dll")]
		static extern bool ClientToScreen(IntPtr hWnd, ref Win32Point lpPoint);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		private static extern int GetCursorPos(ref Win32Point pt);

		[DllImport("gdi32.dll", EntryPoint = "CreateDCW", CharSet = CharSet.Unicode)]
		private static extern IntPtr CreateDCNative(string strDriver, string strDevice, string strOutput, IntPtr pData);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		private static extern bool BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);


		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool DeleteObject(IntPtr hgdiobj);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		internal static extern bool DeleteDC(IntPtr hdc);

		[StructLayout(LayoutKind.Sequential)]
		private struct Win32Point {
			public int x;
			public int y;

			public Win32Point(int x, int y) {
				this.x = x;
				this.y = y;
			}
		}

		public class DC : IDisposable {

			public DC(IntPtr ptr) {
				if (ptr == IntPtr.Zero)
					throw new ImageException();

				this.IntPtr = ptr;
			}

			public IntPtr IntPtr { get; private set; }

			public void Dispose() {
				if (this.IntPtr != IntPtr.Zero)
					NativeMethods.DeleteDC(this.IntPtr);
			}

			public bool BitBlt(int x, int y, int nWidth, int nHeight, DC hSrcDC, int xSrc, int ySrc, int dwRop) {
				return NativeMethods.BitBlt(this.IntPtr, x, y, nWidth, nHeight, hSrcDC.IntPtr, xSrc, ySrc, dwRop);
			}

			public GdiObject SelectObject(GdiObject hgdiobj) {
				return new GdiObject(NativeMethods.SelectObject(this.IntPtr, hgdiobj.IntPtr));
			}

			public DC CreateCompatibleDC() {
				return new DC(NativeMethods.CreateCompatibleDC(this.IntPtr));
			}

			public GdiObject CreateCompatibleBitmap(int width, int height) {
				return new GdiObject(NativeMethods.CreateCompatibleBitmap(this.IntPtr, width, height));
			}
		}

		public class GdiObject : IDisposable {

			public GdiObject(IntPtr ptr) {
				if (ptr == IntPtr.Zero)
					throw new ImageException();

				this.IntPtr = ptr;
			}

			public IntPtr IntPtr { get; private set; }

			public void Dispose() {
				if (this.IntPtr != IntPtr.Zero)
					NativeMethods.DeleteObject(this.IntPtr);
			}

			
		}

		public class ImageException : Exception {
		}
	}
}
