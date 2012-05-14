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
using System.Windows.Navigation;
using System.Windows.Shapes;

using CAClient;
using System.Windows.Controls.Primitives;

namespace WpfApplication1 {

	/**
	 * The main window of the CA GUI
	 **/
	public partial class MainWindow : Window {

		// Our clientUI instance
		private ClientUI clientui;
		// The writeable bitmap we are drawing to
		private WriteableBitmap bitmap;
		// Current state
		private State curState;
		// The state of the CA we are trying to create in the pop-up dialog
		private CreateCAState state;

		/**
		 * Initialize a new MainWindow. We establish a connection to the clientUI and connect a bunch of events.
		 * Also initialize the components such as the bitmap
		 **/
		public MainWindow() {
			InitializeComponent();
			clientui = new ClientUI("net.tcp://localhost:8080");
			bitmap = new WriteableBitmap(500, 500, 96, 96, PixelFormats.Bgr32, null);
			caTransition(State.UnInited);
			clientui.caTransition += caTransition;
			clientui.caError += (t, m) => {
				this.Dispatcher.BeginInvoke(new Action(() => {
					MessageBox.Show(m, t.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
				}));
			};
			clientui.caUpdated += updateUI;
			clientui.caCleared += clearUI;
			clientui.caCleared += listBox_setup;
			clientui.caColorChange += colorChange;
			clientui.caColorChange += listBox_update;
			zoomSlider.ValueChanged += zoomSlider_ValueChanged;
			caImage.Source = bitmap;
			caImage.RenderTransform = new ScaleTransform();
			hScroll.Opacity = 0;
			vScroll.Opacity = 0;

			state = new CreateCAState();
		}

		/**
		 * Transition to the given state. This involves enabling / disabling controls that do / don't make sense
		 * in the new state.
		 *
		 * @param s The state to transition to
		 **/
		private void caTransition(State s) {
			// Do this on the dispatcher thread
			curCA.Dispatcher.BeginInvoke(new Action(() => {
				curCA.Content = s.ToString();
				curState = s;
				switch (s) {
					case State.Running:
						caLoad.IsEnabled = false;
						caCreate.IsEnabled = false;
						stateLoad.IsEnabled = false;
						stateSave.IsEnabled = false;
						stateClear.IsEnabled = false;
						caPlay.IsEnabled = false;
						caStep.IsEnabled = false;
						caPause.IsEnabled = true;
						break;
					case State.UnInited:
						caLoad.IsEnabled = true;
						caCreate.IsEnabled = true;
						stateLoad.IsEnabled = false;
						stateSave.IsEnabled = false;
						stateClear.IsEnabled = false;
						caPlay.IsEnabled = false;
						caStep.IsEnabled = false;
						caPause.IsEnabled = false;
						break;
					case State.Stopped:
						caLoad.IsEnabled = true;
						caCreate.IsEnabled = true;
						stateLoad.IsEnabled = true;
						stateSave.IsEnabled = true;
						stateClear.IsEnabled = true;
						caPlay.IsEnabled = true;
						caStep.IsEnabled = true;
						caPause.IsEnabled = false;
						break;
				}
			}));

		}

		/**
		 * Event for when the zoom slider is changed. Zoom the bitmap accordingly.
		 **/
		private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			zoom(e.NewValue);
		}

		/**
		 * Zoom the bitmap, update the scroll bars to be in the correct positions.
		 *
		 * @param zoom The amount of zoom we are currently under
		 **/
		private void zoom(double zoom) {
			var sc = (ScaleTransform)caImage.RenderTransform;
			// Set the zoom on the bitmap
			sc.ScaleX = zoom;
			sc.ScaleY = zoom;
			// Change the scroll bar length appropriately
			vScroll.SetThumbLength(500 / zoom);
			hScroll.SetThumbLength(500 / zoom);
			// If we are at default zoom, hide the scroll bars
			if (zoom == 1) {
				sc.CenterY = 0;
				sc.CenterX = 0;
				hScroll.Opacity = 0;
				vScroll.Opacity = 0;
			// Otherwise, position them correctly
			} else {
				hScroll.Opacity = 1;
				vScroll.Opacity = 1;
				var xLen = hScroll.GetThumbLength();
				var yLen = vScroll.GetThumbLength();
				var xCen = hScroll.GetThumbCenter();
				var yCen = vScroll.GetThumbCenter();
				sc.CenterY = ((yCen - (yLen / 2)) / (500 - yLen)) * 500;
				sc.CenterX = ((xCen - (xLen / 2)) / (500 - xLen)) * 500;
			}

		}

