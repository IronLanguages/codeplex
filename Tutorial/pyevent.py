#####################################################################################
#  
#  Copyright (c) Microsoft Corporation. All rights reserved.
# 
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
# 
#  You must not remove this notice, or any other, from this software.
# 
######################################################################################

class event(object):
    """Provides CLR event-like functionality for Python.  This is a the public event helper that allows adding and removing of events"""
    __slots__ = ['events']
        
    def __init__(self, *args):
        self.events = []
        for a in args:
            for x in a:
                self.events.append(x)
    
    def __iadd__(self, other):
        if issubclass(other.__class__, event):
            self.events.extend(other.events)
        elif issubclass(other.__class__, event_caller):
            self.events.extend(other.event.events)
        else:
            self.events.append(other)
        return self
        
    def __isub__(self, other):
        if issubclass(other.__class__, event):
            newEv = []
            for x in self.events:
                if not other.events.contains(x):
                    newEv.append(x)
            self.events = newEv
        elif issubclass(other.__class__, event_caller):
            newEv = []
            for x in self.event.events:
                if not other.events.contains(x):
                    newEv.append(x)
            self.events = newEv
        else:
            self.events.remove(other)
        return self

    def make_caller(self):
        return event_caller(self)

class event_caller(object):
    """Provides CLR event-like functionality for Python.  This is the protected event caller that allows the owner to raise the event"""
    __slots__ = ['event']
    
    def __init__(self, event):
        self.event = event
            
    def __call__(self, *args):
        for ev in self.event.events:
            ev(args)
    
    def __set__(self, val):
        raise ValueError, "cannot assign to an event, can only add or remove handlers"
    
	def __delete__(self, val):
		raise ValueError, "cannot delete an event, can only add or remove handlers"

	def __get__(self, instance, owner):
		return self
		
		
def make_event(*args):
    """Creates an event object tuple.  The first value in the tuple can be exposed to allow external code to hook and unhook from the event.  The second
    value can be used to raise the event and can be stored in a private variable"""
    res = event(args)
    
    return (res, res.make_caller())