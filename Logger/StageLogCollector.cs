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
    using System.Text;

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

        public string ToLogString()
        {
            var stack = new Stack<StageLogCollectorItem>();
            var owned = new Dictionary<StageLogCollectorItem, List<StageLogCollectorItem>>();

            var root = new StageLogCollectorItem { ChangeItem =
                                                       new StageSection { 
                DateTime = DateTime.MinValue, EndDateTime = DateTime.MaxValue } };

            stack.Push(root);

            var latest = root;
            foreach (var item in this.Items)
            {
                var dt = item.DateTime;
                var ssDT = latest.ChangeItem.DateTime;
                var seDT = latest.ChangeItem.EndDateTime;

                while (dt > seDT)
                {
                    latest = stack.Pop();
                    ssDT = latest.ChangeItem.DateTime;
                    seDT = latest.ChangeItem.EndDateTime;
                }

                stack.Push(latest);

                if (!owned.ContainsKey(latest))
                {
                    owned[latest] = new List<StageLogCollectorItem>();
                }

                owned[latest].Add(item);

                if (item.ChangeItem != null)
                {
                    stack.Push(item);
                    latest = item;
                }
            }

            var sb = new StringBuilder();
            var indentLevel = 0;
            this.ToLogStringItem(root, owned, sb, indentLevel);
            return sb.ToString();
        }

        private void ToLogStringItem(
            StageLogCollectorItem item,
            Dictionary<StageLogCollectorItem, List<StageLogCollectorItem>> owned,
            StringBuilder sb,
            int indentLevel)
        {
            sb.AppendLine(this.Indent(indentLevel) + item.ChangeItem.Id + ":" + item.ChangeItem.DateTime.ToString());
            indentLevel++;

            if (item.ChangeItem.Events != null)
            {
                foreach (var evt in item.ChangeItem.Events)
                {
                    sb.AppendLine(this.Indent(indentLevel) + "event:" + evt.Key + ":" + evt.Value);
                }
            }

            if (item.ChangeItem.Metrics != null)
            {
                foreach (var evt in item.ChangeItem.Metrics)
                {
                    sb.AppendLine(this.Indent(indentLevel) + "metrc:" + evt.Key + ":" + evt.Value);
                }
            }

            if (owned.ContainsKey(item))
            {
                foreach (var evt in owned[item])
                {
                    if (evt.Item != null)
                    {
                        sb.AppendLine(this.Indent(indentLevel) + "item :" + evt.Item.Message);
                    }
                }

                foreach (var evt in owned[item])
                {
                    if (evt.ChangeItem != null)
                    {
                        this.ToLogStringItem(evt, owned, sb, indentLevel + 1);
                    }
                }
            }
        }

        public string Indent(int indentLevel)
        {
            return "".PadRight(indentLevel * 2, ' ');
        }
    }
}
