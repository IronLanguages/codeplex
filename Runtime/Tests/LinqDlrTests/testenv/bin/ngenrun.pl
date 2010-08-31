use strict;
use Win32::Process;
use File::Spec; 

#
# Driver for NGEN.exe to skip non-test tools and non-clr assemblies when set to SIMULATOR_PIPE
# set SIMULATOR_PIPE=perl -S ngenrun.pl
# Also make sure to use posthook ngenrun_posthook.pl to delete the ngened image
#
			
my (
	$cmd ,			#cmd string
	$mtcmd,			#mt cmd string
	$UNIFWDCMD,		#unifwd cmd string
	$UNITEST,		#flag if it is unicode tests
	$isTool,		#flat if it is a tool			
	$retCode,		#return value
	$arg,			#arguments
	$quotedArg,		#arguments "quoted" (if needed)
	$oldZapRequireValue,	#saved zaprequire value
	$zapRequireLevel,	#zaprequire setting
	$assertFound,		#flag is assert if found in the ngen output
	$ngenswitches,		#switches used by ngen
	$ngendeleteswitches,	#switches used to clean ngened imaged
	@ngenargs,		#arguments passed to ngen
	@combinedArgs,		#ngen arguments including the ones keep.lst
	%keepFileHash, 	#ngen arguments from keep.lst
	$ngen,			#ngen command string
	$mtRetCode,		#mtRetCode
	$_DEBUG,		#flag if _debug is set
	$excludeChkList,	#List of dlls excluded from ZapRequire checking
	$skipMT,		#Skip MT if assembly is strong named
	$zapSet,		#zapsetused
	$GCStressLevel 	#gcstress level
);

# Protypes
sub DebugPrint($$);
sub ngenKeepLst(@);
sub run_cmd($);

#print help messages
if ($ARGV[0] =~ /^[-\/](\?|help)$/i) {
	  print_help();
	  exit(0);
}

#parse environment variables
if(exists($ENV{_DEBUG}) && $ENV{_DEBUG} == 1 ){
	$_DEBUG=1;
} else {
    $_DEBUG=0;
}

#Determine if environment variable UNICODE_NGEN_TEST is set.  
if(exists($ENV{UNICODE_NGEN_TEST}) && $ENV{UNICODE_NGEN_TEST} == 1 )
{
	$UNITEST=1;
	$UNIFWDCMD="unifwd.exe";
} else {
    $UNITEST=0;
    $UNIFWDCMD="";
}

DebugPrint(__LINE__, "UNIFWDCMD = $UNIFWDCMD");

#determine if different zapset is used
$zapSet='';
if(defined($ENV{"COMPLUS_ZAPSET"}) ){
    $zapSet=$ENV{"COMPLUS_ZAPSET"};
    DebugPrint(__LINE__, "ZapSet = $zapSet");
} else {
    $zapSet="";
}

#determine ZapRequire level, default is 2
$zapRequireLevel=2;
if(defined($ENV{"COMPLUS_ZAPREQUIRE"})){
	$zapRequireLevel = $ENV{"COMPLUS_ZAPREQUIRE"};
}
else {
	if(defined($ENV{"ZAPREQUIRE_LEVEL"})){
		$zapRequireLevel = $ENV{"ZAPREQUIRE_LEVEL"};
     }
}
DebugPrint(__LINE__, "Zaprequire Level = $zapRequireLevel");

$GCStressLevel='';
if(defined($ENV{"COMPLUS_GCSTRESS"})){
	$GCStressLevel = $ENV{"COMPLUS_GCSTRESS"};
}
else {
	if(defined($ENV{"GCSTRESS_LEVEL"})){
			$GCStressLevel=$ENV{"GCSTRESS_LEVEL"};
		}
}
DebugPrint(__LINE__, "GCStress Level=$GCStressLevel");
	
#determine ngen option, default is ""
if(defined($ENV{"NGEN_SWITCHES"})){
	$ngenswitches = $ENV{"NGEN_SWITCHES"};
}
else
{
	$ngenswitches="";
}
DebugPrint(__LINE__, "ngen switches:$ngenswitches");

