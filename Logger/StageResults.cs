//-----------------------------------------------------------------------
// <copyright file="StageResults.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    /// <summary>
    /// Enum StageResults
    /// </summary>
    public enum StageResults : int
    {
        /// <summary>
        /// The no callback
        /// </summary>
        NoCallback = -2,
        /// <summary>
        /// The minimum level exceeded
        /// </summary>
        MinimumLevelExceeded = 1,
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The ack
        /// </summary>
        Ack = 1
    }
}
