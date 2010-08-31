################################################################################
#
# Perl module: XML::DOM
#
# By Enno Derksen (official maintainer), enno@att.com
# and Clark Cooper, coopercl@sch.ge.com
#
################################################################################
#
# To do:
#
# * BUG: setOwnerDocument - does not process default attr values correctly,
#   they still point to the old doc.
# * change Exception mechanism
# * entity expansion
# * maybe: more checking of sysId etc.
# * NoExpand mode (don't know what else is useful)
# * various odds and ends: see comments starting with "??"
# * normalize(1) should also expand CDataSections and EntityReferences
# * parse a DocumentFragment?
# * encoding support
# * someone reported an error that an Entity or something contained a single
#   quote and it printed ''' or something...
#
######################################################################

package Stat;
#?? Debugging class - remove later

sub cnt
{
    $cnt{$_[0]}++;
}

sub print
{
    for (keys %cnt)
    {
	print "$_: " . $cnt{$_} . "\n";
    }
}

######################################################################
package XML::DOM;
######################################################################

use strict;
use vars qw( $VERSION @ISA @EXPORT
	     $IgnoreReadOnly $SafeMode $TagStyle
	     %DefaultEntities %DecodeDefaultEntity
	     $ChBaseChar $ChIdeographic
	     $ChLetter $ChDigit $ChExtender $ChCombiningChar $ChNameChar 
	     $ReName $ReNmToken $ReEntityRef $ReCharRef $ReReference $ReAttValue
	   );
use Carp;

BEGIN
{
    require XML::Parser;
    $VERSION = '1.25';

    my $needVersion = '2.23';
    die "need at least XML::Parser version $needVersion (current=" .
		$XML::Parser::VERSION . ")"
	unless $XML::Parser::VERSION >= $needVersion;

    @ISA = qw( Exporter );
    @EXPORT = qw(
	     UNKNOWN_NODE
	     ELEMENT_NODE
	     ATTRIBUTE_NODE
	     TEXT_NODE
	     CDATA_SECTION_NODE
	     ENTITY_REFERENCE_NODE
	     ENTITY_NODE
	     PROCESSING_INSTRUCTION_NODE
	     COMMENT_NODE
	     DOCUMENT_NODE
	     DOCUMENT_TYPE_NODE
	     DOCUMENT_FRAGMENT_NODE
	     NOTATION_NODE
	     ELEMENT_DECL_NODE
	     ATT_DEF_NODE
	     XML_DECL_NODE
	     ATTLIST_DECL_NODE
	    );
}

#---- Constant definitions

# Node types

sub UNKNOWN_NODE                () {0;}		# not in the DOM Spec

sub ELEMENT_NODE                () {1;}
sub ATTRIBUTE_NODE              () {2;}
sub TEXT_NODE                   () {3;}
sub CDATA_SECTION_NODE          () {4;}
sub ENTITY_REFERENCE_NODE       () {5;}
sub ENTITY_NODE                 () {6;}
sub PROCESSING_INSTRUCTION_NODE () {7;}
sub COMMENT_NODE                () {8;}
sub DOCUMENT_NODE               () {9;}
sub DOCUMENT_TYPE_NODE          () {10;}
sub DOCUMENT_FRAGMENT_NODE      () {11;}
sub NOTATION_NODE               () {12;}

sub ELEMENT_DECL_NODE		() {13;}	# not in the DOM Spec
sub ATT_DEF_NODE 		() {14;}	# not in the DOM Spec
sub XML_DECL_NODE 		() {15;}	# not in the DOM Spec
sub ATTLIST_DECL_NODE		() {16;}	# not in the DOM Spec

#
# Definitions of the character classes and regular expressions as defined in the
# XML Spec.
# 
# NOTE: ChLetter maps to the 'Letter' definition in the XML Spec.
#

$ChBaseChar = '(?:[a-zA-Z]|\xC3[\x80-\x96\x98-\xB6\xB8-\xBF]|\xC4[\x80-\xB1\xB4-\xBE]|\xC5[\x81-\x88\x8A-\xBE]|\xC6[\x80-\xBF]|\xC7[\x80-\x83\x8D-\xB0\xB4\xB5\xBA-\xBF]|\xC8[\x80-\x97]|\xC9[\x90-\xBF]|\xCA[\x80-\xA8\xBB-\xBF]|\xCB[\x80\x81]|\xCE[\x86\x88-\x8A\x8C\x8E-\xA1\xA3-\xBF]|\xCF[\x80-\x8E\x90-\x96\x9A\x9C\x9E\xA0\xA2-\xB3]|\xD0[\x81-\x8C\x8E-\xBF]|\xD1[\x80-\x8F\x91-\x9C\x9E-\xBF]|\xD2[\x80\x81\x90-\xBF]|\xD3[\x80-\x84\x87\x88\x8B\x8C\x90-\xAB\xAE-\xB5\xB8\xB9]|\xD4[\xB1-\xBF]|\xD5[\x80-\x96\x99\xA1-\xBF]|\xD6[\x80-\x86]|\xD7[\x90-\xAA\xB0-\xB2]|\xD8[\xA1-\xBA]|\xD9[\x81-\x8A\xB1-\xBF]|\xDA[\x80-\xB7\xBA-\xBE]|\xDB[\x80-\x8E\x90-\x93\x95\xA5\xA6]|\xE0(?:\xA4[\x85-\xB9\xBD]|\xA5[\x98-\xA1]|\xA6[\x85-\x8C\x8F\x90\x93-\xA8\xAA-\xB0\xB2\xB6-\xB9]|\xA7[\x9C\x9D\x9F-\xA1\xB0\xB1]|\xA8[\x85-\x8A\x8F\x90\x93-\xA8\xAA-\xB0\xB2\xB3\xB5\xB6\xB8\xB9]|\xA9[\x99-\x9C\x9E\xB2-\xB4]|\xAA[\x85-\x8B\x8D\x8F-\x91\x93-\xA8\xAA-\xB0\xB2\xB3\xB5-\xB9\xBD]|\xAB\xA0|\xAC[\x85-\x8C\x8F\x90\x93-\xA8\xAA-\xB0\xB2\xB3\xB6-\xB9\xBD]|\xAD[\x9C\x9D\x9F-\xA1]|\xAE[\x85-\x8A\x8E-\x90\x92-\x95\x99\x9A\x9C\x9E\x9F\xA3\xA4\xA8-\xAA\xAE-\xB5\xB7-\xB9]|\xB0[\x85-\x8C\x8E-\x90\x92-\xA8\xAA-\xB3\xB5-\xB9]|\xB1[\xA0\xA1]|\xB2[\x85-\x8C\x8E-\x90\x92-\xA8\xAA-\xB3\xB5-\xB9]|\xB3[\x9E\xA0\xA1]|\xB4[\x85-\x8C\x8E-\x90\x92-\xA8\xAA-\xB9]|\xB5[\xA0\xA1]|\xB8[\x81-\xAE\xB0\xB2\xB3]|\xB9[\x80-\x85]|\xBA[\x81\x82\x84\x87\x88\x8A\x8D\x94-\x97\x99-\x9F\xA1-\xA3\xA5\xA7\xAA\xAB\xAD\xAE\xB0\xB2\xB3\xBD]|\xBB[\x80-\x84]|\xBD[\x80-\x87\x89-\xA9])|\xE1(?:\x82[\xA0-\xBF]|\x83[\x80-\x85\x90-\xB6]|\x84[\x80\x82\x83\x85-\x87\x89\x8B\x8C\x8E-\x92\xBC\xBE]|\x85[\x80\x8C\x8E\x90\x94\x95\x99\x9F-\xA1\xA3\xA5\xA7\xA9\xAD\xAE\xB2\xB3\xB5]|\x86[\x9E\xA8\xAB\xAE\xAF\xB7\xB8\xBA\xBC-\xBF]|\x87[\x80-\x82\xAB\xB0\xB9]|[\xB8\xB9][\x80-\xBF]|\xBA[\x80-\x9B\xA0-\xBF]|\xBB[\x80-\xB9]|\xBC[\x80-\x95\x98-\x9D\xA0-\xBF]|\xBD[\x80-\x85\x88-\x8D\x90-\x97\x99\x9B\x9D\x9F-\xBD]|\xBE[\x80-\xB4\xB6-\xBC\xBE]|\xBF[\x82-\x84\x86-\x8C\x90-\x93\x96-\x9B\xA0-\xAC\xB2-\xB4\xB6-\xBC])|\xE2(?:\x84[\xA6\xAA\xAB\xAE]|\x86[\x80-\x82])|\xE3(?:\x81[\x81-\xBF]|\x82[\x80-\x94\xA1-\xBF]|\x83[\x80-\xBA]|\x84[\x85-\xAC])|\xEA(?:[\xB0-\xBF][\x80-\xBF])|\xEB(?:[\x80-\xBF][\x80-\xBF])|\xEC(?:[\x80-\xBF][\x80-\xBF])|\xED(?:[\x80-\x9D][\x80-\xBF]|\x9E[\x80-\xA3]))';

$ChIdeographic = '(?:\xE3\x80[\x87\xA1-\xA9]|\xE4(?:[\xB8-\xBF][\x80-\xBF])|\xE5(?:[\x80-\xBF][\x80-\xBF])|\xE6(?:[\x80-\xBF][\x80-\xBF])|\xE7(?:[\x80-\xBF][\x80-\xBF])|\xE8(?:[\x80-\xBF][\x80-\xBF])|\xE9(?:[\x80-\xBD][\x80-\xBF]|\xBE[\x80-\xA5]))';

$ChDigit = '(?:[0-9]|\xD9[\xA0-\xA9]|\xDB[\xB0-\xB9]|\xE0(?:\xA5[\xA6-\xAF]|\xA7[\xA6-\xAF]|\xA9[\xA6-\xAF]|\xAB[\xA6-\xAF]|\xAD[\xA6-\xAF]|\xAF[\xA7-\xAF]|\xB1[\xA6-\xAF]|\xB3[\xA6-\xAF]|\xB5[\xA6-\xAF]|\xB9[\x90-\x99]|\xBB[\x90-\x99]|\xBC[\xA0-\xA9]))';

$ChExtender = '(?:\xC2\xB7|\xCB[\x90\x91]|\xCE\x87|\xD9\x80|\xE0(?:\xB9\x86|\xBB\x86)|\xE3(?:\x80[\x85\xB1-\xB5]|\x82[\x9D\x9E]|\x83[\xBC-\xBE]))';

$ChCombiningChar = '(?:\xCC[\x80-\xBF]|\xCD[\x80-\x85\xA0\xA1]|\xD2[\x83-\x86]|\xD6[\x91-\xA1\xA3-\xB9\xBB-\xBD\xBF]|\xD7[\x81\x82\x84]|\xD9[\x8B-\x92\xB0]|\xDB[\x96-\xA4\xA7\xA8\xAA-\xAD]|\xE0(?:\xA4[\x81-\x83\xBC\xBE\xBF]|\xA5[\x80-\x8D\x91-\x94\xA2\xA3]|\xA6[\x81-\x83\xBC\xBE\xBF]|\xA7[\x80-\x84\x87\x88\x8B-\x8D\x97\xA2\xA3]|\xA8[\x82\xBC\xBE\xBF]|\xA9[\x80-\x82\x87\x88\x8B-\x8D\xB0\xB1]|\xAA[\x81-\x83\xBC\xBE\xBF]|\xAB[\x80-\x85\x87-\x89\x8B-\x8D]|\xAC[\x81-\x83\xBC\xBE\xBF]|\xAD[\x80-\x83\x87\x88\x8B-\x8D\x96\x97]|\xAE[\x82\x83\xBE\xBF]|\xAF[\x80-\x82\x86-\x88\x8A-\x8D\x97]|\xB0[\x81-\x83\xBE\xBF]|\xB1[\x80-\x84\x86-\x88\x8A-\x8D\x95\x96]|\xB2[\x82\x83\xBE\xBF]|\xB3[\x80-\x84\x86-\x88\x8A-\x8D\x95\x96]|\xB4[\x82\x83\xBE\xBF]|\xB5[\x80-\x83\x86-\x88\x8A-\x8D\x97]|\xB8[\xB1\xB4-\xBA]|\xB9[\x87-\x8E]|\xBA[\xB1\xB4-\xB9\xBB\xBC]|\xBB[\x88-\x8D]|\xBC[\x98\x99\xB5\xB7\xB9\xBE\xBF]|\xBD[\xB1-\xBF]|\xBE[\x80-\x84\x86-\x8B\x90-\x95\x97\x99-\xAD\xB1-\xB7\xB9])|\xE2\x83[\x90-\x9C\xA1]|\xE3(?:\x80[\xAA-\xAF]|\x82[\x99\x9A]))';

$ChLetter	= "(?:$ChBaseChar|$ChIdeographic)";
$ChNameChar	= "(?:[-._:]|$ChLetter|$ChDigit|$ChCombiningChar|$ChExtender)";

$ReName		= "(?:(?:[:_]|$ChLetter)$ChNameChar*)";
$ReNmToken	= "(?:$ChNameChar)+";
$ReEntityRef	= "(?:\&$ReName;)";
$ReCharRef	= '(?:\&#(?:\d+|x[0-9a-fA-F]+);)';
$ReReference	= "(?:$ReEntityRef|$ReCharRef)";

#?? what if it contains entity references?
$ReAttValue     = "(?:\"(?:[^\"&<]*|$ReReference)\"|'(?:[^\'&<]|$ReReference)*')";


%DefaultEntities = 
(
 "quot"		=> '"',
 "gt"		=> ">",
 "lt"		=> "<",
 "apos"		=> "'",
 "amp"		=> "&"
);

%DecodeDefaultEntity =
(
 '"' => "&quot;",
 ">" => "&gt;",
 "<" => "&lt;",
 "'" => "&apos;",
 "&" => "&amp;"
);

sub encodeCDATA
{
    my ($str) = shift;
    $str =~ s/]]>/]]&gt;/go;
    $str;
}

#
# PI may not contain "?>"
#
sub encodeProcessingInstruction
{
    my ($str) = shift;
    $str =~ s/\?>/?&gt;/go;
    $str;
}

#
#?? Not sure if this is right - must prevent double minus somehow...
#
sub encodeComment
{
    my ($str) = shift;
    return undef unless defined $str;

    $str =~ s/--/&#45;&#45;/go;
    $str;
}

# for debugging
sub toHex
{
    my $str = shift;
    my $len = length($str);
    my @a = unpack ("C$len", $str);
    my $s = "";
    for (@a)
    {
	$s .= sprintf ("%02x", $_);
    }
    $s;
}

#
# 2nd parameter $default: list of Default Entity characters that need to be 
# converted (e.g. "&<" for conversion to "&amp;" and "&lt;" resp.)
#

sub encodeText
{
    my ($str, $default) = @_;
    return undef unless defined $str;
    
    $str =~ s/([\xC0-\xDF].|[\xE0-\xEF]..|[\xF0-\xFF]...)|([$default])|(]]>)/
	defined($1) ? XmlUtf8Decode ($1) : 
	defined ($2) ? $DecodeDefaultEntity{$2} : "]]&gt;" /egs;

#?? could there be references that should not be expanded?
# e.g. should not replace &#nn; &#xAF; and &abc;
#    $str =~ s/&(?!($ReName|#[0-9]+|#x[0-9a-fA-F]+);)/&amp;/go;

    $str;
}

# Used by AttDef - default value

sub encodeAttrValue
{
    encodeText (shift, '"&<');
}

#
# Converts an integer (Unicode - ISO/IEC 10646) to a UTF-8 encoded character 
# sequence.
# Used when converting e.g. &#123; or &#x3ff; to a string value.
#
# Algorithm borrowed from expat/xmltok.c/XmlUtf8Encode()
#
# not checking for bad characters: < 0, x00-x08, x0B-x0C, x0E-x1F, xFFFE-xFFFF

sub XmlUtf8Encode
{
    my $n = shift;
    if ($n < 0x80)
    {
	return chr ($n);
    }
    elsif ($n < 0x800)
    {
	return pack ("CC", (($n >> 6) | 0xc0), (($n & 0x3f) | 0x80));
    }
    elsif ($n < 0x10000)
    {
	return pack ("CCC", (($n >> 12) | 0xe0), ((($n >> 6) & 0x3f) | 0x80),
		     (($n & 0x3f) | 0x80));
    }
    elsif ($n < 0x110000)
    {
	return pack ("CCCC", (($n >> 18) | 0xf0), ((($n >> 12) & 0x3f) | 0x80),
		     ((($n >> 6) & 0x3f) | 0x80), (($n & 0x3f) | 0x80));
    }
    croak "number is too large for Unicode [$n] in &XmlUtf8Encode";
}

#
# Opposite of XmlUtf8Decode plus it adds prefix "&#" or "&#x" and suffix ";"
# The 2nd parameter ($hex) indicates whether the result is hex encoded or not.
#
sub XmlUtf8Decode
{
    my ($str, $hex) = @_;
    my $len = length ($str);
    my $n;

    if ($len == 2)
    {
	my @n = unpack "C2", $str;
	$n = (($n[0] & 0x3f) << 6) + ($n[1] & 0x3f);
    }
    elsif ($len == 3)
    {
	my @n = unpack "C3", $str;
	$n = (($n[0] & 0x1f) << 12) + (($n[1] & 0x3f) << 6) + 
		($n[2] & 0x3f);
    }
    elsif ($len == 4)
    {
	my @n = unpack "C4", $str;
	$n = (($n[0] & 0x0f) << 18) + (($n[1] & 0x3f) << 12) + 
		(($n[2] & 0x3f) << 6) + ($n[3] & 0x3f);
    }
    elsif ($len == 1)	# just to be complete...
    {
	$n = ord ($str);
    }
    else
    {
	croak "bad value [$str] for XmlUtf8Decode";
    }
    $hex ? sprintf ("&#x%x;", $n) : "&#$n;";
}

$IgnoreReadOnly = 0;
$SafeMode = 1;

sub getIgnoreReadOnly
{
    $IgnoreReadOnly;
}

# The global flag $IgnoreReadOnly is set to the specified value and the old 
# value of $IgnoreReadOnly is returned.
#
# To temporarily disable read-only related exceptions (i.e. when parsing
# XML or temporarily), do the following:
#
# my $oldIgnore = XML::DOM::ignoreReadOnly (1);
# ... do whatever you want ...
# XML::DOM::ignoreReadOnly ($oldIgnore);
#
sub ignoreReadOnly
{
    my $i = $IgnoreReadOnly;
    $IgnoreReadOnly = $_[0];
    return $i;
}

# XML spec seems to break its own rules... (see ENTITY xmlpio)
sub forgiving_isValidName
{
    $_[0] =~ /^$ReName$/o;
}

