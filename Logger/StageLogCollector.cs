//-----------------------------------------------------------------------
// <copyright file="StageLogCollector.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class StageLogCollector.
    /// </summary>
    public class StageLogCollector
    {
        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        private int Order { get; set; } = 0;

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<StageLogCollectorItem> Items { get; protected set; } = new List<StageLogCollectorItem>();

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            lock (this.Items)
            {
                this.Order = 0;
                this.Items.Clear();
            }
        }

        /// <summary>
        /// Records the specified change item.
        /// </summary>
        /// <param name="changeItem">The change item.</param>
        public void Record(StageSection changeItem)
        {
            lock (this.Items)
            {
                this.Items
                    .Add(
                        new StageLogCollectorItem
                        {
                            ChangeItem = changeItem,
                            DateTime = DateTime.UtcNow,
                            Order = ++this.Order
                        });
            }
        }

        /// <summary>
        /// Records the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Record(StageItem item)
        {
            lock (this.Items)
            {
                this.Items
                    .Add(
                        new StageLogCollectorItem
                        {
                            Item = item,
                            DateTime = DateTime.UtcNow,
                            Order = ++this.Order
                        });
            }
        }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>System.Int32.</returns>
        public double GetDuration(string id)
        {
            return this.Items.Where(i => i.ChangeItem?.Id == id).Sum(i => i.ChangeItem.Duration);
        }
    }
}
