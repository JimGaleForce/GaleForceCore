//-----------------------------------------------------------------------
// <copyright file="StageLogCollectorItem.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    using System;

    /// <summary>
    /// Class StageLogCollectorItem.
    /// </summary>
    public class StageLogCollectorItem
    {
        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the change item.
        /// </summary>
        public StageSection ChangeItem { get; set; }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public StageItem Item { get; set; }
    }
}
