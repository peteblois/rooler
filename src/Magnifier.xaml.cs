using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Rooler {
	
	public partial class Magnifier : Tool {

		private bool isPaused = false;

		public static RoutedCommand CopyCommand = new RoutedCommand("Copy", typeof(Magnifier));
		public static RoutedCommand SetBasePointCommand = new RoutedCommand("SetBasePoint", typeof(Magnifier));

		private DateTime lastCapture = DateTime.Today;
		private IntPoint lastMousePoint = new IntPoint();
		private IntPoint basePoint = new IntPoint();

		

		public Magnifier(IScreenServiceHost host): base(host) {
			this.InitializeComponent();

			new Dragger(this);

			this.Focusable = true;

			this.Scale = 8;

			DispatcherTimer timer = new DispatcherTimer() {
				Interval = TimeSpan.FromSeconds(.03),
				IsEnabled = true,
			};
			timer.Tick += this.HandleTick;

			this.Loaded += delegate {
				this.Focus();
			};

			this.InputBindings.Add(new InputBinding(Magnifier.CopyCommand, new KeyGesture(Key.C, ModifierKeys.Control)));
			this.CommandBindings.Add(new CommandBinding(Magnifier.CopyCommand, this.CopyCommandExecuted));

			this.InputBindings.Add(new InputBinding(Magnifier.SetBasePointCommand, new KeyGesture(Key.Enter)));
			this.CommandBindings.Add(new CommandBinding(Magnifier.SetBasePointCommand, this.SetBasePointExecuted));
		}
		
		private void CloseMagnifier(object sender, EventArgs e) {
			this.CloseService();
		}


		private void CopyCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
			Clipboard.SetText(this.PixelColor.Text);
		}

		private void SetBasePointExecuted(object sender, ExecutedRoutedEventArgs e) {
			this.basePoint = NativeMethods.GetCursorPos();
		}

		public double Scale { get; set; }

		protected override void OnMouseWheel(MouseWheelEventArgs e) {

			if (e.Delta > 0)
				this.Scale += 1;
			else
				this.Scale = Math.Max(1, this.Scale - 1);

			e.Handled = true;
			base.OnMouseWheel(e);
		}

		private void HandleTick(object sender, EventArgs e) {

			IntPoint mousePoint = NativeMethods.GetCursorPos();

			if (mousePoint == this.lastMousePoint && DateTime.Now - this.lastCapture < TimeSpan.FromSeconds(.2)) {
				return;
			}

			this.lastCapture = DateTime.Now;
			this.lastMousePoint = mousePoint;

			this.MouseX.Text = string.Format(@"X: {0}", mousePoint.X - this.basePoint.X);
			this.MouseY.Text = string.Format(@"Y: {0}", mousePoint.Y - this.basePoint.Y);

			if (this.isPaused)
				return;


			double width = this.Image.ActualWidth / this.Scale;
			double height = this.Image.ActualHeight / this.Scale;

			

			double left = (mousePoint.X - width / 2).Clamp(ScreenShot.FullScreenBounds.Left, ScreenShot.FullScreenBounds.Width - width);
			double top = (mousePoint.Y - height / 2).Clamp(ScreenShot.FullScreenBounds.Top, ScreenShot.FullScreenBounds.Height - height);


			double deltaX = left - (mousePoint.X - width / 2);
			double deltaY = top - (mousePoint.Y - height / 2);

			if (deltaX != 0)
				this.CenterX.Width = new GridLength((this.Image.ActualWidth / 2 - deltaX * this.Scale) + 2 * this.Scale);
			else
				this.CenterX.Width = new GridLength(this.Image.ActualWidth / 2 + 8);

			if (deltaY != 0)
				this.CenterY.Height = new GridLength((this.Image.ActualHeight / 2 - deltaY * this.Scale) + 2 * this.Scale);
			else
				this.CenterY.Height = new GridLength(this.Image.ActualHeight / 2 + 8);

			

			IntRect rect = new IntRect((int)left, (int)top, (int)width, (int)height);

			ScreenShot screenShot = new ScreenShot(rect);

			FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
			newFormatedBitmapSource.BeginInit();
			newFormatedBitmapSource.Source = screenShot.Image;
			newFormatedBitmapSource.DestinationFormat = PixelFormats.Rgb24;
			newFormatedBitmapSource.EndInit();

			this.Image.Source = newFormatedBitmapSource;

			if (width == 0 || height == 0)
				return;

			uint centerPixel = (uint)screenShot.GetScreenPixel((int)mousePoint.X, (int)mousePoint.Y);
			centerPixel = centerPixel | 0xFF000000;
			byte r = (byte)((centerPixel >> 16) & 0xFF);
			byte g = (byte)((centerPixel >> 8) & 0xFF);
			byte b = (byte)((centerPixel >> 0) & 0xFF);

			Brush brush = new SolidColorBrush(Color.FromRgb(r, g, b));
			this.ColorSwatch.Fill = brush;

			this.PixelColor.Text = string.Format(@"#{0:X8}", centerPixel);
		}

		protected override void OnMouseEnter(MouseEventArgs e) {
			base.OnMouseEnter(e);

			//this.isPaused = true;
		}

		protected override void OnMouseLeave(MouseEventArgs e) {
			base.OnMouseLeave(e);

			//this.isPaused = false;
		}
	}
}
