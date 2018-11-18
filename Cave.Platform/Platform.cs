#region CopyRight 2018
/*
    Copyright (c) 2003-2018 Andreas Rohleder (andreas@rohleder.cc)
    All rights reserved
*/
#endregion
#region License LGPL-3
/*
    This program/library/sourcecode is free software; you can redistribute it
    and/or modify it under the terms of the GNU Lesser General Public License
    version 3 as published by the Free Software Foundation subsequent called
    the License.

    You may not use this program/library/sourcecode except in compliance
    with the License. The License is included in the LICENSE file
    found at the installation directory or the distribution package.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be included
    in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
#region Authors & Contributors
/*
   Author:
     Andreas Rohleder <andreas@rohleder.cc>

   Contributors:

 */
#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Cave
{
    /// <summary>
    /// Provides access to the current platform type
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Platform
    {
        static PlatformType type;
        static string systemVersionString;

        /// <summary>
        /// Obtains the <see cref="PlatformType"/> of the current platform
        /// </summary>
        public static PlatformType Type
        {
            get
            {
                if (type == PlatformType.Unknown)
                {
                    switch ((int)Environment.OSVersion.Platform)
                    {
                        case 0: /*Win32S*/
                        case 1: /*Win32NT*/
                        case 2: /*Win32Windows*/
                            type= PlatformType.Windows;
                            break;

                        case 3: /*Windows CE / Compact Framework*/
                            type = PlatformType.CompactFramework;
                            break;

                        case 4: /*Unix, mono returns this on all platforms except windows*/
                            if (AppDom.FindAssembly("Mono.Android", false) != null) return PlatformType.Android;
                            try { if (File.Exists("/usr/lib/libc.dylib")) return PlatformType.MacOS; }
                            catch { /*Exception on Android on this one... why ?*/ }
                            string osType = SystemVersionString.ToLower();
                            if (osType.StartsWith("linux")) type = PlatformType.Linux;
                            else if (osType.StartsWith("darwin")) type = PlatformType.MacOS;
                            else if (osType.StartsWith("solaris")) type = PlatformType.Solaris;
                            else if (osType.StartsWith("bsd")) type = PlatformType.BSD;
                            else if (osType.StartsWith("msys")) type = PlatformType.Windows;
                            else if (osType.StartsWith("cygwin")) type = PlatformType.Windows;
                            else type = PlatformType.UnknownUnix;
                            break;

                        case 5: /*Xbox*/
                            type = PlatformType.Xbox;
                            break;

                        case 6: /*MacOSX*/
                            type = PlatformType.MacOS;
                            break;

                        case 128:
                            type = PlatformType.UnknownUnix;
                            break;

                        default:
                            type = PlatformType.Unknown;
                            break;
                    }
                }
                return type;
            }
        }

        /// <summary>Gets the system version string.</summary>
        /// <value>The system version string.</value>
        public static string SystemVersionString
        {
            get
            {
                if (systemVersionString == null)
                {
					systemVersionString = Environment.OSVersion.VersionString;
                    if (IsMicrosoft)
                    {
                        //all done
                    }
                    else if (IsAndroid)
                    {
                        // TODO Android.OS.Build
                        systemVersionString = "Android " + systemVersionString;
                    }
                    else
                    {
                        Process process = Process.Start(new ProcessStartInfo("uname", "-a") { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, });
                        string systemVersionString = null;
                        ThreadPool.QueueUserWorkItem(delegate { systemVersionString = process.StandardOutput.ReadToEnd(); });
                        if (!process.WaitForExit(10000)) try { process.Kill(); } catch { }
                        int i = systemVersionString?.IndexOf('\n') ?? -1;
                        if (i > -1) systemVersionString = systemVersionString.Substring(0, i);
                    }
                }
                return systemVersionString;
            }
        }

        /// <summary>
        /// Obtains whether we run at a microsoft os or not
        /// </summary>
        public static bool IsMicrosoft
        {
            get
            {
                switch ((int)Environment.OSVersion.Platform)
                {
                    case 0: /*Win32S*/
                    case 1: /*Win32NT*/
                    case 2: /*Win32Windows*/
                    case 3: /*Windows CE / Compact Framework*/
                    case 5: /*Xbox*/
                        return true;

                    default:
                    case 4: /*Unix*/
                    case 6: /*MacOSX*/
                    case 128:
                        return false;
                }
            }
        }


        /// <summary>
        /// Obtains whether we run under mono or not
        /// </summary>
        public static bool IsMono
        {
            get
            {
                return AppDom.FindType("Mono.Runtime", AppDom.LoadMode.NoException) != null;
            }
        }

		/// <summary>
		/// Obtains whether we run at android or not
		/// </summary>
		public static bool IsAndroid
		{
			get
			{
				return (AppDom.FindAssembly("Mono.Android", false) != null);
			}
		}
    }
}
