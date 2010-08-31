require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../shared/numeric'
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "System::UInt64" do
  before(:each) do
    @size = NumericHelper.size_of_u_int64
  end
  
  it_behaves_like "A .NET numeric", System::UInt64
  it_behaves_like :numeric_size, System::UInt64
  it_behaves_like :numeric_conversion, System::UInt64
end
