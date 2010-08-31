use strict;
use Win32::Process;
use File::Spec;
use Cwd;

#
# set SIMULATOR_PIPE=perl -S changeTrust.pl <permissions xml file> <permissions group name> <command to run>
#
# Code is adapted from ngenrun.pl
#
			
my (
	$cmd,			#cmd string
	$arg,			#arguments
	$quotedArg,		#arguments "quoted" (if needed)
	$caspol_cmd,		#the command to run caspol.exe
	$retCode,		#stores the return value of executed commands
	$_DEBUG,		#enables debugging output
	$caspol_output,		#stores the output from the caspol.exe commands
	$permissionsFile,	#user specified file containing the permissions desired
	$permissionsGroup,	#user specified group that they are creating (should match name in xml file)
);

# Protypes
sub DebugPrint($$);
sub run_cmd($);
sub checkCaspol();
sub applyPermissions($$);
sub resetPermissions();
sub CleanupAndExit($);

#print help messages
if ($ARGV[0] =~ /^[-\/](\?|help)$/i) {
	  print_help();
	  exit(0);
}

if (scalar @ARGV < 3) {
	  print_help();
	  exit(-1);
}

# change this to just dump to stdout
$caspol_output = "changeTrust.pl.out";

#parse environment variables
if(exists($ENV{_DEBUG}) && $ENV{_DEBUG} == 1 ){
	$_DEBUG=1;
} else {
    $_DEBUG=0;
}

checkCaspol();

# The first argument is the xml file to use
$permissionsFile = shift @ARGV;
if( $permissionsFile =~ m/\s+/) {
	$permissionsFile = "\"".$permissionsFile."\"";
}

# The second argument is the group to use
$permissionsGroup = shift @ARGV;
if( $permissionsGroup =~ m/\s+/) {
	$permissionsGroup = "\"".$permissionsFile."\"";
}

applyPermissions($permissionsFile, $permissionsGroup);

# parse the arguments to the command we want to run
$cmd = "";
foreach $arg (@ARGV) {
    
    $quotedArg = "";
    #If there is a space in the args, surround it with " "
    if($arg=~m/\s+/){
	$quotedArg = "\"".$arg."\"";
	$cmd=$cmd.$quotedArg." ";
    }
    else {
	$cmd = $cmd.$arg." ";
    }
}

#Remove the last trailing space added in the above loop
$cmd=~ s/\s$//;

DebugPrint(__LINE__,"Running $cmd");
$retCode = (run_cmd($cmd));
#$retCode = system($cmd) >> 8;
DebugPrint(__LINE__, "retCode=$retCode");

resetPermissions();

unlink "$caspol_output" if (!$_DEBUG);

exit($retCode);




####################
#                  #
# Helper Functions #
#                  #
####################

# Check to see that caspol.exe exists and is in our path
sub checkCaspol() {
	# Since this is just a test, redirect the output
	my $caspol_cmd = "caspol.exe -q -m -lg > $caspol_output 2>&1";

	DebugPrint(__LINE__,"Checking to see that caspol.exe is on the path:");
	DebugPrint(__LINE__,"$caspol_cmd");

	my $retVal = system($caspol_cmd)>>8;

	DebugPrint(__LINE__,"Return value: $retVal");

	# Exit with -1 if caspol.exe is not found
	if ($retVal != 0) {
		print "ERROR: caspol.exe not found!";
		exit(-1);
	}
}

# Create a new permission group from the xml file specified and apply
# those permissions to the MyComputer zone (will affect all locally run managed assemblies)
sub applyPermissions($$) {
	my $xmlFile = shift;
	my $group = shift;
	my $retVal;
	my $caspol_cmd;

	DebugPrint(__LINE__,"xmlFile is '$xmlFile'");
	DebugPrint(__LINE__,"group is '$group'");

	$caspol_cmd = "$ENV{ADMIN_PIPE} caspol.exe -q -m -ap $xmlFile";
	DebugPrint(__LINE__,"Attempting to create new permissions group specified in '$xmlFile'");
	DebugPrint(__LINE__,"Executing: $caspol_cmd");

	$retVal = system($caspol_cmd) >> 8;
	DebugPrint(__LINE__,"Return value: $retVal");

	if($retVal != 0) {
		print "ERROR: Unable to create new permission set from $xmlFile.";
		CleanupAndExit(-1);
	}

	$caspol_cmd = "$ENV{ADMIN_PIPE} caspol.exe -q -cg My_Computer_Zone $group";
	DebugPrint(__LINE__,"Attempting to set My_Computer_Zone to have permissions of group $group");
	DebugPrint(__LINE__,"Executing: $caspol_cmd");

	$retVal = system($caspol_cmd) >> 8;
	DebugPrint(__LINE__,"Return value: $retVal");

	if($retVal != 0) {
		print "ERROR: Unable to assign permissions group $group to MyComputer Zone.";
		CleanupAndExit(-1);
	}
}

# Attempt to reset the CLR security back to defaults
sub resetPermissions() {
	my $caspol_cmd = "$ENV{ADMIN_PIPE} caspol.exe -q -rs";
	DebugPrint(__LINE__,"Attempting to reset CLR security to defaults");
	DebugPrint(__LINE__,"Executing: $caspol_cmd");

	my $retVal = system($caspol_cmd) >> 8;
	DebugPrint(__LINE__,"Return value: $retVal");

	if($retVal != 0) {
		print "ERROR: Unable to reset clr permissions to defaults";
		print ">>>>>WARNING: CLR permissions are now probably in an unknown state";
		exit(-1);
	}
}

#taken from runall.pl
#this approach would spawn process with an exitcode more than 8 bits
sub run_cmd($){
	my $cmd = shift;
	my ($proc, $retval);
	my $retval2=0;

	# be very conservative here. note that if we use CreateProcess, the 
	# Perl environment doesn't get imparted to the new process. ActiveState
	# bug #16448, affecting ActivePerl <= 623 at least.
	my $cp = Win32::Process::Create($proc, $ENV{"comspec"}, "/c $cmd", 
					1, &Win32::Process::NORMAL_PRIORITY_CLASS, '.');

	if (!$proc) {
	         print "Unable to start process: ", Win32::FormatMessage( Win32::GetLastError() ), "\n";
	}
	if (!$proc->Wait(INFINITE)) {
			$proc->Kill(0);
			$retval = 5;
	} else {
		# The following line modifies $retval.  That seems wrong, I would expect
		# to have to pass it as a ref (i.e., \$retval), and Perl In A NutShell,
		# 1st Ed., p.586 agrees with me.  But this works, so why argue?
		$proc->GetExitCode($retval2);
	}
	return $retval2;
}

#
# DebugPrint -- print debug message if _Debug= 1
#
sub DebugPrint($$) {
	my ($line, $msg) = @_;
	if($_DEBUG){
		print "[changeTrust.pl:$line] $msg\n";
	}
}

#
# CleanupAndExit -- exits with the specified value
# 
sub CleanupAndExit($) {
	my $retVal = shift;
	# always clean up after ourselves
	resetPermissions();
	exit($retVal);
}

#
# print_help -- print short help message
#
sub print_help
{
    print <<ENDHELP;
changeTrust.pl <permissions xml file> <permissions group name> <command>

This command temporarily creates a permissions group from the xml file
argument and changes the permissions for assemblies in the MyComputer Zone to
that of the group specified.

The command is then executed, the permissions reset to defaults, and the
command return value returned.
ENDHELP
return
}