#determine how to clean the ngened image
if ($ngenswitches =~/\/legacy/i){
	$ngendeleteswitches="/legacy /delete"
}
elsif ($ngenswitches =~/install/i){
	$ngendeleteswitches="uninstall";
}
else {
	$ngendeleteswitches="/delete";
}
DebugPrint(__LINE__, "ngen delete switches:$ngendeleteswitches");	

#determine the zaprequrie exclude list
$excludeChkList="";

my $defaultExcludeList="msvcm80d msvcm80 mfcm80d mfcm80 mfcm80u mfcm80ud";
unless(exists($ENV{CheckCRTZap}) && $ENV{CheckCRTZap} == 1 )
{
	if(defined($ENV{"Complus_ZapRequireExcludeList"})){
		$excludeChkList= $ENV{"Complus_ZapRequireExcludeList"}." ".$defaultExcludeList;
	}
	else {
		$excludeChkList=$defaultExcludeList;
	}
	$ENV{"Complus_ZapRequireExcludeList"}=$excludeChkList;
}
DebugPrint(__LINE__, "excludeChkList = $excludeChkList");

#determin if run MT. MT should not run for strong named assembly
$skipMT = 0;
if(defined($ENV{"SKIPMT"})){
	$skipMT = 1;
}
DebugPrint(__LINE__, "SKIP MT");

#remove ngenout.txt
if(-e "ngenout.txt"){
	unlink "ngenout.txt";
}

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
    if($arg=~m/^\//)
    {
	#argument is a switch, do nothing.  
	DebugPrint(__LINE__, "Ignoring switch: \"" . $arg . "\".\n");
    }
    else
    {
	# Remove if there are any tools don't need to be ngen-d.
	# If an argument is an exe and is not in the current directory,
	# it is assumed to be tools and removeTools will return 1.
	# In that case, the tools are not pushed into the ngenargs and 
	# will not be ngened.Some of the common tools are transfer, stopit,
	# tlbimp, etc
	
	# add .exe to the arg if the extension is omitted
	my @dirs = split(/\\/, $arg);
	my $filename = pop(@dirs);
	my ($name, $ext) = split(/.*\./, $filename); 
	if (!$ext || $ext eq ""){
		if($quotedArg ne "") {
			$quotedArg="\"".$arg."\.exe\"";
		}
		$arg=$arg.".exe";
	}

	if( RemoveTools($arg) == 0 ) {
		# Remove any non CLR PE files and netmodules.
		my $assemblyName = ($quotedArg eq "") ? $arg : $quotedArg;
		if( removeNonAssembly($assemblyName) == 0 ) {
			# pushing on the UNQUOTED version to make later manipulations easier
			push @ngenargs, $arg;
		}
	}
	else{
		$isTool=1;
	}
    }
}
#Remove the last trailing space added in the above loop
$cmd=~ s/\s$//;

#
#Add any dlls in the keep.lst to the combinedArgs
#combinedArgs = commandlineargs + keep.lst
#It is necessary to separate combinedArgs and ngenArgs since it tries
#not to delete the ngen image for dlls in the keep.lst
#
push @combinedArgs, @ngenargs;
ngenKeepLst(\@combinedArgs);

