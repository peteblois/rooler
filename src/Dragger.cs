using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace Rooler {
	public class Dragger {

		private FrameworkElement target;
		private TranslateTransform offset = new TranslateTransform();
		private Point lastMousePt;

		public Dragger(FrameworkElement target) {
			this.target = target;

			this.target.Loaded += (sender, args) =>
			{
				// Calculate the offset:
				// With multiple displays, depending their relative positioning,
				// simply centering the target at the top may be outside the visual area.
				var currentScreen = Screen.FromPoint(System.Windows.Forms.Cursor.Position);
				var fullScreen = ScreenShot.FullScreenBounds;

				var centerOfAllScreensX = fullScreen.Width/2.0;
				var centerOfAllScreensY = (double)fullScreen.Top;

				var centerOfCurrentScreenX = currentScreen.Bounds.Left + currentScreen.Bounds.Width/2;
				var centerOfCurrentScreenY = currentScreen.Bounds.Top;

				// Calculate the offset (difference between the top-center of all screens and that of the current screen.
				var xOffset = centerOfCurrentScreenX - centerOfAllScreensX;
				var yOffset = centerOfCurrentScreenY - centerOfAllScreensY;

				// Transform the WinForms pixels (system dpi) to WPF pixels (based on virtual 96dpi).
				var source = PresentationSource.FromVisual(target);
				if (source?.CompositionTarget != null)
				{
					xOffset = xOffset / source.CompositionTarget.TransformToDevice.M11;
					yOffset = yOffset / source.CompositionTarget.TransformToDevice.M22;
				}

				this.offset.X = xOffset;
				this.offset.Y = yOffset;
			};

			this.target.RenderTransform = this.offset;

			this.target.MouseLeftButtonDown += this.HandleMouseDown;
			this.target.MouseLeftButtonUp += this.HandleMouseUp;
			this.target.MouseMove += this.HandleMouseMove;
			this.target.LostMouseCapture += this.HandleLostCapture;
		}

		private void HandleMouseDown(object sender, MouseButtonEventArgs e) {
			this.lastMousePt = e.GetPosition(this.target);

			if (this.target.CaptureMouse()) {
				this.lastMousePt = e.GetPosition(this.target);
				e.Handled = true;
			}
		}

		private void HandleMouseUp(object sender, MouseButtonEventArgs e) {
			this.target.ReleaseMouseCapture();
		}

		private void HandleMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
			if (this.target.IsMouseCaptured) {
				Point point = e.GetPosition(this.target);

				this.offset.X += point.X - this.lastMousePt.X;
				this.offset.Y += point.Y - this.lastMousePt.Y;
			}
		}

		private void HandleLostCapture(object sender, EventArgs e) {
		}
	}
}
