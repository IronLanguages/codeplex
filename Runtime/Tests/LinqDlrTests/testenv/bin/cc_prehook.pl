&prehook();

sub prehook {
    my($cc_cmd);
    $cc_cmd = "covercmd.exe /reset";
    system("$cc_cmd");
}

