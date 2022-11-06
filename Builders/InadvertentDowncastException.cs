//-----------------------------------------------------------------------
// <copyright file="InadvertentDowncastException.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    using System;

    /// <summary>
    /// Class InadvertentDowncastException. Implements the <see cref="Exception"/>
    /// </summary>
    /// <seealso cref="Exception"/>
    public class InadvertentDowncastException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InadvertentDowncastException"/> class.
        /// </summary>
        public InadvertentDowncastException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InadvertentDowncastException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InadvertentDowncastException(string message)
            : base(message)
        {
        }
    }
}
