include System
include System::Windows
include System::Windows::Browser
include System::Windows::Controls

class FrameworkElement
  def method_missing(m)
    find_name(m.to_s.to_clr_string)
  end
end
