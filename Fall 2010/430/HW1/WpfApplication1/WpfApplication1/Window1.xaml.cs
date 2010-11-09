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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window {
        public Window1() {
            InitializeComponent();
        }

		public string CAName {
			get {
				return nameBox.Text;
			}

			set {
				nameBox.Text = value;
			}
		}

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

		public string Neighborhood {
			get {
				return "{" + neighborhoodBox.Text + "}";
			}

			set {
				neighborhoodBox.Text = value;
			}
		}

		public string Delta {
			get {
				return "{" + deltaBox.Text + "}";
			}

			set {
				deltaBox.Text = value;
			}
		}

		private void ok_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}
    }
		
}
