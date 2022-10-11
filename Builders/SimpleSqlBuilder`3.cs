//-----------------------------------------------------------------------
// <copyright file="SimpleSqlBuilder`3.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    /// <summary>
    /// Class SimpleSqlBuilder.
    /// </summary>
    /// <typeparam name="TRecord">The type of the t record.</typeparam>
    /// <typeparam name="TRecord1">The type of the t record1.</typeparam>
    /// <typeparam name="TRecord2">The type of the t record2.</typeparam>
    public class SimpleSqlBuilder<TRecord, TRecord1, TRecord2> : SimpleSqlBuilder<TRecord>
    {
        /// <summary>
        /// Gets or sets the join key.
        /// </summary>
        /// <value>The join key.</value>
        public Expression<Func<TRecord1, TRecord2, object>> JoinKey { get; protected set; }

        /// <summary>
        /// Gets or sets the join phrase.
        /// </summary>
        /// <value>The join phrase.</value>
        public string JoinPhrase { get; set; }

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public new IEnumerable<Expression<Func<TRecord1, TRecord2, object>>> FieldExpressions
        {
            get;
            protected set;
        } = null;

        public new List<SqlBuilderOrderItem<TRecord, TRecord1, TRecord2>> OrderByList
        {
            get;
            protected set;
        } = new List<SqlBuilderOrderItem<TRecord, TRecord1, TRecord2>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord,
        /// TRecord1,&#xD;&#xA;TRecord2}"/> class.
        /// </summary>
        public SimpleSqlBuilder()
            : base(new Type[] { typeof(TRecord1), typeof(TRecord2) })
        {
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as an expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Select(Expression<Func<TRecord1, TRecord2, object>> field)
        {
            return this.Select(new Expression<Func<TRecord1, TRecord2, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Select(
            params Expression<Func<TRecord1, TRecord2, object>>[] fields)
        {
            this.FieldExpressions = fields;
            return this.Select(this.FieldExpressions);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Select(
            IEnumerable<Expression<Func<TRecord1, TRecord2, object>>> fields)
        {
            this.Command = "SELECT";
            var names = fields.Select(field => this.IncludeAs(field)).ToList();
            this.Select(names);
            return this;
        }

        /// <summary>
        /// Includes as.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>System.String.</returns>
        private string IncludeAs(Expression<Func<TRecord1, TRecord2, object>> field)
        {
            var expString = ParseExpression<TRecord>(this.Types, field.Body, parameters: field.Parameters);
            if (this.AsFields != null && this.AsFields.ContainsKey(field))
            {
                var asStr = ParseExpression<TRecord>(
                    this.Types,
                    this.AsFields[field].Body,
                    parameters: field.Parameters,
                    hideSourceTable: true);
                return expString + " AS " + asStr;
            }

            return expString;
        }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableName1">The table name1.</param>
        /// <param name="tableName2">The table name2.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> From(string tableName1, string tableName2)
        {
            this.TableNames = new string[] { tableName1, tableName2 };
            return this;
        }

        /// <summary>
        /// Selects as.
        /// </summary>
        /// <param name="asField">As field.</param>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> SelectAs(
            Expression<Func<TRecord, object>> asField,
            Expression<Func<TRecord1, TRecord2, object>> field)
        {
            this.AsFields[field] = asField;
            return this.Select(new Expression<Func<TRecord1, TRecord2, object>>[] { field });
        }

        /// <summary>
        /// Inners the join on.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> InnerJoinOn(
            Expression<Func<TRecord1, TRecord2, object>> joinKey)
        {
            this.JoinPhrase = "INNER";
            this.JoinKey = joinKey;
            return this;
        }

        /// <summary>
        /// Lefts the outer join on.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> LeftOuterJoinOn(
            Expression<Func<TRecord1, TRecord2, object>> joinKey)
        {
            this.JoinPhrase = "LEFT OUTER";
            this.JoinKey = joinKey;
            return this;
        }

        /// <summary>
        /// Injects the inner clauses.
        /// </summary>
        /// <param name="sb">The sb.</param>
        public override void InjectInnerClauses(StringBuilder sb)
        {
            if (this.JoinKey != null)
            {
                var keys = ParseExpression<TRecord>(
                    this.Types,
                    this.JoinKey.Body,
                    true,
                    parameters: this.JoinKey.Parameters);
                sb.Append($"{this.JoinPhrase} JOIN {this.TableNames[1]} ON {keys} ");
            }
        }

        /// <summary>
        /// Wheres the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Where(Expression<Func<TRecord1, TRecord2, bool>> condition)
        {
            this.WhereExpression2 = condition as Expression<Func<object, object, bool>>;
            this.WhereString = ParseExpression<TRecord>(
                this.Types,
                condition.Body,
                true,
                parameters: condition.Parameters);
            return this;
        }

        /// <summary>
        /// Sets the maximum count for returned records.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Take(int count)
        {
            this.Count = count;
            return this;
        }

        /// <summary>
        /// Adds an order-by expression ascending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> OrderBy(
            Expression<Func<TRecord, TRecord1, TRecord2, object>> field)
        {
            var name = this.ParseExpression<TRecord>(this.Types, field.Body, parameters: field.Parameters);
            return this.OrderBy(name, true, field);
        }

        private SimpleSqlBuilder<TRecord, TRecord1, TRecord2> OrderBy(
            string fieldName,
            bool isAscending,
            Expression<Func<TRecord, TRecord1, TRecord2, object>> expression = null)
        {
            this.OrderByList.Clear();
            return this.ThenBy(fieldName, isAscending, expression);
        }

        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> ThenBy(
            Expression<Func<TRecord, TRecord1, TRecord2, object>> field)
        {
            var name = this.ParseExpression<TRecord>(this.Types, field.Body);
            return this.ThenBy(name, true, field);
        }

        private SimpleSqlBuilder<TRecord, TRecord1, TRecord2> ThenBy(
            string fieldName,
            bool isAscending,
            Expression<Func<TRecord, TRecord1, TRecord2, object>> expression = null)
        {
            this.OrderByList
                .Add(
                    new SqlBuilderOrderItem<TRecord, TRecord1, TRecord2>
                    {
                        Name = fieldName,
                        IsAscending = isAscending,
                        Expression = expression
                    });
            return this;
        }
    }
}