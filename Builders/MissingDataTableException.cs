//-----------------------------------------------------------------------
// <copyright file="MissingDataTableException.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    using System;

    /// <summary>
    /// Class MissingDataTableException. Implements the <see cref="Exception"/>
    /// </summary>
    /// <seealso cref="Exception"/>
    public class MissingDataTableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDataTableException"/> class.
        /// </summary>
        public MissingDataTableException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDataTableException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MissingDataTableException(string message)
            : base(message)
        {
        }
    }
}
