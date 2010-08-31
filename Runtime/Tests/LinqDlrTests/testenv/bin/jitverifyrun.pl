# Copyright (c) 2004, Microsoft Corporation
# 2004/12/1 - a-samitt - created
# 2006/07/17 - BeBrinck - Fixed a bug where the arguments to the original command would not be passed to the remotedly-executed command

use Cwd;

# Check for any compiler flags


my $SCFLAGS;

if (not $SCFLAGS = $ENV{SCFLAGS}) 
{
	$SCFLAGS = "";
}


# Check for any global compiler flags

my $iSCFLAGS;

my $ExpectedToFail = false;

if (not $iSCFLAGS = $ENV{iSCFLAGS}) 
{
	$iSCFLAGS = "/debug+ /o+";
}


$_ = getcwd();

# replacing : with $ and / with \

s/\:/\$/;

s/\//\\/g;

$_="\\\\".$ENV{"COMPUTERNAME"}."\\".$_."\\".join(" ",@ARGV);

open(CMD,"$_ |");

while(<CMD>)
{
	print;
}

close(CMD);

$retval = $?;

if ("$iSCFLAGS $SCFLAGS" =~ /\/unsafe/)
{
	$ExpectedToFail = true;	
	
	if ($retval ==0)
	{
		exit(1);
	}
	else
	{
		exit(0);
	}

}


exit ($retval>>8);