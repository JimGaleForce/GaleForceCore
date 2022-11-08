//-----------------------------------------------------------------------
// <copyright file="StageLevel.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    // public enum StageType
    // {
    // None,
    // BeginAllStages,
    // EndAllStages,
    // BeginStage,
    // ProgressStage,
    // EndStage,
    // BeginEndStage,
    // BeginStep,
    // ProgressStep,
    // EndStep,
    // BeginEndStep
    // }

    /// <summary>
    /// Enum StageLevel
    /// </summary>
    public enum StageLevel : int
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The debug
        /// </summary>
        Debug = 1,
        /// <summary>
        /// The trace
        /// </summary>
        Trace = 2,
        /// <summary>
        /// The information
        /// </summary>
        Info = 3,
        /// <summary>
        /// The warning
        /// </summary>
        Warning = 4,
        /// <summary>
        /// The error
        /// </summary>
        Error = 5,
        /// <summary>
        /// The fatal
        /// </summary>
        Fatal = 6
    }
}
