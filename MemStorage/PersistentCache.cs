//-----------------------------------------------------------------------
// <copyright file="PersistentCache.cs" company="Gale-Force, LLC">
// Copyright (C) Gale-Force, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GaleForceCore.MemStorage
{
    using System;
    using System.Collections.Generic;
    using System.Timers;

    /// <summary>
    /// Class PersistentCache. Implements the <see cref="Dictionary{T, T2}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2">The type of the t2.</typeparam>
    /// <seealso cref="Dictionary{T, T2}"/>
    public class PersistentCache<T, T2> : Dictionary<T, T2>
    {
        /// <summary>
        /// The idle
        /// </summary>
        private static readonly Timer idle = new Timer() { AutoReset = true, Enabled = false, Interval = 5000 };

        /// <summary>
        /// The timeout
        /// </summary>
        private static readonly Timer timeout = new Timer() { AutoReset = true, Enabled = false, Interval = 30000 };

        /// <summary>
        /// Occurs when [save].
        /// </summary>
        public event Action Save;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentCache{T, T2}"/> class.
        /// </summary>
        public PersistentCache()
        {
            idle.Elapsed += this.Idle_Elapsed;
            timeout.Elapsed += this.Timeout_Elapsed;
        }

        /// <summary>
        /// Handles the Elapsed event of the Timeout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void Timeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.DoSave();
        }

        /// <summary>
        /// Handles the Elapsed event of the Idle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void Idle_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.DoSave();
        }

        /// <summary>
        /// Does the save.
        /// </summary>
        private void DoSave()
        {
            idle.Enabled = false;
            timeout.Enabled = false;

            Save?.Invoke();
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>T2.</returns>
        public T2 Get(T key) => this.ContainsKey(key) ? this[key] : default;

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(T key, T2 value)
        {
            this[key] = value;
            idle.Start();
            idle.Enabled = true;
            if (!timeout.Enabled)
            {
                timeout.Start();
                timeout.Enabled = true;
            }
        }
    }
}