		/**
		 * Connects the the clientUI.updateUI event
		 *
		 * Updates the proper pixels and paints them the proper color
		 *
		 * @param dict The points that have changed
		 * @param colors The colors for each state
		 **/
		private void updateUI(Dictionary<CAutamata.Point, uint> dict, System.Drawing.Color[] colors) {
			// Convert the colors to integer values to write to the bitmap
			int[] cvals = new int[colors.Length];
			for(int i = 0; i < colors.Length; i++) {
				cvals[i] = colors[i].R << 16;
				cvals[i] |= colors[i].G << 8;
				cvals[i] |= colors[i].B;
			}

			// On this dispatcher thread, lock the bitmap, paint the changes
			caImage.Dispatcher.BeginInvoke(new Action(() => {
				bitmap.Lock();

				unsafe {
					int buffer = (int)bitmap.BackBuffer;
					int stride = bitmap.BackBufferStride;

					foreach (var kv in dict) {
						var key = kv.Key;
						int b = buffer;
						b += key.x * stride;
						b += key.y * 4;
						*((int*)b) = cvals[kv.Value];
						bitmap.AddDirtyRect(new Int32Rect(key.y, key.x, 1, 1));
					}
				}

				bitmap.Unlock();

				// Pull more changes. This Creates the animation.
				// This means the animation will run as quickly as possible.
				// Pull changes is a no-op if the state is not running. This is how we stop animating
				clientui.pullChanges();
			}));
		}

		/**
		 * Connect to the clientUI.clearUI event
		 *
		 * Clears the bitmap to the given state
		 *
		 * @param val The state to clear to
		 * @param colors The colors for each state
		 **/
		private void clearUI(uint val, System.Drawing.Color[] colors) {
			// Convert the colors to integer values
			int[] cvals = new int[colors.Length];
			for (int i = 0; i < colors.Length; i++) {
				cvals[i] = colors[i].R << 16;
				cvals[i] |= colors[i].G << 8;
				cvals[i] |= colors[i].B;
			}
			int cval = cvals[val];

			// On the dispatcher thread, repaint the bitmap
			caImage.Dispatcher.BeginInvoke(new Action(() => {
				bitmap.Lock();

				unsafe {
					int* buffer = (int*)bitmap.BackBuffer;
					int stride = bitmap.BackBufferStride;

					for (int i = 0; i < 250000; i++) {
						*buffer++ = cval;
					}
				}

				bitmap.AddDirtyRect(new Int32Rect(0, 0, 500, 500));

				bitmap.Unlock();

				// Pull changes. This is probably a no-op. Could probably be removed.
				clientui.pullChanges();
			}));
		}

		/**
		 * Connects to the clientUI.colorChange event
		 * Completely change the color of some states on the board.
		 * This requires a full repaint
		 *
		 * @param board The full state of the board
		 * @param colors Colors for each state
		 **/
		private void colorChange(uint[][] board, System.Drawing.Color[] colors) {
			// Convert the colors to integer values
			int[] cvals = new int[colors.Length];
			for (int i = 0; i < colors.Length; i++) {
				cvals[i] = colors[i].R << 16;
				cvals[i] |= colors[i].G << 8;
				cvals[i] |= colors[i].B;
			}

			// On the dispatcher thread, repaint the bitmap
			caImage.Dispatcher.BeginInvoke(new Action(() => {
				bitmap.Lock();

				unsafe {
					uint[] bd;
					int* buffer = (int*)bitmap.BackBuffer;
					for (int i = 0; i < 500; i++) {
						bd = board[i];
						for (int j = 0; j < 500; j++) {
							*buffer++ = cvals[bd[j]];
						}
					}
				}

				bitmap.AddDirtyRect(new Int32Rect(0, 0, 500, 500));

				bitmap.Unlock();

			}));
		}

		/**
		 * Connect to the clientUI.caClear event
		 *
		 * Update the colors in the side state list
		 **/
		private void listBox_update(uint[][] bd, System.Drawing.Color[] colors) {
			listBox_setup(0, colors);
		}

