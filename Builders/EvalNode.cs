//-----------------------------------------------------------------------
// <copyright file="EvalNode.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GaleForceCore.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Class EvalNode.
    /// </summary>
    public class EvalNode
    {
        /// <summary>
        /// Gets or sets the exp.
        /// </summary>
        public Expression Exp { get; set; }

        /// <summary>
        /// Gets or sets the type of expression.
        /// </summary>
        public Type TypeOfExpression { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<EvalNode> Children { get; set; } = new List<EvalNode>();

        /// <summary>
        /// Determines whether [has only constants].
        /// </summary>
        /// <returns><c>true</c> if [has only constants]; otherwise, <c>false</c>.</returns>
        public bool HasOnlyConstants()
        {
            EvalNode node = this;
            return node.Children.Any()
                ? node.Children.All(n => n.HasOnlyConstants())
                : node.TypeOfExpression.Equals(typeof(ConstantExpression));
        }
    }
}