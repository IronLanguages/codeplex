def test() {
   import System
   var ht = new System.Collections.Hashtable()
   var sw = new System.Diagnostics.Stopwatch()
   var start = sw.Start
   var stop  = sw.Stop
   var i = 0;
   start()
   
   while (i < 1000000) {
       ht["Key"] = "value"
       ht["Key"]
       i = i + 1
   }

   stop()

   print "Results"
   print i
   print "***************************"
   print sw.ElapsedMilliseconds
   print sw.Elapsed
}

test()