		/**
		 * Connect to the clientUI.colorChange event
		 *
		 * Update the colors in the side state list
		 **/
		private void listBox_setup(uint val, System.Drawing.Color[] colors) {
			// Convert colors to integer values
			int[] cvals = new int[colors.Length];
			for (int i = 0; i < colors.Length; i++) {
				cvals[i] = colors[i].R << 16;
				cvals[i] |= colors[i].G << 8;
				cvals[i] |= colors[i].B;
			}

			// On the dispatcher thread, clear old entries, create a new entry
			// for each state. Set its color to the color for the state
			listBox1.Dispatcher.BeginInvoke(new Action(() => {
				listBox1.Items.Clear();
				int i = 0;
				foreach (int v in cvals) {
					var sp = new StackPanel();
					sp.Orientation = Orientation.Horizontal;
					var img = new Image();
					img.Height = 10;
					img.Width = 10;
					var bmap = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgr32, null);
					img.Source = bmap;
					bmap.Lock();
					unsafe {
						int* buffer = (int*)bmap.BackBuffer;
						*buffer = v;
					}

					bmap.AddDirtyRect(new Int32Rect(0, 0, 1, 1));
					bmap.Unlock();

					sp.Children.Add(img);
					var label = new Label();
					label.Content = "State " + i;
					sp.Children.Add(label);
					int num = i;
					// Add an event to bring up a color chooser and change a state's color
					img.MouseUp += (o, e) => {
						var cp = new Window3();
						cp.setColor(cvals[num]);
						if (cp.ShowDialog() == true) {
							clientui.setColor((uint) num, cp.Color);
						}
					};
					listBox1.Items.Add(sp);
					i++;
				}
			}));
		}

		/**
		 * Start the CA
		 **/
		private void CA_Play(object sender, RoutedEventArgs e) {
			clientui.start();
		}

		/**
		 * Stop the CA
		 **/
		private void CA_Pause(object sender, RoutedEventArgs e) {
			clientui.stop();
		}

		/**
		 * Step the CA
		 **/
		private void CA_Step(object sender, RoutedEventArgs e) {
			clientui.step();
		}

