using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace Rooler
{
	public class Dragger
	{

		private FrameworkElement target;
		private TranslateTransform offset = new TranslateTransform();
		private Point lastMousePt;

		public Dragger(FrameworkElement target)
		{
			this.target = target;

			this.target.Loaded += (sender, args) =>
			{
				// Calculate the initial offset:
				// in the top center of the current screen
				var currentScreen = Screen.FromPoint(System.Windows.Forms.Cursor.Position);
				var fullScreen = ScreenShot.FullScreenBounds;

				var xOffset = Math.Abs(fullScreen.Left) + Math.Abs(currentScreen.Bounds.Left) + currentScreen.Bounds.Width / 2.0;
				var yOffset = (double)Math.Abs(fullScreen.Top) + Math.Abs(currentScreen.Bounds.Top);

				// Transform the WinForms pixels (system dpi) to WPF pixels (based on virtual 96dpi)
				xOffset = xOffset / ScreenShot.XRatio;
				yOffset = yOffset / ScreenShot.YRatio;

				this.offset.X = xOffset;
				this.offset.Y = yOffset;
			};

			this.target.RenderTransform = this.offset;

			this.target.MouseLeftButtonDown += this.HandleMouseDown;
			this.target.MouseLeftButtonUp += this.HandleMouseUp;
			this.target.MouseMove += this.HandleMouseMove;
		}

		private void HandleMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.lastMousePt = e.GetPosition(this.target);

			if (this.target.CaptureMouse())
			{
				this.lastMousePt = e.GetPosition(this.target);
				e.Handled = true;
			}
		}

		private void HandleMouseUp(object sender, MouseButtonEventArgs e)
		{
			this.target.ReleaseMouseCapture();
		}

		private void HandleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (this.target.IsMouseCaptured)
			{
				Point point = e.GetPosition(this.target);

				this.offset.X += point.X - this.lastMousePt.X;
				this.offset.Y += point.Y - this.lastMousePt.Y;
			}
		}
	}
}