# Don't allow names starting with xml (either case)
sub picky_isValidName
{
    $_[0] =~ /^$ReName$/o and $_[0] !~ /^xml/i;
}

# Be forgiving by default, 
*isValidName = \&forgiving_isValidName;

sub allowReservedNames
{
    *isValidName = ($_[0] ? \&forgiving_isValidName : \&picky_isValidName);
}

sub getAllowReservedNames
{
    *isValidName == \&forgiving_isValidName;
}

# Always compress empty tags by default
# This is used by Element::print.
$TagStyle = sub { 0 };

sub setTagCompression
{
    $TagStyle = shift;
}

######################################################################
package XML::DOM::PrintToFileHandle;
######################################################################

#
# Used by XML::DOM::Node::printToFileHandle
#

sub new
{
    my($class, $fn) = @_;
    bless $fn, $class;
}

sub print
{
    my ($self, $str) = @_;
    print $self $str;
}

######################################################################
package XML::DOM::PrintToString;
######################################################################

#
# Used by XML::DOM::Node::toString to concatenate strings
#

sub new
{
    my($class) = @_;
    my $str = "";
    bless \$str, $class;
}

sub print
{
    my ($self, $str) = @_;
    $$self .= $str;
}

sub toString
{
    my $self = shift;
    $$self;
}

sub reset
{
    ${$_[0]} = "";
}

*Singleton = \(new XML::DOM::PrintToString);

######################################################################
package XML::DOM::DOMException;
######################################################################

use Exporter;
use overload '""' => \&stringify;
use vars qw ( @ISA @EXPORT @ErrorNames );

BEGIN
{
  @ISA = qw( Exporter );
  @EXPORT = qw( INDEX_SIZE_ERR
		DOMSTRING_SIZE_ERR
		HIERARCHY_REQUEST_ERR
		WRONG_DOCUMENT_ERR
		INVALID_CHARACTER_ERR
		NO_DATA_ALLOWED_ERR
		NO_MODIFICATION_ALLOWED_ERR
		NOT_FOUND_ERR
		NOT_SUPPORTED_ERR
		INUSE_ATTRIBUTE_ERR
	      );
}

sub UNKNOWN_ERR			() {0;}	# not in the DOM Spec!
sub INDEX_SIZE_ERR		() {1;}
sub DOMSTRING_SIZE_ERR		() {2;}
sub HIERARCHY_REQUEST_ERR	() {3;}
sub WRONG_DOCUMENT_ERR		() {4;}
sub INVALID_CHARACTER_ERR	() {5;}
sub NO_DATA_ALLOWED_ERR		() {6;}
sub NO_MODIFICATION_ALLOWED_ERR	() {7;}
sub NOT_FOUND_ERR		() {8;}
sub NOT_SUPPORTED_ERR		() {9;}
sub INUSE_ATTRIBUTE_ERR		() {10;}

@ErrorNames = (
	       "UNKNOWN_ERR",
	       "INDEX_SIZE_ERR",
	       "DOMSTRING_SIZE_ERR",
	       "HIERARCHY_REQUEST_ERR",
	       "WRONG_DOCUMENT_ERR",
	       "INVALID_CHARACTER_ERR",
	       "NO_DATA_ALLOWED_ERR",
	       "NO_MODIFICATION_ALLOWED_ERR",
	       "NOT_FOUND_ERR",
	       "NOT_SUPPORTED_ERR",
	       "INUSE_ATTRIBUTE_ERR"
	      );

sub new
{
    my ($type, $code, $msg) = @_;
    my $self = bless {Code => $code}, $type;

    $self->{Message} = $msg if defined $msg;

#    print "=> Exception: " . $self->stringify . "\n"; 
    $self;
}

sub getCode
{
    $_[0]->{Code};
}

#------------------------------------------------------------
# Extra method implementations

sub getName
{
    $ErrorNames[$_[0]->{Code}];
}

sub getMessage
{
    $_[0]->{Message};
}

sub stringify
{
    my $self = shift;

    "XML::DOM::DOMException(Code=" . $self->getCode . ", Name=" .
	$self->getName . ", Message=" . $self->getMessage . ")";
}

######################################################################
package XML::DOM::NamedNodeMap;
######################################################################

BEGIN 
{
    import Carp;
    import XML::DOM::DOMException;
}

use vars qw( $Special );

# Constant definition:
# Note: a real Name should have at least 1 char, so nobody else should use this
$Special = "";

sub new 
{
    my ($class, %args) = @_;

    $args{Values} = new XML::DOM::NodeList;

    # Store all NamedNodeMap properties in element $Special
    bless { $Special => \%args}, $class;
}

sub getNamedItem 
{
    # Don't return the $Special item!
    ($_[1] eq $Special) ? undef : $_[0]->{$_[1]};
}

sub setNamedItem 
{
    my ($self, $node) = @_;
    my $prop = $self->{$Special};

    my $name = $node->getNodeName;

    if ($XML::DOM::SafeMode)
    {
	croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR)
	    if $self->isReadOnly;

	croak new XML::DOM::DOMException (WRONG_DOCUMENT_ERR)
	    if $node->{Doc} != $prop->{Doc};

	croak new XML::DOM::DOMException (INUSE_ATTRIBUTE_ERR)
	    if defined ($node->{UsedIn});

	croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR,
		      "can't add name with NodeName [$name] to NamedNodeMap")
	    if $name eq $Special;
    }

    my $values = $prop->{Values};
    my $index = -1;

    my $prev = $self->{$name};
    if (defined $prev)
    {
	# decouple previous node
	delete $prev->{UsedIn};

	# find index of $prev
	$index = 0;
	for my $val (@{$values})
	{
	    last if ($val == $prev);
	    $index++;
	}
    }

    $self->{$name} = $node;    
    $node->{UsedIn} = $self;

    if ($index == -1)
    {
	push (@{$values}, $node);
    }
    else	# replace previous node with new node
    {
	splice (@{$values}, $index, 1, $node);
    }
    
    $prev;
}

sub removeNamedItem 
{
    my ($self, $name) = @_;

    # Be careful that user doesn't delete $Special node!
    croak new XML::DOM::DOMException (NOT_FOUND_ERR)
        if $name eq $Special;

    my $node = $self->{$name};

    croak new XML::DOM::DOMException (NOT_FOUND_ERR)
        unless defined $node;

    # The DOM Spec doesn't mention this Exception - I think it's an oversight
    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR)
	if $self->isReadOnly;

    delete $node->{UsedIn};
    delete $self->{$name};

    # remove node from Values list
    my $values = $self->getValues;
    my $index = 0;
    for my $val (@{$values})
    {
	if ($val == $node)
	{
	    splice (@{$values}, $index, 1, ());
	    last;
	}
	$index++;
    }
    $node;
}

# The following 2 are really bogus. DOM should use an iterator instead (Clark)

sub item 
{
    my ($self, $item) = @_;
    $self->{$Special}->{Values}->[$item];
}

sub getLength 
{
    my ($self) = @_;
    my $vals = $self->{$Special}->{Values};
    int (@$vals);
}

#------------------------------------------------------------
# Extra method implementations

sub isReadOnly
{
    return 0 if $XML::DOM::IgnoreReadOnly;

    my $used = $_[0]->{$Special}->{UsedIn};
    defined $used ? $used->isReadOnly : 0;
}

sub cloneNode
{
    my ($self, $deep) = @_;
    my $prop = $self->{$Special};

    my $map = new XML::DOM::NamedNodeMap (Doc => $prop->{Doc});
    # Not copying Parent property on purpose! 

    my $oldIgnore = XML::DOM::ignoreReadOnly (1);	# temporarily...

    for my $val (@{$prop->{Values}})
    {
	my $key = $val->getNodeName;

	my $newNode = $val->cloneNode ($deep);
	$newNode->{UsedIn} = $map;
	$map->{$key} = $newNode;
	push (@{$map->{$Special}->{Values}}, $newNode);
    }
    XML::DOM::ignoreReadOnly ($oldIgnore);	# restore previous value

    $map;
}

sub setOwnerDocument
{
    my ($self, $doc) = @_;
    my $special = $self->{$Special};

    $special->{Doc} = $doc;
    for my $kid (@{$special->{Values}})
    {
	$kid->setOwnerDocument ($doc);
    }
}

sub getChildIndex
{
    my ($self, $attr) = @_;
    my $i = 0;
    for my $kid (@{$self->{$Special}->{Values}})
    {
	return $i if $kid == $attr;
	$i++;
    }
    -1;	# not found
}

sub getValues
{
    wantarray ? @{ $_[0]->{$Special}->{Values} } : $_[0]->{$Special}->{Values};
}

# Remove circular dependencies. The NamedNodeMap and its values should
# not be used afterwards.
sub dispose
{
    my $self = shift;

    for my $kid (@{$self->getValues})
    {
	delete $kid->{UsedIn};
	$kid->dispose;
    }

    delete $self->{$Special}->{Doc};
    delete $self->{$Special}->{Parent};
    delete $self->{$Special}->{Values};

    for my $key (keys %$self)
    {
	delete $self->{$key};
    }
}

sub setParentNode
{
    $_[0]->{$Special}->{Parent} = $_[1];
}

sub getProperty
{
    $_[0]->{$Special}->{$_[1]};
}

#?? remove after debugging
sub toString
{
    my ($self) = @_;
    my $str = "NamedNodeMap[";
    while (my ($key, $val) = each %$self)
    {
	if ($key eq $Special)
	{
	    $str .= "##Special (";
	    while (my ($k, $v) = each %$val)
	    {
		if ($k eq "Values")
		{
		    $str .= $k . " => [";
		    for my $a (@$v)
		    {
#			$str .= $a->getNodeName . "=" . $a . ",";
			$str .= $a->toString . ",";
		    }
		    $str .= "], ";
		}
		else
		{
		    $str .= $k . " => " . $v . ", ";
		}
	    }
	    $str .= "), ";
	}
	else
	{
	    $str .= $key . " => " . $val . ", ";
	}
    }
    $str . "]";
}

######################################################################
package XML::DOM::NodeList;
######################################################################

use vars qw ( $EMPTY );

# Empty NodeList
$EMPTY = new XML::DOM::NodeList;

sub new 
{
    bless [], $_[0];
}

sub item 
{
    $_[0]->[$_[1]];
}

sub getLength 
{
    int (@{$_[0]});
}

#------------------------------------------------------------
# Extra method implementations

sub dispose
{
    my $self = shift;
    for my $kid (@{$self})
    {
	$kid->dispose;
    }
}

sub setOwnerDocument
{
    my ($self, $doc) = @_;
    for my $kid (@{$self})
    { 
	$kid->setOwnerDocument ($doc);
    }
}

######################################################################
package XML::DOM::DOMImplementation;
######################################################################
 
$XML::DOM::DOMImplementation::Singleton =
  bless \$XML::DOM::DOMImplementation::Singleton, 'XML::DOM::DOMImplementation';
 
sub hasFeature 
{
    my ($self, $feature, $version) = @_;
 
    $feature eq 'XML' and $version eq '1.0';
}

######################################################################
package XML::DOM::Node;
######################################################################

use vars qw( @NodeNames @EXPORT @ISA );

BEGIN 
{
  import XML::DOM::DOMException;
  import Carp;

  require FileHandle;

  @ISA = qw( Exporter );
  @EXPORT = qw(
	     UNKNOWN_NODE
	     ELEMENT_NODE
	     ATTRIBUTE_NODE
	     TEXT_NODE
	     CDATA_SECTION_NODE
	     ENTITY_REFERENCE_NODE
	     ENTITY_NODE
	     PROCESSING_INSTRUCTION_NODE
	     COMMENT_NODE
	     DOCUMENT_NODE
	     DOCUMENT_TYPE_NODE
	     DOCUMENT_FRAGMENT_NODE
	     NOTATION_NODE
	     ELEMENT_DECL_NODE
	     ATT_DEF_NODE
	     XML_DECL_NODE
	     ATTLIST_DECL_NODE
	    );
}

#---- Constant definitions

# Node types

sub UNKNOWN_NODE                () {0;}		# not in the DOM Spec

sub ELEMENT_NODE                () {1;}
sub ATTRIBUTE_NODE              () {2;}
sub TEXT_NODE                   () {3;}
sub CDATA_SECTION_NODE          () {4;}
sub ENTITY_REFERENCE_NODE       () {5;}
sub ENTITY_NODE                 () {6;}
sub PROCESSING_INSTRUCTION_NODE () {7;}
sub COMMENT_NODE                () {8;}
sub DOCUMENT_NODE               () {9;}
sub DOCUMENT_TYPE_NODE          () {10;}
sub DOCUMENT_FRAGMENT_NODE      () {11;}
sub NOTATION_NODE               () {12;}

sub ELEMENT_DECL_NODE		() {13;}	# not in the DOM Spec
sub ATT_DEF_NODE 		() {14;}	# not in the DOM Spec
sub XML_DECL_NODE 		() {15;}	# not in the DOM Spec
sub ATTLIST_DECL_NODE		() {16;}	# not in the DOM Spec

@NodeNames = (
	      "UNKNOWN_NODE",	# not in the DOM Spec!

	      "ELEMENT_NODE",
	      "ATTRIBUTE_NODE",
	      "TEXT_NODE",
	      "CDATA_SECTION_NODE",
	      "ENTITY_REFERENCE_NODE",
	      "ENTITY_NODE",
	      "PROCESSING_INSTRUCTION_NODE",
	      "COMMENT_NODE",
	      "DOCUMENT_NODE",
	      "DOCUMENT_TYPE_NODE",
	      "DOCUMENT_FRAGMENT_NODE",
	      "NOTATION_NODE",

	      "ELEMENT_DECL_NODE",
	      "ATT_DEF_NODE",
	      "XML_DECL_NODE",
	      "ATTLIST_DECL_NODE"
	     );

sub getParentNode
{
    $_[0]->{Parent};
}

sub appendChild
{
    my ($self, $node) = @_;

    # REC 7473
    if ($XML::DOM::SafeMode)
    {
	croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
					  "node is ReadOnly")
	    if $self->isReadOnly;
    }

    my $isFrag = $node->isDocumentFragmentNode;
    my $doc = $self->{Doc};

    if ($isFrag)
    {
	if ($XML::DOM::SafeMode)
	{
	    for my $n (@{$node->{C}})
	    {
		croak new XML::DOM::DOMException (WRONG_DOCUMENT_ERR,
						  "nodes belong to different documents")
		    if $doc != $n->{Doc};
		
		croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
						  "node is ancestor of parent node")
		    if $n->isAncestor ($self);
		
		croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
						  "bad node type")
		    if $self->rejectChild ($n);
	    }
	}

	my @list = @{$node->{C}};	# don't try to compress this
	for my $n (@list)
	{
	    $n->setParentNode ($self);
	}
	push @{$self->{C}}, @list;
    }
    else
    {
	if ($XML::DOM::SafeMode)
	{
	    croak new XML::DOM::DOMException (WRONG_DOCUMENT_ERR,
						  "nodes belong to different documents")
		if $doc != $node->{Doc};
		
	    croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
						  "node is ancestor of parent node")
		if $node->isAncestor ($self);
		
	    croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
						  "bad node type")
		if $self->rejectChild ($node);
	}
	$node->setParentNode ($self);
	push @{$self->{C}}, $node;
    }
    $node;
}

sub getChildNodes
{
    # NOTE: if node can't have children, $self->{C} is undef.
    my $kids = $_[0]->{C};

    # Return a list if called in list context.
    wantarray ? (defined ($kids) ? @{ $kids } : ()) :
	        (defined ($kids) ? $kids : $XML::DOM::NodeList::EMPTY);
}

sub hasChildNodes
{
    my $kids = $_[0]->{C};
    defined ($kids) && @$kids > 0;
}

# This method is overriden in Document
sub getOwnerDocument
{
    $_[0]->{Doc};
}

sub getFirstChild
{
    my $kids = $_[0]->{C};
    defined $kids ? $kids->[0] : undef; 
}

sub getLastChild
{
    my $kids = $_[0]->{C};
    defined $kids ? $kids->[-1] : undef; 
}

sub getPreviousSibling
{
    my $self = shift;

    my $pa = $self->{Parent};
    return undef unless $pa;
    my $index = $pa->getChildIndex ($self);
    return undef unless $index;

    $pa->getChildAtIndex ($index - 1);
}

sub getNextSibling
{
    my $self = shift;

    my $pa = $self->{Parent};
    return undef unless $pa;

    $pa->getChildAtIndex ($pa->getChildIndex ($self) + 1);
}

sub insertBefore
{
    my ($self, $node, $refNode) = @_;

    return $self->appendChild ($node) unless $refNode;	# append at the end

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    my @nodes = ($node);
    @nodes = @{$node->{C}}
	if $node->getNodeType == DOCUMENT_FRAGMENT_NODE;

    my $doc = $self->{Doc};

    for my $n (@nodes)
    {
	croak new XML::DOM::DOMException (WRONG_DOCUMENT_ERR,
					  "nodes belong to different documents")
	    if $doc != $n->{Doc};
	
	croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
					  "node is ancestor of parent node")
	    if $n->isAncestor ($self);

	croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
					  "bad node type")
	    if $self->rejectChild ($n);
    }
    my $index = $self->getChildIndex ($refNode);

    croak new XML::DOM::DOMException (NOT_FOUND_ERR,
				      "reference node not found")
	if $index == -1;

    for my $n (@nodes)
    {
	$n->setParentNode ($self);
    }

    splice (@{$self->{C}}, $index, 0, @nodes);
    $node;
}

sub replaceChild
{
    my ($self, $node, $refNode) = @_;

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    my @nodes = ($node);
    @nodes = @{$node->{C}}
	if $node->getNodeType == DOCUMENT_FRAGMENT_NODE;

    for my $n (@nodes)
    {
	croak new XML::DOM::DOMException (WRONG_DOCUMENT_ERR,
					  "nodes belong to different documents")
	    if $self->{Doc} != $n->{Doc};

	croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
					  "node is ancestor of parent node")
	    if $n->isAncestor ($self);

	croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
					  "bad node type")
	    if $self->rejectChild ($n);
    }

    my $index = $self->getChildIndex ($refNode);
    croak new XML::DOM::DOMException (NOT_FOUND_ERR,
				      "reference node not found")
	if $index == -1;

    for my $n (@nodes)
    {
	$n->setParentNode ($self);
    }
    splice (@{$self->{C}}, $index, 1, @nodes);

    $refNode->removeChildHoodMemories;
    $refNode;
}

sub removeChild
{
    my ($self, $node) = @_;

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    my $index = $self->getChildIndex ($node);

    croak new XML::DOM::DOMException (NOT_FOUND_ERR,
				      "reference node not found")
	if $index == -1;

    splice (@{$self->{C}}, $index, 1, ());

    $node->removeChildHoodMemories;
    $node;
}

