using System;
using Avalonia;
using Avalonia.Media;

namespace raptor
{
	/// <summary>
	/// Summary description for PensBrushes.
	/// </summary>
	/// 
	[Serializable]
	public class PensBrushes
	{
		public static Avalonia.Media.Pen red_pen;
		public static Avalonia.Media.Pen white_pen;
		public static Avalonia.Media.Pen black_pen;
		public static Avalonia.Media.Pen black_dash_pen;
		public static Avalonia.Media.Pen blue_pen;
		public static Avalonia.Media.Pen blue_dash_pen;
		public static Avalonia.Media.Pen green_pen;
		public static Avalonia.Media.Pen chartreuse_pen;

		public static Avalonia.Media.IBrush whitebrush;
		public static Avalonia.Media.IBrush blackbrush;
		public static Avalonia.Media.IBrush redbrush;
		public static Avalonia.Media.IBrush greenbrush;
		public static Avalonia.Media.IBrush bluebrush;

		public static Avalonia.Media.TextAlignment centered_stringFormat;
		public static Avalonia.Media.TextAlignment left_stringFormat;

		public PensBrushes()
		{
		}

		public static void initialize()
		{

			blackbrush = Avalonia.Media.Brushes.Black;
			redbrush = Avalonia.Media.Brushes.Red;
			greenbrush = Avalonia.Media.Brushes.Green;
			bluebrush = Avalonia.Media.Brushes.Blue;
			whitebrush = Avalonia.Media.Brushes.White;

			red_pen = new Avalonia.Media.Pen(redbrush, 1);
			white_pen = new Avalonia.Media.Pen(whitebrush, 1);
			black_pen = new Avalonia.Media.Pen(blackbrush,1);
			black_dash_pen = new Avalonia.Media.Pen(blackbrush,1,DashStyle.Dash);
			blue_pen = new Avalonia.Media.Pen(bluebrush,1);
			blue_dash_pen = new Avalonia.Media.Pen(bluebrush,1,DashStyle.Dash);
			green_pen = new Avalonia.Media.Pen(greenbrush,1);
			chartreuse_pen = new Avalonia.Media.Pen(Brushes.Chartreuse, 4.0);

			centered_stringFormat = Avalonia.Media.TextAlignment.Center;
			left_stringFormat = Avalonia.Media.TextAlignment.Left;


			// Center the block of text (top to bottom) in the rectangle.
			black_dash_pen.DashStyle = Avalonia.Media.DashStyle.DashDot;
			blue_dash_pen.DashStyle = Avalonia.Media.DashStyle.DashDotDot;

		}

	}
}
