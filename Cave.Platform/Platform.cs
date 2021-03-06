using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cave
{
    /// <summary>
    /// Provides access to the current platform type.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Platform
    {
        /// <summary>
        /// Gets the <see cref="PlatformType"/> of the current platform.
        /// </summary>
        public static PlatformType Type => GetCached(nameof(Type), GetPlatformType);

        /// <summary>Gets the system version string.</summary>
        /// <value>The system version string.</value>
        public static string SystemVersionString => GetCached(nameof(SystemVersionString), GetSystemVersionString);

        /// <summary>
        /// Gets a value indicating whether we run under mono or not.
        /// </summary>
        public static bool IsMono => GetCached(nameof(IsMono), GetIsMono);

        /// <summary>
        /// Gets a value indicating whether we run at android or not.
        /// </summary>
        public static bool IsAndroid => GetCached(nameof(IsAndroid), GetIsAndroid);

        /// <summary>
        /// Gets a value indicating whether we run at a microsoft os or not.
        /// </summary>
        public static bool IsMicrosoft => GetCached(nameof(IsMicrosoft), GetIsMicrosoft);

        static Dictionary<string, object> values = new Dictionary<string, object>();

        static T GetCached<T>(string name, Func<T> getter)
        {
            if (!values.TryGetValue(name, out object value))
            {
                value = getter();
                values[name] = value;
            }
            return (T)value;
        }

        static PlatformType GetPlatformType()
        {
            switch ((int)Environment.OSVersion.Platform)
            {
                case 0: /*Win32S*/
                case 1: /*Win32NT*/
                case 2: /*Win32Windows*/
                    return PlatformType.Windows;

                case 3: /*Windows CE / Compact Framework*/
                    return PlatformType.CompactFramework;

                case 4: /*Unix, mono returns this on all platforms except windows*/
                    if (AppDom.FindAssembly("Mono.Android", false) != null)
                    {
                        return PlatformType.Android;
                    }

                    try
                    {
                        if (File.Exists("/usr/lib/libc.dylib"))
                        {
                            return PlatformType.MacOS;
                        }
                    }
                    catch
                    { /*Exception on Android on this one... why ?*/
                    }

                    string osType = SystemVersionString.ToLower();
                    if (osType.StartsWith("linux"))
                    {
                        return PlatformType.Linux;
                    }
                    if (osType.StartsWith("darwin"))
                    {
                        return PlatformType.MacOS;
                    }
                    if (osType.StartsWith("solaris"))
                    {
                        return PlatformType.Solaris;
                    }
                    if (osType.StartsWith("bsd"))
                    {
                        return PlatformType.BSD;
                    }
                    if (osType.StartsWith("msys"))
                    {
                        return PlatformType.Windows;
                    }
                    if (osType.StartsWith("cygwin"))
                    {
                        return PlatformType.Windows;
                    }
                    return PlatformType.UnknownUnix;

                case 5: /*Xbox*/
                    return PlatformType.Xbox;

                case 6: /*MacOSX*/
                    return PlatformType.MacOS;

                case 128:
                    return PlatformType.UnknownUnix;

                default:
                    return PlatformType.Unknown;
            }
        }

        static string GetSystemVersionString()
        {
            var systemVersionString = Environment.OSVersion.VersionString;
            if (IsMicrosoft)
            {
                // all done
            }
            else if (IsAndroid)
            {
                // TODO Android.OS.Build
                systemVersionString = "Android " + systemVersionString;
            }
            else if (File.Exists("/proc/version"))
            {
                systemVersionString = File.ReadAllText("/proc/version");
            }
            else
            {
                var versionString = "";
                var task = Task.Factory.StartNew(() =>
                {
                    Process process = Process.Start(new ProcessStartInfo("uname", "-a")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    });
                    versionString = process.StandardOutput.ReadToEnd();
                });
                if (task.Wait(1000))
                {
                    systemVersionString = versionString;
                }
            }
            return systemVersionString.BeforeFirst('\n');
        }

        static bool GetIsMicrosoft()
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

#if alternative
        static PlatformType GetPlatformType()
        {
            if (AppDom.FindAssembly("Mono.Android", false) != null)
            {
                return PlatformType.Android;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return PlatformType.Windows;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return PlatformType.MacOS;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return PlatformType.Linux;
            }
            try
            {
                if (File.Exists("/usr/lib/libc.dylib"))
                {
                    return PlatformType.MacOS;
                }
            }
            catch
            {
                /*Exception on Android on this one... why ?*/
            }

            string osType = RuntimeInformation.OSDescription.ToLower();
            if (osType.StartsWith("linux"))
            {
                return PlatformType.Linux;
            }
            if (osType.StartsWith("darwin"))
            {
                return PlatformType.MacOS;
            }
            if (osType.StartsWith("solaris"))
            {
                return PlatformType.Solaris;
            }
            if (osType.StartsWith("bsd"))
            {
                return PlatformType.BSD;
            }
            if (osType.StartsWith("msys"))
            {
                return PlatformType.Windows;
            }
            if (osType.StartsWith("cygwin"))
            {
                return PlatformType.Windows;
            }
            return PlatformType.UnknownUnix;
        }

        static string GetSystemVersionString()
        {
            var systemVersionString = RuntimeInformation.OSDescription;
            if (IsMicrosoft)
            {
                // all done
            }
            else if (IsAndroid)
            {
                // TODO Android.OS.Build
                systemVersionString = "Android " + systemVersionString;
            }
            else
            {
                Process process = Process.Start(new ProcessStartInfo("uname", "-a") { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, });
                var task = Task.Factory.StartNew(() => { systemVersionString = process.StandardOutput.ReadToEnd(); });
                if (!process.WaitForExit(10000))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                    }
                }
                task.Wait();

                int i = systemVersionString?.IndexOf('\n') ?? -1;
                if (i > -1)
                {
                    systemVersionString = systemVersionString.Substring(0, i);
                }
            }
            return systemVersionString;
        }

        static bool GetIsMicrosoft()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return true;
            }
            return false;
        }
#endif

        static bool GetIsMono()
        {
            return AppDom.FindType("Mono.Runtime", null, AppDom.LoadMode.NoException) != null;
        }

        static bool GetIsAndroid()
        {
            return AppDom.FindType("Android.Runtime", null, AppDom.LoadMode.NoException) != null;
        }
    }
}
