//-----------------------------------------------------------------------
// <copyright file="UnsupportedOperandException.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GaleForceCore.Builders
{
    using System;

    /// <summary>
    /// Class UnsupportedOperandException. Implements the <see cref="Exception"/>
    /// </summary>
    /// <seealso cref="Exception"/>
    public class UnsupportedOperandException : Exception
    {
        public UnsupportedOperandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}