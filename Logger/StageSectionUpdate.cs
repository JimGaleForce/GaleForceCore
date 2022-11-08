//-----------------------------------------------------------------------
// <copyright file="StageSectionUpdate.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    /// <summary>
    /// Class StageSectionUpdate.
    /// </summary>
    public class StageSectionUpdate
    {
        /// <summary>
        /// Gets or sets the update area.
        /// </summary>
        public string UpdateArea { get; set; }

        /// <summary>
        /// Gets or sets the name of the update.
        /// </summary>
        public string UpdateName { get; set; }

        /// <summary>
        /// Gets or sets the update value.
        /// </summary>
        public string UpdateValue { get; set; }

        /// <summary>
        /// Gets or sets the stage section.
        /// </summary>
        public StageSection StageSection { get; set; }
    }
}
