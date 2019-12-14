/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2019 Ingo Herbote
 * https://www.yetanotherforum.net/
 * 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace YAF.Types.Extensions
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    using YAF.Types;

    #endregion

    /// <summary>
    /// The bit bool extensions.
    /// </summary>
    public static class BitBoolExtensions
    {
        #region Public Methods

        /// <summary>
        /// Gets boolean indicating whether bit on bitShift position in bitValue integer is set or not.
        /// </summary>
        /// <param name="bitValue">
        /// Integer value. 
        /// </param>
        /// <param name="bitShift">
        /// Zero-based position of bit to get. 
        /// </param>
        /// <returns>
        /// Returns boolean indicating whether bit at bitShift position is set or not. 
        /// </returns>
        public static bool GetBitAsBool(int bitValue, int bitShift)
        {
            if (bitShift > 63)
            {
                bitShift %= 63;
            }

            return ((bitValue >> bitShift) & 0x00000001) == 1;
        }

        /// <summary>
        /// Sets or un-sets bit of bitValue integer at position specified by bitShift, depending on value parameter.
        /// </summary>
        /// <param name="bitValue">
        /// Integer value. 
        /// </param>
        /// <param name="bitShift">
        /// Zero-based position of bit to set. 
        /// </param>
        /// <param name="value">
        /// New boolean value of bit. 
        /// </param>
        /// <returns>
        /// Returns new integer value with bit at position specified by bitShift parameter set to value. 
        /// </returns>
        public static int SetBitFromBool(int bitValue, int bitShift, bool value)
        {
            if (bitShift > 63)
            {
                bitShift %= 63;
            }

            if (GetBitAsBool(bitValue, bitShift) == value)
            {
                return bitValue;
            }

            // toggle that value using XOR
            var tV = 0x00000001 << bitShift;
            bitValue ^= tV;

            return bitValue;
        }

        /// <summary>
        /// The to hex string.
        /// </summary>
        /// <param name="hashedBytes">
        /// The hashed bytes. 
        /// </param>
        /// <returns>
        /// The to hex string. 
        /// </returns>
        [NotNull]
        public static string ToHexString([NotNull] this byte[] hashedBytes)
        {
            CodeContracts.VerifyNotNull(hashedBytes, "hashedBytes");

            var builder = new StringBuilder(hashedBytes.Length * 2 + 2);

            hashedBytes.ForEach(b => { builder.AppendFormat("{0:X2}", b); });

            return builder.ToString();
        }

        /// <summary>
        /// Creates an integer value from an array of booleans.
        /// </summary>
        /// <param name="arrayBool">
        /// array of boolean 
        /// </param>
        /// <returns>
        /// bit field of the array 
        /// </returns>
        public static int ToIntOfBits(this IEnumerable<bool> arrayBool)
        {
            var finalValue = 0;
            arrayBool.ForEachIndex((b, i) => finalValue = SetBitFromBool(finalValue, i, b));

            return finalValue;
        }

        public static List<T> ToListOf<T>(this byte[] array, Func<byte[], int, T> bitConverter)
        {
            var size = Marshal.SizeOf(typeof(T));
            return Enumerable.Range(0, array.Length / size)
                .Select(i => bitConverter(array, i * size))
                .ToList();
        }

        #endregion
    }
}