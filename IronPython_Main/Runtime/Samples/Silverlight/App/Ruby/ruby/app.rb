require 'silverlight'

root = Application.current.load_root_visual(UserControl.new, "app.xaml")
root.message.text = "Welcome to Ruby and Silverlight!"
