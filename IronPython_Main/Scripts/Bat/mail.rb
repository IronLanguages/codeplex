require 'win32ole'
require 'tmpdir'
require 'optparse'


options = {:external => true, 
           :to => ["IronRuby External Code Reviewers"], 
           :cc => ["ironruby-core@rubyforge.org"]}
OptionParser.new do |opts|
  opts.banner = "Usage: mail.rb [options] -s shelveset"

  opts.separator ""

  opts.on("-s", "--shelveset SHELVESET", "Shelveset to review") do |s|
    options[:shelveset] = s
  end

  opts.on("-i", "--[no-]internal", "Internal code review") do |i|
    if i
      options[:external] = false
      options[:to] << "Rowan Code Reviewers"
      options[:to].delete("IronRuby External Code Reviewers")
      options[:cc].delete("ironruby-core@rubyforge.org")
    end
  end

  opts.on("-t", "--to [TO]", "Group or alias to send to") do |to|
    options[:to] << to
  end

  opts.on("-c", "--cc [CC]", "Group or alias to cc to") do |cc|
    options[:cc] << cc
  end

  opts.on_tail("-h", "--help", "Show this message") do |n|
    puts opts
    exit
  end
end.parse!

shelveset = options[:shelveset]

# Generate unified diff and dump into temporary directory

temp_output_file = Dir.tmpdir + "\\#{shelveset}_temp.diff"
output_file = Dir.tmpdir + "\\#{shelveset}.diff"
shelveset_info_file = Dir.tmpdir + "\\#{shelveset}.info"

`tf diff /shelveset:#{shelveset} /format:unified > #{temp_output_file}`
`tf shelvesets #{shelveset} /format:detailed > #{shelveset_info_file}`

# Process output file to strip diffs of things that we don't want public
# Today this is:
#
# - Languages/CSharp
# - Languages/JS

# $/DevDiv/Dev11/PU/MQPro/dlr/Languages/CSharp/Program.cs;C425818

permitted_languages = /\$\/.*\/dlr\/Languages\/[Ruby|IronPython]/
permitted_external = /\$\/.*\/dlr\/External.LCA_RESTRICTED\/Languages\/[Ruby|IronPython]/
permitted_runtime = /\$\/.*\/dlr\/\/Runtime\/Microsoft\.Scripting/
permitted_dlr = /\$\/.*\/ndp\/fx\/src\/Core\/Microsoft\/Scripting/
tfs_path = /\$\/DevDiv\/Dev11\/PU\/MQPro/

$write = false

f = File.open temp_output_file
w = File.open output_file, 'w'
i = File.open shelveset_info_file

begin
  f.each do |line|
    if tfs_path =~ line
      $write = (permitted_languages =~ line || permitted_runtime =~ line || permitted_dlr =~ line || permitted_external =~ line)
    end
    if options[:external]
      w.write(line) if $write 
    else 
      w.write(line)
    end
  end
ensure
  w.close
  f.close
end

info = ""
begin 
 i.each do |line|
   info += line
 end
ensure
  i.close
end

if info =~ /^(Comment\s+:.*)^Check-in\s+Notes:/m
  comments = $1
end

FormatUnspecified = 0
FormatPlainText = 1
FormatHTMLText = 2
FormatRichText = 3
subject = "Code Review: #{shelveset}"

body = <<-EOF
tfpt review "/shelveset:#{shelveset};#{ENV['USERDOMAIN']}\\#{ENV['USERNAME']}"
#{comments}
EOF
begin
  outlook = WIN32OLE.new 'Outlook.Application'
  mail = outlook.CreateItem(0)
  mail.BodyFormat = FormatPlainText
  options[:to].each {|to| mail.Recipients.Add(to)}
  mail.Attachments.Add(output_file)
  mail.CC = options[:cc].join(";")
  mail.Subject = "Code Review: #{shelveset}"

  body = <<-EOF
  tfpt review "/shelveset:#{shelveset};#{ENV['USERDOMAIN']}\\#{ENV['USERNAME']}"
  #{comments}
  EOF

  mail.Body = body
  mail.Display
rescue WIN32OLERuntimeError => e
  puts e
  puts subject
  puts body
  puts "attachment: #{output_file}"
end
