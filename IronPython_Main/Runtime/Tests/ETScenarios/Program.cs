using System;
using System.Collections.Generic;
#if !SILVERLIGHT
using System.Windows.Forms;

[assembly:System.Security.AllowPartiallyTrustedCallers]
#endif

namespace ETScenarios {
    public static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
#if !SILVERLIGHT
        [STAThread]
#endif
        static void Main() {
            
        }
    }
}
