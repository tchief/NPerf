﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("NPerf.Experiment")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("NPerf.Experiment")]
[assembly: AssemblyCulture("")]

#if DEBUG
[assembly: InternalsVisibleTo("NPerf.Test.Experiment")]
[assembly: InternalsVisibleTo("NPerf.Test.Helpers")]
#endif

[assembly: ComVisible(false)]
