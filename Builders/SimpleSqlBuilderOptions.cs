//-----------------------------------------------------------------------
// <copyright file="SimpleSqlBuilderOptions.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GaleForceCore.Builders
{
    using System.Collections.Generic;

    /// <summary>
    /// Class SimpleSqlBuilderOptions.
    /// </summary>
    public class SimpleSqlBuilderOptions
    {
        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use parameters].
        /// </summary>
        public bool UseParameters { get; set; } = true;
    }
}