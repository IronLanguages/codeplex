require 'fileutils'

# build-flavors of Silverlight that are stored in the source tree
FLAVORS = ['x86fre', 'x86chk']

# Where on CPVSBUILD is Silverlight stored?
SOURCE_PATH = '"//cpvsbuild/drops/release/#{branch}/raw/#{version}/"'
SOURCE_BINARY_PATH = '"binaries.#{flavor}"'

# All versions of Silverlight that are currently supported; all of their installers
# are stored in the source tree.
SOURCE_VERSIONS = {
  '2' => {
    :branch => 'sl_w2_svcmain', :version => '40216.00', :installers => {
      :windows => '"setup/sfxs/#{flavor}/enu/silverlight/#{type}/Silverlight.2.0#{developer}.exe"',
      :mac     => '"setup/MacPkg/#{macflavor}/enu/Silverlight.2.0#{developer}.dmg"'
    }
  },
  '3' => {
    :branch => 'sl_v3_svc', :version => '50106.00', :installers => {
      :windows => '"setup/sfxs/#{flavor}/enu/silverlight/#{type}/Silverlight.3.0#{developer}.exe"',
      :mac     => '"setup/MacPkg/#{macflavor}/enu/Silverlight.3.0#{developer}.dmg"',
    }
  },
  '4' => {
    :branch => 'sl_v4_rc', :version => '50303.00', :installers => {
      :windows => '"setup/sfxs/#{flavor}/enu/silverlight/#{type}/Silverlight#{developer}.exe"',
      :mac     => '"setup/MacPkg/#{macflavor}/enu/Silverlight#{developer}.dmg"'
    }
  },
  '4-internal' => {
    :branch => 'silverlight_w2', :version => 'current', :installers => {
      :windows => '"setup/sfxs/#{flavor}/enu/silverlight/#{type}/Silverlight#{developer}.exe"',
      :mac     => '"setup/MacPkg/#{macflavor}/enu/Silverlight#{developer}.dmg"'
    }
  }
}

# Silverlight 3 is the current RTM version; change this whenever the next RTM
# version is released.
STABLE_VERSION = '3'

# versions which tests need to be run against, therefore their binaries should
# be stored in the source tree. In general these should be:
# * latest RTM version
# * latest public non-RTM version (alpha/beta/rc)
# * latest internal version
ACTIVE_VERSIONS = %W(3 4 4-internal)

# Location in source tree where Silverlight binaries/installers will be stored
DEST_PATH = File.join(ENV['DLR_ROOT'], "Utilities/Silverlight")

# All files to copy from Silverlight's build:
PATHS = {

  # Normal files to copy. These are things under x86ret and x86chk.
  # This is the list of things that are actually required to run the CoreCLR tests
  :core => %W(
    coreclr.dll 
    dbgshim.dll 
    fxprun.exe 
    fxprun.exe.managed_manifest
    mscordaccore.dll
    mscordbi.dll 
    mscorlib.dll 
    sandboxhelper.dll 
    SOS.dll
    System.dll
    mscorrc.dll
    snskipverf.exe
    System.Core.dll
    System.Net.dll
    System.Xml.dll
    System.Xml.Linq.dll
    en-US\\mscorlib.debug.resources.dll
    slstampkey.exe
  ),
  
  # Unused files to copy. These are things under x86ret\unused and x86chk
  :unused => %W(
    cordacwks.dll 
    cordbg_macppc.exe
    cordbg_macx86.exe 
    mscordbi_macppc.dll
    mscordbi_macx86.dll
    mscoree.h
    spawnnowow.exe
    mscordbc.dll
  ),

  # List of things to copy into symbols.pri
  :sympri => %W(
    dll\\cordacwks.pdb
    dll\\coreclr.pdb
    dll\\dbgshim.pdb
    dll\\mscordbi.pdb
    dll\\mscordbi_macppc.pdb
    dll\\mscordbi_macx86.pdb
    dll\\mscorlib.pdb
    dll\\mscorrc.pdb
    dll\\sandboxhelper.pdb
    dll\\SOS.pdb
    dll\\System.pdb
    exe\\cordbg_macppc.pdb
    exe\\cordbg_macx86.pdb
    exe\\fxprun.pdb
    exe\\snskipverf.pdb 
    exe\\spawnnowow.pdb 
    dll\\mscordbc.pdb
    dll\\System.Core.pdb
    dll\\System.Net.pdb
    dll\\System.Xml.pdb
    dll\\System.Xml.Linq.pdb
    dll\\Microsoft.VisualBasic.pdb
    dll\\System.Windows.pdb
    dll\\System.Windows.Browser.pdb
    dll\\agcore.pdb
    dll\\mscordaccore.pdb
  ).map{|f| 'Symbols.pri\\retail\\' + f},

  # files that are under bin/i386 in the builds
  :bin_i386 => %W(    
    Microsoft.VisualBasic.dll
    System.Windows.dll
    System.Windows.Browser.dll
  ),

  # copied from x86ret\bin\i386\Ignite to Hosts\Silverlight\TestSuites\setup
  :test => %W(
    Microsoft.Silverlight.Testing.dll
    Microsoft.Silverlight.Testing.pdb
    Microsoft.Silverlight.ManagedTestDriver.exe
    Microsoft.Silverlight.ManagedTestDriver.pdb
  ),

  # files to only copy for v4
  :v4only => %W(
    Microsoft.CSharp.dll
    System.Numerics.dll
    System.Json.dll
    ngen.exe
    nidump.exe
  )
}

