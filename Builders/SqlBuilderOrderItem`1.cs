//-----------------------------------------------------------------------
// <copyright file="SqlBuilderOrderItem.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Class SqlBuilderOrderItem.
    /// </summary>
    /// <typeparam name="TRecord">The type of the t record.</typeparam>
    public class SqlBuilderOrderItem<TRecord>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ascending.
        /// </summary>
        /// <value><c>true</c> if this instance is ascending; otherwise, <c>false</c>.</value>
        public bool IsAscending { get; set; }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public Expression<Func<TRecord, object>> Expression { get; set; }
    }

    public class SqlBuilderOrderItem<TRecord1, TRecord2>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ascending.
        /// </summary>
        /// <value><c>true</c> if this instance is ascending; otherwise, <c>false</c>.</value>
        public bool IsAscending { get; set; }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public Expression<Func<TRecord1, TRecord2, object>> Expression { get; set; }
    }

    public class SqlBuilderOrderItem<TRecord1, TRecord2, TRecord3>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ascending.
        /// </summary>
        /// <value><c>true</c> if this instance is ascending; otherwise, <c>false</c>.</value>
        public bool IsAscending { get; set; }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public Expression<Func<TRecord1, TRecord2, TRecord3, object>> Expression { get; set; }
    }

    public class SqlBuilderOrderItem<TRecord1, TRecord2, TRecord3, TRecord4>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ascending.
        /// </summary>
        /// <value><c>true</c> if this instance is ascending; otherwise, <c>false</c>.</value>
        public bool IsAscending { get; set; }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public Expression<Func<TRecord1, TRecord2, TRecord3, TRecord4, object>> Expression { get; set; }
    }
}
