//-----------------------------------------------------------------------
// <copyright file="UnjoinedTablesException.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    using System;

    /// <summary>
    /// Class UnjoinedTablesException. Implements the <see cref="Exception"/>
    /// </summary>
    /// <seealso cref="Exception"/>
    public class UnjoinedTablesException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnjoinedTablesException"/> class.
        /// </summary>
        public UnjoinedTablesException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnjoinedTablesException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UnjoinedTablesException(string message)
            : base(message)
        {
        }
    }
}