# For a given version and flavor of Silverlight, return the list of files that
# should be copied into the source tree
def gather_files(vers_num, vers, flavor, copy_symbols = true)
  puts
  puts "Silverlight '#{vers_num}' (#{vers[:branch]}/#{vers[:version]}/#{flavor})"

  files = []
  dest_folders = []

  branch = vers[:branch]
  version = vers[:version]
  src_path = eval(SOURCE_PATH)
  src = File.join src_path, eval(SOURCE_BINARY_PATH)

  dst = if vers_num !~ /4/
          # keep the old folder name, since we have it hard-coded everywhere
          # in our tree
          (flavor == 'x86fre' ? File.join(DEST_PATH, 'x86ret') : File.join(DEST_PATH, flavor))
        else
          File.join(DEST_PATH, "v#{vers_num}-#{flavor}")
        end
  dest_folders << dst

  puts "\tDestination Folder: #{dst}"

  if ACTIVE_VERSIONS.include? vers_num
    print "\tACTIVE VERSION: gathering individual binaries"
    paths = PATHS[:core]
    paths = paths + PATHS[:sympri] if copy_symbols
    files << paths.map do |f|
      {:src => File.join(src, f), :dst => File.join(dst, f)}
    end
    files << PATHS[:unused].map do |f| 
      {:src => File.join(src, f), :dst => File.join(dst, "#{"unused/" if flavor == 'x86fre'}#{f}")}
    end
    files << PATHS[:bin_i386].map do |f| 
      {:src => File.join(src, 'bin/i386', f), :dst => File.join(dst, f)}
    end
    if flavor == 'x86fre' && vers_num == STABLE_VERSION 
      files << PATHS[:test].map{|f| {:src => File.join(src, 'bin/i386/Ignite', f), :dst => File.join(ENV['DLR_ROOT'], 'Hosts/Silverlight/TestSuites/setup', f)}}
    end
    if vers_num =~ /4/
      print ' and V4-only binaries'
      files << PATHS[:v4only].map{|f| {:src => File.join(src, f), :dst => File.join(dst, f)}}
    end

    # This file must be treated specially because its filename contains the build number
    # (thus it changes every build...)
    special = 'mscordaccore_x86_x86_*.dll'
    path = Dir.glob(File.join(dst, special))
    if path.size > 0
      FileUtils.rm path.first 
    end
    path = Dir.glob(File.join(src, special))
    if path.size > 0
      path = File.basename(path.first)
      files << {:src => File.join(src, path), :dst => File.join(dst, path)}
    end

    puts
  end

  puts "\tGathering Installers"
  macflavor = "intel#{flavor.split('x86').last}"
  vers[:installers].each do |os, installer|
    type = vers_num.split('-')[0].to_i < 4 ? vers_num : "Main"
    ['', "_Developer"].each do |developer|
      type = "Dev" if developer != ''
      installer_src = File.join(src_path, eval(installer))
      installer_dst = File.join(dst, File.basename(installer_src))
      files << {:src => installer_src, :dst => installer_dst}
    end
  end

  [files, dest_folders]
end

# executes tfs commands
def tf(title, cmd, filelist, options = '')
  if filelist.empty?
    puts "No files specified for \"tf #{cmd}\""
    return
  end

  print "#{title} ."

  # batch up the filelist into smaller lists
  maxitems = 20
  numparts = filelist.size / maxitems.to_f
  numparts = numparts.to_i + 1 if numparts.to_i < numparts
  numparts = 1 if numparts < 1
  files = []
  numparts.times do |i|
    min, max = (maxitems * i), (maxitems * (i + 1) - 1)
    files << filelist.slice(min..max)
  end

  files.each_with_index do |batch, id|
    log = (File.dirname(__FILE__) + "/tf-#{cmd}-#{id}.log").gsub('/', "\\")
    File.delete log if File.exist? log
    list = batch.map{|f| f.gsub('/', "\\")}.join(' ')
    tfcmd = "tf #{cmd} #{list} #{options} 1> #{log} 2>&1"
    if system tfcmd
      print "."
      File.delete log if File.exist? log
    else
      raise "tf edit Failed, see #{log} for details"
    end
  end
  puts " done"