# Running ngen.exe, if there's no argument to ngen, no need to run ngen.exe
# If it is a dll, detect if there is DllMain. If it is, do not ngen because it could cause the ngen to hang
for (@combinedArgs) {
	$arg=$_;

#args contains space may be already quoted
$quotedArg="";
if (!($arg=~m/^\".*\"$/)){	
	if($arg=~m/\s+/){
		$quotedArg="\"".$arg."\"";
	}
}


	#Make sure complus_zaprequire is 0 before ngen
	if(defined($ENV{"COMPLUS_ZAPREQUIRE"})){
		$oldZapRequireValue=$ENV{"COMPLUS_ZAPREQUIRE"};
		$ENV{"COMPLUS_ZAPREQUIRE"}="";
	}

	if($arg=~m/\.dll/i){
		my $dllArg = ($quotedArg eq "") ? $arg : $quotedArg;
		
		if (DetectDllMain($dllArg)){
			print "Calling managed 'DllMain': Managed code may not be run under loader lock\n";
			print "$dllArg is deleted to force test fail\n";
			
			# delete the dll to force test fail
			unlink $dllArg;
			exit(1);
		}
	}
	
	#using MT to embed manifest
	$mtRetCode = 0;
	if(!$skipMT && $arg=~m/\.dll/i){
		my $dllArg = ($quotedArg eq "") ? $arg : "\"$arg\"";
		my $dllManifest = ($quotedArg eq "") ? $arg.".manifest" : "\"".$arg.".manifest\"";
		if(-e $dllManifest){
			$mtcmd="$UNIFWDCMD mt /nologo /outputresource:$dllArg;2 /manifest $dllManifest > ngenout.txt 2>&1";	
			DebugPrint(__LINE__, "$mtcmd");
			$mtRetCode=system($mtcmd) >>8;
		}
	}
	
	my $preRetCode = 0;
	if ($ngenswitches =~/install/i && $ENV{'COMPLUS_NGEN_JITNAME'} =~ 'msphxjit.dll'){
		# if ngen install is used, try pre install with '/legacy' to grap assert
		my $newNgenSwitch = $ngenswitches;
 		$newNgenSwitch =~ s/install//;
		if($quotedArg eq "") {
			
			$ngen = "$ENV{ADMIN_PIPE} $UNIFWDCMD ngen.exe /legacy $newNgenSwitch /nologo /silent $arg  >> ngenout.txt 2>&1"; 
		} else {
			$ngen = "$ENV{ADMIN_PIPE} $UNIFWDCMD ngen.exe /legacy $newNgenSwitch /nologo /silent $quotedArg  >> ngenout.txt 2>&1";
		}
		$preRetCode=system($ngen) >>8;
	}
	if($quotedArg eq "") {
		$ngen = "$ENV{ADMIN_PIPE} $UNIFWDCMD ngen.exe $ngenswitches /nologo /silent $arg  >> ngenout.txt 2>&1"; 
	} else {
		$ngen = "$ENV{ADMIN_PIPE} $UNIFWDCMD ngen.exe $ngenswitches /nologo /silent $quotedArg  >> ngenout.txt 2>&1";
	}
	DebugPrint(__LINE__,"$ngen");
	
	$retCode=system($ngen) >>8;

	#Restore the old zaprequire setting.
	if(defined($ENV{"COMPLUS_ZAPREQUIRE"})){
		$ENV{"COMPLUS_ZAPREQUIRE"}=$oldZapRequireValue;
	}
	
	$assertFound = system("findstr /i  \"ASSERT\"  ngenout.txt > NUL") == 0? 1 : 0;
	if(($preRetCode != 0) || ($retCode != 0) || $assertFound) {
		if($mtRetCode != 0)
		{
			print "$mtcmd failed\nretCode=$mtRetCode\n";
		}
		system("type ngenout.txt");
		print "$ngen failed\nretCode=$retCode\n";

		#It is possible that ngen cache is created even though ngen failed. Clean the ngencache anyway
		$arg=($quotedArg eq "")?$arg : $quotedArg;
		DebugPrint(__LINE__, "DeleteNgen $arg");		
		DeleteNgen($arg);
		exit($retCode);
	}
}
unlink "ngenout.txt";

# Running the tools/test executable
$ENV{"COMPLUS_ZapRequire"}=$zapRequireLevel;

if($GCStressLevel ne '')
{	
		$ENV{COMPLUS_GCSTRESS}=$GCStressLevel;
}
DebugPrint(__LINE__,"Running $cmd");
$retCode = (run_cmd($cmd));
DebugPrint(__LINE__, "retCode=$retCode");
system("echo $retCode >ngen_retCode.txt");

if($GCStressLevel ne '')
{	
		$ENV{COMPLUS_GCSTRESS}='';
}

#if there is no tool in arguments passed to ngenrun.pl,  delete ngen cache
if(!defined($ENV{"NGEN_NOCLEAN"})&& !$isTool){
	for (@ngenargs) {
		$arg=$_;
		$arg = ($arg =~ m/\s+/) ? "\"".$arg."\"" : $arg;
		DebugPrint(__LINE__, "DeleteNgen $arg");		
		DeleteNgen($arg);
	}
} else {
	# Store the ngen list 
	if(open NGENLIST, ">>ngened_files.txt" ){
		for (@ngenargs) {
			$arg=$_;
			my $abspath = File::Spec->rel2abs( $arg );
			$abspath = ($abspath =~ m/\s+/) ? "\"".$abspath."\"" : $abspath;
			if(!defined($keepFileHash{$abspath})){
				print NGENLIST  "$ngendeleteswitches,$UNIFWDCMD,$zapSet,$abspath\n";
			}
		}
		close NGENLIST;
	}
	else {
		DebugPrint(__LINE__, "unable to open ngened_files.txt");
	}	
}
exit($retCode);


sub DeleteNgen {
	my $param = shift;
	$ngen = "$ENV{ADMIN_PIPE} $UNIFWDCMD ngen.exe  $ngendeleteswitches /nologo /silent $param  > NUL"; 
	DebugPrint(__LINE__, $ngen);
	system($ngen);
}


sub RemoveTools {
	my $param = shift;
	my $bResult=1;

	#if it is unicode test, forward the call to UniRemoveTools
	if ($UNITEST == 1){
		return UniRemoveTools($param);
	}
	
	# See if the parameter is a file that exists in the current directory
	if( -B $param ) {
		$bResult=0;
	} 

	# Flag to run the tool before the ngen.exe run
	DebugPrint(__LINE__, "RemoveTools $param returns $bResult");
	return $bResult;
}

sub UniRemoveTools {
	my $param = shift;
	my $bResult=1;

	if($param=~/\s+/) {
		$param = "\"$param\"";
	}
	if (system("$UNIFWDCMD unidir $param")==0) {
		$bResult=0;
	}

	DebugPrint(__LINE__, "UniRemoveTools returns $bResult");
	return $bResult;
}

sub removeNonAssembly {
	my $param = shift;
	my $retVal=0;
	my $command;
	my $ngenPlatform='';
	my $imagePlatform='';
	my $ngenExePath='';
	$command= "$UNIFWDCMD IsAssembly $param  > NUL 2>&1";
	if( (system($command) >> 8) != 0 ) {
		$retVal=1;
		DebugPrint(__LINE__, "IsAssembly.exe $param return false. It is not a valid assembly.");
	}
	#If the image is not platform agnostic, determine if the image in on the right platform. It does so 
	#by comparing the platform of the ngen.exe and the image.
	elsif(!isAgnostic($param)){	
		$command="$UNIFWDCMD link.exe /dump /headers \"$param\" | findstr/i machine.*(.*) ";
		$imagePlatform=`$command`;
		
		#get the path to ngen.exe
		open(PS, "whereis ngen.exe|");
		$ngenExePath=<PS>;
		close PS;		
		
		chomp $ngenExePath;	
		$command="link.exe /dump /headers \"$ngenExePath\" 2>&1 | findstr/i machine.*(.*) ";
		$ngenPlatform=`$command`;
		if($ngenPlatform ne "") {
			if ($ngenPlatform ne $imagePlatform ){
					$retVal=1;
					DebugPrint(__LINE__, "$param is not on the right platform");
			}
			else {
					DebugPrint(__LINE__, "$param is  on the right platform");
			}		
		}
		else {
			DebugPrint(__LINE__, "Can not determine $param\'s platform");
		}
	}
	DebugPrint(__LINE__, "removeNonAssembly $param returns $retVal");
	return $retVal;
}

sub isAgnostic {
	my $param=shift;
	my $retVal = 1;
	open(PS, "$UNIFWDCMD corflags.exe /nologo \"$param\"|" );
	while(<PS>){
		#if CLRHEADER  is less than 2 (Everett)  it can not be agnostic
		if($_=~m/^CLR Header.*(\d+)\.(\d+)$/){
			if($1 < 2) {
				DebugPrint(__LINE__, "$param CLRVERSION is $1.$2.");
				$retVal=0;
				last;
			}
		}
		#if it is PE32+, it is not agnostic
		elsif($_=~m/^PE.*PE32\+$/){
			DebugPrint(__LINE__, "$param is PE32+");
			$retVal=0;
			last;
		}
		#If ILONLY IS 0, It is not agnostic
		elsif ($_=~m/^ILONLY.*0$/){
			$retVal=0;
			DebugPrint(__LINE__, "$param ILONLY =1");
			last;
		}
		#If it is 32BIT is 1, it is not agnostic
		elsif($_=~m/^32BIT.*1$/){
			$retVal=0;
			DebugPrint(__LINE__, "$param 32Bit =1");
			last;
		}
	}
	close PS;
	DebugPrint(__LINE__, "IsAgnostic $param returns $retVal");
	return $retVal;
}

sub DetectDllMain{
	my $param = shift;
	my $command="";
	$command = "isManaged.exe $param DllMain > NUL ";
	if( (system($command) >> 8) == 100 ) {
		return 1;
	}
	return 0;
}

#taken from runall.pl
#this approach would spawn process with an exitcode more than 8 bits
sub run_cmd($){
	my $cmd = shift;
	my ($proc, $retval);
	$retval=0;

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
		$proc->GetExitCode($retval);
	}
	return $retval;
}

