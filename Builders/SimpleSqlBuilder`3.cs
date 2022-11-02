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
    using System.Reflection;
    using System.Text;
    using GaleForceCore.Helpers;

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
        public Expression<Func<TRecord1, TRecord2, bool>> JoinKey { get; protected set; }

        /// <summary>
        /// Gets the where condition expression (lambda).
        /// </summary>
        /// <value>The where expression.</value>
        public Expression<Func<TRecord1, TRecord2, bool>> WhereExpression2 { get; protected set; } = null;

        /// <summary>
        /// Gets or sets the join phrase.
        /// </summary>
        /// <value>The join phrase.</value>
        public string JoinPhrase { get; set; }

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public new List<Expression<Func<TRecord1, TRecord2, object>>> FieldExpressions
        {
            get;
            protected set;
        } = new List<Expression<Func<TRecord1, TRecord2, object>>>();

        /// <summary>
        /// Gets or sets the order by list.
        /// </summary>
        public new List<SqlBuilderOrderItem<TRecord1, TRecord2>> OrderByList
        {
            get;
            protected set;
        } = new List<SqlBuilderOrderItem<TRecord1, TRecord2>>();

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="SimpleSqlBuilder{TRecord,&#xD;&#xA;TRecord1,&#xD;&#xA;TRecord2}"/> class.
        /// </summary>
        public SimpleSqlBuilder()
            : base(new Type[] { typeof(TRecord1), typeof(TRecord2) })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord, TRecord1,
        /// TRecord2}"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        public SimpleSqlBuilder(Type[] types)
            : base(types)
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
            this.FieldExpressions.AddRange(fields);
            return this.Select(fields.ToList());
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
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2> From(string tableName1, string tableName2)
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
            Expression<Func<TRecord1, TRecord2, bool>> joinKey)
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
            Expression<Func<TRecord1, TRecord2, bool>> joinKey)
        {
            this.JoinPhrase = "LEFT OUTER";
            this.JoinKey = joinKey;
            return this;
        }

        /// <summary>
        /// Lefts the outer join on.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> RightOuterJoinOn(
            Expression<Func<TRecord1, TRecord2, bool>> joinKey)
        {
            this.JoinPhrase = "RIGHT OUTER";
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
                var keys = this.ParseExpression(
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
            this.WhereExpression2 = condition;
            this.WhereString2 = this.ParseExpression(
                this.Types,
                condition.Body,
                true,
                parameters: condition.Parameters);
            return this;
        }

        /// <summary>
        /// Sets the where condition as an expression (can build, execute).
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Where(Expression<Func<TRecord, bool>> condition)
        {
            this.WhereExpression = condition;
            this.WhereString = this.ParseExpression(
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
            Expression<Func<TRecord1, TRecord2, object>> field)
        {
            var name = this.ParseExpression(this.Types, field.Body, parameters: field.Parameters);
            return this.OrderBy(name, true, field);
        }

        /// <summary>
        /// Executes the specified records.
        /// </summary>
        /// <param name="records1">The records1.</param>
        /// <param name="records2">The records2.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<TRecord> Execute(IEnumerable<TRecord1> records1, IEnumerable<TRecord2> records2)
        {
            switch (this.Command)
            {
                case "SELECT":
                    return this.ExecuteSelect(records1, records2);
                default:
                    throw new NotImplementedException($"{this.Command} is not supported as a query.");
            }
        }

        ///// <summary>
        ///// Executes the specified source.
        ///// </summary>
        ///// <param name="source">The source.</param>
        ///// <param name="target">The target.</param>
        ///// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        // public int ExecuteNonQuery(IEnumerable<TRecord> source, List<TRecord> target)
        // {
        // switch (this.Command)
        // {
        // case "UPDATE":
        // return this.ExecuteUpdate(source, target);
        // case "INSERT":
        // return this.ExecuteInsert(source, target);
        // case "MERGE":
        // return this.ExecuteMerge(source, target);
        // default:
        // throw new NotImplementedException($"{this.Command} is not supported as a 2 table non-query.");
        // }
        // }

        /// <summary>
        /// Executes the expressions within this builder upon these records, returning the results.
        /// </summary>
        /// <param name="records1">The records1.</param>
        /// <param name="records2">The records2.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        public IEnumerable<TRecord> ExecuteSelect(IEnumerable<TRecord1> records1, IEnumerable<TRecord2> records2)
        {
            var result = new List<TRecord>();

            // todo: reform to execute on string fields, not only expressions

            IEnumerable<Tuple<TRecord1, TRecord2>> records = new List<Tuple<TRecord1, TRecord2>>();
            var joinKey = this.JoinKey.Compile();
            switch (this.JoinPhrase)
            {
                case "INNER":
                    records = records1.SelectMany(
                        r1 => records2.Where(r2 => joinKey(r1, r2))
                            .Select(r2 => new Tuple<TRecord1, TRecord2>(r1, r2)))
                        .ToList();
                    break;
                case "LEFT OUTER":
                    records = records1.SelectMany(
                        r1 => (records2.Any(r2 => joinKey(r1, r2))
                            ? records2.Where(r2 => joinKey(r1, r2))
                            : new List<TRecord2>() { (TRecord2)Activator.CreateInstance(typeof(TRecord2)) })
                            .Select(r2 => new Tuple<TRecord1, TRecord2>(r1, r2)))
                        .ToList();
                    break;
                case "RIGHT OUTER":
                    records = records2.SelectMany(
                        r2 => (records1.Any(r1 => joinKey(r1, r2))
                            ? records1.Where(r1 => joinKey(r1, r2))
                            : new List<TRecord1>() { (TRecord1)Activator.CreateInstance(typeof(TRecord1)) })
                            .Select(r1 => new Tuple<TRecord1, TRecord2>(r1, r2)))
                        .ToList();
                    break;
            }

            var current = records;
            if (this.WhereExpression2 != null)
            {
                var we2 = this.WhereExpression2.Compile();
                current = current.Where(t => we2(t.Item1, t.Item2));
            }

            if (this.OrderByList.Count > 0)
            {
                for (var i = this.OrderByList.Count - 1; i > -1; i--)
                {
                    var orderBy = this.OrderByList[i];
                    var orderByX = orderBy.Expression.Compile();
                    if (orderBy.IsAscending)
                    {
                        current = current.OrderBy(t => orderByX(t.Item1, t.Item2));
                    }
                    else
                    {
                        current = current.OrderByDescending(t => orderByX(t.Item1, t.Item2));
                    }
                }
            }

            if (this.Count < int.MaxValue)
            {
                current = current.Take(this.Count);
            }

            var type = typeof(TRecord);
            var props = GetNonIgnoreProperties<TRecord>();

            var asFieldComps = this.AsFields.ToDictionary(af => af.Key, af => af.Value.Compile());
            var fieldExpressions = this.FieldExpressions.Select(fe => fe.Compile()).ToList();

            var propFields = new List<PropertyInfo>();
            for (var i = 0; i < this.Fields.Count(); i++)
            {
                var field = this.Fields[i];
                var fieldName = field.Contains(" AS ") ? this.GrabAs(field) : field;
                fieldName = fieldName.Contains(".") ? fieldName.Substring(fieldName.IndexOf(".") + 1) : fieldName;
                propFields.Add(
                    ExceptionHelpers.ThrowIfNull<PropertyInfo, MissingMemberException>(
                        props.FirstOrDefault(p => p.Name == fieldName),
                        $"{fieldName} property missing from {type.Name}"));
            }

            foreach (var record in current)
            {
                var newRecord = (TRecord)Activator.CreateInstance(type);
                for (var i = 0; i < this.Fields.Count(); i++)
                {
                    var value = fieldExpressions[i](record.Item1, record.Item2);
                    propFields[i].SetValue(newRecord, value);
                }

                result.Add(newRecord);
            }

            if (this.WhereExpression != null)
            {
                var we1 = this.WhereExpression.Compile();
                result = result.Where(t => we1(t)).ToList();
            }

            return result;
        }

        /// <summary>
        /// Grabs as.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>System.String.</returns>
        protected string GrabAs(string field)
        {
            return field.Contains(" AS ") ? field.Substring(field.IndexOf(" AS ") + 4) : field;
        }

        /// <summary>
        /// Sets up the order by clause.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <param name="expression">The expression.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        private SimpleSqlBuilder<TRecord, TRecord1, TRecord2> OrderBy(
            string fieldName,
            bool isAscending,
            Expression<Func<TRecord1, TRecord2, object>> expression = null)
        {
            this.OrderByList.Clear();
            return this.ThenBy(fieldName, isAscending, expression);
        }

        /// <summary>
        /// Sets up the then by clause.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> ThenBy(
            Expression<Func<TRecord1, TRecord2, object>> field)
        {
            var name = this.ParseExpression(this.Types, field.Body);
            return this.ThenBy(name, true, field);
        }

        /// <summary>
        /// Sets up the then by clause.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <param name="expression">The expression.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        private SimpleSqlBuilder<TRecord, TRecord1, TRecord2> ThenBy(
            string fieldName,
            bool isAscending,
            Expression<Func<TRecord1, TRecord2, object>> expression = null)
        {
            this.OrderByList
                .Add(
                    new SqlBuilderOrderItem<TRecord1, TRecord2>
                    {
                        Name = fieldName,
                        IsAscending = isAscending,
                        Expression = expression
                    });
            return this;
        }
    }
}