﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Cave
{
    /// <summary>
    /// Provides <see cref="AppDomain"/> specific extensions
    /// </summary>
    public class AppDom
    {
        /// <summary>Gets all loaded types assignable to the specified one and containing a default constructor.</summary>
        /// <typeparam name="T">Type or interface all types need to be assignable to</typeparam>
        /// <returns>Returns a list of types</returns>
        public static List<T> GetTypes<T>()
        {
            Type interfaceType = typeof(T);
            List<T> types = new List<T>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.IsAbstract) continue;
                    if (interfaceType.IsAssignableFrom(type))
                    {
                        try { T api = (T)Activator.CreateInstance(type); types.Add(api); }
                        catch { Trace.TraceError($"Could not create instance of type {type}!"); }
                    }
                }
            }
            return types;
        }

        /// <summary>Gets the installation unique identifier.</summary>
        /// <returns></returns>
        public static Guid GetInstallationGuid()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string fileName = Path.Combine(root, Path.Combine("CaveSystems", "installation.guid"));
            if (!File.Exists(fileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                File.WriteAllText(fileName, Guid.NewGuid().ToString());
            }
            return new Guid(File.ReadAllLines(fileName)[0]);
        }

        static uint m_ProgramID;

        /// <summary>Gets the installation identifier.</summary>
        /// <value>The installation identifier.</value>
        /// <exception cref="NotSupportedException"></exception>
        public static uint ProgramID
        {
            get
            {
                if (m_ProgramID == 0)
                {
                    m_ProgramID = (uint)(GetInstallationGuid().GetHashCode() ^ AppDomain.CurrentDomain.BaseDirectory.GetHashCode());
                    if (m_ProgramID == 0) throw new NotSupportedException();
                }
                return m_ProgramID;
            }
        }

        /// <summary>
        /// Provides loader modes
        /// </summary>
        public enum LoadMode
        {
            /// <summary>Throw exceptions on loader error and do not load additional assemblies.</summary>
            None = 0,

            /// <summary>Do not throw exceptions</summary>
            NoException = 1,

            /// <summary>Try to load assemblies if type cannot be found (insecure!)</summary>
            /// <remarks>Using this function may result in a security risk if someone can put assemblies to the program folder!</remarks>
            LoadAssemblies = 2,
        }

        /// <summary>Finds the type with the specified name.</summary>
        /// <param name="name">The name of the type.</param>
        /// <param name="mode">The loader mode.</param>
        /// <returns>Returns the first matching type.</returns>
        /// <exception cref="System.TypeLoadException"></exception>
#pragma warning disable CS0618 // Allow obsolete functions/fields
        public static Type FindType(string name, LoadMode mode = 0)
        {
            {
                //try direct load first.
                var type = Type.GetType(name, false);
                if (type != null) return type;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Trace.TraceInformation("Searching for type <cyan>{0}<default> in assembly <cyan>{1}", name, assembly);
                var type = assembly.GetType(name, false);
                if (type != null)
                {
                    Trace.TraceInformation("Using type <green>{0}", type.FullName);
                    return type;
                }
            }

            if (0 != (mode & LoadMode.LoadAssemblies))
            {
                string dll = name;
                while (dll.Length > 1)
                {
                    int i = dll.LastIndexOf('.');
                    if (i < 0) break;
                    dll = dll.Substring(0, i);
                    Trace.TraceInformation("<red>(Insecure)<default> loading assembly <yellow>{0}<default> for typesearch <cyan>{1}", dll, name);
                    var assembly = Assembly.LoadWithPartialName(dll);
                    if (assembly != null)
                    {
                        Trace.TraceInformation("<red>(Insecure)<default> loaded assembly <yellow>{0}", assembly);
                        var type = assembly.GetType(name, false);
                        if (type != null)
                        {
                            Trace.TraceInformation("Using type <yellow>{0}", type.FullName);
                            return type;
                        }
                    }
                    if (dll.IndexOf('.') < 0) break;
                }
            }
            if (0 == (mode & LoadMode.NoException))
            {
                Trace.TraceInformation("Could not find type <red>{0}", name);
                throw new TypeLoadException(string.Format("Cannot load type {0}", name));
            }
            return null;
        }
#pragma warning restore CS0618

        /// <summary>Finds the loaded assembly with the specified name.</summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception if no assembly can be found].</param>
        /// <returns>Returns the first matching type.</returns>
        /// <exception cref="ArgumentException">Cannot find assembly {0}</exception>
        public static Assembly FindAssembly(string name, bool throwException)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == name) return assembly;
            }
            if (throwException) throw new ArgumentException(string.Format("Cannot find assembly {0}", name));
            return null;
        }
    }
}