# Merge all subsequent Text nodes in this subtree
sub normalize
{
    my ($self) = shift;
    my $prev = undef;	# previous Text node

    return unless defined $self->{C};

    my @nodes = @{$self->{C}};
    my $i = 0;
    my $n = @nodes;
    while ($i < $n)
    {
	my $node = $self->getChildAtIndex($i);
	my $type = $node->getNodeType;

	if (defined $prev)
	{
	    # It should not merge CDATASections. Dom Spec says:
	    #  Adjacent CDATASections nodes are not merged by use
	    #  of the Element.normalize() method.
	    if ($type == TEXT_NODE)
	    {
		$prev->appendData ($node->getData);
		$self->removeChild ($node);
		$i--;
		$n--;
	    }
	    else
	    {
		$prev = undef;
		if ($type == ELEMENT_NODE)
		{
		    $node->normalize;
		    for my $attr (@{$node->getAttributes->getValues})
		    {
			$attr->normalize;
		    }
		}
	    }
	}
	else
	{
	    if ($type == TEXT_NODE)
	    {
		$prev = $node;
	    }
	    elsif ($type == ELEMENT_NODE)
	    {
		$node->normalize;
		for my $attr (@{$node->getAttributes->getValues})
		{
		    $attr->normalize;
		}
	    }
	}
	$i++;
    }
}

# Return all Element nodes in the subtree that have the specified tagName.
# If tagName is "*", all Element nodes are returned.
# NOTE: the DOM Spec does not specify a 3rd or 4th parameter
sub getElementsByTagName
{
    my ($self, $tagName, $recurse, $list) = @_;
    $recurse = 1 unless defined $recurse;
    $list = (wantarray ? [] : new XML::DOM::NodeList) unless defined $list;

    return unless defined $self->{C};

    # preorder traversal: check parent node first
    for my $kid (@{$self->{C}})
    {
	if ($kid->isElementNode)
	{
	    if ($tagName eq "*" || $tagName eq $kid->getTagName)
	    {
		push @{$list}, $kid;
	    }
	    $kid->getElementsByTagName ($tagName, $recurse, $list) if $recurse;
	}
    }
    wantarray ? @{ $list } : $list;
}

sub getNodeValue
{
    undef;
}

sub setNodeValue
{
    # no-op
}

# Redefined by XML::DOM::Element
sub getAttributes
{
    undef;
}

#------------------------------------------------------------
# Extra method implementations

sub setOwnerDocument
{
    my ($self, $doc) = @_;
    $self->{Doc} = $doc;

    return unless defined $self->{C};

    for my $kid (@{$self->{C}})
    {
	$kid->setOwnerDocument ($doc);
    }
}

sub cloneChildren
{
    my ($self, $node, $deep) = @_;
    return unless $deep;
    
    return unless defined $self->{C};

    my $oldIgnore = XML::DOM::ignoreReadOnly (1);	# temporarily...

    for my $kid (@{$node->{C}})
    {
	my $newNode = $kid->cloneNode ($deep);
	push @{$self->{C}}, $newNode;
	$newNode->setParentNode ($self);
    }

    XML::DOM::ignoreReadOnly ($oldIgnore);	# restore previous value
}

# For internal use only!
sub removeChildHoodMemories
{
    my ($self) = @_;

#????? remove?
    delete $self->{Parent};
}

# Remove circular dependencies. The Node and its children should
# not be used afterwards.
sub dispose
{
    my $self = shift;

    $self->removeChildHoodMemories;

    if (defined $self->{C})
    {
	$self->{C}->dispose;
	delete $self->{C};
    }
    delete $self->{Doc};
}

# For internal use only!
sub setParentNode
{
    my ($self, $parent) = @_;

    # REC 7473
    my $oldParent = $self->{Parent};
    if (defined $oldParent)
    {
	# remove from current parent
	my $index = $oldParent->getChildIndex ($self);
	splice (@{$oldParent->{C}}, $index, 1, ());

	$self->removeChildHoodMemories;
    }
    $self->{Parent} = $parent;
}

# This function can return 3 values:
# 1: always readOnly
# 0: never readOnly
# undef: depends on parent node 
#
# Returns 1 for DocumentType, Notation, Entity, EntityReference, Attlist, 
# ElementDecl, AttDef. 
# The first 4 are readOnly according to the DOM Spec, the others are always 
# children of DocumentType. (Naturally, children of a readOnly node have to be
# readOnly as well...)
# These nodes are always readOnly regardless of who their ancestors are.
# Other nodes, e.g. Comment, are readOnly only if their parent is readOnly,
# which basically means that one of its ancestors has to be one of the
# aforementioned node types.
# Document and DocumentFragment return 0 for obvious reasons.
# Attr, Element, CDATASection, Text return 0. The DOM spec says that they can 
# be children of an Entity, but I don't think that that's possible
# with the current XML::Parser.
# Attr uses a {ReadOnly} property, which is only set if it's part of a AttDef.
# Always returns 0 if ignoreReadOnly is set.
sub isReadOnly
{
    # default implementation for Nodes that are always readOnly
    ! $XML::DOM::IgnoreReadOnly;
}

sub rejectChild
{
    1;
}

sub getNodeTypeName
{
    $NodeNames[$_[0]->getNodeType];
}

sub getChildIndex
{
    my ($self, $node) = @_;
    my $i = 0;

    return -1 unless defined $self->{C};

    for my $kid (@{$self->{C}})
    {
	return $i if $kid == $node;
	$i++;
    }
    -1;
}

sub getChildAtIndex
{
    my $kids = $_[0]->{C};
    defined ($kids) ? $kids->[$_[1]] : undef;
}

sub isAncestor
{
    my ($self, $node) = @_;

    do
    {
	return 1 if $self == $node;
	$node = $node->{Parent};
    }
    while (defined $node);

    0;
}

# Added for optimization. Overriden in XML::DOM::Text
sub isTextNode
{
    0;
}

# Added for optimization. Overriden in XML::DOM::DocumentFragment
sub isDocumentFragmentNode
{
    0;
}

# Added for optimization. Overriden in XML::DOM::Element
sub isElementNode
{
    0;
}

# Add a Text node with the specified value or append the text to the
# previous Node if it is a Text node.
sub addText
{
    # REC 9456 (if it was called)
    my ($self, $str) = @_;

    my $node = ${$self->{C}}[-1];	# $self->getLastChild

    if (defined ($node) && $node->isTextNode)
    {
	# REC 5475 (if it was called)
	$node->appendData ($str);
    }
    else
    {
	$node = $self->{Doc}->createTextNode ($str);
	$self->appendChild ($node);
    }
    $node;
}

# Add a CDATASection node with the specified value or append the text to the
# previous Node if it is a CDATASection node.
sub addCDATA
{
    my ($self, $str) = @_;

    my $node = ${$self->{C}}[-1];	# $self->getLastChild

    if (defined ($node) && $node->getNodeType == CDATA_SECTION_NODE)
    {
	# REC 5475
	$node->appendData ($str);
    }
    else
    {
	$node = $self->{Doc}->createCDATASection ($str);
	$self->appendChild ($node);
    }
    $node;
}

sub removeChildNodes
{
    my $self = shift;

    my $cref = $self->{C};
    return unless defined $cref;

    my $kid;
    while ($kid = pop @{$cref})
    {
	delete $kid->{Parent};
    }
}

sub toString
{
    my $self = shift;
    my $pr = $XML::DOM::PrintToString::Singleton;
    $pr->reset;
    $self->print ($pr);
    $pr->toString;
}

sub printToFile
{
    my ($self, $fileName) = @_;
    my $fh = new FileHandle ($fileName, "w") || 
	croak "printToFile - can't open output file $fileName";
    
    $self->print ($fh);
    $fh->close;
}

# Use print to print to a FileHandle object (see printToFile code)
sub printToFileHandle
{
    my ($self, $FH) = @_;
    my $pr = new XML::DOM::PrintToFileHandle ($FH);
    $self->print ($pr);
}

# Used by AttDef::setDefault to convert unexpanded default attribute value
sub expandEntityRefs
{
    my ($self, $str) = @_;
    my $doctype = $self->{Doc}->getDoctype;

    $str =~ s/&($XML::DOM::ReName|(#([0-9]+)|#x([0-9a-fA-F]+)));/
	defined($2) ? XML::DOM::XmlUtf8Encode ($3 || hex ($4)) 
		    : expandEntityRef ($1, $doctype)/ego;
    $str;
}

sub expandEntityRef
{
    my ($entity, $doctype) = @_;

    my $expanded = $XML::DOM::DefaultEntities{$entity};
    return $expanded if defined $expanded;

    $expanded = $doctype->getEntity ($entity);
    return $expanded->getValue if (defined $expanded);

#?? is this an error?
    croak "Could not expand entity reference of [$entity]\n";
#    return "&$entity;";	# entity not found
}

######################################################################
package XML::DOM::Attr;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my ($class, $doc, $name, $value, $specified) = @_;

    if ($XML::DOM::SafeMode)
    {
	croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR,
				      "bad Attr name [$name]")
	    unless XML::DOM::isValidName ($name);
    }

    my $self = bless {Doc	=> $doc, 
		      C		=> new XML::DOM::NodeList,
		      Name	=> $name}, $class;
    
    if (defined $value)
    {
	$self->setValue ($value);
	$self->{Specified} = (defined $specified) ? $specified : 1;
    }
    else
    {
	$self->{Specified} = 0;
    }
    $self;
}

sub getNodeType
{
    ATTRIBUTE_NODE;
}

sub isSpecified
{
    $_[0]->{Specified};
}

sub getName
{
    $_[0]->{Name};
}

sub getValue
{
    my $self = shift;
    my $value = "";

    for my $kid (@{$self->{C}})
    {
	$value .= $kid->getData;
    }
    $value;
}

sub setValue
{
    my ($self, $value) = @_;

    # REC 1147
    $self->removeChildNodes;
    $self->appendChild ($self->{Doc}->createTextNode ($value));
    $self->{Specified} = 1;
}

sub getNodeName
{
    $_[0]->getName;
}

sub getNodeValue
{
    $_[0]->getValue;
}

sub setNodeValue
{
    $_[0]->setValue ($_[1]);
}

sub cloneNode
{
    my ($self) = @_;	# parameter deep is ignored

    my $node = $self->{Doc}->createAttribute ($self->getName);
    $node->{Specified} = $self->{Specified};
    $node->{ReadOnly} = 1 if $self->{ReadOnly};

    $node->cloneChildren ($self, 1);
    $node;
}

#------------------------------------------------------------
# Extra method implementations
#

sub isReadOnly
{
    # ReadOnly property is set if it's part of a AttDef
    ! $XML::DOM::IgnoreReadOnly && defined ($_[0]->{ReadOnly});
}

sub print
{
    my ($self, $FILE) = @_;    

    my $name = $self->{Name};

    $FILE->print ("$name=\"");
    for my $kid (@{$self->{C}})
    {
	if ($kid->getNodeType == TEXT_NODE)
	{
	    $FILE->print (XML::DOM::encodeAttrValue ($kid->getData));
	}
	else	# ENTITY_REFERENCE_NODE
	{
	    $kid->print ($FILE);
	}
    }
    $FILE->print ("\"");
}

sub rejectChild
{
    my $t = $_[1]->getNodeType;

    $t != TEXT_NODE && $t != ENTITY_REFERENCE_NODE;
}

######################################################################
package XML::DOM::ProcessingInstruction;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;

}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my ($class, $doc, $target, $data) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR,
			      "bad ProcessingInstruction Target [$target]")
	unless (XML::DOM::isValidName ($target) && $target !~ /^xml$/io);

    bless {Doc		=> $doc,
	   Target	=> $target,
	   Data		=> $data}, $class;
}

sub getNodeType
{
    PROCESSING_INSTRUCTION_NODE;
}

sub getTarget
{
    $_[0]->{Target};
}

sub getData
{
    $_[0]->{Data};
}

sub setData
{
    my ($self, $data) = @_;

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    $self->{Data} = $data;
}

sub getNodeName
{
    $_[0]->{Target};
}

sub getNodeValue
{
    $_[0]->getData;
}

sub setNodeValue
{
    $_[0]->setData ($_[1]);
}

sub cloneNode
{
    my $self = shift;
    $self->{Doc}->createProcessingInstruction ($self->getTarget, 
					       $self->getData);
}

#------------------------------------------------------------
# Extra method implementations

sub isReadOnly
{
    return 0 if $XML::DOM::IgnoreReadOnly;

    my $pa = $_[0]->{Parent};
    defined ($pa) ? $pa->isReadOnly : 0;
}

sub print
{
    my ($self, $FILE) = @_;    

    $FILE->print ("<?");
    $FILE->print ($self->{Target});
    $FILE->print (" ");
    $FILE->print (XML::DOM::encodeProcessingInstruction ($self->{Data}));
    $FILE->print ("?>");
}

######################################################################
package XML::DOM::Notation;
######################################################################

BEGIN
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my ($class, $doc, $name, $base, $sysId, $pubId) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR, 
				      "bad Notation Name [$name]")
	unless XML::DOM::isValidName ($name);

    bless {Doc		=> $doc,
	   Name		=> $name,
	   Base		=> $base,
	   SysId	=> $sysId,
	   PubId	=> $pubId}, $class;
}

sub getNodeType
{
    NOTATION_NODE;
}

sub getPubId
{
    $_[0]->{PubId};
}

sub setPubId
{
    $_[0]->{PubId} = $_[1];
}

sub getSysId
{
    $_[0]->{SysId};
}

sub setSysId
{
    $_[0]->{SysId} = $_[1];
}

sub getName
{
    $_[0]->{Name};
}

sub setName
{
    $_[0]->{Name} = $_[1];
}

sub getBase
{
    $_[0]->{Base};
}

sub getNodeName
{
    $_[0]->{Name};
}

sub print
{
    my ($self, $FILE) = @_;    

    my $name = $self->{Name};
    my $sysId = $self->{SysId};
    my $pubId = $self->{PubId};

    $FILE->print ("<!NOTATION $name ");

    if (defined $pubId)
    {
	$FILE->print (" PUBLIC \"$pubId\"");	
    }
    if (defined $sysId)
    {
	$FILE->print (" SYSTEM \"$sysId\"");	
    }
    $FILE->print (">");
}

sub cloneNode
{
    my ($self) = @_;
    $self->{Doc}->createNotation ($self->{Name}, $self->{Base}, 
				  $self->{SysId}, $self->{PubId});
}


######################################################################
package XML::DOM::Entity;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my ($class, $doc, $par, $notationName, $value, $sysId, $pubId, $ndata) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR, 
				      "bad Entity Name [$notationName]")
	unless XML::DOM::isValidName ($notationName);

    bless {Doc		=> $doc,
	   NotationName	=> $notationName,
	   Parameter	=> $par,
	   Value	=> $value,
	   Ndata	=> $ndata,
	   SysId	=> $sysId,
	   PubId	=> $pubId}, $class;
#?? maybe Value should be a Text node
}

sub getNodeType
{
    ENTITY_NODE;
}

sub getPubId
{
    $_[0]->{PubId};
}

sub getSysId
{
    $_[0]->{SysId};
}

# Dom Spec says: 
#  For unparsed entities, the name of the notation for the
#  entity. For parsed entities, this is null.

#?? do we have unparsed entities?
sub getNotationName
{
    $_[0]->{NotationName};
}

sub getNodeName
{
    $_[0]->{NotationName};
}

sub cloneNode
{
    my $self = shift;
    $self->{Doc}->createEntity ($self->{Parameter}, 
				$self->{NotationName}, $self->{Value}, 
				$self->{SysId}, $self->{PubId}, 
				$self->{Ndata});
}

sub rejectChild
{
    return 1;
#?? if value is split over subnodes, recode this section
# also add:				   c => new XML::DOM::NodeList,

    my $t = $_[1];

    return $t == TEXT_NODE
	|| $t == ENTITY_REFERENCE_NODE 
	|| $t == PROCESSING_INSTRUCTION_NODE
	|| $t == COMMENT_NODE
	|| $t == CDATA_SECTION_NODE
	|| $t == ELEMENT_NODE;
}

sub getValue
{
    $_[0]->{Value};
}

sub isParameterEntity
{
    $_[0]->{Parameter};
}

sub getNdata
{
    $_[0]->{Ndata};
}

sub print
{
    my ($self, $FILE) = @_;    

    my $name = $self->{NotationName};

    my $par = $self->isParameterEntity ? "% " : "";

    $FILE->print ("<!ENTITY $par$name");

    my $value = $self->{Value};
    my $sysId = $self->{SysId};
    my $pubId = $self->{PubId};
    my $ndata = $self->{Ndata};

    if (defined $value)
    {
#?? Not sure what to do if it contains both single and double quote
	$value = ($value =~ /\"/) ? "'$value'" : "\"$value\"";
	$FILE->print (" $value");
    }
    if (defined $pubId)
    {
	$FILE->print (" PUBLIC \"$pubId\"");	
    }
    elsif (defined $sysId)
    {
	$FILE->print (" SYSTEM");
    }

    if (defined $sysId)
    {
	$FILE->print (" \"$sysId\"");
    }
    $FILE->print (" NDATA $ndata") if defined $ndata;
    $FILE->print (">");
}

######################################################################
package XML::DOM::EntityReference;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my ($class, $doc, $name, $parameter) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR, 
		      "bad Entity Name [$name] in EntityReference")
	unless XML::DOM::isValidName ($name);

    bless {Doc		=> $doc,
	   EntityName	=> $name,
	   Parameter	=> ($parameter || 0)}, $class;
}

sub getNodeType
{
    ENTITY_REFERENCE_NODE;
}

sub getNodeName
{
    $_[0]->{EntityName};
}

#------------------------------------------------------------
# Extra method implementations

sub getEntityName
{
    $_[0]->{EntityName};
}

sub isParameterEntity
{
    $_[0]->{Parameter};
}

sub getData
{
    my $self = shift;
    my $name = $self->{EntityName};
    my $parameter = $self->{Parameter};

    my $data = $self->{Doc}->expandEntity ($name, $parameter);

    unless (defined $data)
    {
#?? this is probably an error
	my $pc = $parameter ? "%" : "&";
	$data = "$pc$name;";
    }
    $data;
}

sub print
{
    my ($self, $FILE) = @_;    

    my $name = $self->{EntityName};

#?? or do we expand the entities?

    my $pc = $self->{Parameter} ? "%" : "&";
    $FILE->print ("$pc$name;");
}