end

# Copy all "files" from :src to :dst
def copy_files(files)
  puts "
#{'='*79}
Copying files
#{'='*79}"
  banner = '-'*79
  files.each do |file|
    src, dst = file[:src], file[:dst]
    puts banner
    if not File.exist?(src)
      puts "\n![WARNING] Skipping file (does not exist): #{src}\n"
      next
    end
    puts"Copy:
    From: #{src}
    To: #{dst}"
    unless File.exist? File.dirname(dst)
      FileUtils.mkdir_p File.dirname(dst)
    end
    FileUtils.copy src, dst
  end
  puts "#{'='*79}
Done
#{'='*79}"
end

# make sure there are no pending changes or loose files in the enlistment
def clean_enlistment(directories)
  puts
  print "Cleaning out destination ... "
  directories.each do |dir|
    next unless File.exist? dir
    FileUtils.cd dir do
      system "tf undo . /recursive /noprompt"
      system "tfpt treeclean . /recursive /noprompt"
    end
  end
  puts " done"
end

# given a list of files, split into two lists: one with all the files that
# do not yet exist, and the other with ones that do
def find_added_and_existing_files(files)
  puts
  existing_files = []
  added_files = []
  files.map{|f| f[:dst]}.each do |f|
    if File.exist?(f)
      existing_files << f
    else
      added_files << f
    end
  end
  [added_files, existing_files]
end

# Stamp coreclr: required for mdbg and fxprun, since they run out of the
# checked in binaries, and not the installed ones
def stamp_coreclr(dst)
  slstampkey = File.join(dst, "slstampkey.exe")
  coreclr = File.join(dst, "coreclr.dll")
  puts
  print "Stamping CoreCLR with test key: #{coreclr} ... "
  system "#{slstampkey} -s #{coreclr} -clrkey"
  if $?.exitstatus != 100
    raise "Failed to stamp CoreCLR with test key.\nResult code: #{result}\nCLR path: #{coreclr}\nslstampkey path:#{slstampkey}"
  end
  puts "done"
end

if __FILE__ == $0

  usage = "
Usage: ruby update_silverlight.rb <options>

Options:
      -sl <version>, --silverlight <version>
          
          What version of Silverlight to use: 2, 3, 4, or 4-internal (default: all).
          Can be used multiple times to specify multiple versions.

      -f <buildflavor>, --flavor <buildflavor>
          
          What build flavor to use: x86fre or x86chk (default: all).
          Can be used multiple times to specify multiple flavors

      -np, --no-pdbs
          
          Do not copy Silverlight plaform PDBs (default: copies PDBs).

      -h, --help
      
          Displays this message and exits.
"

  source_versions = {}
  flavors = []
  copy_symbols = true
  while true
    arg = ARGV.shift
    case(arg)
    when '--silverlight', '-sl'
      vers = ARGV.shift
      source_versions[vers] = SOURCE_VERSIONS[vers]
    when '--flavor', '-f'
      flavors << ARGV.shift
    when '--no-pdbs', '-np'
      copy_symbols = false
    when '--help', '-h'
      puts usage
      exit 1
    when nil
      break
    else
      puts "Unrecognized arg: #{arg}"
      puts usage
      exit 1
    end
  end
  source_versions = SOURCE_VERSIONS if source_versions.keys.empty?
  flavors = FLAVORS if flavors.empty?

  directories = []
  files = []
  source_versions.each do |vers_num, vers|
    flavors.each do |flavor|
      f, d = gather_files(vers_num, vers, flavor, copy_symbols)
      files << f
      directories << d
    end
  end
  files.flatten!
  directories.flatten!

  clean_enlistment(directories)
  
  added_files, existing_files = find_added_and_existing_files(files)
  tf "Checking out files", 'edit', existing_files
  copy_files(files)
  #tf 'Adding files to version control', 'add', added_files, "/noprompt"

  directories.each{|d| stamp_coreclr d}

  puts "
#{'='*79}
Success!
   
The next steps are:
cd %DLR_ROOT%\\Util\\Internal\\Silverlight
tfpt online . /diff /recursive /adds /deletes
tfpt uu . /recursive
tf get
tf shelve

Then run checkin tests, and if everything passes, submit the shelveset
#{'='*79}
"
end
