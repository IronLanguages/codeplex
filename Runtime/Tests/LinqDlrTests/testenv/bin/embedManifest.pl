# embedManifest.pl
# 

use strict;

usage() if @ARGV != 1;

my $embedfile = $ARGV[0]; # file to embed manifest into
my $resourceid = 1; # default is exe mode

if($embedfile =~ '\.dll'){ $resourceid = 2; } # if we're doing a DLL, it's in resource 2
if($embedfile =~ '\.netmodule') { $resourceid = 2; }  # likewise for .netmodule

if (not -e "$embedfile.manifest"){
  print "Warning: Manifest file not found!\n";
  exit(0);
}

my $exeline = "mt -outputresource:$embedfile;#$resourceid -manifest $embedfile" . ".manifest";
print "$exeline\n";

my $retval = system("$exeline") >>8;

if($retval){
  print "Error: Embedding the manifest failed!\n";
  exit($retval);
}

print "Success: Manifest embedded.\n";

exit(0);

sub usage()
{
        die "usage: embedManifest.pl <file>\n";
}
