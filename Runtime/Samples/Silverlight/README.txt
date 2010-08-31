Dynamic Language Runtime in Silverlight
=============================================================================

The following samples use the Dynamic Language Runtime (DLR) to run
Ruby and Python code in Silverlight. 

It uses the Iron implementations of the languages; more information about 
them can be found here:

  IronPython: http://codeplex.com/ironpython
  IronRuby:   http://rubyforge.org/projects/ironruby/

For more in-depth tutorials/documentation, visit http://codeplex.com/sdlsdk.

Running the samples
-----------------------------------------------------------------------------

1. Download and install Silverlight Tools: 
   http://go.microsoft.com/fwlink/?LinkId=129043.
   To just run the App samples in of Visual Studio 2008, you just need the 
   Silverlight Runtime: http://go.microsoft.com/fwlink/?LinkId=124807

2. In VS, build SilverlightSamples.sln, set either SilverlightHostingRuby or
   SilverlightHostingPython as the Start Project, and Build/Run (F5).
   You can run all the samples outside of Visual Studio just by run "Run.bat"
   in each one of the sample directories.

Directory structure
-----------------------------------------------------------------------------

  \Hosting - Hosting the DLR in a C# application: requires Silverlight Tools.
  \App - Using a DLR language as the Silverlight entry-point
  \Scripts - Helpful scripts for Building/Cleaning at the command-line
  README.txt - This file
  SilverlightSamples.sln - Solution file for opening in Visual Studio 2008 

Silverlight application in Ruby and Python
-----------------------------------------------------------------------------

The "App\Ruby" and "App\Python" samples shows how 
to write a simple Silverlight application in Ruby and Python. The entry 
point script (ruby\app.rb or python\app.py) loads a XAML file and 
displays a message in a TextBlock.

App\Ruby\ruby\app.rb:

  require 'silverlight'

  root = Application.current.load_root_visual(UserControl.new, "app.xaml")
  root.message.text = "Welcome to Ruby and Silverlight!"

App\Python\python\app.py:

  from System.Windows import Application
  from System.Windows.Controls import UserControl

  root = Application.Current.LoadRootVisual(UserControl(), "app.xaml")
  root.Message.Text = "Welcome to Python and Silverlight!"

Silverlight C# applications hosting Ruby and Python
-----------------------------------------------------------------------------

The "Hosting\Ruby" and "Hosting\Python" samples shows 
how to write a Silverlight C# application which hosts the DLR and runs
Python or Ruby code inside it. You can type in the grey area, and on
hitting return the code is run and the result is printed below.

The App.xaml.cs file uses the DLR and a language:

  using Microsoft.Scripting.Silverlight;
  using Microsoft.Scripting.Hosting;

  // if you want to use IronRuby
  using IronRuby; 

  // if you want to use IronPython
  using IronPython.Hosting; 

And declares a helper class for running code through the DLR:

  // for Ruby replace all 3 occurrences of "Python" with "Ruby"
  class PythonEngine {
      private ScriptEngine _engine;

      public PythonEngine() {
          var runtime = new ScriptRuntime(CreateRuntimeSetup());
          _engine = Ruby.GetEngine(runtime);
      }

      public object Execute(string code) {
          return _engine.Execute(code);
      }

      // ...
  }

Then you can run Python code by creating a Python engine and calling
Execute():

  // again, replace "Python" for "Ruby" if you are using Ruby.
  var python = new PythonEngine();
  var result = python.Execute("2 + 2").ToString();
