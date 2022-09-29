using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Rooler {
	/// <summary>
	/// Interaction logic for Capture.xaml
	/// </summary>
	public partial class Capture : Tool {

		private Point mouseDownLoc = new Point();
		private Rect bounds;
		private ScreenShot screenshot;
		private bool isInDrawMode = true;

		public static RoutedCommand CopyCommand = new RoutedCommand();

		public Capture(IScreenServiceHost host): base(host) {
			InitializeComponent();

			this.InputBindings.Add(new InputBinding(Capture.CopyCommand, new KeyGesture(Key.C, ModifierKeys.Control)));
			this.CommandBindings.Add(new CommandBinding(Capture.CopyCommand, this.HandleCopy));

			this.BoundsWidth.Width = new GridLength(0, GridUnitType.Pixel);
			this.BoundsHeight.Height = new GridLength(0, GridUnitType.Pixel);
			this.Dimensions.Visibility = Visibility.Collapsed;

			this.Dimensions.CloseClicked += delegate {
				this.CloseService();
			};
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {

			if (this.isInDrawMode) {
				if (this.CaptureMouse()) {

					this.mouseDownLoc = e.GetPosition(this);

					this.Bounds = new Rect(this.mouseDownLoc.X, this.mouseDownLoc.Y, 0, 0);

					this.Cursor = Cursors.None;

					e.Handled = true;
				}
			}
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			base.OnMouseLeftButtonUp(e);

			if (this.IsMouseCaptured) {
				this.ReleaseMouseCapture();
			}
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			switch (e.Key) {
				case Key.Enter:
					this.Freeze();
					e.Handled = true;
					break;
			}

			base.OnKeyDown(e);
		}

		protected override void Freeze() {
			base.Freeze();

			this.Cursor = Cursors.Arrow;
			this.Dimensions.CanClose = true;
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);

			if (this.IsMouseCaptured) {

				Point topLeft = new Point(Math.Min(e.GetPosition(this).X, this.mouseDownLoc.X), Math.Min(e.GetPosition(this).Y, this.mouseDownLoc.Y));
				Point bottomRight = new Point(Math.Max(e.GetPosition(this).X, this.mouseDownLoc.X), Math.Max(e.GetPosition(this).Y, this.mouseDownLoc.Y));

				this.Bounds = new Rect(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
			}
		}

		private bool IsGrayed {
			get {
				return ((SolidColorBrush)this.Resources["MaskBackground"]).Color.A != 0;
			}
			set {
				Color currentColor = ((SolidColorBrush)this.Resources["MaskBackground"]).Color;
				if (value)
					((SolidColorBrush)this.Resources["MaskBackground"]).Color = Color.FromArgb(0x40, currentColor.R, currentColor.G, currentColor.B);
				else
					((SolidColorBrush)this.Resources["MaskBackground"]).Color = Color.FromArgb(0x00, currentColor.R, currentColor.G, currentColor.B);
			}
		}

		private Rect Bounds {
			get { return this.bounds; }
			set {
				this.bounds = value;

				this.BoundsOffsetX.Width = new GridLength(this.Bounds.Left, GridUnitType.Pixel);
				this.BoundsOffsetY.Height = new GridLength(this.Bounds.Top, GridUnitType.Pixel);

				this.BoundsWidth.Width = new GridLength(this.Bounds.Width, GridUnitType.Pixel);
				this.BoundsHeight.Height = new GridLength(this.Bounds.Height, GridUnitType.Pixel);

				this.DisplayBounds();
			}
		}

		private void AnimateBounds(Rect bounds) {

			this.bounds = bounds;

			TimeSpan duration = TimeSpan.FromSeconds(.2);
			this.BoundsOffsetX.AnimateTo(this.bounds.Left, duration);
			this.BoundsOffsetY.AnimateTo(this.bounds.Top, duration);

			this.BoundsWidth.AnimateTo(this.bounds.Width, duration);
			this.BoundsHeight.AnimateTo(this.bounds.Height, duration);

			this.DisplayBounds();
		}

		private void DisplayBounds()
		{
			var widthWpf = this.Bounds.Width;
			var heightWpf = this.Bounds.Height;
			var widthNative = widthWpf * ScreenShot.XRatio;
			var heightNative = heightWpf * ScreenShot.YRatio;

			this.Dimensions.Visibility = Visibility.Visible;
			if (!ScreenShot.HasDisplayScaling)
			{
				this.Dimensions.Text = $@"{(int) widthNative} x {(int) heightNative}";
			}
			else
			{
				this.Dimensions.Text = $@"{(int) widthWpf} x {(int) heightWpf} ({(int) widthNative} x {(int) heightNative})";
			}
		}

		protected override void OnLostMouseCapture(MouseEventArgs e) {
			base.OnLostMouseCapture(e);

			if (this.bounds.Width < 3 || this.bounds.Height < 3) {
				this.CloseService();

				return;
			}

			this.Cursor = Cursors.Arrow;

			IntRect startRect = NativeMethods.ClientToScreen(this, this.bounds);

			this.screenshot = new ScreenShot(startRect);

			startRect.Width -= 1;
			startRect.Height -= 1;

			IntRect screenBounds = ScreenCoordinates.Collapse(startRect, this.screenshot);
			Rect bounds = NativeMethods.ScreenToClient(this, screenBounds);

			if (!bounds.IsEmpty) {
				this.AnimateBounds(bounds);
			}

			this.isInDrawMode = false;
			this.Dimensions.CanClose = true;
		}

		private void CaptureIt() {

			IntRect bounds = NativeMethods.ClientToScreen(this, this.Bounds);

			ScreenShot screenshot = new ScreenShot(bounds);

			Clipboard.SetImage(screenshot.Image);

			this.CloseService();
		}

		private void HandleCopy(object sender, ExecutedRoutedEventArgs e) {
			this.CaptureIt();
		}
	}
}