# Dom Spec says:
#     [...] but if such an Entity exists, then
#     the child list of the EntityReference node is the same as that of the
#     Entity node. 
#
#     The resolution of the children of the EntityReference (the replacement
#     value of the referenced Entity) may be lazily evaluated; actions by the
#     user (such as calling the childNodes method on the EntityReference
#     node) are assumed to trigger the evaluation.
sub getChildNodes
{
    my $self = shift;
    my $entity = $self->{Doc}->getEntity ($self->{EntityName});
    defined ($entity) ? $entity->getChildNodes : new XML::DOM::NodeList;
}

sub cloneNode
{
    my $self = shift;
    $self->{Doc}->createEntityReference ($self->{EntityName}, 
					 $self->{Parameter});
}

# NOTE: an EntityReference can't really have children, so rejectChild
# is not reimplemented (i.e. it always returns 0.)

######################################################################
package XML::DOM::AttDef;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

#------------------------------------------------------------
# Extra method implementations

# AttDef is not part of DOM Spec
sub new
{
    my ($class, $doc, $name, $attrType, $default, $fixed) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR,
				      "bad Attr name in AttDef [$name]")
	unless XML::DOM::isValidName ($name);

    my $self = bless {Doc	=> $doc,
		      Name	=> $name,
		      Type	=> $attrType}, $class;

    if (defined $default)
    {
	if ($default eq "#REQUIRED")
	{
	    $self->{Required} = 1;
	}
	elsif ($default eq "#IMPLIED")
	{
	    $self->{Implied} = 1;
	}
	else
	{
	    # strip off quotes - see Attlist handler in XML::Parser
	    $default =~ m#^(["'])(.*)['"]$#;
	    
	    $self->{Quote} = $1;	# keep track of the quote character
	    $self->{Default} = $self->setDefault ($2);
	    
#?? should default value be decoded - what if it contains e.g. "&amp;"
	}
    }
    $self->{Fixed} = $fixed if defined $fixed;

    $self;
}

sub getNodeType
{
    ATT_DEF_NODE;
}

sub getName
{
    $_[0]->{Name};
}

# So it can be added to a NamedNodeMap
sub getNodeName
{
    $_[0]->{Name};
}

sub getDefault
{
    $_[0]->{Default};
}

sub setDefault
{
    my ($self, $value) = @_;

    # specified=0, it's the default !
    my $attr = $self->{Doc}->createAttribute ($self->{Name}, undef, 0);
    $attr->{ReadOnly} = 1;

#?? this should be split over Text and EntityReference nodes, just like other
# Attr nodes - just expand the text for now
    $value = $self->expandEntityRefs ($value);
    $attr->addText ($value);
#?? reimplement in NoExpand mode!

    $attr;
}

sub isFixed
{
    $_[0]->{Fixed} || 0;
}

sub isRequired
{
    $_[0]->{Required} || 0;
}

sub isImplied
{
    $_[0]->{Implied} || 0;
}

sub print
{
    my ($self, $FILE) = @_;    

    my $name = $self->{Name};
    my $type = $self->{Type};
    my $fixed = $self->{Fixed};
    my $default = $self->{Default};

    $FILE->print ("$name $type");
    $FILE->print (" #FIXED") if defined $fixed;

    if ($self->{Required})
    {
	$FILE->print (" #REQUIRED");
    }
    elsif ($self->{Implied})
    {
	$FILE->print (" #IMPLIED");
    }
    elsif (defined ($default))
    {
	my $quote = $self->{Quote};
	$FILE->print (" $quote");
	for my $kid (@{$default->{C}})
	{
	    $kid->print ($FILE);
	}
	$FILE->print ($quote);	
    }
}

sub getDefaultString
{
    my $self = shift;
    my $default;

    if ($self->{Required})
    {
	return "#REQUIRED";
    }
    elsif ($self->{Implied})
    {
	return "#IMPLIED";
    }
    elsif (defined ($default = $self->{Default}))
    {
	my $quote = $self->{Quote};
	$default = $default->toString;
	return "$quote$default$quote";
    }
    undef;
}

sub cloneNode
{
    my $self = shift;
    my $node = new XML::DOM::AttDef ($self->{Doc}, $self->{Name}, $self->{Type},
				     undef, $self->{Fixed});

    $node->{Required} = 1 if $self->{Required};
    $node->{Implied} = 1 if $self->{Implied};
    $node->{Fixed} = $self->{Fixed} if defined $self->{Fixed};

    if (defined $self->{Default})
    {
	$node->{Default} = $self->{Default}->cloneNode(1);
    }
    $node->{Quote} = $self->{Quote};

    $node;
}

sub setOwnerDocument
{
    my ($self, $doc) = @_;
    $self->SUPER::setOwnerDocument ($doc);

    if (defined $self->{Default})
    {
	$self->{Default}->setOwnerDocument ($doc);
    }
}

######################################################################
package XML::DOM::AttlistDecl;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

#------------------------------------------------------------
# Extra method implementations

# AttlistDecl is not part of the DOM Spec
sub new
{
    my ($class, $doc, $name) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR, 
			      "bad Element TagName [$name] in AttlistDecl")
	unless XML::DOM::isValidName ($name);

    my $self = bless {Doc	=> $doc,
		      C		=> new XML::DOM::NodeList,
		      ReadOnly	=> 1,
		      Name	=> $name}, $class;

    $self->{A} = new XML::DOM::NamedNodeMap (Doc	=> $doc,
					     ReadOnly	=> 1,
					     Parent	=> $self);

    $self;
}

sub getNodeType
{
    ATTLIST_DECL_NODE;
}

sub getName
{
    $_[0]->{Name};
}

sub getNodeName
{
    $_[0]->{Name};
}

sub getAttDef
{
    my ($self, $attrName) = @_;
    $self->{A}->getNamedItem ($attrName);
}

sub addAttDef
{
    my ($self, $attrName, $type, $default, $fixed) = @_;
    my $node = $self->getAttDef ($attrName);

    if (defined $node)
    {
	# data will be ignored if already defined
	my $elemName = $self->getName;
	warn "multiple definitions of attribute $attrName for element $elemName, only first one is recognized";
    }
    else
    {
	$node = new XML::DOM::AttDef ($self->{Doc}, $attrName, $type, 
				      $default, $fixed);
	$self->{A}->setNamedItem ($node);
    }
    $node;
}

sub getDefaultAttrValue
{
    my ($self, $attr) = @_;
    my $attrNode = $self->getAttDef ($attr);
    (defined $attrNode) ? $attrNode->getDefault : undef;
}

sub cloneNode
{
    my ($self, $deep) = @_;
    my $node = $self->{Doc}->createAttlistDecl ($self->{Name});
    
    $node->{A} = $self->{A}->cloneNode ($deep);
    $node;
}

sub setOwnerDocument
{
    my ($self, $doc) = @_;
    $self->SUPER::setOwnerDocument ($doc);

    $self->{A}->setOwnerDocument ($doc);
}

sub print
{
    my ($self, $FILE) = @_;    

    my $name = $self->getName;
    my @attlist = @{$self->{A}->getValues};

    if (@attlist > 0)
    {
	$FILE->print ("<!ATTLIST $name");

	if (@attlist == 1)
	{
	    $FILE->print (" ");
	    $attlist[0]->print ($FILE);	    
	}
	else
	{
	    for my $attr (@attlist)
	    {
		$FILE->print ("\x0A  ");
		$attr->print ($FILE);
	    }
	}
	$FILE->print (">");
    }
}

######################################################################
package XML::DOM::ElementDecl;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

#------------------------------------------------------------
# Extra method implementations

# ElementDecl is not part of the DOM Spec
sub new
{
    my ($class, $doc, $name, $model) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR, 
			      "bad Element TagName [$name] in ElementDecl")
	unless XML::DOM::isValidName ($name);

    bless {Doc		=> $doc,
	   Name		=> $name,
	   ReadOnly	=> 1,
	   Model	=> $model}, $class;
}

sub getNodeType
{
    ELEMENT_DECL_NODE;
}

sub getName
{
    $_[0]->{Name};
}

sub getNodeName
{
    $_[0]->{Name};
}

sub getModel
{
    $_[0]->{Model};
}

sub setModel
{
    my ($self, $model) = @_;

    $self->{Model} = $model;
}

sub print
{
    my ($self, $FILE) = @_;    

    my $name = $self->{Name};
    my $model = $self->{Model};

    $FILE->print ("<!ELEMENT $name $model>");
}

sub cloneNode
{
    my $self = shift;
    $self->{Doc}->createElementDecl ($self->{Name}, $self->{Model});
}

######################################################################
package XML::DOM::Element;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my ($class, $doc, $tagName) = @_;

    if ($XML::DOM::SafeMode)
    {
	croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR, 
				      "bad Element TagName [$tagName]")
	    unless XML::DOM::isValidName ($tagName);
    }

    my $self = bless {Doc	=> $doc,
		      C		=> new XML::DOM::NodeList,
		      TagName	=> $tagName}, $class;

    $self->{A} = new XML::DOM::NamedNodeMap (Doc	=> $doc,
					     Parent	=> $self);
    $self;
}

sub getNodeType
{
    ELEMENT_NODE;
}

sub getTagName
{
    $_[0]->{TagName};
}

sub getNodeName
{
    $_[0]->{TagName};
}

sub getAttributeNode
{
    my ($self, $name) = @_;
    $self->getAttributes->{$name};
}

sub getAttribute
{
    my ($self, $name) = @_;
    my $attr = $self->getAttributeNode ($name);
    (defined $attr) ? $attr->getValue : "";
}

sub setAttribute
{
    my ($self, $name, $val) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR,
			      "bad Attr Name [$name]")
	unless XML::DOM::isValidName ($name);

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    my $node = $self->{A}->{$name};
    if (defined $node)
    {
	$node->setValue ($val);
    }
    else
    {
	$node = $self->{Doc}->createAttribute ($name, $val);
	$self->{A}->setNamedItem ($node);
    }
}

sub setAttributeNode
{
    my ($self, $node) = @_;
    my $attr = $self->{A};
    my $name = $node->getNodeName;

    # REC 1147
    if ($XML::DOM::SafeMode)
    {
	croak new XML::DOM::DOMException (WRONG_DOCUMENT_ERR,
				      "nodes belong to different documents")
	    if $self->{Doc} != $node->{Doc};

	croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
					  "node is ReadOnly")
	    if $self->isReadOnly;

	my $attrParent = $node->{UsedIn};
	croak new XML::DOM::DOMException (INUSE_ATTRIBUTE_ERR,
			      "Attr is already used by another Element")
	    if (defined ($attrParent) && $attrParent != $attr);
    }

    my $other = $attr->{$name};
    $attr->removeNamedItem ($name) if defined $other;

    $attr->setNamedItem ($node);

    $other;
}

sub removeAttributeNode
{
    my ($self, $node) = @_;

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    my $attr = $self->{A};
    my $name = $node->getNodeName;
    my $attrNode = $attr->getNamedItem ($name);

#?? should it croak if it's the default value?
    croak new XML::DOM::DOMException (NOT_FOUND_ERR)
	unless $node == $attrNode;

    # Not removing anything if it's the default value already
    return undef unless $node->isSpecified;

    $attr->removeNamedItem ($name);

    # Substitute with default value if it's defined
    my $default = $self->getDefaultAttrValue ($name);
    if (defined $default)
    {
	my $oldIgnore = XML::DOM::ignoreReadOnly (1);	# temporarily

	$default = $default->cloneNode (1);
	$attr->setNamedItem ($default);

	XML::DOM::ignoreReadOnly ($oldIgnore);	# restore previous value
    }
    $node;
}

sub removeAttribute
{
    my ($self, $name) = @_;
    my $node = $self->{A}->getNamedItem ($name);

#?? could use dispose() to remove circular references for gc, but what if
#?? somebody is referencing it?
    $self->removeAttributeNode ($node) if defined $node;
}

sub cloneNode
{
    my ($self, $deep) = @_;
    my $node = $self->{Doc}->createElement ($self->getTagName);

    # Always clone the Attr nodes, even if $deep == 0
    $node->{A} = $self->{A}->cloneNode (1);	# deep=1
    $node->{A}->setParentNode ($node);

    $node->cloneChildren ($self, $deep);
    $node;
}

sub getAttributes
{
    $_[0]->{A};
}

#------------------------------------------------------------
# Extra method implementations

# Added for convenience
sub setTagName
{
    my ($self, $tagName) = @_;

    croak new XML::DOM::DOMException (INVALID_CHARACTER_ERR, 
				      "bad Element TagName [$tagName]")
        unless XML::DOM::isValidName ($tagName);

    $self->{TagName} = $tagName;
}

sub isReadOnly
{
    0;
}

# Added for optimization.
sub isElementNode
{
    1;
}

sub rejectChild
{
    my $t = $_[1]->getNodeType;

    $t != TEXT_NODE
    && $t != ENTITY_REFERENCE_NODE 
    && $t != PROCESSING_INSTRUCTION_NODE
    && $t != COMMENT_NODE
    && $t != CDATA_SECTION_NODE
    && $t != ELEMENT_NODE;
}

sub getDefaultAttrValue
{
    my ($self, $attr) = @_;
    $self->{Doc}->getDefaultAttrValue ($self->{TagName}, $attr);
}

sub dispose
{
    my $self = shift;

    $self->{A}->dispose;
    $self->SUPER::dispose;
}

sub setOwnerDocument
{
    my ($self, $doc) = @_;
    $self->SUPER::setOwnerDocument ($doc);

    $self->{A}->setOwnerDocument ($doc);
}

sub print
{
    my ($self, $FILE) = @_;    

    my $name = $self->{TagName};

    $FILE->print ("<$name");

    for my $att (@{$self->{A}->getValues})
    {
	# skip un-specified (default) Attr nodes
	if ($att->isSpecified)
	{
	    $FILE->print (" ");
	    $att->print ($FILE);
	}
    }

    my @kids = @{$self->{C}};
    if (@kids > 0)
    {
	$FILE->print (">");
	for my $kid (@kids)
	{
	    $kid->print ($FILE);
	}
	$FILE->print ("</$name>");
    }
    else
    {
	my $style = &$XML::DOM::TagStyle ($name, $self);
	if ($style == 0)
	{
	    $FILE->print ("/>");
	}
	elsif ($style == 1)
	{
	    $FILE->print ("></$name>");
	}
	else
	{
	    $FILE->print (" />");
	}
    }
}

######################################################################
package XML::DOM::CharacterData;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

# CharacterData nodes should never be created directly, only subclassed!

sub appendData
{
    my ($self, $data) = @_;

    if ($XML::DOM::SafeMode)
    {
	croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
					  "node is ReadOnly")
	    if $self->isReadOnly;
    }
    $self->{Data} .= $data;
}

sub deleteData
{
    my ($self, $offset, $count) = @_;

    croak new XML::DOM::DOMException (INDEX_SIZE_ERR,
				      "bad offset [$offset]")
	if ($offset < 0 || $offset >= length ($self->{Data}));
#?? DOM Spec says >, but >= makes more sense!

    croak new XML::DOM::DOMException (INDEX_SIZE_ERR,
				      "negative count [$count]")
	if $count < 0;
 
    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    substr ($self->{Data}, $offset, $count) = "";
}

sub getData
{
    $_[0]->{Data};
}

sub getLength
{
    length $_[0]->{Data};
}

sub insertData
{
    my ($self, $offset, $data) = @_;

    croak new XML::DOM::DOMException (INDEX_SIZE_ERR,
				      "bad offset [$offset]")
	if ($offset < 0 || $offset >= length ($self->{Data}));
#?? DOM Spec says >, but >= makes more sense!

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    substr ($self->{Data}, $offset, 0) = $data;
}

sub replaceData
{
    my ($self, $offset, $count, $data) = @_;

    croak new XML::DOM::DOMException (INDEX_SIZE_ERR,
				      "bad offset [$offset]")
	if ($offset < 0 || $offset >= length ($self->{Data}));
#?? DOM Spec says >, but >= makes more sense!

    croak new XML::DOM::DOMException (INDEX_SIZE_ERR,
				      "negative count [$count]")
	if $count < 0;
 
    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    substr ($self->{Data}, $offset, $count) = $data;
}

sub setData
{
    my ($self, $data) = @_;

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    $self->{Data} = $data;
}

sub substringData
{
    my ($self, $offset, $count) = @_;
    my $data = $self->{Data};

    croak new XML::DOM::DOMException (INDEX_SIZE_ERR,
				      "bad offset [$offset]")
	if ($offset < 0 || $offset >= length ($data));
#?? DOM Spec says >, but >= makes more sense!

    croak new XML::DOM::DOMException (INDEX_SIZE_ERR,
				      "negative count [$count]")
	if $count < 0;
    
    substr ($data, $offset, $count);
}

sub getNodeValue
{
    $_[0]->getData;
}

sub setNodeValue
{
    $_[0]->setData ($_[1]);
}

######################################################################
package XML::DOM::CDATASection;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::CharacterData );

sub new
{
    my ($class, $doc, $data) = @_;
    bless {Doc	=> $doc, 
	   Data	=> $data}, $class;
}

sub getNodeName
{
    "#cdata-section";
}

sub getNodeType
{
    CDATA_SECTION_NODE;
}

sub cloneNode
{
    my $self = shift;
    $self->{Doc}->createCDATASection ($self->getData);
}

#------------------------------------------------------------
# Extra method implementations

sub isReadOnly
{
    0;
}

sub print
{
    my ($self, $FILE) = @_;
    $FILE->print ("<![CDATA[");
    $FILE->print (XML::DOM::encodeCDATA ($self->getData));
    $FILE->print ("]]>");
}

######################################################################
package XML::DOM::Comment;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::CharacterData );

#?? setData - could check comment for double minus

sub new
{
    my ($class, $doc, $data) = @_;
    bless {Doc	=> $doc, 
	   Data	=> $data}, $class;
}

sub getNodeType
{
    COMMENT_NODE;
}

sub getNodeName
{
    "#comment";
}

sub cloneNode
{
    my $self = shift;
    $self->{Doc}->createComment ($self->getData);
}

#------------------------------------------------------------
# Extra method implementations

sub isReadOnly
{
    return 0 if $XML::DOM::IgnoreReadOnly;

    my $pa = $_[0]->{Parent};
    defined ($pa) ? $pa->isReadOnly : 0;
}

sub print
{
    my ($self, $FILE) = @_;
    my $comment = XML::DOM::encodeComment ($self->{Data});

    $FILE->print ("<!--$comment-->");
}

######################################################################
package XML::DOM::Text;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
    import Carp;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::CharacterData );

sub new
{
    my ($class, $doc, $data) = @_;
    bless {Doc	=> $doc, 
	   Data	=> $data}, $class;
}

sub getNodeType
{
    TEXT_NODE;
}

sub getNodeName
{
    "#text";
}

