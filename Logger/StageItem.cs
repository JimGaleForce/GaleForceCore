//-----------------------------------------------------------------------
// <copyright file="StageItem.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class StageItem.
    /// </summary>
    public class StageItem
    {
        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        public StageLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        public Dictionary<string, double> Metrics { get; set; } = null;

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        public Dictionary<string, string> Events { get; set; } = null;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public StageType Type { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public StagePosition Position { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public int Result { get; set; }
    }
}
