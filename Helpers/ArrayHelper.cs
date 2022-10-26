//-----------------------------------------------------------------------
// <copyright file="SimpleSqlBuilder.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class ArrayHelper.
    /// </summary>
    public static class ArrayHelper
    {
        /// <summary>
        /// Splits an enumerable to groups of the specified size.
        /// </summary>
        /// <typeparam name="T">Type of array to split.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="size">The size of chunks to split into.</param>
        /// <returns>IEnumerable&lt;IEnumerable&lt;T&gt;&gt;.</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
    }
}
