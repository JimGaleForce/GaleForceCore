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
        public List<Expression<Func<TRecord1, TRecord2, bool>>> WhereExpression2
        {
            get;
            protected set;
        } = new List<Expression<Func<TRecord1, TRecord2, bool>>>();

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
        /// Initializes a new instance of the <see
        /// cref="SimpleSqlBuilder{TRecord,&#xD;&#xA;TRecord1,&#xD;&#xA;TRecord2}"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        public SimpleSqlBuilder(Type[] types)
            : base(types)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="SimpleSqlBuilder{TRecord,&#xD;&#xA;TRecord1,&#xD;&#xA;TRecord2}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="types">The types.</param>
        public SimpleSqlBuilder(SimpleSqlBuilderOptions options, Type[] types)
            : base(options, types)
        {
        }

        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> TraceTo(StringBuilder sb)
        {
            base.TraceTo(sb);
            return this;
        }

        /// <summary>
        /// Sets the options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2> SetOptions(SimpleSqlBuilderOptions options)
        {
            base.SetOptions(options);
            return this;
        }

        /// <summary>
        /// Sets the syntax type to build with - currently only builds for SQLServer.
        /// </summary>
        /// <param name="syntaxType">Type of the syntax.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2> For(SimpleSqlBuilderType syntaxType)
        {
            base.For(syntaxType);
            return this;
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
            base.Select(names);
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
            this.From(new string[] { tableName1, tableName2 });
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

                var refTableName = this.TableNames[1] != this.TableNamesActual[1]
                    ? this.TableNames[1] + " "
                    : string.Empty;
                sb.Append($"{this.JoinPhrase} JOIN {this.TableNamesActual[1]} {refTableName}ON {keys} ");
            }
        }

        /// <summary>
        /// Wheres the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Where(Expression<Func<TRecord1, TRecord2, bool>> condition)
        {
            this.WhereCheck();
            this.WhereExpression2.Add(condition);
            this.WhereString2
                .Add(
                    this.ParseExpression(
                        this.Types,
                        condition.Body,
                        true,
                        parameters: condition.Parameters));
            return this;
        }

        /// <summary>
        /// Sets the where condition as an expression (can build, execute).
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        //public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Where(Expression<Func<TRecord, bool>> condition)
        // {
        // this.WhereCheck();
        // this.WhereExpression.Add(condition);
        // this.WhereString
        // .Add(
        // this.ParseExpression(
        // this.Types,
        // condition.Body,
        // true,
        // parameters: condition.Parameters));
        // return this;
        // }

        /// <summary>
/// Clears the accumulative where clauses.
/// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2> ClearWhere()
        {
            this.WhereExpression2.Clear();
            this.WhereString2.Clear();
            base.ClearWhere();
            return this;
        }

        /// <summary>
        /// Optionally add a clause when a condition is true.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="trueClause">The clause to eval if true.</param>
        /// <param name="falseClause">The clause to eval if false.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> If(
            bool condition,
            Action<SimpleSqlBuilder<TRecord, TRecord1, TRecord2>> trueClause = null,
            Action<SimpleSqlBuilder<TRecord, TRecord1, TRecord2>> falseClause = null)
        {
            if (condition && trueClause != null)
            {
                trueClause(this);
            }
            else if (!condition && falseClause != null)
            {
                falseClause(this);
            }

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
        /// <exception cref="GaleForceCore.Builders.UnjoinedTablesException"></exception>
        public IEnumerable<TRecord> ExecuteSelect(IEnumerable<TRecord1> records1, IEnumerable<TRecord2> records2)
        {
            var result = new List<TRecord>();

            if (this.IsTracing)
            {
                this.Trace.AppendLine("ExecuteSelect2: records1 count=" + records1.Count());
            }

            if (this.JoinKey == null)
            {
                throw new UnjoinedTablesException(
                    typeof(TRecord1).Name +
                        " and " +
                        typeof(TRecord2).Name +
                        " are unjoined. Add a join clause to complete.");
            }

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

            if (this.IsTracing)
            {
                this.Trace
                    .AppendLine(
                        $"ExecuteSelect2: after {this.JoinPhrase} 1-2 clause: " +
                            this.JoinKey.ToString() +
                            ": count=" +
                            records.Count());
            }

            var current = records;
            foreach (var whereExpression in this.WhereExpression2)
            {
                var we2 = whereExpression.Compile();
                current = current.Where(t => we2(t.Item1, t.Item2));

                if (this.IsTracing)
                {
                    this.Trace
                        .AppendLine(
                            "ExecuteSelect2: after Where2 clause: " +
                                whereExpression.ToString() +
                                ": count=" +
                                current.Count());
                }
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

            foreach (var whereExpression in this.WhereExpression)
            {
                var we1 = whereExpression.Compile();
                result = result.Where(we1).ToList();

                if (this.IsTracing)
                {
                    this.Trace
                        .AppendLine(
                            "ExecuteSelect2: after Where clause: " +
                                whereExpression.ToString() +
                                ": count=" +
                                result.Count());
                }
            }

            if (this.IsTracing)
            {
                this.Trace
                    .AppendLine(
                        "ExecuteSelect2: final count: " +
                            result.Count());
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

        /// <summary>
        /// Adds an order-by expression descending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> OrderByDescending(
            Expression<Func<TRecord1, TRecord2, object>> field)
        {
            var name = this.ParseExpression(this.Types, field.Body, parameters: field.Parameters);
            return this.OrderBy(name, false, field);
        }

        /// <summary>
        /// Adds an then-by expression descending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> ThenByDescending(
            Expression<Func<TRecord1, TRecord2, object>> field)
        {
            var name = this.ParseExpression(this.Types, field.Body, parameters: field.Parameters);
            return this.ThenBy(name, false, field);
        }

        /// <summary>
        /// Injects the order by clauses.
        /// </summary>
        /// <param name="sb">The sb.</param>
        public override void InjectOrderByClauses(StringBuilder sb)
        {
            if (this.OrderByList.Count > 0)
            {
                sb.Append("ORDER BY ");
                var comma = false;
                foreach (var item in this.OrderByList)
                {
                    if (comma)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(item.Name);
                    sb.Append(" ");
                    sb.Append(item.IsAscending ? "ASC" : "DESC");
                    comma = true;
                }
            }
        }
    }
}