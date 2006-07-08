/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Threading;

using IronPython.Hosting;
using IronPython.Runtime.Exceptions;

namespace IronPythonConsole {
    public class SuperConsole : IConsole {

        public ConsoleColor PromptColor = Console.ForegroundColor;
        public ConsoleColor OutColor = Console.ForegroundColor;
        public ConsoleColor ErrorColor = Console.ForegroundColor;

        public void SetupColors() {
            PromptColor = ConsoleColor.DarkGray;
            OutColor = ConsoleColor.DarkBlue;
            ErrorColor = ConsoleColor.DarkRed;
        }

        /// <summary>
        /// Class managing the command history.
        /// </summary>
        class History {
            protected ArrayList list = new ArrayList();
            protected int current = 0;

            public int Count {
                get {
                    return list.Count;
                }
            }

            public string Current {
                get {
                    return current >= 0 && current < list.Count ? (string)list[current] : String.Empty;
                }
            }

            public void Clear() {
                list.Clear();
                current = -1;
            }

            public void Add(string line) {
                if (line != null && line.Length > 0) {
                    list.Add(line);
                }
            }

            public void AddLast(string line) {
                if (line != null && line.Length > 0) {
                    current = list.Add(line) + 1;
                }
            }

            public string First() {
                current = 0;
                return Current;
            }

            public string Last() {
                current = list.Count - 1;
                return Current;
            }

            public string Previous() {
                if (list.Count > 0) {
                    current = ((current - 1) + list.Count) % list.Count;
                }
                return Current;
            }

            public string Next() {
                if (list.Count > 0) {
                    current = (current + 1) % list.Count;
                }
                return Current;
            }
        }

        /// <summary>
        /// List of available options
        /// </summary>
        class SuperConsoleOptions : History {
            private string root;

            public string Root {
                get {
                    return root;
                }
                set {
                    root = value;
                }
            }
        }

        /// <summary>
        /// Cursor position management
        /// </summary>
        struct Cursor {
            /// <summary>
            /// Beginning position of the cursor - top coordinate.
            /// </summary>
            private int anchorTop;
            /// <summary>
            /// Beginning position of the cursor - left coordinate.
            /// </summary>
            private int anchorLeft;

            public int Top {
                get {
                    return anchorTop;
                }
            }
            public int Left {
                get {
                    return anchorLeft;
                }
            }

            public void Anchor() {
                anchorTop = Console.CursorTop;
                anchorLeft = Console.CursorLeft;
            }

            public void Reset() {
                Console.CursorTop = anchorTop;
                Console.CursorLeft = anchorLeft;
            }

            public void Place(int index) {
                Console.CursorLeft = (anchorLeft + index) % Console.BufferWidth;
                int cursorTop = anchorTop + (anchorLeft + index) / Console.BufferWidth;
                if (cursorTop >= Console.BufferHeight) {
                    anchorTop -= cursorTop - Console.BufferHeight + 1;
                    cursorTop = Console.BufferHeight - 1;
                }
                Console.CursorTop = cursorTop;
            }

            public void Move(int delta) {
                int position = Console.CursorTop * Console.BufferWidth + Console.CursorLeft + delta;

                Console.CursorLeft = position % Console.BufferWidth;
                Console.CursorTop = position / Console.BufferWidth;
            }
        };

        /// <summary>
        /// The console input buffer.
        /// </summary>
        private StringBuilder input = new StringBuilder();
        /// <summary>
        /// Current position - index into the input buffer
        /// </summary>
        private int current = 0;
        /// <summary>
        /// The number of white-spaces displayed for the auto-indenation of the current line
        /// </summary>
        private int autoIndentSize = 0;
        /// <summary>
        /// Length of the output currently rendered on screen.
        /// </summary>
        private int rendered = 0;
        /// <summary>
        /// Input has changed.
        /// </summary>
        private bool changed = true;
        /// <summary>
        /// Command history
        /// </summary>
        private History history = new History();
        /// <summary>
        /// Tab options available in current context
        /// </summary>
        private SuperConsoleOptions options = new SuperConsoleOptions();
        /// <summary>
        /// Cursort anchor - position of cursor when the routine was called
        /// </summary>
        Cursor cursor;
        /// <summary>
        /// Python Engine reference. Used for the tab-completion lookup.
        /// </summary>
        private PythonEngine engine;

