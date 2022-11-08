//-----------------------------------------------------------------------
// <copyright file="IncompatibleClauseException.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    using System;

    /// <summary>
    /// Class IncompatibleClauseException. Implements the <see cref="Exception"/>
    /// </summary>
    /// <seealso cref="Exception"/>
    public class IncompatibleClauseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleClauseException"/> class.
        /// </summary>
        public IncompatibleClauseException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleClauseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IncompatibleClauseException(string message)
            : base(message)
        {
        }
    }
}
