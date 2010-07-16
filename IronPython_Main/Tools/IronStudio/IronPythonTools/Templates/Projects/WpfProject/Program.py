import clr
clr.AddReference('PresentationFramework')

from System.Windows import Application, Window

class MyWindow(Window):
    def __init__(self):
        clr.LoadComponent('$safeprojectname$.xaml', self)
    

if __name__ == '__main__':
	Application().Run(MyWindow())
