require_dependency 'models/course'
require 'logger'

ActiveRecord::Base.logger = Logger.new("debug.log")

ActiveRecord::Base.configurations = {
  'arunit' => {
    :mode     => 'ADONET',
    :adapter  => 'sqlserver',
    :host     => ENV['COMPUTERNAME'] + "\\SQLEXPRESS",
    :integrated_security => 'true',
    :database => 'activerecord_unittest'
  },
  'arunit2' => {
    :mode     => 'ADONET',
    :adapter  => 'sqlserver',
    :host     => ENV['COMPUTERNAME'] + "\\SQLEXPRESS",
    :integrated_security => 'true',
    :database => 'activerecord_unittest2'
  }
}

ActiveRecord::Base.establish_connection 'arunit'
Course.establish_connection 'arunit2'
