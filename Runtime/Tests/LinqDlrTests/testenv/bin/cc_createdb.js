// Create a CC database on a remote server, if the database does not exist.
// Creates the .INI file used by the Magellan tools at the specified location
// If the DB exists, it won't get deleted. The .INI file will be generated,
// as normal.

//
// Execute as admin
// Using a 32bit command prompt

var objArgs           = WScript.Arguments;
var objFile           = new ActiveXObject("Scripting.FileSystemObject");

if (objArgs.Count() != 3) {
    WScript.Echo("usage: createccdb <server> <database> <inifile>");
    WScript.Quit(1);
}

var server   = objArgs(0);
var database = objArgs(1);
var inifile  = objArgs(2);

var obj = new ActiveXObject("Magellan.Application")
var db = obj.OpenCoverageDatabase(1, server , database, true, null, null);   // 1 = MagDatabaseOpenType.magDatabaseOpenOrCreate
									     // true = Integrated Auth

fsStream = objFile.CreateTextFile(inifile, true /* overwrite */, false /* ASCII */ );

fsStream.WriteLine("[Magellan Options]");
fsStream.WriteLine("Server=" + db.Server);
fsStream.WriteLine("Database=" + db.Database);

