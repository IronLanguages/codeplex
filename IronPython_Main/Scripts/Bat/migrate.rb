if !ARGV[1]
  puts "USAGE: ruby migrate.rb oldbranch newbranch"
  exit -1
end
old_path, new_path = ARGV[0..1]
def write_and_execute(cmd)
  puts cmd
  `#{cmd}`
end
workfold = `tf workfold`
mapping = workfold.grep(/\$\//).map {|e| e.strip.chomp.split(": ")}
workspace = workfold.match(/^Workspace\s?: ([\w-]*)/)[1]
mapping.each do |e|
  unless e[0] =~ /^\(cloaked\)/
    write_and_execute "tf workfold /unmap #{e[0]} /workspace:#{workspace}"
    write_and_execute "tf workfold /map #{e[0].gsub(old_path, new_path)} #{e[1]} /workspace:#{workspace}"
  else
    e = e[0].gsub("(cloaked) ","").slice(0..-2)
    write_and_execute "tf workfold /decloak #{e} /workspace:#{workspace}"
    write_and_execute "tf workfold /cloak #{e.gsub(old_path, new_path)} /workspace:#{workspace}"
  end
end

puts `tf workfold`
