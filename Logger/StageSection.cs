//-----------------------------------------------------------------------
// <copyright file="StageSection.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    using System;

    /// <summary>
    /// Class StageChangeItem.
    /// </summary>
    public class StageSection : StageItem
    {
        /// <summary>
        /// Gets or sets the type of the previous.
        /// </summary>
        public StageType PreviousType { get; set; }

        /// <summary>
        /// Gets or sets the previous position.
        /// </summary>
        public StagePosition PreviousPosition { get; set; }

        /// <summary>
        /// Gets or sets the previous identifier.
        /// </summary>
        public string PreviousId { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        public DateTime EndDateTime { get; set; }
    }
}
