using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Rooler
{
	public partial class Magnifier : Tool
	{
		private bool isPaused = false;

		public static RoutedCommand CopyCommand = new RoutedCommand("Copy", typeof(Magnifier));
		public static RoutedCommand SetBasePointCommand = new RoutedCommand("SetBasePoint", typeof(Magnifier));

		private DateTime lastCapture = DateTime.Today;
		private IntPoint lastMousePoint = new IntPoint();

		/// <summary>
		/// The base point for calculating the X/Y offset.
		/// </summary>
		/// <remarks>
		/// This is scaled to WPF pixels.
		/// </remarks>
		private IntPoint basePointWpf = new IntPoint();

		/// <summary>
		/// The base point for calculating the X/Y offset.
		/// </summary>
		private IntPoint basePoint = new IntPoint();

		public Magnifier(IScreenServiceHost host) : base(host)
		{
			this.InitializeComponent();

			new Dragger(this);

			this.Focusable = true;

			this.Scale = 8;

			DispatcherTimer timer = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds(.03),
				IsEnabled = true,
			};
			timer.Tick += this.HandleTick;

			this.Loaded += delegate
			{
				this.Focus();
			};

			this.InputBindings.Add(new InputBinding(Magnifier.CopyCommand, new KeyGesture(Key.C, ModifierKeys.Control)));
			this.CommandBindings.Add(new CommandBinding(Magnifier.CopyCommand, this.CopyCommandExecuted));

			this.InputBindings.Add(new InputBinding(Magnifier.SetBasePointCommand, new KeyGesture(Key.Enter)));
			this.CommandBindings.Add(new CommandBinding(Magnifier.SetBasePointCommand, this.SetBasePointExecuted));
		}

		private void CloseMagnifier(object sender, EventArgs e)
		{
			this.CloseService();
		}

		private void CopyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Clipboard.SetText(this.PixelColor.Text);
		}

		private void SetBasePointExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			this.basePoint = NativeMethods.GetCursorPos();
			this.basePointWpf = new IntPoint((int)(this.basePoint.X / ScreenShot.XRatio), (int)(this.basePoint.Y / ScreenShot.YRatio));
		}

		public double Scale { get; set; }

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{

			if (e.Delta > 0)
				this.Scale += 1;
			else
				this.Scale = Math.Max(1, this.Scale - 1);

			e.Handled = true;
			base.OnMouseWheel(e);
		}

		private void HandleTick(object sender, EventArgs e)
		{
			IntPoint mousePointNative = NativeMethods.GetCursorPos();

			if (mousePointNative == this.lastMousePoint && DateTime.Now - this.lastCapture < TimeSpan.FromSeconds(.2))
			{
				return;
			}

			this.lastCapture = DateTime.Now;
			this.lastMousePoint = mousePointNative;

			var mousePointWpf = new IntPoint((int)(mousePointNative.X / ScreenShot.XRatio), (int)(mousePointNative.Y / ScreenShot.YRatio));

			var xPosWpf = mousePointWpf.X - this.basePointWpf.X;
			var yPosWpf = mousePointWpf.Y - this.basePointWpf.Y;
			var xPosNative = mousePointNative.X - this.basePoint.X;
			var yPosNative = mousePointNative.Y - this.basePoint.Y;

			if (!ScreenShot.HasDisplayScaling)
			{
				this.MouseX.Text = $@"X: {xPosNative}";
				this.MouseY.Text = $@"Y: {yPosNative}";
			}
			else
			{

				this.MouseX.Text = $@"X: {xPosWpf} ({xPosNative})";
				this.MouseY.Text = $@"Y: {yPosWpf} ({yPosNative})";
			}

			if (this.isPaused)
				return;


			double widthWpf = this.Image.ActualWidth / this.Scale;
			double heightWpf = this.Image.ActualHeight / this.Scale;
			double widthNative = this.Image.ActualWidth * ScreenShot.XRatio / this.Scale;
			double heightNative = this.Image.ActualHeight * ScreenShot.YRatio/ this.Scale;


			double leftWpf = (mousePointWpf.X - widthWpf / 2).Clamp(ScreenShot.FullScreenBoundsWpf.Left, ScreenShot.FullScreenBoundsWpf.Width - widthWpf);
			double topWpf = (mousePointWpf.Y - heightWpf / 2).Clamp(ScreenShot.FullScreenBoundsWpf.Top, ScreenShot.FullScreenBoundsWpf.Height - heightWpf);
			double leftNative = (mousePointNative.X - widthNative / 2).Clamp(ScreenShot.FullScreenBounds.Left, ScreenShot.FullScreenBounds.Width - widthNative);
			double topNative = (mousePointNative.Y - heightNative / 2).Clamp(ScreenShot.FullScreenBounds.Top, ScreenShot.FullScreenBounds.Height - heightNative);


			double deltaXWpf = leftWpf - (mousePointWpf.X - widthWpf / 2);
			double deltaYWpf = topWpf - (mousePointWpf.Y - heightWpf / 2);

			if (deltaXWpf != 0)
				this.CenterX.Width = new GridLength((this.Image.ActualWidth / 2 - deltaXWpf * this.Scale) + 2 * this.Scale);
			else
				this.CenterX.Width = new GridLength(this.Image.ActualWidth / 2 + 8);

			if (deltaYWpf != 0)
				this.CenterY.Height = new GridLength((this.Image.ActualHeight / 2 - deltaYWpf * this.Scale) + 2 * this.Scale);
			else
				this.CenterY.Height = new GridLength(this.Image.ActualHeight / 2 + 8);



			IntRect rect = new IntRect((int)leftNative, (int)topNative, (int)widthNative, (int)heightNative);

			ScreenShot screenShot = new ScreenShot(rect);

			FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
			newFormatedBitmapSource.BeginInit();
			newFormatedBitmapSource.Source = screenShot.Image;
			newFormatedBitmapSource.DestinationFormat = PixelFormats.Rgb24;
			newFormatedBitmapSource.EndInit();

			this.Image.Source = newFormatedBitmapSource;

			if (widthWpf == 0 || heightWpf == 0)
				return;

			uint centerPixel = (uint)screenShot.GetScreenPixel((int)mousePointNative.X, (int)mousePointNative.Y);
			centerPixel = centerPixel | 0xFF000000;
			byte r = (byte)((centerPixel >> 16) & 0xFF);
			byte g = (byte)((centerPixel >> 8) & 0xFF);
			byte b = (byte)((centerPixel >> 0) & 0xFF);

			Brush brush = new SolidColorBrush(Color.FromRgb(r, g, b));
			this.ColorSwatch.Fill = brush;

			this.PixelColor.Text = $@"#{centerPixel:X8}";
		}
	}
}
