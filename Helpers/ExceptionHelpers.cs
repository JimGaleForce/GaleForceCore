//-----------------------------------------------------------------------
// <copyright file="ExceptionHelpers.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Helpers
{
    using System;

    /// <summary>
    /// Class ExceptionHelpers.
    /// </summary>
    public static class ExceptionHelpers
    {
        /// <summary>
        /// Throws an exception if value is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="message">The message.</param>
        /// <returns>T.</returns>
        /// <exception cref="System.Exception"></exception>
        public static T ThrowIfNull<T, T2>(T value, string message)
            where T2 : Exception
        {
            if (value == null)
            {
                throw (T2) Activator.CreateInstance(typeof(T2), message);
            }

            return value;
        }
    }
}
