//-----------------------------------------------------------------------
// <copyright file="SimpleSqlBuilder`4.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    using System.Linq.Expressions;
    /// <summary>
    /// Class JoinKeyItem.
    /// </summary>
    /// <typeparam name="TRecord1">The type of the t record1.</typeparam>
    /// <typeparam name="TRecord2">The type of the t record2.</typeparam>
    /// <typeparam name="TRecord3">The type of the t record3.</typeparam>
    public class JoinKeyItem<TRecord1, TRecord2, TRecord3>
    {
        /// <summary>
        /// Gets or sets the join key.
        /// </summary>
        public Expression JoinKey { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public JoinType Type { get; set; }

        /// <summary>
        /// Gets or sets the index of the join left.
        /// </summary>
        public int JoinLeftIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the join right.
        /// </summary>
        public int JoinRightIndex { get; set; }

        /// <summary>
        /// Gets the join phrase.
        /// </summary>
        public string JoinPhrase
        {
            get
            {
                switch (this.Type)
                {
                    case JoinType.INNER:
                        return "INNER";
                    case JoinType.LEFTOUTER:
                        return "LEFT OUTER";
                    case JoinType.RIGHTOUTER:
                        return "RIGHT OUTER";
                }

                return null;
            }
        }
    }
}