sub splitText
{
    my ($self, $offset) = @_;

    my $data = $self->getData;
    croak new XML::DOM::DOMException (INDEX_SIZE_ERR,
				      "bad offset [$offset]")
	if ($offset < 0 || $offset >= length ($data));
#?? DOM Spec says >, but >= makes more sense!

    croak new XML::DOM::DOMException (NO_MODIFICATION_ALLOWED_ERR,
				      "node is ReadOnly")
	if $self->isReadOnly;

    my $rest = substring ($data, $offset);

    $self->setData (substring ($data, 0, $offset));
    my $node = $self->{Doc}->createTextNode ($rest);

    # insert new node after this node
    $self->{Parent}->insertAfter ($node, $self);

    $node;
}

sub cloneNode
{
    my $self = shift;
    $self->{Doc}->createTextNode ($self->getData);
}

#------------------------------------------------------------
# Extra method implementations

sub isReadOnly
{
    0;
}

sub print
{
    my ($self, $FILE) = @_;
    $FILE->print (XML::DOM::encodeText ($self->getData, "<&"));
}

sub isTextNode
{
    1;
}

######################################################################
package XML::DOM::XMLDecl;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

#------------------------------------------------------------
# Extra method implementations

# XMLDecl is not part of the DOM Spec
sub new
{
    my ($class, $doc, $version, $encoding, $standalone) = @_;
    my $self = bless {Doc => $doc}, $class;

    $self->{Version} = $version if defined $version;
    $self->{Encoding} = $encoding if defined $encoding;
    $self->{Standalone} = $standalone if defined $standalone;

    $self;
}

sub setVersion
{
    if (defined $_[1])
    {
	$_[0]->{Version} = $_[1];
    }
    else
    {
	delete $_[0]->{Version};
    }
}

sub getVersion
{
    $_[0]->{Version};
}

sub setEncoding
{
    if (defined $_[1])
    {
	$_[0]->{Encoding} = $_[1];
    }
    else
    {
	delete $_[0]->{Encoding};
    }
}

sub getEncoding
{
    $_[0]->{Encoding};
}

sub setStandalone
{
    if (defined $_[1])
    {
	$_[0]->{Standalone} = $_[1];
    }
    else
    {
	delete $_[0]->{Standalone};
    }
}

sub getStandalone
{
    $_[0]->{Standalone};
}

sub getNodeType
{
    XML_DECL_NODE;
}

sub cloneNode
{
    my $self = shift;

    new XML::DOM::XMLDecl ($self->{Doc}, $self->{Version}, 
			   $self->{Encoding}, $self->{Standalone});
}

sub print
{
    my ($self, $FILE) = @_;

    my $version = $self->{Version};
    my $encoding = $self->{Encoding};
    my $standalone = $self->{Standalone};
    $standalone = ($standalone ? "yes" : "no") if defined $standalone;

    $FILE->print ("<?xml");
    $FILE->print (" version=\"$version\"")	 if defined $version;    
    $FILE->print (" encoding=\"$encoding\"")	 if defined $encoding;
    $FILE->print (" standalone=\"$standalone\"") if defined $standalone;
    $FILE->print ("?>");
}

######################################################################
package XML::DOM::DocumentType;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my $class = shift;
    my $doc = shift;

    my $self = bless {Doc	=> $doc,
		      ReadOnly	=> 1,
		      C		=> new XML::DOM::NodeList}, $class;

    $self->{Entities} =  new XML::DOM::NamedNodeMap (Doc	=> $doc,
						     Parent	=> $self,
						     ReadOnly	=> 1);
    $self->{Notations} = new XML::DOM::NamedNodeMap (Doc	=> $doc,
						     Parent	=> $self,
						     ReadOnly	=> 1);
    $self->setParams (@_);
    $self;
}

sub getNodeType
{
    DOCUMENT_TYPE_NODE;
}

sub getNodeName
{
    $_[0]->{Name};
}

sub getName
{
    $_[0]->{Name};
}

sub getEntities
{
    $_[0]->{Entities};
}

sub getNotations
{
    $_[0]->{Notations};
}

sub setParentNode
{
    my ($self, $parent) = @_;
    $self->SUPER::setParentNode ($parent);

    $parent->{Doctype} = $self 
	if $parent->getNodeType == DOCUMENT_NODE;
}

sub cloneNode
{
    my ($self, $deep) = @_;

    my $node = new XML::DOM::DocumentType ($self->{Doc}, $self->{Name}, 
					   $self->{SysId}, $self->{PubId}, 
					   $self->{Internal});

#?? does it make sense to make a shallow copy?

    # clone the NamedNodeMaps
    $node->{Entities} = $self->{Entities}->cloneNode ($deep);

    $node->{Notations} = $self->{Notations}->cloneNode ($deep);

    $node->cloneChildren ($self, $deep);

    $node;
}

#------------------------------------------------------------
# Extra method implementations

sub getSysId
{
    $_[0]->{SysId};
}

sub getPubId
{
    $_[0]->{PubId};
}

sub setSysId
{
    $_[0]->{SysId} = $_[1];
}

sub setPubId
{
    $_[0]->{PubId} = $_[1];
}

sub setName
{
    $_[0]->{Name} = $_[1];
}

sub removeChildHoodMemories
{
    my ($self, $dontWipeReadOnly) = @_;

    my $parent = $self->{Parent};
    if (defined $parent && $parent->getNodeType == DOCUMENT_NODE)
    {
	delete $parent->{Doctype};
    }
    $self->SUPER::removeChildHoodMemories;
}

sub dispose
{
    my $self = shift;

    $self->{Entities}->dispose;
    $self->{Notations}->dispose;
    $self->SUPER::dispose;
}

sub setOwnerDocument
{
    my ($self, $doc) = @_;
    $self->SUPER::setOwnerDocument ($doc);

    $self->{Entities}->setOwnerDocument ($doc);
    $self->{Notations}->setOwnerDocument ($doc);
}

sub expandEntity
{
    my ($self, $ent, $param) = @_;

    my $kid = $self->{Entities}->getNamedItem ($ent);
    return $kid->getValue
	if (defined ($kid) && $param == $kid->isParameterEntity);

    undef;	# entity not found
}

sub getAttlistDecl
{
    my ($self, $elemName) = @_;
    for my $kid (@{$_[0]->{C}})
    {
	return $kid if ($kid->getNodeType == ATTLIST_DECL_NODE &&
			$kid->getName eq $elemName);
    }
    undef;	# not found
}

sub getElementDecl
{
    my ($self, $elemName) = @_;
    for my $kid (@{$_[0]->{C}})
    {
	return $kid if ($kid->getNodeType == ELEMENT_DECL_NODE &&
			$kid->getName eq $elemName);
    }
    undef;	# not found
}

sub addElementDecl
{
    my ($self, $name, $model) = @_;
    my $node = $self->getElementDecl ($name);

#?? could warn
    unless (defined $node)
    {
	$node = $self->{Doc}->createElementDecl ($name, $model);
	$self->appendChild ($node);
    }
    $node;
}

sub addAttlistDecl
{
    my ($self, $name) = @_;
    my $node = $self->getAttlistDecl ($name);

    unless (defined $node)
    {
	$node = $self->{Doc}->createAttlistDecl ($name);
	$self->appendChild ($node);
    }
    $node;
}

sub addNotation
{
    my $self = shift;
    my $node = $self->{Doc}->createNotation (@_);
    $self->{Notations}->setNamedItem ($node);
    $node;
}

sub addEntity
{
    my $self = shift;
    my $node = $self->{Doc}->createEntity (@_);

    $self->{Entities}->setNamedItem ($node);
    $node;
}

# All AttDefs for a certain Element are merged into a single ATTLIST
sub addAttDef
{
    my $self = shift;
    my $elemName = shift;

    # create the AttlistDecl if it doesn't exist yet
    my $elemDecl = $self->addAttlistDecl ($elemName);
    $elemDecl->addAttDef (@_);
}

sub getDefaultAttrValue
{
    my ($self, $elem, $attr) = @_;
    my $elemNode = $self->getAttlistDecl ($elem);
    (defined $elemNode) ? $elemNode->getDefaultAttrValue ($attr) : undef;
}

sub getEntity
{
    my ($self, $entity) = @_;
    $self->{Entities}->getNamedItem ($entity);
}

sub setParams
{
    my ($self, $name, $sysid, $pubid, $internal) = @_;

    $self->{Name} = $name;

#?? not sure if we need to hold on to these...
    $self->{SysId} = $sysid if defined $sysid;
    $self->{PubId} = $pubid if defined $pubid;
    $self->{Internal} = $internal if defined $internal;

    $self;
}

sub rejectChild
{
    # DOM Spec says: DocumentType -- no children
    not $XML::DOM::IgnoreReadOnly;
}

sub print
{
    my ($self, $FILE) = @_;

    my $name = $self->{Name};

    my $sysId = $self->{SysId};
    my $pubId = $self->{PubId};

    $FILE->print ("<!DOCTYPE $name");
    if (defined $pubId)
    {
	$FILE->print (" PUBLIC \"$pubId\" \"$sysId\"");
    }
    elsif (defined $sysId)
    {
	$FILE->print (" SYSTEM \"$sysId\"");
    }

    my @entities = @{$self->{Entities}->getValues};
    my @notations = @{$self->{Notations}->getValues};
    my @kids = @{$self->{C}};

    if (@entities || @notations || @kids)
    {
	$FILE->print (" [\x0A");

	for my $kid (@entities)
	{
	    $FILE->print (" ");
	    $kid->print ($FILE);
	    $FILE->print ("\x0A");
	}

	for my $kid (@notations)
	{
	    $FILE->print (" ");
	    $kid->print ($FILE);
	    $FILE->print ("\x0A");
	}

	for my $kid (@kids)
	{
	    $FILE->print (" ");
	    $kid->print ($FILE);
	    $FILE->print ("\x0A");
	}
	$FILE->print ("]");
    }
    $FILE->print (">");
}

######################################################################
package XML::DOM::DocumentFragment;
######################################################################

BEGIN 
{
    import XML::DOM::Node;
    import XML::DOM::DOMException;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my ($class, $doc) = @_;
    bless {Doc	=> $doc,
	   C	=> new XML::DOM::NodeList}, $class;
}

sub getNodeType
{
    DOCUMENT_FRAGMENT_NODE;
}

sub getNodeName
{
    "#document-fragment";
}

sub cloneNode
{
    my ($self, $deep) = @_;
    my $node = $self->{Doc}->createDocumentFragment;

    $node->cloneChildren ($self, $deep);
    $node;
}

#------------------------------------------------------------
# Extra method implementations

sub isReadOnly
{
    0;
}

sub print
{
    my ($self, $FILE) = @_;

    for my $node (@{$self->{C}})
    {
	$node->print ($FILE);
    }
}

sub rejectChild
{
    my $t = $_[1]->getNodeType;

    $t != TEXT_NODE
	&& $t != ENTITY_REFERENCE_NODE 
	&& $t != PROCESSING_INSTRUCTION_NODE
	&& $t != COMMENT_NODE
	&& $t != CDATA_SECTION_NODE
	&& $t != ELEMENT_NODE;
}

sub isDocumentFragmentNode
{
    1;
}

######################################################################
package XML::DOM::Document;
######################################################################

BEGIN 
{
    import Carp;
    import XML::DOM::Node;
    import XML::DOM::DOMException;
}

use vars qw( @ISA );
@ISA = qw( XML::DOM::Node );

sub new
{
    my ($class) = @_;
    my $self = bless {C => new XML::DOM::NodeList}, $class;

    # keep Doc pointer, even though getOwnerDocument returns undef
    $self->{Doc} = $self;

    $self;
}

sub getNodeType
{
    DOCUMENT_NODE;
}

sub getNodeName
{
    "#document";
}

#?? not sure about keeping a fixed order of these nodes....
sub getDoctype
{
    $_[0]->{Doctype};
}

sub getDocumentElement
{
    my ($self) = @_;
    for my $kid (@{$self->{C}})
    {
	return $kid if $kid->isElementNode;
    }
    undef;
}

sub getOwnerDocument
{
    undef;
}

sub getImplementation 
{
    $XML::DOM::DOMImplementation::Singleton;
}

#
# Added extra parameters ($val, $specified) that are passed straight to the
# Attr constructor
# 
sub createAttribute
{
    new XML::DOM::Attr (@_);
}

sub createCDATASection
{
    new XML::DOM::CDATASection (@_);
}

sub createComment
{
    new XML::DOM::Comment (@_);

}

sub createElement
{
    new XML::DOM::Element (@_);
}

sub createTextNode
{
    new XML::DOM::Text (@_);
}

sub createProcessingInstruction
{
    new XML::DOM::ProcessingInstruction (@_);
}

sub createEntityReference
{
    new XML::DOM::EntityReference (@_);
}

sub createDocumentFragment
{
    new XML::DOM::DocumentFragment (@_);
}

sub createDocumentType
{
    new XML::DOM::DocumentType (@_);
}

sub cloneNode
{
    my ($self, $deep) = @_;
    my $node = new XML::DOM::Document;

    $node->cloneChildren ($self, $deep);

    my $xmlDecl = $self->{XmlDecl};
    $node->{XmlDecl} = $xmlDecl->cloneNode ($deep) if defined $xmlDecl;

    $node;
}

sub appendChild
{
    my ($self, $node) = @_;

    # Extra check: make sure sure we don't end up with more than 1 Elements.
    # Don't worry about multiple DocType nodes, because DocumentFragment
    # can't contain DocType nodes.

    my @nodes = ($node);
    @nodes = @{$node->{C}}
        if $node->getNodeType == DOCUMENT_FRAGMENT_NODE;
    
    my $elem = 0;
    for my $n (@nodes)
    {
	$elem++ if $n->isElementNode;
    }
    
    if ($elem > 0 && defined ($self->getDocumentElement))
    {
	croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
					  "document can have only 1 Element");
    }
    $self->SUPER::appendChild ($node);
}

sub insertBefore
{
    my ($self, $node, $refNode) = @_;

    # Extra check: make sure sure we don't end up with more than 1 Elements.
    # Don't worry about multiple DocType nodes, because DocumentFragment
    # can't contain DocType nodes.

    my @nodes = ($node);
    @nodes = @{$node->{C}}
	if $node->getNodeType == DOCUMENT_FRAGMENT_NODE;
    
    my $elem = 0;
    for my $n (@nodes)
    {
	$elem++ if $n->isElementNode;
    }
    
    if ($elem > 0 && defined ($self->getDocumentElement))
    {
	croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
					  "document can have only 1 Element");
    }
    $self->SUPER::insertBefore ($node, $refNode);
}

sub replaceChild
{
    my ($self, $node, $refNode) = @_;

    # Extra check: make sure sure we don't end up with more than 1 Elements.
    # Don't worry about multiple DocType nodes, because DocumentFragment
    # can't contain DocType nodes.

    my @nodes = ($node);
    @nodes = @{$node->{C}}
	if $node->getNodeType == DOCUMENT_FRAGMENT_NODE;
    
    my $elem = 0;
    $elem-- if $refNode->isElementNode;

    for my $n (@nodes)
    {
	$elem++ if $n->isElementNode;
    }
    
    if ($elem > 0 && defined ($self->getDocumentElement))
    {
	croak new XML::DOM::DOMException (HIERARCHY_REQUEST_ERR,
					  "document can have only 1 Element");
    }
    $self->SUPER::appendChild ($node, $refNode);
}

#------------------------------------------------------------
# Extra method implementations

sub isReadOnly
{
    0;
}

sub print
{
    my ($self, $FILE) = @_;

    my $xmlDecl = $self->getXMLDecl;
    if (defined $xmlDecl)
    {
	$xmlDecl->print ($FILE);
	$FILE->print ("\x0A");
    }

    for my $node (@{$self->{C}})
    {
	$node->print ($FILE);
	$FILE->print ("\x0A");
    }
}

sub setDoctype
{
    my ($self, $doctype) = @_;
    my $oldDoctype = $self->{Doctype};
    if (defined $oldDoctype)
    {
	$self->replaceChild ($doctype, $oldDoctype);
    }
    else
    {
#?? before root element!
	$self->appendChild ($doctype);
    }
    $_[0]->{Doctype} = $_[1];
}

sub removeDoctype
{
    my $self = shift;
    my $doctype = $self->removeChild ($self->{Doctype});

    delete $self->{Doctype};
    $doctype;
}

sub rejectChild
{
    my $t = $_[1]->getNodeType;
    $t != ELEMENT_NODE
	&& $t != PROCESSING_INSTRUCTION_NODE
	&& $t != COMMENT_NODE
	&& $t != DOCUMENT_TYPE_NODE;
}

sub expandEntity
{
    my ($self, $ent, $param) = @_;
    my $doctype = $self->getDoctype;

    (defined $doctype) ? $doctype->expandEntity ($ent, $param) : undef;
}

sub getDefaultAttrValue
{
    my ($self, $elem, $attr) = @_;
    
    my $doctype = $self->getDoctype;

    (defined $doctype) ? $doctype->getDefaultAttrValue ($elem, $attr) : undef;
}

sub getEntity
{
    my ($self, $entity) = @_;
    
    my $doctype = $self->getDoctype;

    (defined $doctype) ? $doctype->getEntity ($entity) : undef;
}

sub dispose
{
    my $self = shift;

    $self->{XmlDecl}->dispose if defined $self->{XmlDecl};
    delete $self->{XmlDecl};
    delete $self->{Doctype};
    $self->SUPER::dispose;
}

sub setOwnerDocument
{
    # Do nothing, you can't change the owner document!
}

sub getXMLDecl
{
    $_[0]->{XmlDecl};
}

sub setXMLDecl
{
    $_[0]->{XmlDecl} = $_[1];
}

sub createXMLDecl
{
    new XML::DOM::XMLDecl (@_);
}

sub createNotation
{
    new XML::DOM::Notation (@_);
}

sub createElementDecl
{
    new XML::DOM::ElementDecl (@_);
}

sub createAttlistDecl
{
    new XML::DOM::AttlistDecl (@_);
}

sub createEntity
{
    new XML::DOM::Entity (@_);
}

######################################################################
package XML::DOM::Parser;
######################################################################
use vars qw ( @ISA );
@ISA = qw( XML::Parser );

sub new
{
    my ($class, %args) = @_;

    $args{Style} = 'Dom';
    $class->SUPER::new (%args);
}

