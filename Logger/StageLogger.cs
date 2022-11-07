//-----------------------------------------------------------------------
// <copyright file="StageLogger.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        public Func<StageChangeItem, int> StageChangeCallback { get; set; } = null;

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
        public Stack<StageChangeItem> Changes { get; set; } = new Stack<StageChangeItem>();

        public void ChangeItemUpdated(StageChangeItem changeItem, string areaChange, string name, string value)
        {
            changeItem.CurrentChangeArea = areaChange;
            changeItem.CurrentChangeName = name;
            changeItem.CurrentChangeValue = value;

            this.StageChangeCallback(changeItem);
        }

        /// <summary>
        /// Continues the stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="position">The position.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>System.Int32.</returns>
        public StageChangeItem ContinueStage(string id, StageType type, StagePosition position, string tags = null)
        {
            var changeItem = new StageChangeItem
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

            this.CurrentId = id;
            this.CurrentPosition = position;
            this.CurrentType = type;

            this.Collector?.Record(changeItem);

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
                    item.Duration = changeItem.DateTime.Subtract(item.DateTime).Milliseconds;
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
                changeItem.Result = this.StageChangeCallback.Invoke(changeItem);
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
        public StageChangeItem BeginStage(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem EndStage(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem BeginStep(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem EndStep(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem BeginItem(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem EndItem(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem BeginEndStage(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem BeginEndStep(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem BeginEndItem(string id, string tags = null) => this.ContinueStage(
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
        public StageChangeItem Begin(string id, StageType type, string tags = null)
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
        public StageChangeItem End(string id, StageType type, string tags = null)
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
        public StageChangeItem ChangeItem { get; set; }

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
            this.Logger.End(this.Id, this.Type);
        }
    }

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
        public void Record(StageChangeItem changeItem)
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
        public int GetDuration(string id)
        {
            return this.Items.Where(i => i.ChangeItem?.Id == id).Sum(i => i.ChangeItem.Duration);
        }
    }

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
        public StageChangeItem ChangeItem { get; set; }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public StageItem Item { get; set; }
    }

    /// <summary>
    /// Class StageChangeItem.
    /// </summary>
    public class StageChangeItem : StageItem
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
        public int Duration { get; set; }

        public string CurrentChangeArea { get; set; }

        public string CurrentChangeName { get; set; }

        public string CurrentChangeValue { get; set; }
    }

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

    /// <summary>
    /// Enum StageResults
    /// </summary>
    public enum StageResults : int
    {
        /// <summary>
        /// The no callback
        /// </summary>
        NoCallback = -2,
        /// <summary>
        /// The minimum level exceeded
        /// </summary>
        MinimumLevelExceeded = 1,
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The ack
        /// </summary>
        Ack = 1
    }

    /// <summary>
    /// Enum StageType
    /// </summary>
    public enum StageType
    {
        /// <summary>
        /// The none
        /// </summary>
        None,
        /// <summary>
        /// All
        /// </summary>
        All,
        /// <summary>
        /// The stage
        /// </summary>
        Stage,
        /// <summary>
        /// The step
        /// </summary>
        Step,
        /// <summary>
        /// The item
        /// </summary>
        Item
    }

    /// <summary>
    /// Enum StagePosition
    /// </summary>
    public enum StagePosition
    {
        /// <summary>
        /// The none
        /// </summary>
        None,
        /// <summary>
        /// The begin
        /// </summary>
        Begin,
        /// <summary>
        /// The inside
        /// </summary>
        Inside,
        /// <summary>
        /// The about
        /// </summary>
        About,
        /// <summary>
        /// The end
        /// </summary>
        End,
        /// <summary>
        /// The begin end
        /// </summary>
        BeginEnd
    }

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