        private AutoResetEvent ctrlCEvent;
        private Thread MainEngineThread = Thread.CurrentThread;

        public SuperConsole(PythonEngine engine, bool colorfulConsole) {
            this.engine = engine;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            ctrlCEvent = new AutoResetEvent(false);
            if (colorfulConsole) {
                SetupColors();
            }
        }

        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC) {
                e.Cancel = true;
                ctrlCEvent.Set();
                MainEngineThread.Abort(new PythonKeyboardInterruptException(""));
            }
        }

        private bool GetOptions() {
            options.Clear();

            int len;
            for (len = input.Length; len > 0; len --) {
                char c = input[len - 1];
                if (Char.IsLetterOrDigit(c)) {
                    continue;
                } else if (c == '.' || c == '_') {
                    continue;
                } else {
                    break;
                }
            }

            string name = input.ToString(len, input.Length - len);
            if (name.Trim().Length > 0) {
                int lastDot = name.LastIndexOf('.');
                string attr, pref, root;
                if (lastDot < 0) {
                    attr = String.Empty;
                    pref = name;
                    root = input.ToString(0, len);
                } else {
                    attr = name.Substring(0, lastDot);
                    pref = name.Substring(lastDot + 1);
                    root = input.ToString(0, len + lastDot + 1);
                }

                try {
                    IEnumerable result = engine.Evaluate(String.Format("dir({0})", attr)) as IEnumerable;
                    options.Root = root;
                    foreach (string option in result) {
                        if (option.StartsWith(pref, StringComparison.CurrentCultureIgnoreCase)) {
                            options.Add(option);
                        }
                    }
                } catch {
                    options.Clear();
                }
                return true;
            } else {
                return false;
            }
        }

        private void SetInput(string line) {
            input.Length = 0;
            input.Append(line);

            current = input.Length;

            Render();
        }

        private void Initialize() {
            cursor.Anchor();
            input.Length = 0;
            current = 0;
            rendered = 0;
            changed = false;
        }

        // Check if the user is backspacing the auto-indentation. In that case, we go back all the way to
        // the previous indentation level.
        // Return true if we did backspace the auto-indenation.
        private bool BackspaceAutoIndentation() {
            if (input.Length == 0 || input.Length > autoIndentSize) return false;

            // Is the auto-indenation all white space, or has the user since edited the auto-indentation?
            for (int i = 0; i < input.Length; i++) {
                if (input[i] != ' ') return false;
            }

            // Calculate the previous indentation level
            int newLength = ((input.Length - 1) / ConsoleOptions.AutoIndentSize) * ConsoleOptions.AutoIndentSize;

            int backspaceSize = input.Length - newLength;
            input.Remove(newLength, backspaceSize);
            current -= backspaceSize;
            Render();
            return true;
        }

        private void Backspace() {
            if (BackspaceAutoIndentation()) return;

            if (input.Length > 0 && current > 0) {
                input.Remove(current - 1, 1);
                current--;
                Render();
            }
        }

        private void Delete() {
            if (input.Length > 0 && current < input.Length) {
                input.Remove(current, 1);
                Render();
            }
        }

        private void Insert(ConsoleKeyInfo key) {
            char c;
            if (key.Key == ConsoleKey.F6) {
                c = '\x1A';
            } else {
                c = key.KeyChar;
            }
            Insert(c);
        }

        private void Insert(char c) {
            if (current == input.Length) {
                if (Char.IsControl(c)) {
                    string s = MapCharacter(c);
                    current++;
                    input.Append(c);
                    Console.Write(s);
                    rendered += s.Length;
                } else {
                    current++;
                    input.Append(c);
                    Console.Write(c);
                    rendered++;
                }
            } else {
                input.Insert(current, c);
                current++;
                Render();
            }
        }

        private string MapCharacter(char c) {
            switch (c) {
                case '\x1A': return "^Z";
                default: return "^?";
            }
        }

        private int GetCharacterSize(char c) {
            if (Char.IsControl(c)) {
                return MapCharacter(c).Length;
            } else {
                return 1;
            }
        }

        private void Render() {
            cursor.Reset();
            StringBuilder output = new StringBuilder();
            int position = -1;
            for (int i = 0; i < input.Length; i++) {
                if (i == current) {
                    position = output.Length;
                }
                char c = input[i];
                if (Char.IsControl(c)) {
                    output.Append(MapCharacter(c));
                } else {
                    output.Append(c);
                }
            }

            if (current == input.Length) {
                position = output.Length;
            }

            string text = output.ToString();
            Console.Write(text);

            if (text.Length < rendered) {
                Console.Write(new String(' ', rendered - text.Length));
            }
            rendered = text.Length;
            cursor.Place(position);
        }

        private void MoveRight() {
            if (current < input.Length) {
                char c = input[current];
                current++;
                cursor.Move(GetCharacterSize(c));
            }
        }

        private void MoveLeft() {
            if (current > 0 && (current - 1 < input.Length)) {
                current--;
                char c = input[current];
                cursor.Move(-GetCharacterSize(c));
            }
        }

        private const int TabSize = 4;
        private void InsertTab() {
            for (int i = TabSize - (current % TabSize); i > 0; i--) {
                Insert(' ');
            }
        }

        private void MoveHome() {
            current = 0;
            cursor.Reset();
        }

        private void MoveEnd() {
            current = input.Length;
            cursor.Place(rendered);
        }

        public string ReadLine(int autoIndentSizeInput) {
            Initialize();

            autoIndentSize = autoIndentSizeInput;
            for (int i = 0; i < autoIndentSize; i++)
                Insert(' ');

            for (; ; ) {
                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.Key) {
                    case ConsoleKey.Backspace:
                        Backspace();
                        break;
                    case ConsoleKey.Delete:
                        Delete();
                        break;
                    case ConsoleKey.Enter:
                        Console.Write("\n");
                        string line = input.ToString();
                        if (line == "\x1A") return null;
                        if (line.Length > 0) {
                            history.AddLast(line);
                        }
                        return line;
                    case ConsoleKey.Tab: {
                            bool prefix = false;
                            if (changed) {
                                prefix = GetOptions();
                                changed = false;
                            }

                            if (options.Count > 0) {
                                string part = (key.Modifiers & ConsoleModifiers.Shift) != 0 ? options.Previous() : options.Next();
                                SetInput(options.Root + part);
                            } else {
                                if (prefix) {
                                    Console.Beep();
                                } else {
                                    InsertTab();
                                }
                            }
                            continue;
                        }
                    case ConsoleKey.UpArrow:
                        SetInput(history.Previous());
                        break;
                    case ConsoleKey.DownArrow:
                        SetInput(history.Next());
                        break;
                    case ConsoleKey.RightArrow:
                        MoveRight();
                        break;
                    case ConsoleKey.LeftArrow:
                        MoveLeft();
                        break;
                    case ConsoleKey.Escape:
                        SetInput(String.Empty);
                        break;
                    case ConsoleKey.Home:
                        MoveHome();
                        break;
                    case ConsoleKey.End:
                        MoveEnd();
                        break;
                    default:
                        Insert(key);
                        break;
                }
                changed = true;
            }
        }

        public void Write(string text, Style style) {
            switch (style) {
                case Style.Prompt: WriteColor(text, PromptColor); break;
                case Style.Out: WriteColor(text, OutColor); break;
                case Style.Error: WriteColor(text, ErrorColor); break;
            }
        }

        public void WriteLine(string text, Style style) {
            Write(text + Environment.NewLine, style);
        }

        private void WriteColor(string s, ConsoleColor c) {
            ConsoleColor origColor = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.Write(s);
            Console.ForegroundColor = origColor;
        }

    }
}
