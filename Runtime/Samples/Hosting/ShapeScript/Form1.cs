using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Shapes;
using System.Diagnostics;

namespace ShapeScript {

    public partial class Form1 : System.Windows.Forms.Form {

        const string NEWLINE = "\n";
        const string ERROR_PROMPT = "<<Error>> ";

        internal static PictureBox Canvas{ get; set; }

        private RichTextBox rtb1;
        private bool _inMultiLineEdit = false;
        private int _lastPromptPosition;
        Host _host;
        CommandHistory _history;

        private static string _promptString = "$$>";
        internal static string PromptString {
            get {
                return _promptString;
            }
            set {
                _promptString = value;
            }
        }

        Circle _c;
        public Form1() {
            InitializeComponent();
            this.Text = "Draw different shapes";
            pictureBox1.Paint += new PaintEventHandler(pictureBox1_Paint);
            Canvas = pictureBox1;
            rtb1 = richTextBox1;

            _host = new Host();
            _host.ExecuteInCurrentScope("from Shapes import *");
            _history = new CommandHistory();

            ResetConsole();
        }

        void pictureBox1_Paint(object sender, PaintEventArgs e) {
            
            //Graphics g1 = Form1.Canvas.CreateGraphics();
            Pen pen = new Pen(Color.YellowGreen, 2);
            e.Graphics.DrawEllipse(pen, 100, 100, 100,100);


            _host.ExecuteInCurrentScope("c=Circle(150,150,50)");
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e) {
            if (sender is RichTextBox) {
                if (HandleRTFKeyPress(e))
                    return;
            }
            if ((int)e.KeyChar == (int)Keys.Escape) {
                this.Close();
            }
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyData) {
                case Keys.Enter:
                    rtb1.SelectionStart = rtb1.TextLength;
                    return;
                case Keys.Down:
                    if (!_inMultiLineEdit) {
                        _CurrentCommand = _history.GetNext();
                        e.Handled = e.SuppressKeyPress = true;
                    }
                    return;
                case Keys.Up:
                    if (!_inMultiLineEdit) {
                        _CurrentCommand = _history.GetPrevious();
                        e.Handled = e.SuppressKeyPress = true;
                    }
                    return;
                case Keys.Home:
                    rtb1.SelectionStart = 0;
                    e.SuppressKeyPress = e.Handled = true;
                    return;
                case Keys.End:
                    rtb1.SelectionStart = rtb1.TextLength;
                    e.SuppressKeyPress = e.Handled = true;
                    return;
                case Keys.Back:
                    HandleBackSpace(e);
                    return;
                default:
                    if (!IsKeyAlwaysAllowed(e))
                        SuppressKeyIfCurrentSelIsReadOnly(e);
                    return;
            }
        }

        private bool HandleRTFKeyPress(KeyPressEventArgs e) {
            switch ((Keys)(int)e.KeyChar) {
                case Keys.Enter:
                    HandleEnterKey();
                    return true;
            }
            return false;
        }
    }
}
