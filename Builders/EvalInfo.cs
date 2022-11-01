//-----------------------------------------------------------------------
// <copyright file="EvalInfo.cs" company="Gale-Force">
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
    /// Class EvalInfo.
    /// </summary>
    public class EvalInfo
    {
        /// <summary>
        /// Gets or sets the nodes.
        /// </summary>
        public Dictionary<Expression, EvalNode> Nodes { get; set; } = new Dictionary<Expression, EvalNode>();

        /// <summary>
        /// Registers the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="typeOfExpression">The type of expression.</param>
        /// <param name="value">The value.</param>
        /// <returns>EvalNode.</returns>
        public EvalNode Register(Expression exp, Type typeOfExpression, string value)
        {
            if (this.Nodes.ContainsKey(exp))
            {
                this.Nodes[exp].Value = value;
                return this.Nodes[exp];
            }
            else
            {
                var node = new EvalNode { Exp = exp, TypeOfExpression = typeOfExpression, Value = value };
                this.Nodes.Add(exp, node);
                return node;
            }
        }

        /// <summary>
        /// Registers the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="typeOfExpression">The type of expression.</param>
        /// <returns>EvalNode.</returns>
        public EvalNode Register(Expression exp, Type typeOfExpression)
        {
            if (exp == null)
            {
                return null;
            }

            var node = new EvalNode { Exp = exp, TypeOfExpression = typeOfExpression };
            this.Nodes.Add(exp, node);
            return node;
        }

        /// <summary>
        /// Registers the children.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="children">The children.</param>
        public void RegisterChildren(EvalNode parent, params Expression[] children)
        {
            if (parent != null)
            {
                parent.Children.AddRange(children.Where(c => c != null).Select(c => this.Nodes[c]));
            }
        }

        /// <summary>
        /// Determines whether [has only constants] [the specified exp].
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns><c>true</c> if [has only constants] [the specified exp]; otherwise, <c>false</c>.</returns>
        public bool HasOnlyConstants(Expression exp)
        {
            return this.Nodes[exp].HasOnlyConstants();
        }
    }
}