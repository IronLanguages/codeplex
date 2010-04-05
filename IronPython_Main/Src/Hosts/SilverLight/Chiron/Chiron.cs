/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Chiron {
    static class Chiron {
        static int _port;
        static string _dir;
        static string _xapfile;
        static bool _webserver;
        static bool _browser;
        static bool _silent;
        static bool _nologo;
        static bool _help;
        static bool _zipdlr;
        static bool _saveManifest;
        static bool _notifyIcon;
        static string _error;
        static string _startPage;
        static string[] _localPath;

        // these properties are lazy loaded so we don't parse the configuration file unless necessary
        static AppManifestTemplate _ManifestTemplate;
        static Dictionary<string, LanguageInfo> _Languages;
        static string _UrlPrefix, _LocalAssemblyPath, _ExternalUrlPrefix;
        static Dictionary<string, string> _MimeMap;
        static bool? _DetectLanguageFromXAP;
        static bool? _UseExtensions;

        static int Main(string[] args) {
            ParseOptions(args);

            if (!_nologo) {
                Console.WriteLine(
                  "Chiron - Silverlight Development Utility. Version {0}", 
                  typeof(Chiron).Assembly.GetName().Version
                );
            }

            if (_help) {
                Console.WriteLine(
@"Usage: Chiron [<options>]

Common usages:

  Chiron.exe /b
    Starts the web-server on port 2060, and opens the default browser
    to the root of the web-server. This is used for developing an
    application, as Chiron will rexap you application's directory for
    every request.
    
  Chiron.exe /d:app /z:app.xap
    Takes the contents of the app directory and generates an app.xap 
    from it, which embeds the DLR and language assemblies according to
    the settings in Chiron.exe.config. This is used for deployment,
    so you can take the generated app.xap, along with any other files,
    and host them on any web-server.

Options:

  Note: forward-slashes (/) in option names can be substituted for dashes (-).
        For example ""Chiron.exe -w"" instead of ""Chiron.exe /w"".

  /w[ebserver][:<port number>]
    Launches a development web server that automatically creates
    XAP files for dynamic language applications (runs /z for every
    request of a XAP file, but generates it in memory).
    Optionally specifies server port number (default: 2060)

  /b[rowser][:<start url>]
    Launches the default browser and starts the web server
    Implies /w, cannot be combined with /x or /z

  /z[ipdlr]:<file>
    Generates a XAP file, including dynamic language dependencies, and
    auto-generates AppManifest.xaml (equivalent of /m in memory), 
    if it does not exist.
    Does not start the web server, cannot be combined with /w or /b

  /m[anifest]
    Saves the generated AppManifest.xaml file to disk
    Use /d to set the directory containing the sources
    Can only be combined with /d, /n and /s

  /d[ir[ectory]]:<path>
    Specifies directory on disk (default: the current directory).
    Implies /w.

  /r[efpath]:<path>
    Path where assemblies are located. Defaults to the same directory
    where Chiron.exe exists.
    Overrides appSettings.localAssemblyPath in Chiron.exe.config.

  /p[ath]:<path1;path2;..;pathn>
    Semi-colon-separated directories to be included in the XAP file,
    in addition to what is specified by /d
    
  /u[rlprefix]:<relative or absolute uri>
    Appends a relative or absolute Uri to each language assembly or extension
    added to the AppManifest.xaml. If a relative Uri is provided, Chiron 
    will serve all files located in the /refpath at this Uri, relative to the
    root of the web-server.
    Overrides appSettings.urlPrefix in Chiron.exe.config.
  
  /l /detectLanguages:true|false (default true)
    Scans the current application directory for files with a valid language's
    file extension, and only makes those languages available to the XAP file.
    See /useExtensions for whether the languages assemblies or extension files
    are used.
    If false, no language-specific assemblies/extensions are added to the XAP,
    so the Silverlight application is responsible for parsing the languages.config
    file in the XAP and downloading the languages it needs.
    Overrides appSettings.detectLanguages in Chiron.exe.config.
  
  /e /useExtensions:true|false (default false)
    Toggles whether or not language and DLR assemblies are embedded in
    the XAP file, or whether their equivalent extension files are used.

  /x[ap[file]]:<file>
    Specifies XAP file to generate. Only XAPs a directory; does not
    generate a manifest or add dynamic language DLLs; see /z for that
    functionality.
    Does not start the web server, cannot be combined with /w or /b

  /notification
    Display notification icon
  
  /n[ologo]
    Suppresses display of the logo banner

  /s[ilent]
    Suppresses display of all output
");
            }
            else if (_error != null) {
                return Error(1000, "options", _error);
            }
            else if (!string.IsNullOrEmpty(_xapfile)) {
                try {
                    if (!_silent)
                        Console.WriteLine("Generating XAP {0} from {1}", _xapfile, _dir);

                    if (_zipdlr) {
                        XapBuilder.XapToDisk(_dir, _xapfile);
                    } else {
                        ZipArchive xap = new ZipArchive(_xapfile, FileAccess.Write);
                        XapBuilder.AddPathDirectories(xap);
                        xap.CopyFromDirectory(_dir, "");
                        xap.Close();
                    }
                }
                catch (Exception ex) {
                    return Error(1001, "xap", ex.Message);
                }
            }
            else if (_saveManifest) {
                try {
                    string manifest = Path.Combine(_dir, "AppManifest.xaml");
                    if (File.Exists(manifest)) {
                        return Error(3002, "manifest", "AppManifest.xaml already exists at path " + manifest);
                    }

                    // Generate the AppManifest.xaml file to disk, as we would if we were
                    // generating it in the XAP
                    XapBuilder.GenerateManifest(_dir).Save(manifest);
                } catch (Exception ex) {
                    return Error(3001, "manifest", ex.Message);
                }
            }
            else {
                string uri = string.Format("http://localhost:{0}/", _port);

                if (!_silent)
                    Console.WriteLine("Chiron serving '{0}' as {1}", _dir, uri);

                try {
                    HttpServer server = new HttpServer(_port, _dir);
                    server.Start();

                    if (_browser) {
                        if (_startPage != null) {
                            uri += _startPage;
                        }

                        ProcessStartInfo startInfo = new ProcessStartInfo(uri);
                        startInfo.UseShellExecute = true;
                        startInfo.WorkingDirectory = _dir;

                        Process p = new Process();
                        p.StartInfo = startInfo;
                        p.Start();
                    }

                    if (_notifyIcon) {
                        Thread notify = new Thread(
                            () => {
                                Application.EnableVisualStyles();
                                Application.SetCompatibleTextRenderingDefault(false);

                                var notification = new Notification(_dir, _port);
                                Application.Run();
                            }
                        );
                        notify.SetApartmentState(ApartmentState.STA);
                        notify.IsBackground = true;
                        notify.Start();
                    }

                    while (server.IsRunning) Thread.Sleep(500);
                } catch (Exception ex) {
                    return Error(2001, "server", ex.Message);
                }
            }

            return 0;
        }

        

        // Print out an error in a format suitable for msbuild. For example:
        // chiron.exe: XAP error CH1001: Access to the path 'app.xap' is denied.
        private static int Error(int code, string category, string message) {
            if (!_silent) {
                string toolname = Path.GetFileName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                Console.WriteLine("{0}: {1} error CH{2}: {3}", toolname, category, code, message);
            }

            return code;
        }

        static void ParseOptions(string[] args) {
            _port = 2060;
            _dir = Directory.GetCurrentDirectory();
            _xapfile = null;
            _zipdlr = false;
            _webserver = false;
            _browser = false;
            _silent = false;
            _nologo = false;
            _help = false;
            _localPath = new string[]{ };

            foreach (string option in args) {
                if (option[0] != '-' && option[0] != '/') {
                    _error = string.Format("Invalid option '{0}'", option);
                    return;
                }

                string opt = option.Substring(1);
                string val = string.Empty;
                int i = opt.IndexOf(':');
                if (i > 0) {
                    val = opt.Substring(i + 1);
                    opt = opt.Substring(0, i);
                }
                opt = opt.ToLowerInvariant();

                switch (opt) {
                case "w": case "webserver":
                    if (!string.IsNullOrEmpty(val)) {
                        if (!int.TryParse(val, out _port) || _port <= 0) {
                            _error = string.Format("Invalid port '{0}'", val);
                            return;
                        }
                    }
                    _webserver = true;
                    break;
                case "d": case "dir": case "directory":
                    try {
                        _dir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), val));
                        if (!Directory.Exists(_dir)) throw new InvalidDataException();
                    }
                    catch {
                        _error = string.Format("Invalid directory '{0}'", val);
                        return;
                    }
                    break;
                case "x": case "xap": case "xapfile":
                case "z": case "zipdlr":
                    if (string.IsNullOrEmpty(val)) {
                        _error = "missing xapfile name";
                        return;
                    }
                    try {
                        _xapfile = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), val));
                    }
                    catch {
                        _error = string.Format("Invalid xapfile '{0}'", val);
                        return;
                    }
                    if (opt.StartsWith("z")) {
                        _zipdlr = true;
                    }
                    break;
                case "r": case "refpath":
                    try {
                        _LocalAssemblyPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), val));
                        if (!Directory.Exists(_LocalAssemblyPath)) throw new InvalidDataException();
                    } catch {
                        _error = string.Format("Invalid refpath '{0}'", val);
                        return;
                    }
                    break;
                case "b": case "browser":
                    if (!string.IsNullOrEmpty(val)) {
                        _startPage = val.Replace(Path.DirectorySeparatorChar, '/');
                    }
                    _browser = true;
                    _webserver = true;
                    break;
                case "u": case "urlPrefix":
                    try {
                        UrlPrefix = val;
                    } catch {
                        _error = string.Format("Invalid urlPrefix '{0}'", val);
                        return;
                    }
                    break;
                case "e": case "useExtensions":
                    try {
                        UseExtensions = bool.Parse(val);
                    } catch {
                        _error = string.Format("useExtensions should be a boolean value, was '{0}'", val);
                        return;
                    }
                    break;
                case "l": case "detectLanguages":
                    try {
                        DetectLanguage = bool.Parse(val);
                    } catch {
                        _error = string.Format("detectLanguage should be a boolean value, was '{0}'", val);
                        return;
                    }
                    break;
                case "p": case "path":
                    ParseAndSetLocalPath(val);
                    break;
                case "m": case "manifest":
                    _saveManifest = true;
                    break;
                case "n": case "nologo":
                    _nologo = true;
                    break;
                case "notification":
                    _notifyIcon = true;
                    break;
                case "s": case "silent":
                    _silent = true;
                    _nologo = true;
                    break;
                case "?": case "h": case "help":
                    _help = true;
                    return;
                default:
                    _error = string.Format("Invalid option '{0}'", option);
                    return;
                }
            }

            if (_xapfile != null && _webserver)
                _error = "/x or /z cannot be used together with /w or /b";

            if (_saveManifest && (_xapfile != null || _webserver))
                _error = "/m can only be used with /d, /s and /n";

            if (args.Length == 0)
                _help = true;
        }

        internal static void ParseAndSetLocalPath(string pathString) {
            var __path = new List<string>();
            string[] paths = pathString.Split(';');
            foreach (string path in paths) {
                var fullPath = path;
                if (!Path.IsPathRooted(path))
                    fullPath = Path.Combine(_dir, path);
                if (Directory.Exists(fullPath))
                    __path.Add(fullPath);
            }
            _localPath = __path.ToArray();
        }

        public static void Log(int statusCode, string uri, int byteCount, string message) {
            if (!_silent) {
                if (string.IsNullOrEmpty(message)) {
                    Console.WriteLine("{0,8:HH:mm:ss} {1,3} {2,9:n0} {3}",
                        DateTime.Now, statusCode, byteCount, uri);
                }
                else {
                    Console.WriteLine("{0,8:HH:mm:ss} {1,3} {2,9:n0} {3} [{4}]",
                        DateTime.Now, statusCode, byteCount, uri, message);
                }
            }
        }

        internal static string[] LocalPath {
            get { return _localPath; }
        }

        internal static AppManifestTemplate ManifestTemplate {
            get {
                if (_ManifestTemplate == null) {
                    _ManifestTemplate = (AppManifestTemplate)ConfigurationManager.GetSection("AppManifest.xaml");
                    if (_ManifestTemplate == null) {
                        throw new ConfigurationErrorsException("Could not find application configuration file, or could not find AppManifest.xaml section");
                    }
                }
                return _ManifestTemplate;
            }
        }

        internal static Dictionary<string, LanguageInfo> Languages {
            get {
                if (_Languages == null) {
                    _Languages = (Dictionary<string, LanguageInfo>)ConfigurationManager.GetSection("Languages");
                    if (_Languages == null) {
                        throw new ConfigurationErrorsException("Could not find application configuration file, or could not find Languages section");
                    }
                }
                return _Languages;
            }
        }

        /// <summary>
        /// Optional URL prefix for language assemblies
        /// </summary>
        internal static string UrlPrefix {
            get {
                if (_UrlPrefix == null) {
                    UrlPrefix = ConfigurationManager.AppSettings["urlPrefix"];
                }
                return _UrlPrefix;
            }
            set {
                _UrlPrefix = value;
                if (_UrlPrefix == null) _UrlPrefix = "";
                else {
                    if (!_UrlPrefix.EndsWith("/")) _UrlPrefix += '/';
                    // validate
                    Uri uri = new Uri(_UrlPrefix, UriKind.RelativeOrAbsolute);
                    if (!uri.IsAbsoluteUri && !_UrlPrefix.StartsWith("/")) {
                        _UrlPrefix = null;
                        throw new ConfigurationErrorsException("urlPrefix must be an absolute URI or start with a /");
                    }
                }
            }
        }

        internal static bool DetectLanguage {
            get {
                if (_DetectLanguageFromXAP == null) {
                    string value = ConfigurationManager.AppSettings["detectLanguage"] ?? "true";
                    _DetectLanguageFromXAP = bool.Parse(value);
                }
                return _DetectLanguageFromXAP.Value;
            }
            set {
                _DetectLanguageFromXAP = value;
            }
        }

        internal static bool UseExtensions {
            get {
                if (_UseExtensions == null) {
                    string value = ConfigurationManager.AppSettings["useExtensions"] ?? "false";
                    _UseExtensions = bool.Parse(value);
                }
                return _UseExtensions.Value;
            }
            set {
                _UseExtensions = value;
            }
        }

        internal static Dictionary<string, string> MimeMap {
            get {
                if (_MimeMap == null) {
                    _MimeMap = (Dictionary<string, string>)ConfigurationManager.GetSection("MimeTypes");
                    if (_MimeMap == null) {
                        throw new ConfigurationErrorsException("Could not find application configuration file, or could not find MimeTypes section");
                    }
                }
                return _MimeMap;
            }
        }

        private static string ChironPath() {
            return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        }

        // Looks for a DLR/language assembly relative to Chiron.exe
        // The path is set in localAssemblyPath in Chiron.exe.config's appSettings section
        internal static string TryGetAssemblyPath(string name) {
            if (_LocalAssemblyPath == null) {

                _LocalAssemblyPath = ConfigurationManager.AppSettings["localAssemblyPath"] ?? "";
                if (!Path.IsPathRooted(_LocalAssemblyPath)) {
                    _LocalAssemblyPath = Path.Combine(ChironPath(), _LocalAssemblyPath);
                }
                if (!Directory.Exists(_LocalAssemblyPath)) {
                    // fallback to Chiron install location
                    _LocalAssemblyPath = ChironPath();
                }
            }

            string path = Path.Combine(_LocalAssemblyPath, Path.GetFileName(name));
            return File.Exists(path) ? path : null;
        }
    }
}
