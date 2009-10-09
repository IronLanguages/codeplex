.. contents::

*******************************************************************************
Loading .NET assemblies
*******************************************************************************

The smallest unit of distribution of functionality in .NET is an *assembly* which
usually corresponds to a single file with the .dll extension. The assembly is 
available either in the installation folder of the application, or in the
GAC (Global assembly cache). Assemblies can be loaded by using the methods of
the :mod:`clr` module. The following code will load the System.Xml.dll assembly
which is part of the standard .NET implementation, and installed in the GAC::

   import clr
   clr.AddReference("System.Xml")

All .NET assemblies have a unique version number which allows using a specific
version of a given assembly. You can use the specific version of System.Xml that 
ships with Windows Vista or Windows 7 by specifying the full assembly name. This
ensures that you load the specific version of System.Xml.dll even if older or
newer versions of the assembly are also installed on the machine::

   import clr
   clr.AddReference("System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")

The full list of assemblies loaded by IronPython is available in 
:ref: ``clr.References``.

.. note::

   IronPython only knows about assemblies that have been loaded using one of
   the `clr.AddReference` methods. It is possible for other assemblies to be
   loaded by other parts of the application by calling 
   `System.AppDomain.LoadAssembly`, but IronPython will not see these.

==============================================================================
Assemblies loaded by default
==============================================================================

When you use `ipy.exe`, mscorlib.dll and System.dll are automatically loaded.
This enables you to start using these assemblies (which IronPython itself is
dependent on) without having to call `clr.AddReference`.

In a Silverlight application, TODO...

When IronPython code is embedded in an application, the application controls 
which assemblies are loaded by default.

*******************************************************************************
Using .NET types
*******************************************************************************

Once an assembly is loaded, the namespaces and types contained in the assembly
can be accessed from IronPython code.

==============================================================================
Importing .NET namespaces and types
==============================================================================

\.NET namespaces (of loaded assemblies) are comparable to Python modules. 
The types and sub-namespaces can be accessed just like Python attributes.
The following code shows how to access the `System.Environment` class
from mscorlib.dll (which is loaded by default)::

   import System
   dir(System.Environment)

Just like with normal Python modules, you can also use all the other forms
of `import` as well::

   from System import Environment
   dir(Environment)

::

   from System import *
   dir(Environment)

------------------------------------------------------------------------------
Accessing generic types
------------------------------------------------------------------------------

\.NET supports generic types which allow the same code to support multiple
type parameters which retaining the advantages of types safety. Collection
types (like lists, vectors, etc) are the canonical example where generic types
are useful. .NET has a number of generic collection types in the
`System.Collections.Generic` namespace.

IronPython exposes generic types as a special `type` object which supports
indexing with a `type` object as the index (or indices)::

   from System.Collections.Generic import List
   IntList = List[int]
   int_list = IntList()

Note that there might exist a non-generic type as well as one or more 
generic types (with different number of type parameters) with the same name.
In this case, the name can be used without any indexing to access the 
non-generic type, and it can be indexed with different number of types to
access the generic type with the corresponding number of type parameters.

   from System import EventHandler, EventArgs
   # Access the non-generic type
   dir(EventHandler)
   # Access the generic type with 1 type paramter
   dir(EventHandler[EventArgs])

------------------------------------------------------------------------------
Importing .NET methods from a type
------------------------------------------------------------------------------

\.NET types generally map to Python classes. Like Python classes, you cannot
import the attributes of .NET types::

   >>> from System.AppDomain import *
   Traceback (most recent call last):
     File "<stdin>", line 1, in <module>
   ImportError: no module named AppDomain

However, some .NET types only have static methods, and are comparable to
namespaces. `C#` refers to them as *static classes*, and only allows such
classes to have static methods. IronPython allows you to import the attributes
of such *static classes*. `System.Environment` is an example of a static class::

   from System.Environment import *

------------------------------------------------------------------------------
Type-system unification (`type` and `System.Type`)
------------------------------------------------------------------------------

\.NET represents types using ``System.Type``. However, when you access a 
.NET type in Python code, you get a Python ``type`` object. 
It is *not* an instance of ``System.Type``. This allows a unified (Pythonic)
view of both Python and .NET types. For example, `isinstance` works with
.NET types as well::

   from System.Collections import BitArray
   ba = BitArray(5)
   isinstance(ba, BitArray) # returns True

Note that the .NET types behave like builtin types (like `list`), and are
immutable. i.e. you cannot add or delete descriptors from .NET types.

If need to get the System.Type instance for the .NET type, you need to use the 
``clr.GetClrType`` method. Conversely, you can use ``clr.GetPythonType`` to get
a `type` object corresponding to a `System.Type` object.

