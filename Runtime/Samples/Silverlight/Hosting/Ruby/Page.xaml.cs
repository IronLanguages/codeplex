using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightHostingRuby {
    public partial class Page : UserControl {

        private RubyEngine _ruby;

        public Page() {
            InitializeComponent();
            Result.Text += "\n\n";
            _ruby = new RubyEngine();
        }

        private void Code_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Result.Text = "\n\n" +
                  ">> " + Code.Text + "\n" +
                  (_ruby.Execute(Code.Text).ToString()) +
                  Result.Text;
                Code.Text = "";
            }
        }

    }
}
