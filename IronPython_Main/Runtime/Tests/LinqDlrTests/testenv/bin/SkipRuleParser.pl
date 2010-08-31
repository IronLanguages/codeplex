$skiprulefile       = $ARGV[0];
$testroot           = $ARGV[1];
$current_context    = $ARGV[2];
$current_targetarch = $ARGV[3];
$current_osarch     = $ARGV[4];
$current_PRODUCTFLAVOUR  = $ARGV[5];

$processing_rule = 0;	# 1 if we are processing a rule
			# A rule start when a # is found


open INPUT, $skiprulefile;

while(<INPUT>) {
   chomp;

   # Skip comments, i.e. a line that starts with #
   next if($_ =~ /^#/);		

   # New rule? (a line that starts with .)
   if($_ =~ /^\.(.*)/) {
       if($processing_rule==1) {
           emit_rule();
       }

       $processing_rule = 1;

       # cleanup old rule
       $path="";
       $test="";
       $targetarch="";
       $osarch="";
       $context="";
       $comment="";
       $PRODUCTFLAVOUR="";

       $rule_name = $1;
   }

   if($processing_rule) {
       $path       = $1 if(/PATH=(.+)/);
       $test       = $1 if(/TEST=(.+)/);
       $targetarch = $1 if(/TARGETARCH=(.+)/);
       $osarch     = $1 if(/OSARCH=(.+)/);
       $context    = $1 if(/CONTEXT=(.+)/);
       $comment    = $1 if(/COMMENT=(.+)/);
       $PRODUCTFLAVOUR  = $1 if(/PRODUCTFLAVOUR=(.+)/);
   }

}

# Were we processing a rule? If so, emit it.
if($processing_rule==1) {
  emit_rule();
}

close(INPUT);

# Escape characters with special meaning in RegEx
sub escape {
   ($s) = @_;
   $_ = $s;
   s/\\/\\\\/;
   s/\|/\\\|/;
   s/\(/\\\(/;
   s/\)/\\\)/;
   s/\[/\\\[/;
   s/\{/\\\{/;
   s/\^/\\\^/;
   s/\$/\\\$/;
   s/\*/\\\*/;
   s/\+/\\\+/;
   s/\?/\\\?/;
   s/\./\\\./;
   return $_;
}

sub emit_rule {
    my(@ta, @co, @comp, $does_not_match,$fullpath,@fl);
    @ta   = split /\|/, $targetarch;
    @co   = split /\|/, $context;
    @os   = split /\|/, $osarch;
    @cscfl= split /\|/, $PRODUCTFLAVOUR;
    $does_not_match = 1;
    $processing_rule=0;

#print "ta= @ta\n";
#print "co= @co\n";
#print "current_targetarch = $current_targetarch\n";
#print "current_context = $current_context\n";

    # context matches?
    $match_count=0;
    foreach $c (@co) {
        $match_count++ if ($current_context =~ escape($c));
    }
#print "$match_count\n";
    return if($match_count==0 && @co>0);

    # target architecture matches?
    $match_count=0;
    foreach $c (@ta) {
        $match_count++ if (uc($current_targetarch) =~ uc($c));
    }
#print "$match_count\n";
    return if($match_count==0 && @ta>0);

    # os architecture matches?
    $match_count=0;
    foreach $c (@os) {
        $match_count++ if (uc($current_osarch) =~ uc($c));
    }
#print "$match_count\n";
    return if($match_count==0 && @os>0);


    # csc flavor matches?
    $match_count=0;
    foreach $c (@cscfl) {
        $match_count++ if (uc($current_PRODUCTFLAVOUR) =~ uc($c));
    }
#print "$match_count\n";
    return if($match_count==0 && @cscfl>0);


    # If $current_context is empty, we do not want the colon (:) before
    # the test name (this is mandated by runall.pl)
    $fullpath = ($testroot eq "") ? "$path" : "$testroot\\$path";
    #if($current_context eq "") {
        print "$fullpath ($test) -- $comment ($rule_name)\n";
    #} else {
    #    print "$fullpath ($current_context:$test) -- $comment ($rule_name)\n";
    #}
}