The unification also extends to other type system entities like methods. .NET
methods are exposed as instances of the `method` type::

   >>> type(BitArray.Xor)
   <type 'method_descriptor'>
   >>> type(ba.Xor)
   <type 'builtin_function_or_method'>

==============================================================================
Instantiating .NET types
==============================================================================

\.NET types are comparable to Python classes, and you can do many of the
same operations on .NET types as with Python classes. In either cases, you 
create an instance by calling the type::

   from System.Collections import BitArray
   ba = BitArray(5) # Creates a bit array of size 5

IronPython also supports inline initializing of the attributes of the instance.
Consider the following two lines::

   ba = BitArray(5)
   ba.Length = 10

The above two lines are equivalent to this single line::

   ba = BitArray(5, Length = 10)

==============================================================================
Invoking .NET methods
==============================================================================

Invoking .NET methods works just like invoking Python methods.

-----------------------------------------------------------------------------
Invoking .NET instance methods
-----------------------------------------------------------------------------

Invoking .NET instance methods works just like invoking methods on a Python
object using the attribute notation::

   from System.Collections import BitArray
   ba = BitArray(5)
   ba.Set(0, True)
   print ba[0] # prints "True"

IronPython also supports named arguments::

   ba.Set(index = 1, value = True)
   print ba[1] # prints "True"

IronPython also supports dict and keyword arguments::

   args = [2, True] # list of arguments
   ba.Set(*args)
   print ba[2] # prints "True"

   args = { "index" : 3, "value" : True }
   ba.Set(**args)
   print ba[3] # prints "True"

-----------------------------------------------------------------------------
Argument conversions
-----------------------------------------------------------------------------

When the argument type does not exactly match the parameter type expected
by the .NET method, IronPython tries to convert the argument. This snippet
shows how arguments are converted when calling the 
`Set(System.Int32, System.Boolean)` method::

   from System.Collections import BitArray
   ba = BitArray(5)
   ba.Set(0, "hello") # converts the second argument to True.
   print ba[0] # prints "True"
   ba.Set(1, None) # converts the second argument to False.
   print ba[1] # prints "False"

Note that some Python types are implemented as .NET types and no conversion
is required in such cases. See :ref: for the mapping. 

Some of the conversions supported are:

==================================   ============
Python argument type                 .NET method parameter type 
==================================   ============
int                                  System.Int8, System.Int16
float                                System.Float
tuple with only elements of type T   System.Collections.Generic.IEnumerable<T>
function, method                     System.Delegate and any of its sub-classes
==================================   ============

See the Appendix for the detailed conversion rules.

-----------------------------------------------------------------------------
Method overloads
-----------------------------------------------------------------------------

\.NET supports overloading methods by both number of arguments and type of
arguments. When IronPython code calls an overloaded method, IronPython
tries to select one of the overloads based on the number and type of arguments
passed to the method, and names of named arguments. In most cases, the right 
overload gets selected::

   from System.Collections import BitArray

   # Call with the exact type as the method signature
   ba = BitArray(5) # calls __new__(System.Int32)
   ba = BitArray(5, True) # calls __new__(System.Int32, System.Boolean)
   ba = BitArray(ba) # calls __new__(System.Collections.BitArray)

The argument types do not have be an exact match with the method signature. 
IronPython will try to convert the arguments if an unamibguous conversion
exists to one of the overload signatures. The following code calls 
`__new__(System.Int32)` even though there are two constructors which take
one argument, and neither of them accept a `System.Double` as an argument::

   ba = BitArray(5.0)

However, note that IronPython will raise a TypeError if there are conversions
to more than one of the overloads::

   >>> BitArray((1, 2, 3))
   Traceback (most recent call last):
     File "<stdin>", line 1, in <module>
   TypeError: Multiple targets could match: BitArray(Array[Byte]), BitArray(Array[bool]), BitArray(Array[int])

If you want to control the exact overload that gets called, you can use the
``Overloads`` method on `method` objects::

   new_method = BitArray.__new__.Overloads[int, type(True)]
   ba = new_method(BitArray, 5, True) # Calls __new__(System.Int32, System.Boolean)
   ba = new_method(BitArray, 5, "hello") # converts "hello" to a System.Boolan
   ba = new_method(BitArray, 5) # raises a TypeError since there are fewer arguments

TODO - Example of indexing Overloads with an Array, byref, etc

-----------------------------------------------------------------------------
Using unbound class instance methods
-----------------------------------------------------------------------------

