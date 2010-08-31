#
# Hook to be used to export binaries
# E.g.
#    perl -S runall.pl -posthook bin_exp_posthook.pl
# Note: fix BIN_EXP_DESTROOT below before running it.
#
#

&posthook();

sub posthook {

    my($dirx, $testx, $dirfp);

    $ENV{BIN_EXP_DESTROOT} = "F:\\CompQA\\csharp\\Source" unless defined $ENV{BIN_EXP_DESTROOT};
    $ENV{BIN_EXP_CURRDIR} = "" unless defined $ENV{BIN_EXP_CURRDIR};
    $ENV{BIN_EXP_COUNTER} = 0 unless defined $ENV{BIN_EXP_COUNTER};

    $_ = $test->[2] . $label;
    /^(.+) \((.+)\)/;
    $dirx=$1; $testx=$2;

    if ($ENV{BIN_EXP_CURRDIR} ne $dirx) {
        $ENV{BIN_EXP_COUNTER} = 1; 			# Reset counter if new folder
	$ENV{BIN_EXP_CURRDIR} = $dirx;
    } else {
       $ENV{BIN_EXP_COUNTER}++;
    }

    $dirfp = "$ENV{BIN_EXP_DESTROOT}\\$dirx\\$ENV{BIN_EXP_COUNTER}";
    system("mkdir \"$dirfp\"");					# Yes, it will handle spaces correctly :)
    system("for /F \%i IN ('dir *.* /b /A-R') DO \@copy /y \%i \"$dirfp\\\"");
    system("del \"$dirfp\\*.pdb\"");
}

