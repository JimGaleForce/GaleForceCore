//-----------------------------------------------------------------------
// <copyright file="DynamicMethodException.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    using System;

    /// <summary>
    /// Class DynamicMethodException. This indicates that a method was used that doesn't resolve to
    /// a constant. The method results cannot be resolved for the purpose of building a string, as
    /// it requires data from the records, or other unresolved information, to complete. Only
    /// methods that returns constants can be evaluated to produce a string for SQL. Implements the
    /// <see cref="InvalidOperationException"/>
    /// </summary>
    /// <seealso cref="InvalidOperationException"/>
    public class DynamicMethodException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicMethodException"/> class. This
        /// indicates that a method was used that doesn't resolve to a constant. The method results
        /// cannot be resolved for the purpose of building a string, as it requires data from the
        /// records, or other unresolved information, to complete. Only methods that returns
        /// constants can be evaluated to produce a string for SQL.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference (Nothing in Visual Basic), the current exception is
        /// raised in a catch block that handles the inner exception.
        /// </param>
        public DynamicMethodException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
