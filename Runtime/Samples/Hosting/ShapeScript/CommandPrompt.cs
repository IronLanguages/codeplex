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

    public partial class Form1 {

        private void ProcessCurrentCommand() {
            _history.Add(_CurrentCommand);
            string output = _host.ExecuteInCurrentScope(_CurrentCommand);
            if (output != null) {
                AddToConsole(output);
            }
            else {//probably an error
                AddErrorToConsole(_host.ErrorFromLastExecution);
            }
        }

        private void HandleEnterKey() {
            if (!_inMultiLineEdit) {
                if (!_CurrentCommand.IsNullOrEmpty()) {
                    if (_CurrentCommand.EndsWith(":")) {
                        _inMultiLineEdit = true;
                        return;
                    }
                    ProcessCurrentCommand();
                }
                ShowPrompt();
            }
            else {
                string curLine = GetLastLine(_CurrentCommand);
                if (curLine.IsNullOrEmptyOrWhiteSpace()) {
                    _inMultiLineEdit = false;
                    ProcessCurrentCommand();
                    ShowPrompt();
                }
            }
        }

        private string GetLastLine(string contents) {

            //return rtb.Lines.Last();

            int idxLast = contents.LastIndexOf('\n');
            Debug.Assert(idxLast != -1);

            int idxPrev = contents.LastIndexOf('\n', idxLast - 1);
            Debug.Assert(idxPrev != -1);
            Debug.Assert(idxPrev < idxLast);

            //hell\nwo\n

            return contents.Substring(idxPrev + 1, idxLast - idxPrev - 1);
        }

        private string/*!*/ _CurrentCommand {
            get {
                string code = rtb1.Text.Substring(_lastPromptPosition);
                if (code.IsNullOrEmpty() || code.IsWhiteSpace()) {
                    return "";
                }
                if (_inMultiLineEdit) {
                    return code.TrimEnd(new char[] { ' ' });
                }
                else {
                    return code.TrimEnd(new char[] { ' ', '\n' });
                }
            }
            set {
                rtb1.SelectionStart = _lastPromptPosition;
                rtb1.SelectionLength = rtb1.TextLength - _lastPromptPosition;

                rtb1.SelectedText = value;
            }
        }

        private void HandleBackSpace(KeyEventArgs e) {
            if (rtb1.SelectionStart <= _lastPromptPosition) {
                e.Handled = e.SuppressKeyPress = true;
            }
            else {
                e.Handled = false;
            }
        }

        private bool IsKeyAlwaysAllowed(KeyEventArgs e) {
            var keyCode = e.KeyCode;

            //CTRL-C is always allowed
            if (
                (keyCode == Keys.C) && (e.Control == true) ||
                ((keyCode >= Keys.PageUp) && (keyCode <= Keys.Down)) ||
                (keyCode == Keys.Escape)) {
                return true;
            }

            return false;
        }

        private void SuppressKeyIfCurrentSelIsReadOnly(KeyEventArgs e) {
            if (rtb1.SelectionStart < _lastPromptPosition) {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else {
                e.Handled = false;
            }
        }

        /// <summary>
        /// Not yet implemented. Currently it just adds the text
        /// </summary>
        private void AddToRTBWithFormatting(RichTextBox rtb, string text, Color color, bool bold) {
            rtb.Text += text;
        }

        private void AddToConsole(string message, bool addNewLine) {
            StringBuilder sb = new StringBuilder();
            sb.Append(message);

            rtb1.Text += sb.ToString();
        }

        private void AddToConsole(string message) {
            AddToConsole(message, true);
        }

        private void AddErrorToConsole(string error) {
            StringBuilder sb = new StringBuilder();
            sb.Append(ERROR_PROMPT);
            sb.AppendLine(error);

            rtb1.Text += sb.ToString();
        }

        private void ResetConsole() {
            rtb1.Clear();
            ShowPrompt();
        }


        private void ShowPrompt() {
            string contents = rtb1.Text;
            if ((contents.Length != 0) && !contents.EndsWith(NEWLINE)) {
                rtb1.Text += NEWLINE;
            }
            AddToRTBWithFormatting(rtb1, PromptString, Color.Brown, true);
            rtb1.SelectionStart = rtb1.TextLength;

            _lastPromptPosition = rtb1.SelectionStart;

        }

    }
}