		/**
		 * Shutdown the clientUI on window close
		 **/
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			clientui.shutdown();
		}

		/**
		 * Show a dialog to select a file then attempt to load a state from the selected file
		 **/
		private void State_Load(object sender, RoutedEventArgs e) {
			var fd = new Microsoft.Win32.OpenFileDialog();
			if (fd.ShowDialog() == true) {
				clientui.loadState(fd.SafeFileName);
			}

		}

		/**
		 * Show a dialog to select a save path, then attempt to save the state to file
		 **/
		private void State_Save(object sender, RoutedEventArgs e) {
			var fd = new Microsoft.Win32.SaveFileDialog();
			if (fd.ShowDialog() == true) {
				clientui.saveState(fd.SafeFileName);
			}
		}

		/**
		 * Show a dialog to pick what state to clear to. Attempt to clear to that state
		 **/
		private void State_Clear(object sender, RoutedEventArgs e) {
			var w2 = new Window2();
			if (w2.ShowDialog() == true) {
				uint val;
				if (!uint.TryParse(w2.Text, out val)) {
					MessageBox.Show("State must be an unsigned integer", "Clear", MessageBoxButton.OK, MessageBoxImage.Error);
				} else {
					clientui.clearState(val);
				}
			}
		}

		/**
		 * Show a dialog to pick a CASettings file to load. Attempt to load those CASettings
		 **/
		private void CA_Load(object sender, RoutedEventArgs e) {
			var fd = new Microsoft.Win32.OpenFileDialog();
			if (fd.ShowDialog() == true) {
				clientui.loadCA(fd.SafeFileName);
			}
		}

		/**
		 * Pop up the dialog to create a CASettings directly.
		 * Save the values then attempt to load the CA to the clientUI.
		 * We save the state to make it easier to fix errors if they occur.
		 **/
		private void CA_Create(object sender, RoutedEventArgs e) {
			var w1 = new Window1();
			w1.CAName = state.Name;
			w1.NumStates = state.NumStates;
			w1.Neighborhood = state.Neighborhood;
			w1.Delta = state.Delta;
			if (w1.ShowDialog() == true) {
				state.Name = w1.CAName;
				state.NumStates = w1.NumStates;
				state.Neighborhood = w1.Neighborhood.Substring(1,w1.Neighborhood.Length-2);
				state.Delta = w1.Delta.Substring(1, w1.Delta.Length - 2);
				clientui.loadCA(w1.CAName, w1.NumStates, 0, w1.Neighborhood, w1.Delta);
			}
		}

		/**
		 * Horizontal scrolling event.
		 * Update the bitmap positioning
		 **/
		private void vScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			var sc = (ScaleTransform)caImage.RenderTransform;
			if (sc.ScaleY == 1) {
				sc.CenterY = 0;
				sc.CenterX = 0;
			} else {
				var yLen = vScroll.GetThumbLength();
				var yCen = vScroll.GetThumbCenter();
				sc.CenterY = ((yCen - (yLen / 2)) / (500 - yLen)) * 500;
			}
		}

		/**
		 * Vertical scrolling event.
		 * Update the bitmap positioning
		 **/
		private void hScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			var sc = (ScaleTransform)caImage.RenderTransform;
			if (sc.ScaleX == 1) {
				sc.CenterY = 0;
				sc.CenterX = 0;
			} else {
				var xLen = hScroll.GetThumbLength();
				var xCen = hScroll.GetThumbCenter();
				sc.CenterX = ((xCen - (xLen / 2)) / (500 - xLen)) * 500;
			}
		}

		/**
		 * Mouse wheel scrolling.
		 * The logic here-in should position the bitmap such that the pixel under the cursor does not move if possible.
		 **/
		private void caImage_MouseWheel(object sender, MouseWheelEventArgs e) {
			var sc = (ScaleTransform) caImage.RenderTransform;
			var pt = e.GetPosition(caImage);
			var xCen = sc.CenterX;
			var yCen = sc.CenterY;
			var xPort = hScroll.GetThumbLength();
			var yPort = vScroll.GetThumbLength();
			var xRat = (pt.X - xCen + (xPort / 2)) / xPort;
			var yRat = (pt.Y - yCen + (yPort / 2)) / yPort;
			var isOne = (zoomSlider.Value == 1);
			if (e.Delta > 0) {
				zoomSlider.Value = (zoomSlider.Value > 95) ? 100 : (zoomSlider.Value + 5);
			} else {
				zoomSlider.Value = (zoomSlider.Value < 6) ? 1 : (zoomSlider.Value - 5);
			}
			zoom(zoomSlider.Value);
			xPort = hScroll.GetThumbLength();
			yPort = vScroll.GetThumbLength();
			if (double.IsInfinity(xPort)) {
				hScroll.Value = 0;
				hScroll.ViewportSize = 5;
				sc.CenterX = 0;
				vScroll.Value = 0;
				vScroll.ViewportSize = 5;
				sc.CenterY = 0;
				return;
			}
			sc.CenterX = (isOne) ? pt.X : pt.X + (xPort / 2) - (xRat * xPort);
			sc.CenterY = (isOne) ? pt.Y : pt.Y + (yPort / 2) - (yRat * yPort);
			xCen = ((sc.CenterX / 500) * (500 - xPort)) + (xPort / 2);
			yCen = ((sc.CenterY / 500) * (500 - yPort)) + (yPort / 2);
			hScroll.SetThumbCenter(xCen);
			vScroll.SetThumbCenter(yCen);
			hScroll_ValueChanged(null, null);
			vScroll_ValueChanged(null, null);
		}

		/**
		 * Mousedown event on the bitmap, toggle the point corresponding to the pixel the cursor is over.
		 **/
		private void caImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			if (curState == State.Stopped) {
				var p = e.GetPosition(caImage);
				clientui.toggleState(new CAutamata.Point((int)p.Y, (int)p.X));
			}
		}
	}

	/**
	 * State struct for the CreateCA dialog
	 **/
	internal struct CreateCAState {
		public string Name;
		public uint NumStates;
		public string Neighborhood;
		public string Delta;
	}


	/**
	 * Useful extension methods for scrollbars found at the following link.
	 **/
	// http://www.wpfmentor.com/2008/12/how-to-set-thumb-position-and-length-of.html
	internal static class ScrollBarExtensions {
		public static double GetThumbCenter(this ScrollBar s) {
			double thumbLength = GetThumbLength(s);
			double trackLength = s.Maximum - s.Minimum;

			return thumbLength / 2 + s.Minimum + (s.Value - s.Minimum) *
			  (trackLength - thumbLength) / trackLength;
		}

		public static void SetThumbCenter(this ScrollBar s, double thumbCenter) {
			double thumbLength = GetThumbLength(s);
			double trackLength = s.Maximum - s.Minimum;

			if (thumbCenter >= s.Maximum - thumbLength / 2) {
				s.Value = s.Maximum;
			} else if (thumbCenter <= s.Minimum + thumbLength / 2) {
				s.Value = s.Minimum;
			} else if (thumbLength >= trackLength) {
				s.Value = s.Minimum;
			} else {
				s.Value = s.Minimum + trackLength *
					((thumbCenter - s.Minimum - thumbLength / 2)
					/ (trackLength - thumbLength));
			}
		}

		public static double GetThumbLength(this ScrollBar s) {
			double trackLength = s.Maximum - s.Minimum;
			return trackLength * s.ViewportSize /
				 (trackLength + s.ViewportSize);
		}

		public static void SetThumbLength(this ScrollBar s, double thumbLength) {
			double trackLength = s.Maximum - s.Minimum;

			if (thumbLength < 0) {
				s.ViewportSize = 0;
			} else if (thumbLength < trackLength) {
				s.ViewportSize = trackLength * thumbLength / (trackLength - thumbLength);
			} else {
				s.ViewportSize = double.MaxValue;
			}
		}
	}
}
