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

        /// <summary>
        /// Continues the stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="position">The position.</param>
        /// <returns>System.Int32.</returns>
        public int ContinueStage(string id, StageType type, StagePosition position)
        {
            var changeItem = new StageChangeItem
            {
                Id = id,
                Position = position,
                Type = type,
                PreviousId = this.CurrentId,
                PreviousPosition = this.CurrentPosition,
                PreviousType = this.CurrentType
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

                if (item.Id == id && item.Type == type && this.Changes.Count > 0)
                {
                    var peek = this.Changes.Peek();
                    this.CurrentId = peek.Id;
                    this.CurrentPosition = peek.Position;
                    this.CurrentType = peek.Type;
                }
            }

            if (this.LogCallback == null)
            {
                return (int) StageResults.NoCallback;
            }

            return this.StageChangeCallback.Invoke(changeItem);
        }

        /// <summary>
        /// Begins the logging.
        /// </summary>
        public void BeginLogging()
        {
            this.ContinueStage("All", StageType.All, StagePosition.Begin);
        }

        /// <summary>
        /// Ends the logging.
        /// </summary>
        public void EndLogging()
        {
            this.ContinueStage("All", StageType.All, StagePosition.End);
        }

        /// <summary>
        /// Begins the stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void BeginStage(string id) => this.ContinueStage(id, StageType.Stage, StagePosition.Begin);

        /// <summary>
        /// Ends the stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void EndStage(string id) => this.ContinueStage(id, StageType.Stage, StagePosition.End);

        /// <summary>
        /// Begins the step.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void BeginStep(string id) => this.ContinueStage(id, StageType.Step, StagePosition.Begin);

        /// <summary>
        /// Ends the step.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void EndStep(string id) => this.ContinueStage(id, StageType.Step, StagePosition.End);

        /// <summary>
        /// Begins the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void BeginItem(string id) => this.ContinueStage(id, StageType.Item, StagePosition.Begin);

        /// <summary>
        /// Ends the item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void EndItem(string id) => this.ContinueStage(id, StageType.Item, StagePosition.End);

        /// <summary>
        /// Begins the end stage.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void BeginEndStage(string id) => this.ContinueStage(id, StageType.Stage, StagePosition.BeginEnd);

        /// <summary>
        /// Begins the end step.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void BeginEndStep(string id) => this.ContinueStage(id, StageType.Step, StagePosition.BeginEnd);

        /// <summary>
        /// Begins the end item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void BeginEndItem(string id) => this.ContinueStage(id, StageType.Item, StagePosition.BeginEnd);

        /// <summary>
        /// Stages the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>StageLogIteration.</returns>
        public StageLogIteration Stage(string id) => new StageLogIteration(this, id, StageType.Stage);

        /// <summary>
        /// Items the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>StageLogIteration.</returns>
        public StageLogIteration Item(string id) => new StageLogIteration(this, id, StageType.Item);

        /// <summary>
        /// Steps the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>StageLogIteration.</returns>
        public StageLogIteration Step(string id) => new StageLogIteration(this, id, StageType.Step);

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
                return (int) StageResults.NoCallback;
            }

            if (item.Level < this.LevelMinimum)
            {
                return (int) StageResults.MinimumLevelExceeded;
            }

            return this.LogCallback.Invoke(item);
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
        public void Begin(string id, StageType type)
        {
            switch (type)
            {
                case StageType.Stage:
                    this.BeginStage(id);
                    break;
                case StageType.Step:
                    this.BeginStep(id);
                    break;
                case StageType.Item:
                    this.BeginItem(id);
                    break;
            }
        }

        /// <summary>
        /// Ends the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        public void End(string id, StageType type)
        {
            switch (type)
            {
                case StageType.Stage:
                    this.EndStage(id);
                    break;
                case StageType.Step:
                    this.EndStep(id);
                    break;
                case StageType.Item:
                    this.EndItem(id);
                    break;
            }
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
        /// Initializes a new instance of the <see cref="StageLogIteration"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        public StageLogIteration(StageLogger logger, string id, StageType type)
        {
            this.Logger = logger;
            this.Id = id;
            this.Type = type;
            logger.Begin(id, type);
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
    public class StageChangeItem
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public StageType Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the previous.
        /// </summary>
        public StageType PreviousType { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public StagePosition Position { get; set; }

        /// <summary>
        /// Gets or sets the previous position.
        /// </summary>
        public StagePosition PreviousPosition { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the previous identifier.
        /// </summary>
        public string PreviousId { get; set; }
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