#
# The following is added for the csharp suite which does not use %simulator_pipe% transfer
# This script will check for keep.lst.  If it exists, it will ngen any dll that is listed in it.
#
sub ngenKeepLst(@) {
	my $ngenArgRef=shift;
	my $fileName='';
	my $dllName='';
	my $quotedDllName='';
	my @files;
	# See if there is a keep.lst file in the 
	if ( -e "keep.lst" ){
		if( open KEEPF, "keep.lst"){
			while(<KEEPF>){
				chomp;
				@files=glob $_;
				foreach $fileName (@files){
					#get the dllName
					$dllName='';
					$quotedDllName='';
					if($fileName=~m/(\")(.*\.dll)\1/i){
						$quotedDllName="\"".$2."\"";
						$dllName=$2;
					}
					elsif ($fileName=~m/(\S+\.dll)/i){
						#check if it is a valid clr image
						$dllName=$1;
					}
					#if a dllName is found, add to the ngen list if it is a valid clr image
					if($dllName ne ''){			
						my $dllNameArg= ($quotedDllName eq '') ? $dllName : $quotedDllName;
						if( (RemoveTools($dllName) == 0 ) && (removeNonAssembly($dllNameArg) ==0)){
								$dllName = File::Spec->rel2abs( $dllName );
								$dllName = ($dllName =~ m/\s+/) ? "\"".$dllName."\"" : $dllName;
								unshift @$ngenArgRef, $dllName;
								$keepFileHash{$dllName}=1;
						}
					}
				}
			}
		}
		close(KEEPF);		
	}
	return;
}

#
# DebugPrint -- print debug message if _Debug= 1
#
sub DebugPrint($$) {
	my ($line, $msg) = @_;
	if($_DEBUG){
		print "[ngenrun.pl:$line] $msg\n";
	}
}

#
# print_help -- print short help message
#
sub print_help
{
    print <<ENDHELP;
ngenrun.pl <command>
It parses the parameters passed in. Exe/dlls are ngened if the following conditions are met:
1. It is not a tool. Determined by that it is in the current directory.
2. It is a valid managed assembly. Determined by IsAssembly return 0.
After the exes/dlls are ngened, it sets complus_zaprequire=2 and executes the command.
Environment Variables used by ngenrun.pl.
_Debug=1 					Print Debug Messages
ZapRequire_Level=0|1|2|3			Overwrite the default Zaprequire level
CCOMPLUS_ZapRequireExcludeList=assemblyA...     Specify the assemblies that are excluded from zaprequire checking
ChkCRTZap=1					Require msvcm80(d).dll to be ngened
UNICODE_NGEN_TEST=1				It is unicode test. Uses unifwd.exe
NGEN_SWITCHES					Overwrite switches passed to ngen
NGEN_NOCLEAN=1					Do not clean ngened images
SKIPMT=1					Do not embeded manifest
COMPLUS_ZAPSET		Set zapset used 
GCStress_Level		Set the gcstress level
ENDHELP
return
}
