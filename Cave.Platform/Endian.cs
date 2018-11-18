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
using System.Diagnostics.CodeAnalysis;

namespace Cave
{
    /// <summary>
    /// Provides Endian Tools
    /// </summary>
    public static class Endian
    {
        /// <summary>Swaps the endian type of the specified data.</summary>
        /// <param name="data">The data.</param>
        /// <param name="bytes">The bytes to swap (2..x).</param>
        /// <returns></returns>
        public static byte[] Swap(byte[] data, int bytes)
        {
            if (bytes < 2) throw new ArgumentOutOfRangeException(nameof(bytes));
            byte[] result = new byte[data.Length];
            bytes--;
            for (int i = 0; i < data.Length;)
            {
                int e = i + bytes;
                for (int n = 0; n <= bytes; n++, i++, e--)
                {
                    result[e] = data[i];
                }
            }
            return result;
        }

        /// <summary>Swaps the byte order of a value</summary>
        /// <param name="value">Value to swap the byte order of</param>
        /// <returns>Byte order-swapped value</returns>
        public static ushort Swap(ushort value)
        {
            return (ushort)((value >> 8) | ((value & 0xFF) << 8));
        }

        /// <summary>Swaps the byte order of a value</summary>
        /// <param name="value">Value to swap the byte order of</param>
        /// <returns>Byte order-swapped value</returns>
        public static uint Swap(uint value)
        {
            return (value >> 24) | ((value & 0xFF00) << 8) | ((value >> 8) & 0xFF00) | (value << 24);
        }

        /// <summary>Swaps the byte order of a value</summary>
        /// <param name="value">Value to swap the byte order of</param>
        /// <returns>Byte order-swapped value</returns>
        public static ulong Swap(ulong value)
        {
            return ((value >> 56) | (0xFF00 & (value >> 40)) | (0xFF0000 & (value >> 24)) | (0xFF000000 & (value >> 8)) |
                ((value & 0xFF000000) << 8) | ((value & 0xFF0000) << 24) | ((value & 0xFF00) << 40) | (value << 56));
        }

        /// <summary>
        /// Obtains the machine endian type
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public static EndianType MachineType
        {
            get
            {
                byte[] bytes = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 };
                const ulong BigEndianValue = 0x123456789ABCDEF0;
                const ulong LittleEndianValue = 0xF0DEBC9A78563412;
                ulong value;
                unsafe { fixed (byte* ptr = &bytes[0]) { value = *((ulong*)ptr); } }
                if (value == LittleEndianValue) return EndianType.LittleEndian;
                if (value == BigEndianValue) return EndianType.BigEndian;
                return EndianType.None;
            }
        }
    }
}
