#runall posthook used for ngenrun. 
use strict;
my $ngen;
my $_DEBUG=0;
#determine ngen delete switches
my $ngendeleteswitches="/delete";
my $UNIFWDCMD='';
my $zapset='';
my $filePath='';
my $FrameworkDir=quotemeta "$ENV{'windir'}\\Microsoft.NET\\Framework";

if($ENV{"_DEBUG"} == 1 ){
	$_DEBUG=1;
}

if(-e "ngened_files.txt"){
	open(NGENF, "<ngened_files.txt");
	while(<NGENF>) {
		chop;
	 ($ngendeleteswitches, $UNIFWDCMD, $zapset,$filePath )=split(',',$_);
		unless($zapset eq '')
		{
	 		DebugPrint(__LINE__, "ZapSet=$zapset");
	 		$ENV{"COMPLUS_ZAPSET"}=$zapset;
		}
		unless($filePath=~m/$FrameworkDir/i){
			$ngen = "$ENV{ADMIN_PIPE} $UNIFWDCMD ngen.exe  $ngendeleteswitches /nologo /silent $filePath > NUL"; 
			DebugPrint(__LINE__, "$ngen");
			system($ngen);
		}
		else{
			DebugPrint(__LINE__, "$filePath is a system dll and is not deleted.");
		}
		
	}
	close(NGENF);
	unlink("ngened_files.txt");
}

#
# DebugPrint -- print debug message if _Debug= 1
#
sub DebugPrint($$) {
	my ($line, $msg) = @_;
	if($_DEBUG){
		print "[ngenrun_posthook:$line] $msg\n";
	}
}

