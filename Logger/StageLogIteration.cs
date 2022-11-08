//-----------------------------------------------------------------------
// <copyright file="StageLogIteration.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class StageLogIteration. Implements the <see cref="IDisposable"/>
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class StageLogIteration : IDisposable
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public StageLogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public StageType Type { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets the change item.
        /// </summary>
        public StageSection ChangeItem { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageLogIteration"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="tags">The tags.</param>
        public StageLogIteration(StageLogger logger, string id, StageType type, string tags = null)
        {
            this.Logger = logger;
            this.Id = id;
            this.Type = type;
            this.Tags = tags;
            this.ChangeItem = logger.Begin(id, type, tags);
        }

        /// <summary>
        /// Adds the event.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>StageLogIteration.</returns>
        public StageLogIteration AddEvent(string name, string value)
        {
            if (this.ChangeItem.Events == null)
            {
                this.ChangeItem.Events = new Dictionary<string, string>();
            }

            this.ChangeItem.Events.Add(name, value);
            this.Logger.ChangeItemUpdated(this.ChangeItem, "Event", name, value);
            return this;
        }

        /// <summary>
        /// Adds the metric.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>StageLogIteration.</returns>
        public StageLogIteration AddMetric(string name, double value)
        {
            if (this.ChangeItem.Metrics == null)
            {
                this.ChangeItem.Metrics = new Dictionary<string, double>();
            }

            this.ChangeItem.Metrics.Add(name, value);
            this.Logger.ChangeItemUpdated(this.ChangeItem, "Metric", name, value.ToString());

            return this;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Logger
                .ContinueStage(this.ChangeItem, this.ChangeItem.Id, this.ChangeItem.Type, StagePosition.End, null);
        }
    }
}