# This method needed to be overriden so we can restore some global 
# variables when an exception is thrown
sub parse
{
    my $self = shift;

    local $XML::Parser::Dom::_DP_doc;
    local $XML::Parser::Dom::_DP_elem;
    local $XML::Parser::Dom::_DP_doctype;
    local $XML::Parser::Dom::_DP_in_prolog;
    local $XML::Parser::Dom::_DP_end_doc;
    local $XML::Parser::Dom::_DP_saw_doctype;
    local $XML::Parser::Dom::_DP_in_CDATA;
    local $XML::Parser::Dom::_DP_keep_CDATA;
    local $XML::Parser::Dom::_DP_last_text;


    # Temporarily disable checks that Expat already does (for performance)
    local $XML::DOM::SafeMode = 0;
    # Temporarily disable ReadOnly checks
    local $XML::DOM::IgnoreReadOnly = 1;

    my $ret;
    eval {
	$ret = $self->SUPER::parse (@_);
    };
    my $err = $@;

    if ($err)
    {
	my $doc = $XML::Parser::Dom::_DP_doc;
	if ($doc)
	{
	    $doc->dispose;
	}
	die $err;
    }

    $ret;
}

######################################################################
package XML::Parser::Dom;
######################################################################

use vars qw( $_DP_doc
	     $_DP_elem
	     $_DP_doctype
	     $_DP_in_prolog
	     $_DP_end_doc
	     $_DP_saw_doctype
	     $_DP_in_CDATA
	     $_DP_keep_CDATA
	     $_DP_last_text
	   );

# This adds a new Style to the XML::Parser class.
# From now on you can say: $parser = new XML::Parser ('Style' => 'Dom' );
# but that is *NOT* how a regular user should use it!
$XML::Parser::Built_In_Styles{Dom} = 1;

sub Init
{
    $_DP_elem = $_DP_doc = new XML::DOM::Document();
    $_DP_doctype = new XML::DOM::DocumentType ($_DP_doc);
    $_DP_doc->setDoctype ($_DP_doctype);
    $_DP_keep_CDATA = $_[0]->{KeepCDATA};
  
    # Prepare for document prolog
    $_DP_in_prolog = 1;
#    $expat->{DOM_inProlog} = 1;

    # We haven't passed the root element yet
    $_DP_end_doc = 0;

    undef $_DP_last_text;
}

sub Final
{
    unless ($_DP_saw_doctype)
    {
	my $doctype = $_DP_doc->removeDoctype;
	$doctype->dispose;
    }
    $_DP_doc;
}

sub Char
{
    my $str = $_[1];

    if ($_DP_in_CDATA && $_DP_keep_CDATA)
    {
	undef $_DP_last_text;
	# Merge text with previous node if possible
	$_DP_elem->addCDATA ($str);
    }
    else
    {
	# Merge text with previous node if possible
	# Used to be:	$expat->{DOM_Element}->addText ($str);
	if ($_DP_last_text)
	{
	    $_DP_last_text->{Data} .= $str;
	}
	else
	{
	    $_DP_last_text = $_DP_doc->createTextNode ($str);
	    $_DP_last_text->{Parent} = $_DP_elem;
	    push @{$_DP_elem->{C}}, $_DP_last_text;
	}
    }
}

sub Start
{
    my ($expat, $elem, @attr) = @_;
    my $parent = $_DP_elem;
    my $doc = $_DP_doc;
    
    if ($parent == $doc)
    {
	# End of document prolog, i.e. start of first Element
	$_DP_in_prolog = 0;
    }
    
    undef $_DP_last_text;
    my $node = $doc->createElement ($elem);
    $_DP_elem = $node;
    $parent->appendChild ($node);
    
    my $first_default = $expat->specified_attr;
    my $i = 0;
    my $n = @attr;
    while ($i < $n)
    {
	my $specified = $i < $first_default;
	my $name = $attr[$i++];
	undef $_DP_last_text;
	my $attr = $doc->createAttribute ($name, $attr[$i++], $specified);
	$node->setAttributeNode ($attr);
    }
}

sub End
{
    $_DP_elem = $_DP_elem->{Parent};
    undef $_DP_last_text;

    # Check for end of root element
    $_DP_end_doc = 1 if ($_DP_elem == $_DP_doc);
}

# Called at end of file, i.e. whitespace following last closing tag
# Also for Entity references
# May also be called at other times...
sub Default
{
    my ($expat, $str) = @_;

#    shift; deb ("Default", @_);

    if ($_DP_in_prolog)	# still processing Document prolog...
    {
#?? could try to store this text later
#?? I've only seen whitespace here so far
    }
    elsif (!$_DP_end_doc)	# ignore whitespace at end of Document
    {
#	if ($expat->{NoExpand})
#	{
	    $str =~ /^&(.+);$/os;
	    return unless defined ($1);
	    # Got a TextDecl (<?xml ...?>) from an external entity here once

	    $_DP_elem->appendChild (
			$_DP_doc->createEntityReference ($1));
	    undef $_DP_last_text;
#	}
#	else
#	{
#	    $expat->{DOM_Element}->addText ($str);
#	}
    }
}

# XML::Parser 2.19 added support for CdataStart and CdataEnd handlers
# If they are not defined, the Default handler is called instead
# with the text "<![CDATA[" and "]]"
sub CdataStart
{
    $_DP_in_CDATA = 1;
}

sub CdataEnd
{
    $_DP_in_CDATA = 0;
}

sub Comment
{
    undef $_DP_last_text;
    my $comment = $_DP_doc->createComment ($_[1]);
    $_DP_elem->appendChild ($comment);
}

sub deb
{
    return;

    my $name = shift;
    print "$name (" . join(",", map {defined($_)?$_ : "(undef)"} @_) . ")\n";
}

sub Doctype
{
    my $expat = shift;
#    deb ("Doctype", @_);

    $_DP_doctype->setParams (@_);
    $_DP_saw_doctype = 1;
}

sub Attlist
{
    my $expat = shift;
#    deb ("Attlist", @_);

    $_DP_doctype->addAttDef (@_);
}

sub XMLDecl
{
    my $expat = shift;
#    deb ("XMLDecl", @_);

    undef $_DP_last_text;
    $_DP_doc->setXMLDecl (new XML::DOM::XMLDecl ($_DP_doc, @_));
}

sub Entity
{
    my $expat = shift;
#    deb ("Entity", @_);
    
    # Parameter Entities names are passed starting with '%'
    my $parameter = 0;
    if ($_[0] =~ /^%(.*)/s)
    {
	$_[0] = $1;
	$parameter = 1;
    }

    undef $_DP_last_text;
    $_DP_doctype->addEntity ($parameter, @_);
}

# Unparsed is called when it encounters e.g:
#
#   <!ENTITY logo SYSTEM "http://server/logo.gif" NDATA gif>
#
sub Unparsed
{
    Entity (@_);	# same as regular ENTITY, as far as DOM is concerned
}

sub Element
{
    shift;
#    deb ("Element", @_);

    undef $_DP_last_text;
    $_DP_doctype->addElementDecl (@_);
}

sub Notation
{
    shift;
#    deb ("Notation", @_);

    undef $_DP_last_text;
    $_DP_doctype->addNotation (@_);
}

sub Proc
{
    shift;
#    deb ("Proc", @_);

    undef $_DP_last_text;
    $_DP_elem->appendChild (new XML::DOM::ProcessingInstruction ($_DP_doc, @_));
}

# ExternEnt is called when an external entity, such as:
#
#	<!ENTITY externalEntity PUBLIC "-//Enno//TEXT Enno's description//EN" 
#	                        "http://server/descr.txt">
#
# is referenced in the document, e.g. with: &externalEntity;
# If ExternEnt is not specified, the entity reference is passed to the Default
# handler as e.g. "&externalEntity;", where an EntityReference onbject is added.
#
#sub ExternEnt
#{
#    deb ("ExternEnt", @_);
#}

1; # module return code

__END__

=head1 NAME

XML::DOM - A perl module for building DOM Level 1 compliant document structures

=head1 SYNOPSIS

 use XML::DOM;

 my $parser = new XML::DOM::Parser;
 my $doc = $parser->parsefile ("file.xml");

 # print all HREF attributes of all CODEBASE elements
 my $nodes = $doc->getElementsByTagName ("CODEBASE");
 my $n = $nodes->getLength;

 for (my $i = 0; $i < $n; $i++)
 {
     my $node = $nodes->item ($i);
     my $href = $node->getAttribute ("HREF");
     print $href->getValue . "\n";
 }

 $doc->printToFile ("out.xml");

 print $doc->toString;

=head1 DESCRIPTION

This module extends the XML::Parser module by Clark Cooper. 
The XML::Parser module is built on top of XML::Parser::Expat, 
which is a lower level interface to James Clark's expat library.

XML::DOM::Parser is derived from XML::Parser. It parses XML strings or files
and builds a data structure that conforms to the API of the Document Object 
Model as described at http://www.w3.org/TR/REC-DOM-Level-1.
See the XML::Parser manpage for other available features of the 
XML::DOM::Parser class. 
Note that the 'Style' property should not be used (it is set internally.)

The XML::Parser I<NoExpand> option is more or less supported, in that it will
generate EntityReference objects whenever an entity reference is encountered
in character data. I'm not sure how useful this is. Any comments are welcome.

As described in the synopsis, when you create an XML::DOM::Parser object, 
the parse and parsefile methods create an I<XML::DOM::Document> object
from the specified input. This Document object can then be examined, modified and
written back out to a file or converted to a string.

When using XML::DOM with XML::Parser version 2.19 and up, setting the 
XML::DOM::Parser option I<KeepCDATA> to 1 will store CDATASections in
CDATASection nodes, instead of converting them to Text nodes.
Subsequent CDATASection nodes will be merged into one. Let me know if this
is a problem.

A Document has a tree structure consisting of I<Node> objects. A Node may contain
other nodes, depending on its type.
A Document may have Element, Text, Comment, and CDATASection nodes. 
Element nodes may have Attr, Element, Text, Comment, and CDATASection nodes. 
The other nodes may not have any child nodes. 

This module adds several node types that are not part of the DOM spec (yet.)
These are: ElementDecl (for <!ELEMENT ...> declarations), AttlistDecl (for
<!ATTLIST ...> declarations), XMLDecl (for <?xml ...?> declarations) and AttDef
(for attribute definitions in an AttlistDecl.)

=head1 DOM API

=over 4

=item XML::DOM

=over 4

=item Constant definitions

The following predefined constants indicate which type of node it is.

=back

 UNKNOWN_NODE (0)                The node type is unknown (not part of DOM)

 ELEMENT_NODE (1)                The node is an Element.
 ATTRIBUTE_NODE (2)              The node is an Attr.
 TEXT_NODE (3)                   The node is a Text node.
 CDATA_SECTION_NODE (4)          The node is a CDATASection.
 ENTITY_REFERENCE_NODE (5)       The node is an EntityReference.
 ENTITY_NODE (6)                 The node is an Entity.
 PROCESSING_INSTRUCTION_NODE (7) The node is a ProcessingInstruction.
 COMMENT_NODE (8)                The node is a Comment.
 DOCUMENT_NODE (9)               The node is a Document.
 DOCUMENT_TYPE_NODE (10)         The node is a DocumentType.
 DOCUMENT_FRAGMENT_NODE (11)     The node is a DocumentFragment.
 NOTATION_NODE (12)              The node is a Notation.

 ELEMENT_DECL_NODE (13)		 The node is an ElementDecl (not part of DOM)
 ATT_DEF_NODE (14)		 The node is an AttDef (not part of DOM)
 XML_DECL_NODE (15)		 The node is an XMLDecl (not part of DOM)
 ATTLIST_DECL_NODE (16)		 The node is an AttlistDecl (not part of DOM)

 Usage:

   if ($node->getNodeType == ELEMENT_NODE)
   {
       print "It's an Element";
   }

B<Not In DOM Spec>: The DOM Spec does not mention UNKNOWN_NODE and, 
quite frankly, you should never encounter it. The last 4 node types were added
to support the 4 added node classes.

=head2 Global Variables

=over 4

=item $VERSION

The variable $XML::DOM::VERSION contains the version number of this 
implementation, e.g. "1.07".

=head2 Additional methods not in the DOM Spec

=item getIgnoreReadOnly and ignoreReadOnly (readOnly)

The DOM Level 1 Spec does not allow you to edit certain sections of the document,
e.g. the DocumentType, so by default this implementation throws DOMExceptions
(i.e. NO_MODIFICATION_ALLOWED_ERR) when you try to edit a readonly node. 
These readonly checks can be disabled by (temporarily) setting the global 
IgnoreReadOnly flag.

The ignoreReadOnly method sets the global IgnoreReadOnly flag and returns its
previous value. The getIgnoreReadOnly method simply returns its current value.

 my $oldIgnore = XML::DOM::ignoreReadOnly (1);
 eval {
 ... do whatever you want, catching any other exceptions ...
 };
 XML::DOM::ignoreReadOnly ($oldIgnore);     # restore previous value

=item isValidName (name)

Whether the specified name is a valid "Name" as specified in the XML spec.
Characters with Unicode values > 127 are now also supported.

=item getAllowReservedNames and allowReservedNames (boolean)

The first method returns whether reserved names are allowed. 
The second takes a boolean argument and sets whether reserved names are allowed.
The initial value is 1 (i.e. allow reserved names.)

The XML spec states that "Names" starting with (X|x)(M|m)(L|l)
are reserved for future use. (Amusingly enough, the XML version of the XML spec
(REC-xml-19980210.xml) breaks that very rule by defining an ENTITY with the name 
'xmlpio'.)
A "Name" in this context means the Name token as found in the BNF rules in the
XML spec.

XML::DOM only checks for errors when you modify the DOM tree, not when the
DOM tree is built by the XML::DOM::Parser.

=item setTagCompression (funcref)

There are 3 possible styles for printing empty Element tags:

=over 4

=item Style 0

 <empty/> or <empty attr="val"/>

XML::DOM uses this style by default for all Elements.

=item Style 1

  <empty></empty> or <empty attr="val"></empty>

=item Style 2

  <empty /> or <empty attr="val" />

This style is sometimes desired when using XHTML. 
(Note the extra space before the slash "/")
See http://www.w3.org/TR/WD-html-in-xml Appendix C for more details.

=back

By default XML::DOM compresses all empty Element tags (style 0.)
You can control which style is used for a particular Element by calling
XML::DOM::setTagCompression with a reference to a function that takes
2 arguments. The first is the tag name of the Element, the second is the
XML::DOM::Element that is being printed. 
The function should return 0, 1 or 2 to indicate which style should be used to
print the empty tag. E.g.

 XML::DOM::setTagCompression (\&my_tag_compression);

 sub my_tag_compression
 {
    my ($tag, $elem) = @_;

    # Print empty br, hr and img tags like this: <br />
    return 2 if $tag =~ /^(br|hr|img)$/;

    # Print other empty tags like this: <empty></empty>
    return 1;
 }

=back

=item XML::DOM::Node

=head2 Global Variables

=over 4

=item @NodeNames

The variable @XML::DOM::Node::NodeNames maps the node type constants to strings.
It is used by XML::DOM::Node::getNodeTypeName.

=head2 Methods

=item getNodeType

Return an integer indicating the node type. See XML::DOM constants.

=item getNodeName

Return a property or a hardcoded string, depending on the node type.
Here are the corresponding functions or values:

 Attr			getName
 AttDef			getName
 AttlistDecl		getName
 CDATASection		"#cdata-section"
 Comment		"#comment"
 Document		"#document"
 DocumentType		getNodeName
 DocumentFragment	"#document-fragment"
 Element		getTagName
 ElementDecl		getName
 EntityReference	getEntityName
 Entity			getNotationName
 Notation		getName
 ProcessingInstruction	getTarget
 Text			"#text"
 XMLDecl		"#xml-declaration"

B<Not In DOM Spec>: AttDef, AttlistDecl, ElementDecl and XMLDecl were added for
completeness.

=item getNodeValue and setNodeValue (value)

Returns a string or undef, depending on the node type. This method is provided 
for completeness. In other languages it saves the programmer an upcast.
The value is either available thru some other method defined in the subclass, or
else undef is returned. Here are the corresponding methods: 
Attr::getValue, Text::getData, CDATASection::getData, Comment::getData, 
ProcessingInstruction::getData.

=item getParentNode and setParentNode (parentNode)

The parent of this node. All nodes, except Document,
DocumentFragment, and Attr may have a parent. However, if a
node has just been created and not yet added to the tree, or
if it has been removed from the tree, this is undef.

=item getChildNodes

A NodeList that contains all children of this node. If there
are no children, this is a NodeList containing no nodes. The
content of the returned NodeList is "live" in the sense that,
for instance, changes to the children of the node object that
it was created from are immediately reflected in the nodes
returned by the NodeList accessors; it is not a static
snapshot of the content of the node. This is true for every
NodeList, including the ones returned by the
getElementsByTagName method.

NOTE: this implementation does not return a "live" NodeList for
getElementsByTagName. See L<CAVEATS>.

When this method is called in a list context, it returns a regular perl list
containing the child nodes. Note that this list is not "live". E.g.

 @list = $node->getChildNodes;	      # returns a perl list
 $nodelist = $node->getChildNodes;    # returns a NodeList (object reference)
 for my $kid ($node->getChildNodes)   # iterate over the children of $node

=item getFirstChild

The first child of this node. If there is no such node, this returns undef.

=item getLastChild

The last child of this node. If there is no such node, this returns undef.

=item getPreviousSibling

The node immediately preceding this node. If there is no such 
node, this returns undef.

=item getNextSibling

The node immediately following this node. If there is no such node, this returns 
undef.

=item getAttributes

A NamedNodeMap containing the attributes (Attr nodes) of this node 
(if it is an Element) or undef otherwise.
Note that adding/removing attributes from the returned object, also adds/removes
attributes from the Element node that the NamedNodeMap came from.

=item getOwnerDocument

The Document object associated with this node. This is also
the Document object used to create new nodes. When this node
is a Document this is undef.

=item insertBefore (newChild, refChild)

Inserts the node newChild before the existing child node
refChild. If refChild is undef, insert newChild at the end of
the list of children.

If newChild is a DocumentFragment object, all of its children
are inserted, in the same order, before refChild. If the
newChild is already in the tree, it is first removed.

Return Value: The node being inserted.

DOMExceptions:

=over 4

=item * HIERARCHY_REQUEST_ERR

Raised if this node is of a type that does not allow children of the type of
the newChild node, or if the node to insert is one of this node's ancestors.

=item * WRONG_DOCUMENT_ERR

Raised if newChild was created from a different document than the one that 
created this node.

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=item * NOT_FOUND_ERR

Raised if refChild is not a child of this node.

=back

=item replaceChild (newChild, oldChild)

Replaces the child node oldChild with newChild in the list of
children, and returns the oldChild node. If the newChild is
already in the tree, it is first removed.

Return Value: The node replaced.

DOMExceptions:

=over 4

=item * HIERARCHY_REQUEST_ERR

Raised if this node is of a type that does not allow children of the type of
the newChild node, or it the node to put in is one of this node's ancestors.

