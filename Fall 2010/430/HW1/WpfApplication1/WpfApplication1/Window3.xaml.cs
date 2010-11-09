using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1 {
	/// <summary>
	/// Interaction logic for Window3.xaml
	/// </summary>
	public partial class Window3 : Window {

		private WriteableBitmap bmap;
		
		public Window3() {
			InitializeComponent();
			bmap = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgr32, null);
			img.Source = bmap;
		}

		public System.Drawing.Color Color {
			get {
				return System.Drawing.Color.FromArgb((int) redSlider.Value, (int) greenSlider.Value, (int) blueSlider.Value);
			}
		}

		private void updateBmap() {
			bmap.Lock();

			unsafe {
				int* buffer = (int*)bmap.BackBuffer;

				int c = ((int)redSlider.Value) << 16;
				c |= ((int)greenSlider.Value) << 8;
				c |= ((int)blueSlider.Value);

				*buffer = c;

			}

			bmap.AddDirtyRect(new Int32Rect(0, 0, 1, 1));

			bmap.Unlock();
		}

		private void redSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			updateBmap();
		}

		private void greenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			updateBmap();
		}

		private void blueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			updateBmap();
		}

		private void ok_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		internal void setColor(int val) {
			redSlider.Value = (val >> 16) & (0xff);
			greenSlider.Value = (val >> 8) & (0xff);
			blueSlider.Value = val & (0xff);
			updateBmap();
		}
	}
}