It is sometimes desirable to invoke an instance method using the unbound
class instance method and passing an explicit `self` object as the first argument.
For example, .NET allows a class to declare an instance method with the same name
as a method in a base type, but without overriding the base method. See
``System.Reflection.MethodAttributes.NewSlot <http://msdn.microsoft.com/en-us/library/system.reflection.methodattributes.aspx>``_
for more information/. In such cases, using the unbound class instance method
syntax allows you chose precisely which slot you wish to call::

   import System
   System.ICloneable.Clone("hello") # same as : "hello".Clone()

-----------------------------------------------------------------------------
Calling explicitly-implemented methods
-----------------------------------------------------------------------------

\.NET allows a method with a different name to override a base method
implementation or interface method slot. This is useful if a type implements
two interfaces with methods with the same name. This is known as
`explicity implemented interface methods <http://msdn.microsoft.com/en-us/library/4taxa8t2.aspx>`_. For example, `Microsoft.Win32.RegistryKey`
implements `System.IDisposable.Dispose` explicitly::

   print clr.GetClrType(Microsoft.Win32.RegistryKey).GetMethod("Flush") # "Void Flush()"
   print clr.GetClrType(Microsoft.Win32.RegistryKey).GetMethod("Dispose") # "None"

In such cases, IronPython tries to expose the method using its simple name -
if there is not ambiguity::

   from Microsoft.Win32 import Registry
   rkey = Registry.CurrentUser.OpenSubKey("Software")
   rkey.Dispose()

However, it is possible that the type has another method with the same name.
In that case, the explicitly implemented method is not accessible as an attribute.
However, it can still be called by using the unbound class instance method syntax::

   from Microsoft.Win32 import Registry
   rkey = Registry.CurrentUser.OpenSubKey("Software")
   System.IDisposable.Dispose(rkey)

-----------------------------------------------------------------------------
Invoking static .NET methods
-----------------------------------------------------------------------------

Invoking static .NET methods is similar to invoking Python static methods.

-----------------------------------------------------------------------------
Invoking generic methods
-----------------------------------------------------------------------------

Generic methods are exposed as attributes which can be indexed with `type`
objects::

   from System import Activator, Guid
   guid = Activator.CreateInstance[Guid]()

-----------------------------------------------------------------------------
Type parameter inference while invoking generic methods
-----------------------------------------------------------------------------