=item * WRONG_DOCUMENT_ERR

Raised if newChild was created from a different document than the one that 
created this node.

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=item * NOT_FOUND_ERR

Raised if oldChild is not a child of this node.

=back

=item removeChild (oldChild)

Removes the child node indicated by oldChild from the list of
children, and returns it.

Return Value: The node removed.

DOMExceptions:

=over 4

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=item * NOT_FOUND_ERR

Raised if oldChild is not a child of this node.

=back

=item appendChild (newChild)

Adds the node newChild to the end of the list of children of
this node. If the newChild is already in the tree, it is
first removed. If it is a DocumentFragment object, the entire contents of 
the document fragment are moved into the child list of this node

Return Value: The node added.

DOMExceptions:

=over 4

=item * HIERARCHY_REQUEST_ERR

Raised if this node is of a type that does not allow children of the type of
the newChild node, or if the node to append is one of this node's ancestors.

=item * WRONG_DOCUMENT_ERR

Raised if newChild was created from a different document than the one that 
created this node.

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=back

=item hasChildNodes

This is a convenience method to allow easy determination of
whether a node has any children.

Return Value: 1 if the node has any children, 0 otherwise.

=item cloneNode (deep)

Returns a duplicate of this node, i.e., serves as a generic
copy constructor for nodes. The duplicate node has no parent
(parentNode returns undef.).

Cloning an Element copies all attributes and their values,
including those generated by the XML processor to represent
defaulted attributes, but this method does not copy any text
it contains unless it is a deep clone, since the text is
contained in a child Text node. Cloning any other type of
node simply returns a copy of this node.

Parameters: 
 I<deep>   If true, recursively clone the subtree under the specified node.
If false, clone only the node itself (and its attributes, if it is an Element).

Return Value: The duplicate node.

=item normalize

Puts all Text nodes in the full depth of the sub-tree
underneath this Element into a "normal" form where only
markup (e.g., tags, comments, processing instructions, CDATA
sections, and entity references) separates Text nodes, i.e.,
there are no adjacent Text nodes. This can be used to ensure
that the DOM view of a document is the same as if it were
saved and re-loaded, and is useful when operations (such as
XPointer lookups) that depend on a particular document tree
structure are to be used.

B<Not In DOM Spec>: In the DOM Spec this method is defined in the Element and 
Document class interfaces only, but it doesn't hurt to have it here...

=item getElementsByTagName (name [, recurse])

Returns a NodeList of all descendant elements with a given
tag name, in the order in which they would be encountered in
a preorder traversal of the Element tree.

Parameters:
 I<name>  The name of the tag to match on. The special value "*" matches all tags.
 I<recurse>  Whether it should return only direct child nodes (0) or any descendant that matches the tag name (1). This argument is optional and defaults to 1. It is not part of the DOM spec.

Return Value: A list of matching Element nodes.

NOTE: this implementation does not return a "live" NodeList for
getElementsByTagName. See L<CAVEATS>.

When this method is called in a list context, it returns a regular perl list
containing the result nodes. E.g.

 @list = $node->getElementsByTagName("tag");       # returns a perl list
 $nodelist = $node->getElementsByTagName("tag");   # returns a NodeList (object ref.)
 for my $elem ($node->getElementsByTagName("tag")) # iterate over the result nodes

=head2 Additional methods not in the DOM Spec

=item getNodeTypeName

Return the string describing the node type. 
E.g. returns "ELEMENT_NODE" if getNodeType returns ELEMENT_NODE.
It uses @XML::DOM::Node::NodeNames.

=item toString

Returns the entire subtree as a string.

=item printToFile (filename)

Prints the entire subtree to the file with the specified filename.

Croaks: if the file could not be opened for writing.

=item printToFileHandle (handle)

Prints the entire subtree to the file handle.
E.g. to print to STDOUT:

 $node->printToFileHandle (\*STDOUT);

=item print (obj)

Prints the entire subtree using the object's print method. E.g to print to a
FileHandle object:

 $f = new FileHandle ("file.out", "w");
 $node->print ($f);

=item getChildIndex (child)

Returns the index of the child node in the list returned by getChildNodes.

Return Value: the index or -1 if the node is not found.

=item getChildAtIndex (index)

Returns the child node at the specifed index or undef.

=item addText (text)

Appends the specified string to the last child if it is a Text node, or else 
appends a new Text node (with the specified text.)

Return Value: the last child if it was a Text node or else the new Text node.

=item dispose

Removes all circular references in this node and its descendants so the 
objects can be claimed for garbage collection. The objects should not be used
afterwards.

=item setOwnerDocument (doc)

Sets the ownerDocument property of this node and all its children (and 
attributes etc.) to the specified document.
This allows the user to cut and paste document subtrees between different
XML::DOM::Documents. The node should be removed from the original document
first, before calling setOwnerDocument.

This method does nothing when called on a Document node.

=item isAncestor (parent)

Returns 1 if parent is an ancestor of this node or if it is this node itself.

=item expandEntityRefs (str)

Expands all the entity references in the string and returns the result.
The entity references can be character references (e.g. "&#123;" or "&#x1fc2"),
default entity references ("&quot;", "&gt;", "&lt;", "&apos;" and "&amp;") or
entity references defined in Entity objects as part of the DocumentType of
the owning Document. Character references are expanded into UTF-8.
Parameter entity references (e.g. %ent;) are not expanded.

=back

=item Interface XML::DOM::NodeList

The NodeList interface provides the abstraction of an ordered
collection of nodes, without defining or constraining how this
collection is implemented.

The items in the NodeList are accessible via an integral index,
starting from 0.

Although the DOM spec states that all NodeLists are "live" in that they
allways reflect changes to the DOM tree, the NodeList returned by
getElementsByTagName is not live in this implementation. See L<CAVEATS>
for details.

=over 4

=item item (index)

Returns the indexth item in the collection. If index is
greater than or equal to the number of nodes in the list,
this returns undef.

=item getLength

The number of nodes in the list. The range of valid child
node indices is 0 to length-1 inclusive.

=head2 Additional methods not in the DOM Spec

=item dispose

Removes all circular references in this NodeList and its descendants so the 
objects can be claimed for garbage collection. The objects should not be used
afterwards.

=back

=item Interface XML::DOM::NamedNodeMap

Objects implementing the NamedNodeMap interface are used to represent
collections of nodes that can be accessed by name. Note that
NamedNodeMap does not inherit from NodeList; NamedNodeMaps are not
maintained in any particular order. Objects contained in an object
implementing NamedNodeMap may also be accessed by an ordinal index, but
this is simply to allow convenient enumeration of the contents of a
NamedNodeMap, and does not imply that the DOM specifies an order to
these Nodes.

Note that in this implementation, the objects added to a NamedNodeMap
are kept in order.

=over 4

=item getNamedItem (name)

Retrieves a node specified by name.

Return Value: A Node (of any type) with the specified name, or undef if
the specified name did not identify any node in the map.

=item setNamedItem (arg)

Adds a node using its nodeName attribute.

As the nodeName attribute is used to derive the name which
the node must be stored under, multiple nodes of certain
types (those that have a "special" string value) cannot be
stored as the names would clash. This is seen as preferable
to allowing nodes to be aliased.

Parameters:
 I<arg>  A node to store in a named node map. 

The node will later be accessible using the value of the nodeName
attribute of the node. If a node with that name is
already present in the map, it is replaced by the new one.

Return Value: If the new Node replaces an existing node with the same
name the previously existing Node is returned, otherwise undef is returned.

DOMExceptions:

=over 4

=item * WRONG_DOCUMENT_ERR

Raised if arg was created from a different document than the one that 
created the NamedNodeMap.

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this NamedNodeMap is readonly.

=item * INUSE_ATTRIBUTE_ERR

Raised if arg is an Attr that is already an attribute of another Element object.
The DOM user must explicitly clone Attr nodes to re-use them in other elements.

=back

=item removeNamedItem (name)

Removes a node specified by name. If the removed node is an
Attr with a default value it is immediately replaced.

Return Value: The node removed from the map or undef if no node with
such a name exists.

DOMException:

=over 4

=item * NOT_FOUND_ERR

Raised if there is no node named name in the map.

=back

=item item (index)

Returns the indexth item in the map. If index is greater than
or equal to the number of nodes in the map, this returns undef.

Return Value: The node at the indexth position in the NamedNodeMap, or
undef if that is not a valid index.

=item getLength

Returns the number of nodes in the map. The range of valid child node
indices is 0 to length-1 inclusive.

=head2 Additional methods not in the DOM Spec

=item getValues

Returns a NodeList with the nodes contained in the NamedNodeMap.
The NodeList is "live", in that it reflects changes made to the NamedNodeMap.

When this method is called in a list context, it returns a regular perl list
containing the values. Note that this list is not "live". E.g.

 @list = $map->getValues;	 # returns a perl list
 $nodelist = $map->getValues;    # returns a NodeList (object ref.)
 for my $val ($map->getValues)   # iterate over the values

=item getChildIndex (node)

Returns the index of the node in the NodeList as returned by getValues, or -1
if the node is not in the NamedNodeMap.

=item dispose

Removes all circular references in this NamedNodeMap and its descendants so the 
objects can be claimed for garbage collection. The objects should not be used
afterwards.

=back

=item Interface XML::DOM::CharacterData extends XML::DOM::Node

The CharacterData interface extends Node with a set of attributes and
methods for accessing character data in the DOM. For clarity this set
is defined here rather than on each object that uses these attributes
and methods. No DOM objects correspond directly to CharacterData,
though Text, Comment and CDATASection do inherit the interface from it. 
All offsets in this interface start from 0.

=over 4

=item getData and setData (data)

The character data of the node that implements this
interface. The DOM implementation may not put arbitrary
limits on the amount of data that may be stored in a
CharacterData node. However, implementation limits may mean
that the entirety of a node's data may not fit into a single
DOMString. In such cases, the user may call substringData to
retrieve the data in appropriately sized pieces.

=item getLength

The number of characters that are available through data and
the substringData method below. This may have the value zero,
i.e., CharacterData nodes may be empty.

=item substringData (offset, count)

Extracts a range of data from the node.

Parameters:
 I<offset>  Start offset of substring to extract.
 I<count>   The number of characters to extract.

Return Value: The specified substring. If the sum of offset and count
exceeds the length, then all characters to the end of
the data are returned.

=item appendData (str)

Appends the string to the end of the character data of the
node. Upon success, data provides access to the concatenation
of data and the DOMString specified.

=item insertData (offset, arg)

Inserts a string at the specified character offset.

Parameters:
 I<offset>  The character offset at which to insert.
 I<arg>     The DOMString to insert.

=item deleteData (offset, count)

Removes a range of characters from the node. 
Upon success, data and length reflect the change.
If the sum of offset and count exceeds length then all characters 
from offset to the end of the data are deleted.

Parameters: 
 I<offset>  The offset from which to remove characters. 
 I<count>   The number of characters to delete. 

=item replaceData (offset, count, arg)

Replaces the characters starting at the specified character
offset with the specified string.

Parameters:
 I<offset>  The offset from which to start replacing.
 I<count>   The number of characters to replace. 
 I<arg>     The DOMString with which the range must be replaced.

If the sum of offset and count exceeds length, then all characters to the end of
the data are replaced (i.e., the effect is the same as a remove method call with 
the same range, followed by an append method invocation).

=back

=item XML::DOM::Attr extends XML::DOM::Node

=over 4

The Attr nodes built by the XML::DOM::Parser always have one child node
which is a Text node containing the expanded string value (i.e. EntityReferences
are always expanded.) EntityReferences may be added when modifying or creating
a new Document.

The Attr interface represents an attribute in an Element object.
Typically the allowable values for the attribute are defined in a
document type definition.

Attr objects inherit the Node interface, but since they are not
actually child nodes of the element they describe, the DOM does not
consider them part of the document tree. Thus, the Node attributes
parentNode, previousSibling, and nextSibling have a undef value for Attr
objects. The DOM takes the view that attributes are properties of
elements rather than having a separate identity from the elements they
are associated with; this should make it more efficient to implement
such features as default attributes associated with all elements of a
given type. Furthermore, Attr nodes may not be immediate children of a
DocumentFragment. However, they can be associated with Element nodes
contained within a DocumentFragment. In short, users and implementors
of the DOM need to be aware that Attr nodes have some things in common
with other objects inheriting the Node interface, but they also are
quite distinct.

The attribute's effective value is determined as follows: if this
attribute has been explicitly assigned any value, that value is the
attribute's effective value; otherwise, if there is a declaration for
this attribute, and that declaration includes a default value, then
that default value is the attribute's effective value; otherwise, the
attribute does not exist on this element in the structure model until
it has been explicitly added. Note that the nodeValue attribute on the
Attr instance can also be used to retrieve the string version of the
attribute's value(s).

In XML, where the value of an attribute can contain entity references,
the child nodes of the Attr node provide a representation in which
entity references are not expanded. These child nodes may be either
Text or EntityReference nodes. Because the attribute type may be
unknown, there are no tokenized attribute values.

=item getValue

On retrieval, the value of the attribute is returned as a string. 
Character and general entity references are replaced with their values.

=item setValue (str)

DOM Spec: On setting, this creates a Text node with the unparsed contents of the 
string.

=item getName

Returns the name of this attribute.

=back

=item XML::DOM::Element extends XML::DOM::Node

By far the vast majority of objects (apart from text) that authors
encounter when traversing a document are Element nodes. Assume the
following XML document:

     <elementExample id="demo">
       <subelement1/>
       <subelement2><subsubelement/></subelement2>
     </elementExample>

When represented using DOM, the top node is an Element node for
"elementExample", which contains two child Element nodes, one for
"subelement1" and one for "subelement2". "subelement1" contains no
child nodes.

Elements may have attributes associated with them; since the Element
interface inherits from Node, the generic Node interface method
getAttributes may be used to retrieve the set of all attributes for an
element. There are methods on the Element interface to retrieve either
an Attr object by name or an attribute value by name. In XML, where an
attribute value may contain entity references, an Attr object should be
retrieved to examine the possibly fairly complex sub-tree representing
the attribute value. On the other hand, in HTML, where all attributes
have simple string values, methods to directly access an attribute
value can safely be used as a convenience.

=over 4

=item getTagName

The name of the element. For example, in:

               <elementExample id="demo">
                       ...
               </elementExample>

tagName has the value "elementExample". Note that this is
case-preserving in XML, as are all of the operations of the
DOM.

=item getAttribute (name)

Retrieves an attribute value by name.

Return Value: The Attr value as a string, or the empty string if that
attribute does not have a specified or default value.

=item setAttribute (name, value)

Adds a new attribute. If an attribute with that name is
already present in the element, its value is changed to be
that of the value parameter. This value is a simple string,
it is not parsed as it is being set. So any markup (such as
syntax to be recognized as an entity reference) is treated as
literal text, and needs to be appropriately escaped by the
implementation when it is written out. In order to assign an
attribute value that contains entity references, the user
must create an Attr node plus any Text and EntityReference
nodes, build the appropriate subtree, and use
setAttributeNode to assign it as the value of an attribute.


DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the specified name contains an invalid character.

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=back

=item removeAttribute (name)

Removes an attribute by name. If the removed attribute has a
default value it is immediately replaced.

DOMExceptions:

=over 4

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=back

=item getAttributeNode

Retrieves an Attr node by name.

Return Value: The Attr node with the specified attribute name or undef
if there is no such attribute.

=item setAttributeNode (attr)

Adds a new attribute. If an attribute with that name is
already present in the element, it is replaced by the new one.

Return Value: If the newAttr attribute replaces an existing attribute
with the same name, the previously existing Attr node is
returned, otherwise undef is returned.

DOMExceptions:

=over 4

=item * WRONG_DOCUMENT_ERR

Raised if newAttr was created from a different document than the one that created
the element.

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=item * INUSE_ATTRIBUTE_ERR

Raised if newAttr is already an attribute of another Element object. The DOM
user must explicitly clone Attr nodes to re-use them in other elements.

=back

=item removeAttributeNode (oldAttr)

Removes the specified attribute. If the removed Attr has a default value it is
immediately replaced. If the Attr already is the default value, nothing happens
and nothing is returned.

Parameters:
 I<oldAttr>  The Attr node to remove from the attribute list. 

Return Value: The Attr node that was removed.

DOMExceptions:

=over 4

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=item * NOT_FOUND_ERR

Raised if oldAttr is not an attribute of the element.

=back

=head2 Additional methods not in the DOM Spec

=item setTagName (newTagName)

Sets the tag name of the Element. Note that this method is not portable
between DOM implementations.

DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the specified name contains an invalid character.

=back

=back

=item XML::DOM::Text extends XML::DOM::CharacterData

The Text interface represents the textual content (termed character
data in XML) of an Element or Attr. If there is no markup inside an
element's content, the text is contained in a single object
implementing the Text interface that is the only child of the element.
If there is markup, it is parsed into a list of elements and Text nodes
that form the list of children of the element.

When a document is first made available via the DOM, there is only one
Text node for each block of text. Users may create adjacent Text nodes
that represent the contents of a given element without any intervening
markup, but should be aware that there is no way to represent the
separations between these nodes in XML or HTML, so they will not (in
general) persist between DOM editing sessions. The normalize() method
on Element merges any such adjacent Text objects into a single node for
each block of text; this is recommended before employing operations
that depend on a particular document structure, such as navigation with
XPointers.

B<Not Implemented>: The XML::DOM::Parser converts all CDATASections to regular 
text, so as far as I know, there is know way to preserve them. 
If you add CDATASection nodes to a Document yourself, they will be preserved.

=over 4

=item splitText (offset)

Breaks this Text node into two Text nodes at the specified
offset, keeping both in the tree as siblings. This node then
only contains all the content up to the offset point. And a
new Text node, which is inserted as the next sibling of this
node, contains all the content at and after the offset point.

Parameters:
 I<offset>  The offset at which to split, starting from 0.

Return Value: The new Text node.

DOMExceptions:

=over 4

=item * INDEX_SIZE_ERR

Raised if the specified offset is negative or greater than the number of 
characters in data.

=item * NO_MODIFICATION_ALLOWED_ERR

Raised if this node is readonly.

=back

=back

=item XML::DOM::Comment extends XML::DOM::CharacterData

This represents the content of a comment, i.e., all the characters
between the starting '<!--' and ending '-->'. Note that this is the
definition of a comment in XML, and, in practice, HTML, although some
HTML tools may implement the full SGML comment structure.

=item XML::DOM::CDATASection extends XML::DOM::CharacterData

CDATA sections are used to escape blocks of text containing characters
that would otherwise be regarded as markup. The only delimiter that is
recognized in a CDATA section is the "]]>" string that ends the CDATA
section. CDATA sections can not be nested. The primary purpose is for
including material such as XML fragments, without needing to escape all
the delimiters.

