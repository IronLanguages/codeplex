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
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

using IronPython.Runtime;

[assembly: PythonModule("nt", typeof(IronPython.Modules.PythonNT))]
namespace IronPython.Modules {
    public static class PythonNT {

        #region Public API Surface

        [PythonName("abort")]
        public static void Abort() {
            System.Environment.FailFast("IronPython os.abort");
        }

        [PythonName("chdir")]
        public static void ChangeDirectory(string path) {
            Directory.SetCurrentDirectory(path);
        }

        [PythonName("chmod")]
        public static void ChangeMod(string path, int mode) {
            FileInfo fi = new FileInfo(path);
            if ((mode & S_IREAD) != 0) fi.Attributes |= FileAttributes.ReadOnly;
            else if ((mode & S_IWRITE) != 0) fi.Attributes &= ~(FileAttributes.ReadOnly);
        }

        [PythonName("close")]
        public static void CloseFileDescriptor(int fd) {
            PythonFile pf = PythonFileManager.GetFileFromId(fd);
            pf.Close();
        }

        public static object Environment {
            [PythonName("environ")]
            get {
                return new EnvironmentDict();
            }
        }

        public static object error = ExceptionConverter.GetPythonExceptionByName("nt_error");

        [PythonName("_exit")]
        public static void ExitProcess(int code) {
            System.Environment.Exit(code);
        }

        [PythonName("fdopen")]
        public static object FileForDescriptor(ICallerContext context, int fd) {
            return FileForDescriptor(context, fd, "r");
        }

        [PythonName("fdopen")]
        public static object FileForDescriptor(ICallerContext context, int fd, string mode) {
            return FileForDescriptor(context, fd, mode, 0);
        }

        [PythonName("fdopen")]
        public static object FileForDescriptor(ICallerContext context, int fd, string mode, int bufsize) {
            PythonFile pf = PythonFileManager.GetFileFromId(fd);
            return pf;
        }

        [PythonName("fstat")]
        public static object GetFileFStats(int fd) {
            PythonFile pf = PythonFileManager.GetFileFromId(fd);
            return GetFileStats(pf.name);
        }

        [PythonName("getcwd")]
        public static string GetCurrentDirectory() {
            return Directory.GetCurrentDirectory();
        }

        [PythonName("getcwdu")]
        public static string GetCurrentDirectoryUnicode() {
            return Directory.GetCurrentDirectory();
        }

