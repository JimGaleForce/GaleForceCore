//-----------------------------------------------------------------------
// <copyright file="SimpleSqlBuilder`4.cs" company="Gale-Force">
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
    /// <typeparam name="TRecord3">The type of the t record3.</typeparam>
    public class SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> : SimpleSqlBuilder<TRecord>
    {
        /// <summary>
        /// Gets or sets the join key.
        /// </summary>
        /// <value>The join key.</value>
        public List<Expression<Func<TRecord1, TRecord2, TRecord3, object>>> JoinKey
        {
            get;
            protected set;
        } = new List<Expression<Func<TRecord1, TRecord2, TRecord3, object>>>();

        /// <summary>
        /// Gets or sets the join phrase.
        /// </summary>
        /// <value>The join phrase.</value>
        public List<string> JoinPhrase { get; set; } = new List<string>();

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public new IEnumerable<Expression<Func<TRecord1, TRecord2, TRecord3, object>>> FieldExpressions
        {
            get;
            protected set;
        } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord,
        /// TRecord1,&#xD;&#xA;TRecord2, TRecord3}"/> class.
        /// </summary>
        public SimpleSqlBuilder()
            : base(new Type[] { typeof(TRecord1), typeof(TRecord2), typeof(TRecord3) })
        {
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as an expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> Select(
            Expression<Func<TRecord1, TRecord2, TRecord3, object>> field)
        {
            return this.Select(new Expression<Func<TRecord1, TRecord2, TRecord3, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> Select(
            params Expression<Func<TRecord1, TRecord2, TRecord3, object>>[] fields)
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
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> Select(
            IEnumerable<Expression<Func<TRecord1, TRecord2, TRecord3, object>>> fields)
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
        private string IncludeAs(Expression<Func<TRecord1, TRecord2, TRecord3, object>> field)
        {
            var expString = this.ParseExpression(this.Types, field.Body, parameters: field.Parameters);
            if (this.AsFields != null && this.AsFields.ContainsKey(field))
            {
                var asStr = this.ParseExpression(
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
        /// <param name="tableName3">The table name3.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> From(
            string tableName1,
            string tableName2,
            string tableName3)
        {
            this.TableNames = new string[] { tableName1, tableName2, tableName3 };
            return this;
        }

        /// <summary>
        /// Selects as.
        /// </summary>
        /// <param name="asField">As field.</param>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> SelectAs(
            Expression<Func<TRecord, object>> asField,
            Expression<Func<TRecord1, TRecord2, TRecord3, object>> field)
        {
            this.AsFields[field] = asField;
            return this.Select(new Expression<Func<TRecord1, TRecord2, TRecord3, object>>[] { field });
        }

        /// <summary>
        /// Inners the join on.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> InnerJoinOn(
            Expression<Func<TRecord1, TRecord2, TRecord3, object>> joinKey)
        {
            this.JoinPhrase.Add("INNER");
            this.JoinKey.Add(joinKey);
            return this;
        }

        /// <summary>
        /// Lefts the outer join on.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> LeftOuterJoinOn(
            Expression<Func<TRecord1, TRecord2, TRecord3, object>> joinKey)
        {
            this.JoinPhrase.Add("LEFT OUTER");
            this.JoinKey.Add(joinKey);
            return this;
        }

        /// <summary>
        /// Injects the inner clauses.
        /// </summary>
        /// <param name="sb">The sb.</param>
        public override void InjectInnerClauses(StringBuilder sb)
        {
            if (this.JoinKey.Any())
            {
                for (var i = 0; i < this.JoinKey.Count; i++)
                {
                    var joinKey = this.JoinKey[i];
                    var joinPhrase = this.JoinPhrase[i];

                    var keys = this.ParseExpression(this.Types, joinKey.Body, true, parameters: joinKey.Parameters);
                    sb.Append($"{joinPhrase} JOIN {this.TableNames[i + 1]} ON {keys} ");
                }
            }
        }

        /// <summary>
        /// Wheres the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> Where(
            Expression<Func<TRecord1, TRecord2, TRecord3, bool>> condition)
        {
            this.WhereExpression3 = condition as Expression<Func<object, object, object, bool>>;
            this.WhereString = this.ParseExpression(this.Types, condition.Body, true, parameters: condition.Parameters);
            return this;
        }

        /// <summary>
        /// Sets the maximum count for returned records.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> Take(int count)
        {
            this.Count = count;
            return this;
        }
    }
}