The DOMString attribute of the Text node holds the text that is
contained by the CDATA section. Note that this may contain characters
that need to be escaped outside of CDATA sections and that, depending
on the character encoding ("charset") chosen for serialization, it may
be impossible to write out some characters as part of a CDATA section.

The CDATASection interface inherits the CharacterData interface through
the Text interface. Adjacent CDATASections nodes are not merged by use
of the Element.normalize() method.

B<Not Implemented>: see Text node comments about CDATASections being converted 
to Text nodes when parsing XML input.

=item XML::DOM::ProcessingInstruction extends XML::DOM::Node

The ProcessingInstruction interface represents a "processing
instruction", used in XML as a way to keep processor-specific
information in the text of the document. An example:

 <?PI processing instruction?>

Here, "PI" is the target and "processing instruction" is the data.

=over 4

=item getTarget

The target of this processing instruction. XML defines this
as being the first token following the markup that begins the
processing instruction.

=item getData and setData (data)

The content of this processing instruction. This is from the
first non white space character after the target to the
character immediately preceding the ?>.

=back

=item XML::DOM::Notation extends XML::DOM::Node

This node represents a Notation, e.g.

 <!NOTATION gs SYSTEM "GhostScript">

 <!NOTATION name PUBLIC "pubId">

 <!NOTATION name PUBLIC "pubId" "sysId">

 <!NOTATION name SYSTEM "sysId">

=over 4

=item getName and setName (name)

Returns (or sets) the Notation name, which is the first token after the 
NOTATION keyword.

=item getSysId and setSysId (sysId)

Returns (or sets) the system ID, which is the token after the optional
SYSTEM keyword.

=item getPubId and setPubId (pubId)

Returns (or sets) the public ID, which is the token after the optional
PUBLIC keyword.

=item getBase

This is passed by XML::Parser in the Notation handler. 
I don't know what it is yet.

=item getNodeName

Returns the same as getName.

=back

=item XML::DOM::Entity extends XML::DOM::Node

This node represents an Entity declaration, e.g.

 <!ENTITY % draft 'INCLUDE'>

 <!ENTITY hatch-pic SYSTEM "../grafix/OpenHatch.gif" NDATA gif>

The first one is called a parameter entity and is referenced like this: %draft;
The 2nd is a (regular) entity and is referenced like this: &hatch-pic;

=over 4

=item getNotationName

Returns the name of the notation for the entity.

I<Not Implemented> The DOM Spec says: For unparsed entities, the name of the 
notation for the entity. For parsed entities, this is null.
(This implementation does not support unparsed entities.)

=item getSysId

Returns the system id, or undef.

=item getPubId

Returns the public id, or undef.

=head2 Additional methods not in the DOM Spec

=item isParameterEntity

Whether it is a parameter entity (%ent;) or not (&ent;)

=item getValue

Returns the entity value.

=item getNdata

Returns the NDATA declaration (for general unparsed entities), or undef.

=back

=item XML::DOM::DocumentType extends XML::DOM::Node

Each Document has a doctype attribute whose value is either null or a
DocumentType object. The DocumentType interface in the DOM Level 1 Core
provides an interface to the list of entities that are defined for the
document, and little else because the effect of namespaces and the
various XML scheme efforts on DTD representation are not clearly
understood as of this writing. 
The DOM Level 1 doesn't support editing DocumentType nodes.

B<Not In DOM Spec>: This implementation has added a lot of extra 
functionality to the DOM Level 1 interface. 
To allow editing of the DocumentType nodes, see XML::DOM::ignoreReadOnly.

=over 4

=item getName

Returns the name of the DTD, i.e. the name immediately following the
DOCTYPE keyword.

=item getEntities

A NamedNodeMap containing the general entities, both external
and internal, declared in the DTD. Duplicates are discarded.
For example in:

 <!DOCTYPE ex SYSTEM "ex.dtd" [
  <!ENTITY foo "foo">
  <!ENTITY bar "bar">
  <!ENTITY % baz "baz">
 ]>
 <ex/>

the interface provides access to foo and bar but not baz.
Every node in this map also implements the Entity interface.

The DOM Level 1 does not support editing entities, therefore
entities cannot be altered in any way.

B<Not In DOM Spec>: See XML::DOM::ignoreReadOnly to edit the DocumentType etc.

=item getNotations

A NamedNodeMap containing the notations declared in the DTD.
Duplicates are discarded. Every node in this map also
implements the Notation interface.

The DOM Level 1 does not support editing notations, therefore
notations cannot be altered in any way.

B<Not In DOM Spec>: See XML::DOM::ignoreReadOnly to edit the DocumentType etc.

=head2 Additional methods not in the DOM Spec

=item Creating and setting the DocumentType

A new DocumentType can be created with:

	$doctype = $doc->createDocumentType ($name, $sysId, $pubId, $internal);

To set (or replace) the DocumentType for a particular document, use:

	$doc->setDocType ($doctype);

=item getSysId and setSysId (sysId)

Returns or sets the system id.

=item getPubId and setPubId (pudId)

Returns or sets the public id.

=item setName (name)

Sets the name of the DTD, i.e. the name immediately following the
DOCTYPE keyword. Note that this should always be the same as the element
tag name of the root element.

=item getAttlistDecl (elemName)

Returns the AttlistDecl for the Element with the specified name, or undef.

=item getElementDecl (elemName)

Returns the ElementDecl for the Element with the specified name, or undef.

=item getEntity (entityName)

Returns the Entity with the specified name, or undef.

=item addAttlistDecl (elemName)

Adds a new AttDecl node with the specified elemName if one doesn't exist yet.
Returns the AttlistDecl (new or existing) node.

=item addElementDecl (elemName, model)

Adds a new ElementDecl node with the specified elemName and model if one doesn't 
exist yet.
Returns the AttlistDecl (new or existing) node. The model is ignored if one
already existed.

=item addEntity (parameter, notationName, value, sysId, pubId, ndata)

Adds a new Entity node. Don't use createEntity and appendChild, because it should
be added to the internal NamedNodeMap containing the entities.

Parameters:
 I<parameter>	 whether it is a parameter entity (%ent;) or not (&ent;).
 I<notationName> the entity name.
 I<value>        the entity value.
 I<sysId>        the system id (if any.)
 I<pubId>        the public id (if any.)
 I<ndata>        the NDATA declaration (if any, for general unparsed entities.)

SysId, pubId and ndata may be undefined.

DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the notationName does not conform to the XML spec.

=back

=item addNotation (name, base, sysId, pubId)

Adds a new Notation object. 

Parameters:
 I<name>   the notation name.
 I<base>   the base to be used for resolving a relative URI.
 I<sysId>  the system id.
 I<pubId>  the public id.

Base, sysId, and pubId may all be undefined.
(These parameters are passed by the XML::Parser Notation handler.)

DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the notationName does not conform to the XML spec.

=back

=item addAttDef (elemName, attrName, type, default, fixed)

Adds a new attribute definition. It will add the AttDef node to the AttlistDecl
if it exists. If an AttDef with the specified attrName already exists for the
given elemName, this function only generates a warning.

See XML::DOM::AttDef::new for the other parameters.

=item getDefaultAttrValue (elem, attr)

Returns the default attribute value as a string or undef, if none is available.

Parameters:
 I<elem>    The element tagName.
 I<attr>    The attribute name.

=item expandEntity (entity [, parameter])

Expands the specified entity or parameter entity (if parameter=1) and returns
its value as a string, or undef if the entity does not exist.
(The entity name should not contain the '%', '&' or ';' delimiters.)

=back

=item XML::DOM::DocumentFragment extends XML::DOM::Node

DocumentFragment is a "lightweight" or "minimal" Document object. It is
very common to want to be able to extract a portion of a document's
tree or to create a new fragment of a document. Imagine implementing a
user command like cut or rearranging a document by moving fragments
around. It is desirable to have an object which can hold such fragments
and it is quite natural to use a Node for this purpose. While it is
true that a Document object could fulfil this role, a Document object
can potentially be a heavyweight object, depending on the underlying
implementation. What is really needed for this is a very lightweight
object. DocumentFragment is such an object.

Furthermore, various operations -- such as inserting nodes as children
of another Node -- may take DocumentFragment objects as arguments; this
results in all the child nodes of the DocumentFragment being moved to
the child list of this node.

The children of a DocumentFragment node are zero or more nodes
representing the tops of any sub-trees defining the structure of the
document. DocumentFragment nodes do not need to be well-formed XML
documents (although they do need to follow the rules imposed upon
well-formed XML parsed entities, which can have multiple top nodes).
For example, a DocumentFragment might have only one child and that
child node could be a Text node. Such a structure model represents
neither an HTML document nor a well-formed XML document.

When a DocumentFragment is inserted into a Document (or indeed any
other Node that may take children) the children of the DocumentFragment
and not the DocumentFragment itself are inserted into the Node. This
makes the DocumentFragment very useful when the user wishes to create
nodes that are siblings; the DocumentFragment acts as the parent of
these nodes so that the user can use the standard methods from the Node
interface, such as insertBefore() and appendChild().

=item XML::DOM::DOMImplementation

The DOMImplementation interface provides a number of methods for
performing operations that are independent of any particular instance
of the document object model.

The DOM Level 1 does not specify a way of creating a document instance,
and hence document creation is an operation specific to an
implementation. Future Levels of the DOM specification are expected to
provide methods for creating documents directly.

=over 4

=item hasFeature (feature, version)

Returns 1 if and only if feature equals "XML" and version equals "1.0".

=back

=item XML::DOM::Document extends XML::DOM::Node

This is the main root of the document structure as returned by 
XML::DOM::Parser::parse and XML::DOM::Parser::parsefile.

Since elements, text nodes, comments, processing instructions, etc.
cannot exist outside the context of a Document, the Document interface
also contains the factory methods needed to create these objects. The
Node objects created have a getOwnerDocument method which associates
them with the Document within whose context they were created.

=over 4

=item getDocumentElement

This is a convenience method that allows direct access to
the child node that is the root Element of the document.

=item getDoctype

The Document Type Declaration (see DocumentType) associated
with this document. For HTML documents as well as XML
documents without a document type declaration this returns
undef. The DOM Level 1 does not support editing the Document
Type Declaration.

B<Not In DOM Spec>: This implementation allows editing the doctype. 
See I<XML::DOM::ignoreReadOnly> for details.

=item getImplementation

The DOMImplementation object that handles this document. A
DOM application may use objects from multiple implementations.

=item createElement (tagName)

Creates an element of the type specified. Note that the
instance returned implements the Element interface, so
attributes can be specified directly on the returned object.

DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the tagName does not conform to the XML spec.

=back

=item createTextNode (data)

Creates a Text node given the specified string.

=item createComment (data)

Creates a Comment node given the specified string.

=item createCDATASection (data)

Creates a CDATASection node given the specified string.

=item createAttribute (name [, value [, specified ]])

Creates an Attr of the given name. Note that the Attr
instance can then be set on an Element using the setAttribute method.

B<Not In DOM Spec>: The DOM Spec does not allow passing the value or the 
specified property in this method. In this implementation they are optional.

Parameters:
 I<value>     The attribute's value. See Attr::setValue for details.
              If the value is not supplied, the specified property is set to 0.
 I<specified> Whether the attribute value was specified or whether the default
              value was used. If not supplied, it's assumed to be 1.

DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the name does not conform to the XML spec.

=back

=item createProcessingInstruction (target, data)

Creates a ProcessingInstruction node given the specified name and data strings.

Parameters:
 I<target>  The target part of the processing instruction.
 I<data>    The data for the node.

DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the target does not conform to the XML spec.

=back

=item createDocumentFragment

Creates an empty DocumentFragment object.

=item createEntityReference (name)

Creates an EntityReference object.

=head2 Additional methods not in the DOM Spec

=item getXMLDecl and setXMLDecl (xmlDecl)

Returns the XMLDecl for this Document or undef if none was specified.
Note that XMLDecl is not part of the list of child nodes.

=item setDoctype (doctype)

Sets or replaces the DocumentType. 
B<NOTE>: Don't use appendChild or insertBefore to set the DocumentType.
Even though doctype will be part of the list of child nodes, it is handled
specially.

=item getDefaultAttrValue (elem, attr)

Returns the default attribute value as a string or undef, if none is available.

Parameters:
 I<elem>    The element tagName.
 I<attr>    The attribute name.

=item getEntity (name)

Returns the Entity with the specified name.

=item createXMLDecl (version, encoding, standalone)

Creates an XMLDecl object. All parameters may be undefined.

=item createDocumentType (name, sysId, pubId)

Creates a DocumentType object. SysId and pubId may be undefined.

=item createNotation (name, base, sysId, pubId)

Creates a new Notation object. Consider using 
XML::DOM::DocumentType::addNotation!

=item createEntity (parameter, notationName, value, sysId, pubId, ndata)

Creates an Entity object. Consider using XML::DOM::DocumentType::addEntity!

=item createElementDecl (name, model)

Creates an ElementDecl object.

DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the element name (tagName) does not conform to the XML spec.

=back

=item createAttlistDecl (name)

Creates an AttlistDecl object.

DOMExceptions:

=over 4

=item * INVALID_CHARACTER_ERR

Raised if the element name (tagName) does not conform to the XML spec.

=back

=item expandEntity (entity [, parameter])

Expands the specified entity or parameter entity (if parameter=1) and returns
its value as a string, or undef if the entity does not exist.
(The entity name should not contain the '%', '&' or ';' delimiters.)

=back

=head1 EXTRA NODE TYPES

=item XML::DOM::XMLDecl extends XML::DOM::Node

This node contains the XML declaration, e.g.

 <?xml version="1.0" encoding="UTF-16" standalone="yes"?>

See also XML::DOM::Document::getXMLDecl.

=over 4

=item getVersion and setVersion (version)

Returns and sets the XML version. At the time of this writing the version should
always be "1.0"

=item getEncoding and setEncoding (encoding)

undef may be specified for the encoding value.

=item getStandalone and setStandalone (standalone)

undef may be specified for the standalone value.

=back

=item XML::DOM::ElementDecl extends XML::DOM::Node

This node represents an Element declaration, e.g.

 <!ELEMENT address (street+, city, state, zip, country?)>

=over 4

=item getName

Returns the Element tagName.

=item getModel and setModel (model)

Returns and sets the model as a string, e.g. 
"(street+, city, state, zip, country?)" in the above example.

=back

=item XML::DOM::AttlistDecl extends XML::DOM::Node

This node represents an ATTLIST declaration, e.g.

 <!ATTLIST person
   sex      (male|female)  #REQUIRED
   hair     CDATA          "bold"
   eyes     (none|one|two) "two"
   species  (human)        #FIXED "human"> 

Each attribute definition is stored a separate AttDef node. The AttDef nodes can
be retrieved with getAttDef and added with addAttDef.
(The AttDef nodes are stored in a NamedNodeMap internally.)

=over 4

=item getName

Returns the Element tagName.

=item getAttDef (attrName)

Returns the AttDef node for the attribute with the specified name.

=item addAttDef (attrName, type, default, [ fixed ])

Adds a AttDef node for the attribute with the specified name.

Parameters:
 I<attrName> the attribute name.
 I<type>     the attribute type (e.g. "CDATA" or "(male|female)".)
 I<default>  the default value enclosed in quotes (!), the string #IMPLIED or 
             the string #REQUIRED.
 I<fixed>    whether the attribute is '#FIXED' (default is 0.)

=back

=item XML::DOM::AttDef extends XML::DOM::Node

Each object of this class represents one attribute definition in an AttlistDecl.

=over 4

=item getName

Returns the attribute name.

=item getDefault

Returns the default value, or undef.

=item isFixed

Whether the attribute value is fixed (see #FIXED keyword.)

=item isRequired

Whether the attribute value is required (see #REQUIRED keyword.)

=item isImplied

Whether the attribute value is implied (see #IMPLIED keyword.)

=back

=head1 IMPLEMENTATION DETAILS

=over 4

=item * Perl Mappings

The value undef was used when the DOM Spec said null.

The DOM Spec says: Applications must encode DOMString using UTF-16 (defined in 
Appendix C.3 of [UNICODE] and Amendment 1 of [ISO-10646]).
In this implementation we use plain old Perl strings encoded in UTF-8 instead of
UTF-16.

=item * Text and CDATASection nodes

The Expat parser expands EntityReferences and CDataSection sections to 
raw strings and does not indicate where it was found. 
This implementation does therefore convert both to Text nodes at parse time.
CDATASection and EntityReference nodes that are added to an existing Document 
(by the user) will be preserved.

Also, subsequent Text nodes are always merged at parse time. Text nodes that are 
added later can be merged with the normalize method. Consider using the addText
method when adding Text nodes.

=item * Printing and toString

When printing (and converting an XML Document to a string) the strings have to 
encoded differently depending on where they occur. E.g. in a CDATASection all 
substrings are allowed except for "]]>". In regular text, certain characters are
not allowed, e.g. ">" has to be converted to "&gt;". 
These routines should be verified by someone who knows the details.

=item * Quotes

Certain sections in XML are quoted, like attribute values in an Element.
XML::Parser strips these quotes and the print methods in this implementation 
always uses double quotes, so when parsing and printing a document, single quotes
may be converted to double quotes. The default value of an attribute definition
(AttDef) in an AttlistDecl, however, will maintain its quotes.

=item * AttlistDecl

Attribute declarations for a certain Element are always merged into a single
AttlistDecl object.

=item * Comments

Comments in the DOCTYPE section are not kept in the right place. They will become
child nodes of the Document.

=back

=head1 SEE ALSO

The Japanese version of this document by Takanori Kawai (Hippo2000)
at http://member.nifty.ne.jp/hippo2000/perltips/xml/dom.htm 

The DOM Level 1 specification at http://www.w3.org/TR/REC-DOM-Level-1

The XML spec (Extensible Markup Language 1.0) at http://www.w3.org/TR/REC-xml

The XML::Parser and XML::Parser::Expat manual pages.

=head1 CAVEATS

The method getElementsByTagName() does not return a "live" NodeList.
Whether this is an actual caveat is debatable, but a few people on the 
www-dom mailing list seemed to think so. I haven't decided yet. It's a pain
to implement, it slows things down and the benefits seem marginal.
Let me know what you think. 

(To subscribe to the www-dom mailing list send an email with the subject 
"subscribe" to www-dom-request@w3.org. I only look here occasionally, so don't
send bug reports or suggestions about XML::DOM to this list, send them
to enno@att.com instead.)

=head1 AUTHORS

Enno Derksen <F<enno@att.com>> and Clark Cooper <F<coopercl@sch.ge.com>>.
Please send bugs, comments and suggestions to Enno.

=cut