In many cases, the type parameter can be inferred based on the arguments
passed to the method call. Consider the following use of a generic method [#]_::

   from System.Collections.Generic import IEnumerable, List
   list = List[int]([1, 2, 3])
   import clr
   clr.AddReference("System.Core")
   from System.Linq import Enumerable
   Enumerable.Any[int](list, lambda x : x < 2) # prints "True"

With generic type parameter inference, the last statement can also be written
as::

   Enumerable.Any(list, lambda x : x < 2)

[#]_ System.Core.dll is part of .NET 3.0 and higher.

-----------------------------------------------------------------------------
`ref` and `out` parameters
-----------------------------------------------------------------------------

The Python language passes all arguments by-value. There is not syntax to
indicate that an argument should be passed by-reference like there is in
.NET languages like C# and VB.NET. IronPython has two ways of passing 
ref or out arguments to a method, an implicit way and an explicit way. 
In the implicit way, an argument is passed normally to the method call,
and its (potentially) updated value is returned from the method call
along with the normal return value (if any). This composes well with
the Python feature of multiple return values.
`System.Collections.Generic.Dictionary` has a method with 
`bool TryGetValue(K key, out value)`. It can be called from IronPython
with just one argument, and the call returns a `tuple` where the 
first element is a boolean and the second element is the value (or the
default value of 0.0 if the first element is `False`)::

   d = { "a":100.1, "b":200.2, "c":300.3 }
   from System.Collections.Generic import Dictionary
   d = Dictionary[str, float](d)
   d.TryGetValue("b") # returns (True, 200.2)
   d.TryGetValue("z") # returns (False, 0.0)

In the explicit way, you can pass an instance of ``clr.Reference[T]`` for the
ref or out argument, and its `Value` field will get set by the call. The
explicit way is useful if there are multiple overloads with ref parameters::

   import clr
   r = clr.Reference[float]()
   d.TryGetValue("b", r) # returns True
   print r.Value # prints 200.2

-----------------------------------------------------------------------------
Extension methods
-----------------------------------------------------------------------------

Extension methods are currently not natively supported by IronPython. Hence,
they cannot be invoked like instance methods. Instead, they have to be
invoked like static methods.

==============================================================================
Accessing .NET properties
==============================================================================

\.NET properties are exposed similar to Python attributes. Under the hood,
.NET properties are implemented as a pair of methods to get and set the
property, and IronPython calls the appropriate method depending on
whether you are reading or writing to the properity::

   ba = BitArray(5)
   print ba.Length # calls "BitArray.get_Length()" and prints 5
   ba.Length = 10 # calls "BitArray.set_Length()"

To call the get or set method using the unbound class instance method syntax,
IronPython exposes methods called `GetValue` and `SetValue` on the property
descriptor. The code above is equivalent to the following::

   ba = BitArray(5)
   print BitArray.Length.GetValue(ba)
   BitArray.Length.SetValue(ba, 10)

==============================================================================
Accessing .NET events
==============================================================================

\.NET events are exposed as objects with __iadd__ and __isub__ methods which
allows using `+=` and `-=` to subscribe and unsubscribe from the event.

TODO - invoking a .NET event

==============================================================================
Special .NET types
==============================================================================

-----------------------------------------------------------------------------
.NET arrays 
-----------------------------------------------------------------------------

IronPython supports indexing of `System.Array` with a `type` to access 
strongly-typed arrays. IronPython also adds a `__new__` that accepts
a `IList<T>` to initialize the array. This allows using a Python `list`
literal to initialize a .NET array.

   a = System.Array[int]([1, 2, 3])

-----------------------------------------------------------------------------
.NET Exceptions
-----------------------------------------------------------------------------

The `raise` keyword can raise both Python exceptions as well as .NET 
exceptions::

   >>> raise ZeroDivisionError()
   Traceback (most recent call last):
     File "<stdin>", line 1, in <module>
   ZeroDivisionError
   >>> raise System.DivideByZeroException()
   Traceback (most recent call last):
     File "<stdin>", line 1, in <module>
   ZeroDivisionError: Attempted to divide by zero.

The `except` keyword can catch both Python exceptions as well as .NET
exceptions::

   >>> try:
   ...    raise System.DivideByZeroException()
   ... except System.DivideByZeroException:
   ...    print "This line will get printed..."
   ...
   This line will get printed...
   >>>

^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
The underlying exception object
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

IronPython implements the Python exception mechanism on top of the .NET
exception mechanism. This allows Python exception thrown from Python code to
be caught by non-Python code, and vice versa. However, Python exception
objects need to behave like Python user objects, not builtin types. 
For example, Python code can set arbitrary attributes on Python exception
objects, but not on .NET exception objects::

   e = ZeroDivisionError()
   e.foo = 1 # this should work
   e = System.DivideByZeroException()
   e.foo = 1 # this should raise an AttributeError

To support these two different views, IronPython creates a pair of objects,
a Python exception object and a .NET exception object, where the Python type
and the .NET exception type have a unique one-to-one mapping as defined
in the table below. Both objects
know about each other. The .NET exception object is the one that actually
gets thrown by the IronPython runtime when Python code executes a `raise`
statement. As a result, when Python code uses the `except` keyword to
catch the Python exception, the Python exception object is used. However,
if the exception is caught by C# (for example) code that called the Python
code, then the C# code naturally catches the .NET exception object.

The .NET exception object corresponding to a Python exception object
can be accessed by using the ``clsException`` attribute (if the module
has excecuted `import clr`)::

   import clr
   try:
       1/0
   except ZeroDivisionError as e:
       # prints "<type 'exceptions.ZeroDivisionError'> <type 'DivideByZeroException'>"
       print type(e), type(e.clsException)

=========================== ======================================= =============================================
Python exception            .NET exception
--------------------------- -------------------------------------------------------------------------------------
                            .NET system exception type              IronPython runtime exception type
=========================== ======================================= =============================================
Exception                   System.Exception
SystemExit                                                          IP.O.SystemExit
StopIteration               System.InvalidOperationException
                            subtype
StandardError               System.SystemException
KeyboardInterrupt                                                   IP.O.KeyboardInterruptException
ImportError                                                         IP.O.PythonImportError
EnvironmentError                                                    IP.O.PythonEnvironmentError
IOError                     System.IO.IOException
OSError                     S.R.InteropServices.ExternalException
WindowsError                System.ComponentModel.Win32Exception
EOFError                    System.IO.EndOfStreamException
RuntimeError                IP.O.RuntimeException
NotImplementedError         System.NotImplementedException
NameError                                                           IP.O.NameException
UnboundLocalError                                                   IP.O.UnboundLocalException
AttributeError              System.MissingMemberException
SyntaxError                                                         IP.O.SyntaxErrorException
                                                                    (System.Data has something close)
IndentationError                                                    IP.O.IndentationErrorException
TabError                                                            IP.O.TabErrorException
TypeError                                                           Microsoft.Scripting.ArgumentTypeException
AssertionError                                                      IP.O.AssertionException
LookupError                                                         IP.O.LookupException
IndexError                  System.IndexOutOfRangeException
KeyError                    S.C.G.KeyNotFoundException
ArithmeticError             System.ArithmeticException
OverflowError               System.OverflowException
ZeroDivisionError           System.DivideByZeroException
FloatingPointError                                                  IP.O.PythonFloatingPointError
ValueError                  ArgumentException
UnicodeError                                                        IP.O.UnicodeException
UnicodeEncodeError          System.Text.EncoderFallbackException
UnicodeDecodeError          System.Text.DecoderFallbackException
UnicodeTranslateError                                               IP.O.UnicodeTranslateException
ReferenceError                                                      IP.O.ReferenceException
SystemError                                                         IP.O.PythonSystemError
MemoryError                 System.OutOfMemoryException
Warning                     System.ComponentModel.WarningException
UserWarning                                                         IP.O.PythonUserWarning
DeprecationWarning                                                  IP.O.PythonDeprecationWarning
PendingDeprecationWarning                                           IP.O.PythonPendingDeprecationWarning
SyntaxWarning                                                       IP.O.PythonSyntaxWarning
OverflowWarning                                                     IP.O.PythonOverflowWarning
RuntimeWarning                                                      IP.O.PythonRuntimeWarning
FutureWarning                                                       IP.O.PythonFutureWarning
=========================== ======================================= =============================================

^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Revisiting the `rescue` keyword
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Given that `raise` results in the creation of both a Python exception object
and a .NET exception object, and given that the `rescue` keyword can catch
both Python exceptions and .NET exceptions, a question arises of which of
the exception objects will be used by the `rescue` keyword. The answer is 
that it is the type used in the `rescue` clause. i.e. if the `rescue` clause
uses the Python exception, then the Python exception object
will be used. If the `rescue` clause uses the .NET exception, then the 
.NET exception object will be used.

The following example shows how `1/0` results in the creation of two objects,
and how they are linked to each other. The exception is first caught as a
.NET exception. The .NET exception is raised again, but is then caught as
a Python exception::

   import System
   
   try:
       try:
           1/0
       except System.DivideByZeroException as e1:
           raise e1
   except ZeroDivisionError as e2:
      pass
   
   # prints "<type 'DivideByZeroException'> <type 'exceptions.ZeroDivisionError'>"
   print type(e1), type(e2)
   # prints "True"
   print e2.clsException is e1

-----------------------------------------------------------------------------
Enumerations
-----------------------------------------------------------------------------

\.NET enumeration types are sub-types of `System.Enum`. The enumeration values
of an enumeration type are exposed as class attributes::

   print System.AttributeTargets.All # access the value "All"

IronPython also supports using the bit-wise operators with the enumeration
values::

   >>> System.AttributeTargets.Class | System.AttributeTargets.Method
   <enum System.AttributeTargets: Class, Method>

-----------------------------------------------------------------------------
Value types
-----------------------------------------------------------------------------

Python expects all mutable values to be represented as a reference type. .NET, 
on the other hand, introduces the concept of value types which are mostly 
copied instead of referenced. In particular .NET methods and properties 
returning a value type will always return a copy. 

This can be confusing from a Python programmer’s perspective since a subsequent 
update to a field of such a value type will occur on the local copy, not within 
whatever enclosing object originally provided the value type.

While most .NET value types are designed to be immutable, and the .NET design
guidelines recommend value tyeps be immutable, this is not enforced by .NET, 
and so there do exist some .NET valuetype that are mutable. TODO - Example.

For example, take the following C# definitions::

   struct Point {
       # Poorly defined struct - structs should be immutable
       public int x;
       public int y;
   }
   
   class Line {
       public Point start;
       public Point end;
   	
       public Point Start { get { return start; } }
       public Point End { get { return end; } }
   }

If `line` is an instance of the reference type Line, then a Python programmer 
may well expect "`line.Start.x = 1`" to set the x coordinate of the start of 
that line. In fact the property `Start` returned a copy of the `Point` 
value type and it’s to that copy the update is made::

   print line.Start.x    # prints ‘0’
   line.Start.x = 1
   print line.Start.x    # still prints ‘0’
	
This behavior is subtle and confusing enough that C# produces a compile-time 
error if similar code is written (an attempt to modify a field of a value type 
just returned from a property invocation).

Even worse, when an attempt is made to modify the value type directly 
via the start field exposed by Line (i.e. “`line.start.x = 1`”), IronPython 
will still update a local copy of the `Point` structure. That’s because 
Python is structured so that “foo.bar” will always produce a useable 
value: in the case above “line.start” needs to return a full value 
type which in turn implies a copy.

C#, on the other hand, interprets the entirety of the “`line.start.x = 1`” 
statement and actually yields a value type reference for the “line.start” 
part which in turn can be used to set the “x” field in place.

This highlights a difference in semantics between the two languages. 
In Python “line.start.x = 1” and “foo = line.start; foo.x = 1” are 
semantically equivalent. In C# that is not necessarily so.

So in summary: a Python programmer making updates to a value type 
embedded in an object will silently have those updates lost where the 
same syntax would yield the expected semantics in C#. An update to 
a value type returned from a .NET property will also appear to 
succeed will updating a local copy and will not cause an error 
as it does in the C# world. These two issues could easily become 
the source of subtle, hard to trace bugs within a large application.

In an effort to prevent the unintended update of local value type copies 
and at the same time preserve as pythonic and consistent a view of 
the world as possible, direct updates to value type fields are not
allowed by IronPython, and raise a ValueError::

   >>> line.start.x = 1
   Traceback (most recent call last):
      File , line 0, in input##7
   ValueError Attempt to update field x on value type Point; value type fields can not be directly modified

This renders value types “mostly” immutable; updates are still possible 
via instance methods on the value type itself.

-----------------------------------------------------------------------------
Proxy types
-----------------------------------------------------------------------------

IronPython cannot directly use `System.MarshalByRefObject` instances. IronPython 
uses reflection at runtime to determine how to access an object. 
However, `System.MarshalByRefObject` instances do not support reflection.

You *can* use the unbound class instance method syntax :ref: to call methods
on such proxy objects.


*******************************************************************************
Subclassing .NET types
*******************************************************************************

Sub-classing of .NET types and interfaces is supported using the Python `class`
syntax. .NET types and methods can be used as one of the sub-types in the
`class` construct::

   class MyClass(System.Attribute, System.ICloneable, System.IComparable): pass

\.NET does not support multiple inheritance while Python does. IronPython
allows using multiple Python classes as subtypes, and also multiple .NET
interfaces, but there can only be one .NET class in the set of subtypes::

   class MyPythonClass1(object): pass
   class MyPythonClass2(object): pass
   class MyMixedClass(MyPythonClass1, MyPythonClass2, System.Attribute): pass

Instances of the class do actually inherit from the specified .NET
base type. This is important because this means that statically-typed
.NET code can access the object using the .NET type. The following snippet
uses Reflection to show that the object can be cast to the .NET sub-class::

   class MyClass(System.ICloneable): pass
   o = MyClass()
   System.Type.GetType("System.ICloneable").IsAssignableFrom(o.GetType()) # returns True

Note that the Python class does not really inherit from the .NET sub-class.
See :ref: .

==============================================================================
Overriding methods 
==============================================================================

Base type methods can be overriden by defining a Python method with the same
name::

   class MyClass(System.ICloneable):
       def Clone(self): return MyClass()
   o = MyClass()
   o.Clone()

IronPython does require you to provide implementations of interface methods
in the class declaration. The method lookup is done dynamically when the method
is accessed. Here we see that AttributeError is raised if the method is not
defined::

   class MyClass(System.ICloneable): pass
   o = MyClass()
   o.Clone() # raises AttributeError

------------------------------------------------------------------------------
Methods with multiple overloads
------------------------------------------------------------------------------

Python does not support method overloading. A class can have only one method
with a given name. As a result, you cannot override specific method overloads
of a .NET sub-type. Instead, you need to use <TODO> arguments, and then
determine the method overload that was invoked by inspecting the types of
the arguments::

   import clr
   import System
   StringComparer = System.Collections.Generic.IEqualityComparer[str]
   
   class MyComparer(StringComparer):
       def GetHashCode(self, *args):
            if len(args) == 0:
                print "Object.GetHashCode() called"
                return id(self)
            
            if len(args) == 1 and type(args[0]) == str:
                print "StringComparer.GetHashCode() called"
                return args[0].GetHashCode()
                
            assert("Should never get here")
   
   comparer = MyComparer()
   getHashCode1 = clr.GetClrType(System.Object).GetMethod("GetHashCode")
   args = System.Array[object](["another string"])
   getHashCode2 = clr.GetClrType(StringComparer).GetMethod("GetHashCode")
   
   # Used Reflection instead of using a statically-typed language to call the two overloads
   getHashCode1.Invoke(comparer, None) # prints "Object.GetHashCode() called"
   getHashCode1.Invoke(comparer, args)  # prints "StringComparer.GetHashCode() called"

.. note::

   Determining the exact overload that was invoked may not be possible, for
   example, if `None` is passed in as an argument.   

------------------------------------------------------------------------------
Methods with ref or out parameters
------------------------------------------------------------------------------

Python does not have syntax for specifying whether a method paramter is
passed by-reference since arguments are always passed by-value. When overriding
a .NET method with ref or out parameters, the ref or out paramter is received
as a ``clr.Reference`` instance. The incoming argument value is accessed by
reading the `Value` property, and the resulting value is specified by setting
the `Value` property::

   import clr
   import System
   StrFloatDictionary = System.Collections.Generic.IDictionary[str, float]
   
   class MyDictionary(StrFloatDictionary):
       def TryGetValue(self, key, value):
           if key == "yes":
               value.Value = 100.1 # set the *out* parameter
               return True
           else:
               value.Value = 0.0  # set the *out* parameter
               return False
       # Other methods of IDictionary not overriden for brevity
   
   d = MyDictionary()
   # Used Reflection instead of using a statically-typed language
   tryGetValue = clr.GetClrType(StrFloatDictionary).GetMethod("TryGetValue")
   for key in ("yes", "no"):
       args = System.Array[object]([key, 0.0])
       result = tryGetValue.Invoke(d, args)
       print result, args[1] # First time : "True 100.1". Second time : "False 0.0"

------------------------------------------------------------------------------
Generic methods
------------------------------------------------------------------------------


==============================================================================
Overriding properties
==============================================================================

\.NET properties are backed by a pair of .NET methods for reading and writing
the property. The C# compiler automatically names them as `get_<PropertyName>`
and `set_<PropertyName>`. However, the CLR itself does not require any 
specific naming pattern for these methods, and the names are stored in the
the metadata associated with the property definition. The names can be 
accessed using the `GetGetMethod` and `GetSetMethods` of the
`System.Reflection.PropertyInfo` class. Overriding a virtual property
requires defining a Python method with the same names as the underlying
getter or setter .NET method::

   import clr
   import System
   StringCollection = System.Collections.Generic.ICollection[str]
   
   class MyCollection(StringCollection):
       def get_Count(self):
           return 100
       # Other methods of ICollection not overriden for brevity
   
   c = MyCollection()
   getCount = clr.GetClrType(StringCollection).GetProperty("Count").GetGetMethod()
   # Used Reflection instead of using a statically-typed language
   print getCount.Invoke(c, None) # prints 100

==============================================================================
Overiding events
==============================================================================

    class PySubclass(IEvent10):
        def __init__(self):
            self.events = []
        def add_Act(self, value):
            self.events.append(value)
        def remove_Act(self, value):
            self.events.remove(value)
        def call(self):
            for x in self.events:
                x(1, 2)

==============================================================================
Calling base constructor
==============================================================================


*******************************************************************************
Declaring .NET types
*******************************************************************************

==============================================================================
Relationship of classes in Python code and normal .NET types
==============================================================================

A class definition in Python does not map directly to a unique .NET type. This 
is because the semantics of classes is different between Python and .NET. For 
example, in Python it is possible to change the base types just by assigning 
to the __bases__ attribute on the type object. However, the same is not 
possible with .NET types. Hence, IronPython implements Python classes without 
mapping them directly to .NET types. IronPython *does* use some .NET type
for the objects, but it is members do not match the Python attributes at
all. Instead, the Python class is stored in a .NET field called `.class`, and 
Python instance attributes are stored in a dictionary that is stored in a .NET 
field called `.dict` [#]_ ::

   import clr
   
   class MyClass(object): pass
   o = MyClass()
   
   print o.GetType().FullName # prints something like "IronPython.NewTypes.System.Object_1$1"
   fieldNames = [field.Name for field in o.GetType().GetFields()]
   print fieldNames # prints "['.class', '.dict', '.slots_and_weakref']"
   print o.GetType().GetField(".class").GetValue(o) == MyClass # prints "True"
   
   class MyClass2(MyClass): pass
   o2 = MyClass2()
   print o.GetType() == o2.GetType() # prints True!

Also See :ref: "Type-system unification (type and System.Type)"

[#]_ These field names are implementation details, and could change.

==============================================================================
__clrtype__
==============================================================================

It is sometimes required to have control over the .NET type generated for the 
Python class. This is because some .NET APIs expect the user to define a .NET
type with certain attributes and members. For example, to define a pinvoke 
method, the user is required to define a .NET type with a .NET method marked 
with ``DllImportAttribute <http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.dllimportattribute.aspx>``_,
and where the signature of the .NET method exactly describes the target platform method.

Starting with IronPython 2.6, IronPython supports a low-level hook which 
allows customization of the .NET type corresponding to a Python class. If the 
metaclass of a Python class has an attribute called `__clrtype__`, the 
attribute is called to generate a .NET type. This allows the user to control
the the details of the generated .NET type. However, this is a low-level hook, 
and the user is expected to build on top of it. 

The ClrType sample available in the IronPython website shows how to build on 
top of the __clrtype__ hook.

*******************************************************************************
Accessing Python code from other .NET code
*******************************************************************************

Statically-typed languages like C# and VB.Net can be compiled into an assembly
that can then be used by other .NET code. However, IronPython code is executed
dynamically using `ipy.exe`. If you want to run Python code from other .NET 
code, there are a number of ways of doing it.

==============================================================================
Using the DLR Hosting APIs
==============================================================================

==============================================================================
Compiling Python code into an assembly
==============================================================================


==============================================================================
`dynamic`
==============================================================================

Starting with .NET 4.0, C# and VB.Net support access to IronPython objects
using the `dynamic` keyword.

*******************************************************************************
Integration with Python features
*******************************************************************************

* Type system integration. See :ref: "Type-system unification (type and System.Type)"

* List comprehension works with any .NET type that implements IList

* `with` works with with any System.IEnumerable

* pickle works with ISerializable

* __doc__ uses XML comments


==============================================================================
Mapping between .NET concepts and Python concepts
==============================================================================

Some method names are treated specially by some languages even if they are
not specified in the ``Common Language Specification <http://todo>``_.
This is a list of method names that IronPython treats specially.

* op_Implicit
  This is used for type conversions.
* op_Explicit
  This is used for type conversions.
* op_Addition
  This is exposed as `__add__`
* get_Item, set_Item, Item
  This is exposed as `__getelem__` TODO

Idisposable -> __enter__ / __exit__
Ienumerator -> next()
Icollection/Icollection<T> -> __len__
Ienumerable/Ienumerator/Ienumerable<T>/Ienumerator<T> -> __iter__
Iformattable -> __format__
Idictionary<T, K> / Icollection<T> / Ilist / Idictionary / Ienumerable / IEnumerator / Ienumerable<T> Ienumerator<T> -> __contains__
op_Addition, etc… -> __add__


*******************************************************************************
OleAutomation and COM interop 
*******************************************************************************

*******************************************************************************
Miscellaneous
*******************************************************************************

==============================================================================
Security model
==============================================================================

All the IronPython assemblies are SecurityTransparent.

==============================================================================
Mapping between Python builtin types and .NET types
==============================================================================

IronPython is an implementation of the Python language on top of .NET. As such,
IronPython uses various .NET types to implement Python types. Usually, you do
not have to think about this. However, you may sometimes have to know about it.

=====================   ============
Python type             .NET type 
=====================   ============
object                  System.Object
int                     System.Int32
long                    System.Numeric.BigInteger [#]_
float                   System.Double
str, unicode            System.String
TrueClass, FalseClass   System.Boolean
=====================   ============

.. [#] This is true only in CLR 4. In previous versions of the CLR, `long` is
       implemented by IronPython itself.

.. [#] This is not completely correct. In Python, True and False are singleton 
       objects whereas
       implemented by IronPython itself.

------------------------------------------------------------------------------
`import clr` and builtin types
------------------------------------------------------------------------------

Since some Python builtin types are implemented as .NET types, the question
arises whether the types work like Python types or like .NET types. The answer
is that by default, the types work like Python types. However, if a module
executes `import clr`, the types work like both Python types and like .NET types.
For example, by default, object' does not have the `System.Object` method called 
`GetHashCode`::

   >>> hasattr(object, "__hash__")
   True
   >>> hasattr(object, "__hash__")
   False

However, once you do `import clr`, `object` has both `__hash__` as well as
`GetHashCode`::

   >>> import clr
   >>> hasattr(object, "__hash__")
   True
   >>> hasattr(object, "__hash__")
   False

*******************************************************************************
Reference documentation
*******************************************************************************

`import clr` exposes extra functionality on some Python types (even though
they do not map to any .NET type)

Method objects
- Overloads

*******************************************************************************
Appendix - Detailed type conversion rules
*******************************************************************************

=========================================   ============================================
Python argument type                        .NET method parameter type 
=========================================   ============================================
int                                         System.Byte, System.SByte, 
                                            System.UInt16, System.Int16
User object with __int__ method             *Same as int*
str or unicode of size 1                    System.Char
User object with __str__ method             *Same as str*
float                                       System.Float
tuple with T-typed elements                 System.Collections.Generic.IEnumerable<T> or
                                            System.Collections.Generic.IList<T>
function, method                            System.Delegate and any of its sub-classes
dict with K-typed keys and V-typed values   System.Collections.Generic.IDictionary<K,V>
type                                        System.Type
=========================================   ============================================


*******************************************************************************
Appendix - Detailed method overload resolution rules
*******************************************************************************

TODO: This is not correct

- Same type, or numerically compatible type with a lossless conversion
- Implicit conversion
- Conversion according to Appendix above
- Explicit conversion