&posthook();

sub posthook {
    my($marathon_comp_id) = $ENV{CSCQA_COMPONENTID};	# 1=CSharp,2=Alink,3=CSEE,4=JSC,5=ASPX
    my($marathon_iscflags) = $ENV{iSCFLAGS};		# /debug+ /o+, etc...

    my($cc_tracename);
    my($cc_custver) = $ENV{"CC_CUSTOMVER"};		# Code coverage custom version (something like lab22dev.50727.42)
    my($cc_cmd);

    $_ = $test->[2] . $label;
    /^(.+) \((.+)\)/;
    $cc_tracename = ":3.1.$marathon_comp_id.$_:($marathon_iscflags)";

    $cc_cmd = "covercmd.exe /save /as \"$cc_tracename\" /CustomVer \"$cc_custver\"";
    system("$cc_cmd");

    # In "Maddog" runs (X_MODE=RELEASE), we let Maddog collect the data
    # In "Marathon" runs (X_MODE!=RELEASE), we collect the traces incrementally
    if( ($ENV{"X_MODE"} ne "RELEASE") && 
        ((++$ENV{"CC_NUMTRACES"} % 2000) == 0)) {
        system("$ENV{TESTROOT}\\testenv\\bin\\CC_CollectData.bat");
        system("$ENV{TESTROOT}\\testenv\\bin\\CC_DeleteData.bat");
    }

}