        [PythonName("getpid")]
        public static int GetCurrentProcessId() {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        [PythonName("listdir")]
        public static List ListDirectory(string path) {
            List ret = List.Make();
            string[] files = Directory.GetFiles(path);
            addBase(files, ret);
            addBase(Directory.GetDirectories(path), ret);
            return ret;
        }

        // 
        // lstat(path) -> stat result
        // Like stat(path), but do not follow symbolic links.!!!
        // 
        [PythonName("lstat")]
        public static object GetFileLStats(string path) {
            return GetFileStats(path);
        }

        [PythonName("mkdir")]
        public static void MakeDirectory(string path) {
            if (Directory.Exists(path)) throw Ops.IOError("directory already exists");
            Directory.CreateDirectory(path);
        }

        [PythonName("mkdir")]
        public static void MakeDirectory(string path, int mode) {
            if (Directory.Exists(path)) throw Ops.IOError("directory already exists");
            // we ignore mode
            Directory.CreateDirectory(path);
        }
        [PythonName("open")]
        public static object Open(ICallerContext context, string filename, int flag) {
            return Open(context, filename, flag, 0777);
        }

        [PythonName("open")]
        public static object Open(ICallerContext context, string filename, int flag, int mode) {
            FileStream fs = File.Open(filename, FileModeFromFlags(flag), FileAccessFromFlags(flag));
            PythonFile pf = PythonFile.Make(context, TypeCache.PythonFile, fs);
            return PythonFileManager.GetIdFromFile(pf);
        }

        [PythonName("popen")]
        public static PythonFile OpenPipedCommand(ICallerContext context, string command) {
            return OpenPipedCommand(context, command, "r");
        }

        [PythonName("popen")]
        public static PythonFile OpenPipedCommand(ICallerContext context, string command, string mode) {
            return OpenPipedCommand(context, command, mode, 4096);
        }

        [PythonName("popen")]
        public static PythonFile OpenPipedCommand(ICallerContext context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "r";
            ProcessStartInfo psi = GetProcessInfo(command);
            Process p;
            PythonFile res;

            switch (mode) {
                case "r":
                    psi.RedirectStandardOutput = true;
                    p = Process.Start(psi);

                    res = new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r");
                    break;
                case "w":
                    psi.RedirectStandardInput = true;
                    p = Process.Start(psi);

                    res = new POpenFile(context, command, p, p.StandardInput.BaseStream, "w");
                    break;
                default:
                    throw Ops.ValueError("expected 'r' or 'w' for mode, got {0}", mode);
            }

            return res;
        }

        [PythonName("popen2")]
        public static Tuple OpenPipedCommandBoth(ICallerContext context, string command) {
            return OpenPipedCommandBoth(context, command, "t");
        }

        [PythonName("popen2")]
        public static Tuple OpenPipedCommandBoth(ICallerContext context, string command, string mode) {
            return OpenPipedCommandBoth(context, command, "t", 4096);
        }

        [PythonName("popen2")]
        public static Tuple OpenPipedCommandBoth(ICallerContext context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "t";
            if (mode != "t" && mode != "b") throw Ops.ValueError("mode must be 't' or 'b' (default is t)");
            if (mode == "t") mode = String.Empty;

            ProcessStartInfo psi = GetProcessInfo(command);
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            Process p = Process.Start(psi);

            return Tuple.MakeTuple(new POpenFile(context, command, p, p.StandardInput.BaseStream, "w" + mode),
                    new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r" + mode));
        }

        [PythonName("popen3")]
        public static Tuple OpenPipedCommandAll(ICallerContext context, string command) {
            return OpenPipedCommandAll(context, command, "t");
        }

        [PythonName("popen3")]
        public static Tuple OpenPipedCommandAll(ICallerContext context, string command, string mode) {
            return OpenPipedCommandAll(context, command, "t", 4096);
        }

        [PythonName("popen3")]
        public static Tuple OpenPipedCommandAll(ICallerContext context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "t";
            if (mode != "t" && mode != "b") throw Ops.ValueError("mode must be 't' or 'b' (default is t)");
            if (mode == "t") mode = String.Empty;

            ProcessStartInfo psi = GetProcessInfo(command);
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            Process p = Process.Start(psi);

            return Tuple.MakeTuple(new POpenFile(context, command, p, p.StandardInput.BaseStream, "w" + mode),
                    new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r" + mode),
                    new POpenFile(context, command, p, p.StandardError.BaseStream, "r+" + mode));
        }

        [PythonName("putenv")]
        public static void PutEnvironment(string varname, string value) {
            System.Environment.SetEnvironmentVariable(varname, value);
        }

        [PythonName("read")]
        public static string ReadFromFileDescriptor(int fd, int buffersize) {
            PythonFile pf = PythonFileManager.GetFileFromId(fd);
            return pf.Read();
        }

        [PythonName("remove")]
        public static void RemoveFile(string path) {
            UnlinkFile(path);
        }

        [PythonName("rename")]
        public static void Rename(string src, string dst) {
            Directory.Move(src, dst);
        }

        [PythonName("rmdir")]
        public static void RemoveDirectory(string path) {
            Directory.Delete(path);
        }

        [PythonName("spawnl")]
        public static object SpawnProcess(int mode, string path, params object[] args) {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = path;
            process.StartInfo.UseShellExecute = false;
            if (args != null && args.Length > 0) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                bool space = false;
                foreach (object arg in args) {
                    string strarg = Ops.ToString(arg);
                    if (space) {
                        sb.Append(' ');
                    }
                    if (strarg.IndexOf(' ') != -1) {
                        sb.Append('"');
                        sb.Append(strarg);
                        sb.Append('"');
                    } else {
                        sb.Append(strarg);
                    }
                    space = true;
                }
                process.StartInfo.Arguments = sb.ToString();
            }
            if (!process.Start()) {
                throw Ops.OSError("Cannot start process: {0}", path);
            }
            if (mode == 0) {
                process.WaitForExit();
                int exitCode = process.ExitCode;
                process.Close();
                return exitCode;
            } else {
                return process.Id;
            }
        }

        [PythonName("startfile")]
        public static void StartFile(string filename) {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        [PythonType("stat_result")]
        public class StatResult {
            internal long mode, ino, dev, nlink, uid, gid, size, atime, mtime, ctime;

            public StatResult() {
            }

            public StatResult(long mode, long ino, long dev, long nlink, long uid,
                               long gid, long size, long atime, long mtime, long ctime) {
                this.mode = mode;
                this.ino = ino;
                this.dev = dev;
                this.nlink = nlink;
                this.uid = uid;
                this.gid = gid;
                this.size = size;
                this.atime = atime;
                this.mtime = mtime;
                this.ctime = ctime;
            }

            public long StatATime {
                [PythonName("st_atime")]
                get {
                    return atime;
                }
            }
            public long StatCTime {
                [PythonName("st_ctime")]
                get {
                    return ctime;
                }
            }
            public long StatDev {
                [PythonName("st_dev")]
                get {
                    return dev;
                }
            }
            public long StatGid {
                [PythonName("st_gid")]
                get {
                    return gid;
                }
            }
            public long StatIno {
                [PythonName("st_ino")]
                get {
                    return ino;
                }
            }
            public long StatMode {
                [PythonName("st_mode")]
                get {
                    return mode;
                }
            }
            public long StatMTime {
                [PythonName("st_mtime")]
                get {
                    return mtime;
                }
            }
            public long StatNLink {
                [PythonName("st_nlink")]
                get {
                    return nlink;
                }
            }
            public long StatSize {
                [PythonName("st_size")]
                get {
                    return size;
                }
            }
            public long StatUid {
                [PythonName("st_uid")]
                get {
                    return uid;
                }
            }

            public object this[int index] {
                get {
                    switch (index) {
                        case 0: return StatMode;
                        case 1: return StatIno;
                        case 2: return StatDev;
                        case 3: return StatNLink;
                        case 4: return StatUid;
                        case 5: return StatGid;
                        case 6: return StatSize;
                        case 7: return StatATime;
                        case 8: return StatMTime;
                        case 9: return StatCTime;
                    }
                    throw Ops.IndexError("index out of range: {0}", index);
                }
            }
        }

        [Documentation("stat(path) -> stat result\nGathers statistics about the specified file or directory")]
        [PythonName("stat")]
        public static object GetFileStats(string path) {
            StatResult sr = new StatResult();

            try {
                sr.atime = (long)Directory.GetLastAccessTime(path).Subtract(DateTime.MinValue).TotalSeconds;
                sr.ctime = (long)Directory.GetCreationTime(path).Subtract(DateTime.MinValue).TotalSeconds;
                sr.mtime = (long)Directory.GetLastWriteTime(path).Subtract(DateTime.MinValue).TotalSeconds;

                if (Directory.Exists(path)) {
                    sr.mode = 0x4000;
                } else if (File.Exists(path)) {
                    FileInfo fi = new FileInfo(path);
                    sr.size = fi.Length;
                    sr.mode = 0x8000; //@TODO - Set other valid mode types (S_IFCHR, S_IFBLK, S_IFIFO, S_IFLNK, S_IFSOCK) (to the degree that they apply)
                } else {
                    throw new IOException("file does not exist");
                }
            } catch (Exception e) {
                throw ExceptionConverter.CreateThrowable(error, e.Message);
            }

            return sr;
        }

        [PythonName("tempnam")]
        public static string GetTemporaryFileName() {
            return GetTemporaryFileName(null);
        }

        [PythonName("tempnam")]
        public static string GetTemporaryFileName(string dir) {
            return GetTemporaryFileName(null, null);
        }

        [PythonName("tempnam")]
        public static string GetTemporaryFileName(string dir, string prefix) {
            if (dir == null) dir = Path.GetTempPath();
            else dir = Path.GetDirectoryName(dir);

            if (prefix == null) prefix = "";

            string path;
            if (dir.Length > 0 && (dir[dir.Length - 1] == Path.DirectorySeparatorChar || dir[dir.Length - 1] == Path.AltDirectorySeparatorChar))
                path = dir + prefix + Path.GetRandomFileName();
            else
                path = dir + Path.DirectorySeparatorChar + prefix + Path.GetRandomFileName();

            return path;
        }

        [PythonName("times")]
        public static object GetProcessTimeInfo() {
            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();

            return Tuple.MakeTuple(p.UserProcessorTime.TotalSeconds,
                p.PrivilegedProcessorTime.TotalSeconds,
                0,  // child process system time
                0,  // child process os time
                DateTime.Now.Subtract(p.StartTime).TotalSeconds);
        }

        [PythonName("tmpfile")]
        public static PythonFile GetTemporaryFile(ICallerContext context) {
            FileStream sw = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);

            return new PythonFile(sw, context.SystemState.DefaultEncoding, "w+b");
        }

        [PythonName("tmpnam")]
        public static string GetTempPath() {
            return Path.GetTempPath();
        }

        [PythonName("unlink")]
        public static void UnlinkFile(string path) {
            File.Delete(path);
        }

        [PythonName("unsetenv")]
        public static void DeleteEnvironmentVariable(string varname) {
            System.Environment.SetEnvironmentVariable(varname, null);
        }

        [PythonName("urandom")]
        public static object GetRandomData(int n) {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[n];
            rng.GetBytes(data);

            return PythonBinaryReader.PackDataIntoString(data, n);
        }

        [PythonName("utime")]
        public static void SetFileTimes(string path, Tuple times) {
            FileInfo fi = new FileInfo(path);
            if (times == null) {
                fi.LastAccessTime = DateTime.Now;
                fi.LastWriteTime = DateTime.Now;
            } else if (times.Count == 2) {
                DateTime atime = new DateTime(1969, 12, 31, 16, 0, 0).Add(TimeSpan.FromSeconds(Converter.ConvertToDouble(times[0])));
                DateTime mtime = new DateTime(1969, 12, 31, 16, 0, 0).Add(TimeSpan.FromSeconds(Converter.ConvertToDouble(times[1])));

                fi.LastAccessTime = atime;
                fi.LastAccessTime = mtime;
            } else {
                throw Ops.TypeError("times value must be a 2-value tuple (atime, mtime)");
            }
        }

        [PythonName("waitpid")]
        public static Tuple WaitForProcess(int pid, object options) {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(pid);
            if (process == null) {
                throw Ops.OSError("Cannot find process {0}", pid);
            }
            process.WaitForExit();
            return Tuple.MakeTuple(pid, process.ExitCode);
        }

        [PythonName("write")]
        public static void WriteToFileDescriptor(int fd, string text) {
            PythonFile pf = PythonFileManager.GetFileFromId(fd);
            pf.Write(text);
        }

        public static object O_APPEND = 0x8;
        public static object O_CREAT = 0x100;
        public static object O_TRUNC = 0x200;

        public static object O_EXCL = 0x400;
        public static object O_NOINHERIT = 0x80;

        public static object O_RANDOM = 0x10;
        public static object O_SEQUENTIAL = 0x20;

        public static object O_SHORT_LIVED = 0x1000;
        public static object O_TEMPORARY = 0x40;

        public static object O_WRONLY = 0x1;
        public static object O_RDONLY = 0x0;
        public static object O_RDWR = 0x2;

        public static object O_BINARY = 0x8000;
        public static object O_TEXT = 0x4000;

        #endregion

        #region Private implementation details

        private const int S_IWRITE = 00200;
        private const int S_IREAD = 00400;

        private static void addBase(string[] files, List ret) {
            foreach (string file in files) {
                ret.AddNoLock(Path.GetFileName(file));
            }
        }

        private static FileMode FileModeFromFlags(int flags) {
            if ((flags & (int)O_APPEND) != 0) return FileMode.Append;
            if ((flags & (int)O_CREAT) != 0) return FileMode.CreateNew;
            if ((flags & (int)O_TRUNC) != 0) return FileMode.Truncate;
            return FileMode.Open;
        }

        private static FileAccess FileAccessFromFlags(int flags) {
            if ((flags & (int)O_RDWR) != 0) return FileAccess.ReadWrite;
            if ((flags & (int)O_WRONLY) != 0) return FileAccess.Write;

            return FileAccess.Read;
        }

        [PythonType(typeof(PythonFile))]
        private class POpenFile : PythonFile {
            private Process process;

            [PythonName("__new__")]
            public static object Make(ICallerContext context, string command, Process process, Stream stream, string mode) {
                return new POpenFile(context, command, process, stream, mode);
            }

            internal POpenFile(ICallerContext context, string command, Process process, Stream stream, string mode)
                : base(stream, context.SystemState.DefaultEncoding, command, mode) {
                this.process = process;
            }

            public override object Close() {
                base.Close();

                if (process.HasExited && process.ExitCode != 0) {
                    return process.ExitCode;
                }

                return null;
            }
        }

        private static ProcessStartInfo GetProcessInfo(string command) {
            ProcessStartInfo psi;
            string baseCommand = command;
            string args = null;
            bool fInQuote = false;
            int argStart = command.Length;

            for (int i = 0; i < command.Length; i++) {
                if (command[i] == '\"') fInQuote = !fInQuote;
                else if (!fInQuote && command[i] == ' ') {
                    argStart = i + 1;
                    break;
                }
            }

            if (argStart < command.Length) {
                args = command.Substring(argStart);
                baseCommand = command.Substring(0, argStart - 1);
            }

            baseCommand = GetCommandFullPath(baseCommand);
            psi = new ProcessStartInfo(baseCommand, args);
            psi.UseShellExecute = false;

            return psi;
        }

        private static string GetCommandFullPath(string command) {
            string fullpath = Path.GetFullPath(command);
            if (File.Exists(fullpath)) return fullpath;

            // TODO: need revisit
            string sysdir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.System);
            foreach (string suffix in new string[] { string.Empty, ".com", ".exe", "cmd", ".bat" }) {
                fullpath = Path.Combine(sysdir, command + suffix);
                if (File.Exists(fullpath)) return fullpath;
            }

            throw Ops.WindowsError("The system can not find command '{0}'", command);
        }
        #endregion
    }
}
