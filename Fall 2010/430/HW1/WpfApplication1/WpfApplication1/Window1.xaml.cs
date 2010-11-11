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

	/**
	 * The dialog for creating a CA
	 **/
	public partial class Window1 : Window {
		public Window1() {
			InitializeComponent();
		}

		/**
		 * Name of the CA
		 **/
		public string CAName {
			get {
				return nameBox.Text;
			}

			set {
				nameBox.Text = value;
			}
		}

		/**
		 * Number of states in the CA
		 **/
		public uint NumStates {
			get {
				uint val;
				if (uint.TryParse(statesBox.Text, out val)) {
					return val;
				}
				return 0;
			}

			set {
				statesBox.Text = value.ToString();
			}
		}

		/**
		 * Neighborhood string for the CA
		 **/
		public string Neighborhood {
			get {
				return "{" + neighborhoodBox.Text + "}";
			}

			set {
				neighborhoodBox.Text = value;
			}
		}

		/**
		 * Delta function for the CA
		 **/
		public string Delta {
			get {
				return "{" + deltaBox.Text + "}";
			}

			set {
				deltaBox.Text = value;
			}
		}

		/**
		 * Set dialog result to true so the dialog knows to close
		 **/
		private void ok_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}
	}

}
