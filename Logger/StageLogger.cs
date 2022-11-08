//-----------------------------------------------------------------------
// <copyright file="StageLogger.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class StageLogger.
    /// </summary>
    public class StageLogger
    {
        /// <summary>
        /// Gets or sets the log callback.
        /// </summary>
        public Func<StageItem, int> LogCallback { get; set; } = null;

        /// <summary>
        /// Gets or sets the stage change callback.
        /// </summary>
        public Func<StageSectionUpdate, int> StageChangeCallback { get; set; } = null;

        /// <summary>
        /// Gets or sets the collector.
        /// </summary>
        public StageLogCollector Collector { get; set; } = null;

        /// <summary>
        /// Gets or sets the level minimum.
        /// </summary>
        public StageLevel LevelMinimum { get; set; } = StageLevel.Info;

        /// <summary>
        /// Gets or sets the type of the current.
        /// </summary>
        public StageType CurrentType { get; set; } = StageType.None;

        /// <summary>
        /// Gets or sets the current position.
        /// </summary>
        public StagePosition CurrentPosition { get; set; } = StagePosition.None;

        /// <summary>
        /// Gets or sets the current identifier.
        /// </summary>
        public string CurrentId { get; set; } = null;

        /// <summary>
        /// Gets or sets the changes.
        /// </summary>
        public Stack<StageSection> Changes { get; set; } = new Stack<StageSection>();

        /// <summary>
        /// Changes the item updated.
        /// </summary>
        /// <param name="changeItem">The change item.</param>
        /// <param name="areaChange">The area change.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void ChangeItemUpdated(StageSection changeItem, string areaChange, string name, string value)
        {
            var update = new StageSectionUpdate
            {
                StageSection = changeItem,
                UpdateArea = areaChange,
                UpdateName = name,
                UpdateValue = value
            };

            this.StageChangeCallback(update);
        }

        /// <summary>
        /// Continues the stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="position">The position.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>System.Int32.</returns>
        public StageSection ContinueStage(string id, StageType type, StagePosition position, string tags = null)
        {
            var changeItem = new StageSection
            {
                Id = id,
                Position = position,
                Type = type,
                PreviousId = this.CurrentId,
                PreviousPosition = this.CurrentPosition,
                PreviousType = this.CurrentType,
                Tags = tags,
                DateTime = DateTime.UtcNow
            };

            this.Collector?.Record(changeItem);
            return this.ContinueStage(changeItem, id, type, position, tags);
        }

        /// <summary>
        /// Continues the stage.
        /// </summary>
        /// <param name="changeItem">The change item.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="position">The position.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageSection.</returns>
        public StageSection ContinueStage(
            StageSection changeItem,
            string id,
            StageType type,
            StagePosition position,
            string tags = null)
        {
            this.CurrentId = id;
            this.CurrentPosition = position;
            this.CurrentType = type;

            if (position == StagePosition.Begin)
            {
                this.Changes.Push(changeItem);
            }
            else if (position == StagePosition.End)
            {
                var item = this.Changes.Pop();
                while ((item.Id != id || item.Type != type) && this.Changes.Count > 0)
                {
                    item = this.Changes.Pop();
                }

                if (item.Id == id && item.Type == type)
                {
                    var now = DateTime.UtcNow;
                    item.EndDateTime = now;
                    item.Duration = now.Subtract(item.DateTime).TotalMilliseconds;
                    if (this.Changes.Count > 0)
                    {
                        var peek = this.Changes.Peek();
                        this.CurrentId = peek.Id;
                        this.CurrentPosition = peek.Position;
                        this.CurrentType = peek.Type;
                    }
                }
            }

            if (this.StageChangeCallback == null)
            {
                changeItem.Result = (int) StageResults.NoCallback;
            }
            else
            {
                var pos = "Inside";
                switch (position)
                {
                    case StagePosition.Begin:
                        pos = "Begin";
                        break;
                    case StagePosition.End:
                        pos = "End";
                        break;
                }

                var update = new StageSectionUpdate
                {
                    StageSection = changeItem,
                    UpdateArea = "Section",
                    UpdateName = id,
                    UpdateValue = pos
                };

                changeItem.Result = this.StageChangeCallback.Invoke(update);
            }

            return changeItem;
        }

        /// <summary>
        /// Begins the logging.
        /// </summary>
        /// <param name="tags">The tags.</param>
        public void BeginLogging(string tags = null)
        {
            this.ContinueStage(
                "All",
                StageType.All,
                StagePosition.Begin,
                tags);
        }

        /// <summary>
        /// Ends the logging.
        /// </summary>
        /// <param name="tags">The tags.</param>
        public void EndLogging(string tags = null)
        {
            this.ContinueStage(
                "All",
                StageType.All,
                StagePosition.End,
                tags);
        }

        /// <summary>
        /// Begins the stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection BeginStage(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Stage,
            StagePosition.Begin,
            tags);

        /// <summary>
        /// Ends the stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection EndStage(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Stage,
            StagePosition.End,
            tags);

        /// <summary>
        /// Begins the step.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection BeginStep(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Step,
            StagePosition.Begin,
            tags);

        /// <summary>
        /// Ends the step.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection EndStep(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Step,
            StagePosition.End,
            tags);

        /// <summary>
        /// Begins the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection BeginItem(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Item,
            StagePosition.Begin,
            tags);

        /// <summary>
        /// Ends the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection EndItem(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Item,
            StagePosition.End,
            tags);

        /// <summary>
        /// Begins the end stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection BeginEndStage(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Stage,
            StagePosition.BeginEnd,
            tags);

        /// <summary>
        /// Begins the end step.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection BeginEndStep(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Step,
            StagePosition.BeginEnd,
            tags);

        /// <summary>
        /// Begins the end item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection BeginEndItem(string id, string tags = null) => this.ContinueStage(
            id,
            StageType.Item,
            StagePosition.BeginEnd,
            tags);

        /// <summary>
        /// Stages the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageLogIteration.</returns>
        public StageLogIteration Stage(string id, string tags = null) => new StageLogIteration(
            this,
            id,
            StageType.Stage,
            tags);

        /// <summary>
        /// Items the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageLogIteration.</returns>
        public StageLogIteration Item(string id, string tags = null) => new StageLogIteration(
            this,
            id,
            StageType.Item,
            tags);

        /// <summary>
        /// Steps the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageLogIteration.</returns>
        public StageLogIteration Step(string id, string tags = null) => new StageLogIteration(
            this,
            id,
            StageType.Step,
            tags);

        /// <summary>
        /// Adds the collector.
        /// </summary>
        /// <returns>StageLogger.</returns>
        public StageLogger AddCollector()
        {
            this.Collector = new StageLogCollector();
            return this;
        }

        /// <summary>
        /// Logs the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.Int32.</returns>
        public int Log(StageItem item)
        {
            item.Id = this.CurrentId;
            item.Type = this.CurrentType;
            item.Position = this.CurrentPosition;

            this.Collector?.Record(item);

            if (this.LogCallback == null)
            {
                item.Result = (int) StageResults.NoCallback;
                return item.Result;
            }

            if (item.Level < this.LevelMinimum)
            {
                item.Result = (int) StageResults.MinimumLevelExceeded;
                return item.Result;
            }

            item.Result = this.LogCallback.Invoke(item);
            return item.Result;
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="level">The level.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>System.Int32.</returns>
        public int Log(string message, StageLevel level = StageLevel.Info, string tags = null)
        {
            return this.Log(
                new StageItem
                {
                    Message = message,
                    Level = level,
                    Tags = tags
                });
        }

        /// <summary>
        /// Logs the specified progress.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <param name="level">The level.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>System.Int32.</returns>
        public int Log(double progress, StageLevel level = StageLevel.Info, string tags = null)
        {
            return this.Log(
                new StageItem
                {
                    Progress = progress,
                    Level = level,
                    Tags = tags
                });
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="level">The level.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>System.Int32.</returns>
        public int Log(string message, double progress, StageLevel level = StageLevel.Info, string tags = null)
        {
            return this.Log(
                new StageItem
                {
                    Message = message,
                    Progress = progress,
                    Level = level,
                    Tags = tags
                });
        }

        /// <summary>
        /// Logs the metric.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="level">The level.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>System.Int32.</returns>
        public int LogMetric(string name, double value, StageLevel level = StageLevel.Info, string tags = null)
        {
            var metrics = new Dictionary<string, double>();
            metrics.Add(name, value);
            return this.Log(
                new StageItem
                {
                    Metrics = metrics,
                    Level = level,
                    Tags = tags
                });
        }

        /// <summary>
        /// Logs the event.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="level">The level.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>System.Int32.</returns>
        public int LogEvent(string name, string value, StageLevel level = StageLevel.Info, string tags = null)
        {
            var events = new Dictionary<string, string>();
            events.Add(name, value);
            return this.Log(
                new StageItem
                {
                    Events = events,
                    Level = level,
                    Tags = tags
                });
        }

        /// <summary>
        /// Begins the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection Begin(string id, StageType type, string tags = null)
        {
            switch (type)
            {
                case StageType.Stage:
                    return this.BeginStage(id, tags);
                case StageType.Step:
                    return this.BeginStep(id, tags);
                case StageType.Item:
                    return this.BeginItem(id, tags);
            }

            return null;
        }

        /// <summary>
        /// Ends the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>StageChangeItem.</returns>
        public StageSection End(string id, StageType type, string tags = null)
        {
            switch (type)
            {
                case StageType.Stage:
                    return this.EndStage(id, tags);
                case StageType.Step:
                    return this.EndStep(id, tags);
                case StageType.Item:
                    return this.EndItem(id, tags);
            }

            return null;
        }
    }
}
