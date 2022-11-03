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
    using System.Reflection;
    using System.Text;
    using GaleForceCore.Helpers;

    /// <summary>
    /// Class SimpleSqlBuilder.
    /// </summary>
    /// <typeparam name="TRecord">The type of the t record.</typeparam>
    /// <typeparam name="TRecord1">The type of the t record1.</typeparam>
    /// <typeparam name="TRecord2">The type of the t record2.</typeparam>
    /// <typeparam name="TRecord3">The type of the t record3.</typeparam>
    public class SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> : SimpleSqlBuilder<TRecord, TRecord1, TRecord2>
    {
        /// <summary>
        /// Gets or sets the join key.
        /// </summary>
        /// <value>The join key.</value>
        public List<JoinKeyItem<TRecord1, TRecord2, TRecord3>> JoinKeys
        {
            get;
            protected set;
        } = new List<JoinKeyItem<TRecord1, TRecord2, TRecord3>>();

        /// <summary>
        /// Gets the where condition expression (lambda).
        /// </summary>
        /// <value>The where expression.</value>
        public List<Expression<Func<TRecord1, TRecord2, TRecord3, bool>>> WhereExpression3
        {
            get;
            protected set;
        } = new List<Expression<Func<TRecord1, TRecord2, TRecord3, bool>>>();

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public new List<Expression<Func<TRecord1, TRecord2, TRecord3, object>>> FieldExpressions
        {
            get;
            protected set;
        } = new List<Expression<Func<TRecord1, TRecord2, TRecord3, object>>>();

        /// <summary>
        /// Gets or sets the order by list.
        /// </summary>
        public new List<SqlBuilderOrderItem<TRecord1, TRecord2, TRecord3>> OrderByList
        {
            get;
            protected set;
        } = new List<SqlBuilderOrderItem<TRecord1, TRecord2, TRecord3>>();

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="SimpleSqlBuilder{TRecord,&#xD;&#xA;TRecord1,&#xD;&#xA;TRecord2, TRecord3}"/>
        /// class.
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
            this.FieldExpressions.AddRange(fields);
            return this.Select(fields.ToList());
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
        /// Includes AS (field) in build string.
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
        /// Selects a field member AS another field expression.
        /// </summary>
        /// <param name="asField">As field.</param>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> SelectAs(
            Expression<Func<TRecord, object>> asField,
            Expression<Func<TRecord1, TRecord2, TRecord3, object>> field)
        {
            this.AsFields[field] = asField;
            this.Select(new Expression<Func<TRecord1, TRecord2, TRecord3, object>>[] { field });
            return this;
        }

        /// <summary>
        /// Sets up an INNER JOIN.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> InnerJoin12On(
            Expression<Func<TRecord1, TRecord2, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.INNER);

        /// <summary>
        /// Sets up an INNER JOIN.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> InnerJoin13On(
            Expression<Func<TRecord1, TRecord3, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.INNER);

        /// <summary>
        /// Sets up an INNER JOIN.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> InnerJoin23On(
            Expression<Func<TRecord2, TRecord3, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.INNER);

        /// <summary>
        /// Handles all joins.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <param name="type">The type.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2, TRecord3&gt;.</returns>
        private SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> JoinOn(
            Expression<Func<TRecord1, TRecord2, bool>> joinKey,
            JoinType type)
        {
            var jkey = new JoinKeyItem<TRecord1, TRecord2, TRecord3>()
            {
                JoinKey = joinKey,
                JoinLeftIndex = 0,
                JoinRightIndex = 1,
                Type = type
            };

            this.JoinKeys.Add(jkey);
            return this;
        }

        /// <summary>
        /// Handles all joins.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <param name="type">The type.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2, TRecord3&gt;.</returns>
        private SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> JoinOn(
            Expression<Func<TRecord1, TRecord3, bool>> joinKey,
            JoinType type)
        {
            var jkey = new JoinKeyItem<TRecord1, TRecord2, TRecord3>()
            {
                JoinKey = joinKey,
                JoinLeftIndex = 0,
                JoinRightIndex = 2,
                Type = type
            };

            this.JoinKeys.Add(jkey);
            return this;
        }

        /// <summary>
        /// Handles all joins.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <param name="type">The type.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2, TRecord3&gt;.</returns>
        private SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> JoinOn(
            Expression<Func<TRecord2, TRecord3, bool>> joinKey,
            JoinType type)
        {
            var jkey = new JoinKeyItem<TRecord1, TRecord2, TRecord3>()
            {
                JoinKey = joinKey,
                JoinLeftIndex = 1,
                JoinRightIndex = 2,
                Type = type
            };

            this.JoinKeys.Add(jkey);
            return this;
        }

        // here!

        /// <summary>
        /// Sets up a left outer join.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> LeftOuterJoin12On(
            Expression<Func<TRecord1, TRecord2, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.LEFTOUTER);

        /// <summary>
        /// Sets up a left outer join.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> LeftOuterJoin13On(
            Expression<Func<TRecord1, TRecord3, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.LEFTOUTER);

        /// <summary>
        /// Sets up a left outer join.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> LeftOuterJoin23On(
            Expression<Func<TRecord2, TRecord3, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.LEFTOUTER);

        /// <summary>
        /// Sets up a right outer join.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> RightOuterJoin12On(
            Expression<Func<TRecord1, TRecord2, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.RIGHTOUTER);

        /// <summary>
        /// Sets up a right outer join.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> RightOuterJoin13On(
            Expression<Func<TRecord1, TRecord3, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.RIGHTOUTER);

        /// <summary>
        /// Sets up a right outer join.
        /// </summary>
        /// <param name="joinKey">The join key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> RightOuterJoin23On(
            Expression<Func<TRecord2, TRecord3, bool>> joinKey) =>
            this.JoinOn(joinKey, JoinType.RIGHTOUTER);

        /// <summary>
        /// Injects the inner clauses.
        /// </summary>
        /// <param name="sb">The sb.</param>
        public override void InjectInnerClauses(StringBuilder sb)
        {
            if (this.JoinKeys.Any())
            {
                for (var i = 0; i < this.JoinKeys.Count; i++)
                {
                    var joinKeySet = this.JoinKeys[i];
                    var joinPhrase = joinKeySet.JoinPhrase;

                    string keys = null;
                    if (joinKeySet.JoinLeftIndex == 0)
                    {
                        if (joinKeySet.JoinRightIndex == 1)
                        {
                            var joinKey = joinKeySet.JoinKey as Expression<Func<TRecord1, TRecord2, bool>>;
                            keys = this.ParseExpression(
                                new Type[] { this.Types[0], this.Types[1] },
                                joinKey.Body,
                                true,
                                parameters: joinKey.Parameters,
                                tableNames: new string[] { this.TableNames[0], this.TableNames[1] });
                        }
                        else if (joinKeySet.JoinRightIndex == 2)
                        {
                            var joinKey = joinKeySet.JoinKey as Expression<Func<TRecord1, TRecord3, bool>>;
                            keys = this.ParseExpression(
                                new Type[] { this.Types[0], this.Types[2] },
                                joinKey.Body,
                                true,
                                parameters: joinKey.Parameters,
                                tableNames: new string[] { this.TableNames[0], this.TableNames[2] });
                        }
                    }
                    else if (joinKeySet.JoinLeftIndex == 1)
                    {
                        if (joinKeySet.JoinRightIndex == 2)
                        {
                            var joinKey = joinKeySet.JoinKey as Expression<Func<TRecord2, TRecord3, bool>>;
                            keys = this.ParseExpression(
                                new Type[] { this.Types[1], this.Types[2] },
                                joinKey.Body,
                                true,
                                parameters: joinKey.Parameters,
                                tableNames: new string[] { this.TableNames[1], this.TableNames[2] });
                        }
                    }

                    sb.Append($"{joinPhrase} JOIN {this.TableNames[i + 1]} ON {keys} ");
                }
            }
        }

        /// <summary>
        /// Sets up the where condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> Where(
            Expression<Func<TRecord1, TRecord2, TRecord3, bool>> condition)
        {
            this.WhereExpression3.Add(condition as Expression<Func<TRecord1, TRecord2, TRecord3, bool>>);
            this.WhereString3
                .Add(this.ParseExpression(this.Types, condition.Body, true, parameters: condition.Parameters));
            return this;
        }

        /// <summary>
        /// Clears the accumulative where clauses.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public new SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> ClearWhere()
        {
            this.WhereExpression3.Clear();
            this.WhereString3.Clear();
            base.ClearWhere();
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

        /// <summary>
        /// Optionally add a clause when a condition is true.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="clause">The clause.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord, TRecord1, TRecord2, TRecord3&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3> If(
            bool condition,
            Action<SimpleSqlBuilder<TRecord, TRecord1, TRecord2, TRecord3>> clause)
        {
            if (condition)
            {
                clause(this);
            }

            return this;
        }

        /// <summary>
        /// Executes the specified records.
        /// </summary>
        /// <param name="records1">The records1.</param>
        /// <param name="records2">The records2.</param>
        /// <param name="records3">The records3.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<TRecord> Execute(
            IEnumerable<TRecord1> records1,
            IEnumerable<TRecord2> records2,
            IEnumerable<TRecord3> records3)
        {
            switch (this.Command)
            {
                case "SELECT":
                    return this.ExecuteSelect(records1, records2, records3);
                default:
                    throw new NotImplementedException($"{this.Command} is not supported as a query.");
            }
        }

        /// <summary>
        /// Executes the select.
        /// </summary>
        /// <param name="records1">The records1.</param>
        /// <param name="records2">The records2.</param>
        /// <param name="records3">The records3.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        public IEnumerable<TRecord> ExecuteSelect(
            IEnumerable<TRecord1> records1,
            IEnumerable<TRecord2> records2,
            IEnumerable<TRecord3> records3)
        {
            var result = new List<TRecord>();

            var records = this.ExecuteJoins(records1, records2, records3);

            var emptyRecs = new Tuple<TRecord1, TRecord2, TRecord3>(
                (TRecord1)Activator.CreateInstance(typeof(TRecord1)),
                (TRecord2)Activator.CreateInstance(typeof(TRecord2)),
                (TRecord3)Activator.CreateInstance(typeof(TRecord3)));

            var current = records;
            foreach (var whereExpression in this.WhereExpression3)
            {
                var we3 = whereExpression.Compile();
                current = current.Where(
                    t => we3(
                        (TRecord1)(t.Item1 ?? emptyRecs.Item1),
                        (TRecord2)(t.Item2 ?? emptyRecs.Item2),
                        (TRecord3)(t.Item3 ?? emptyRecs.Item3)))
                    .ToList();
            }

            if (this.OrderByList.Count > 0)
            {
                for (var i = this.OrderByList.Count - 1; i > -1; i--)
                {
                    var orderBy = this.OrderByList[i];
                    var orderByX = orderBy.Expression.Compile();
                    if (orderBy.IsAscending)
                    {
                        current = current.OrderBy(
                            t => orderByX((TRecord1)t.Item1, (TRecord2)t.Item2, (TRecord3)t.Item3))
                            .ToList();
                    }
                    else
                    {
                        current = current.OrderByDescending(
                            t => orderByX((TRecord1)t.Item1, (TRecord2)t.Item2, (TRecord3)t.Item3))
                            .ToList();
                    }
                }
            }

            if (this.Count < int.MaxValue)
            {
                current = current.Take(this.Count).ToList();
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
                    var value = fieldExpressions[i](
                        (TRecord1)(record.Item1 ?? emptyRecs.Item1),
                        (TRecord2)(record.Item2 ?? emptyRecs.Item2),
                        (TRecord3)(record.Item3 ?? emptyRecs.Item3));
                    propFields[i].SetValue(newRecord, value);
                }

                result.Add(newRecord);
            }

            foreach (var whereExpression in this.WhereExpression)
            {
                var we1 = whereExpression.Compile();
                result = result.Where(we1).ToList();
            }

            return result;
        }

        /// <summary>
        /// Executes the joins.
        /// </summary>
        /// <param name="records1">The records1.</param>
        /// <param name="records2">The records2.</param>
        /// <param name="records3">The records3.</param>
        /// <returns>List&lt;ItemSet&gt;.</returns>
        private List<ItemSet> ExecuteJoins(
            IEnumerable<TRecord1> records1,
            IEnumerable<TRecord2> records2,
            IEnumerable<TRecord3> records3)
        {
            List<ItemSet> records =
                records1.Select(r1 => new ItemSet { Item1 = r1 }).ToList();

            var inners = this.JoinKeys.Where(jks => jks.Type == JoinType.INNER).ToList();
            foreach (var inner in inners)
            {
                var jk = inner.JoinKey;
                var jp = inner.JoinPhrase;

                if (inner.JoinLeftIndex == 0)
                {
                    if (inner.JoinRightIndex == 1)
                    {
                        var joinKey = (inner.JoinKey as Expression<Func<TRecord1, TRecord2, bool>>).Compile();
                        foreach (var r in records.ToList())
                        {
                            var recs2 = records2.Where(r2 => joinKey((TRecord1)r.Item1, r2));
                            if (recs2.Any())
                            {
                                r.Item2 = recs2.First();
                                if (recs2.Count() > 1)
                                {
                                    var moreRecs = recs2.Skip(1);
                                    foreach (var moreRec in moreRecs)
                                    {
                                        records.Add(
                                            new ItemSet
                                            {
                                                Item1 = (TRecord1)r.Item1,
                                                Item2 = moreRec,
                                                Item3 =
                                                    (TRecord3)r.Item3
                                            });
                                    }
                                }
                            }
                        }

                        records.RemoveAll(r => r.Item2 == null);
                    }
                    else if (inner.JoinRightIndex == 2)
                    {
                        var joinKey = (inner.JoinKey as Expression<Func<TRecord1, TRecord3, bool>>).Compile();
                        foreach (var r in records.ToList())
                        {
                            var recs3 = records3.Where(r3 => joinKey((TRecord1)r.Item1, r3));
                            if (recs3.Any())
                            {
                                r.Item3 = recs3.First();
                                if (recs3.Count() > 1)
                                {
                                    var moreRecs = recs3.Skip(1);
                                    foreach (var moreRec in moreRecs)
                                    {
                                        records.Add(
                                            new ItemSet
                                            {
                                                Item1 = (TRecord1)r.Item1,
                                                Item3 = moreRec,
                                                Item2 =
                                                    (TRecord2)r.Item2
                                            });
                                    }
                                }
                            }
                        }

                        records.RemoveAll(r => r.Item3 == null);
                    }
                }
                else if (inner.JoinLeftIndex == 1)
                {
                    if (inner.JoinRightIndex == 2)
                    {
                        var joinKey = (inner.JoinKey as Expression<Func<TRecord2, TRecord3, bool>>).Compile();
                        foreach (var r in records.ToList())
                        {
                            var recs3 = records3.Where(r3 => joinKey((TRecord2)r.Item2, r3));
                            if (recs3.Any())
                            {
                                r.Item3 = recs3.First();
                                if (recs3.Count() > 1)
                                {
                                    var moreRecs = recs3.Skip(1);
                                    foreach (var moreRec in moreRecs)
                                    {
                                        records.Add(
                                            new ItemSet
                                            {
                                                Item1 = (TRecord1)r.Item1,
                                                Item3 = moreRec,
                                                Item2 =
                                                    (TRecord2)r.Item2
                                            });
                                    }
                                }
                            }
                        }

                        records.RemoveAll(r => r.Item3 == null);
                    }
                }
            }

            var outers = this.JoinKeys
                .Where(jks => jks.Type == JoinType.LEFTOUTER || jks.Type == JoinType.RIGHTOUTER)
                .ToList();
            foreach (var outer in outers)
            {
                var jk = outer.JoinKey;
                var jp = outer.JoinPhrase;

                if (outer.JoinLeftIndex == 0)
                {
                    if (outer.JoinRightIndex == 1)
                    {
                        var joinKey = (outer.JoinKey as Expression<Func<TRecord1, TRecord2, bool>>).Compile();

                        foreach (var r in records.ToList())
                        {
                            if (outer.Type == JoinType.LEFTOUTER)
                            {
                                var recs2 = records2.Where(r2 => joinKey((TRecord1)r.Item1, r2));
                                if (recs2.Any())
                                {
                                    var fillFirst = r.Item2 == null ? 1 : 0;
                                    if (fillFirst == 1)
                                    {
                                        r.Item2 = recs2.First();
                                    }

                                    if (recs2.Count() > fillFirst)
                                    {
                                        var moreRecs = recs2.Skip(fillFirst);
                                        foreach (var moreRec in moreRecs)
                                        {
                                            records.Add(
                                                new ItemSet
                                                {
                                                    Item1 = (TRecord1)r.Item1,
                                                    Item2 = moreRec,
                                                    Item3 =
                                                        (TRecord3)r.Item3
                                                });
                                        }
                                    }
                                }
                            }
                            else if (outer.Type == JoinType.RIGHTOUTER)
                            {
                                var recs1 = records1.Where(r1 => joinKey(r1, (TRecord2)r.Item2));
                                if (recs1.Any())
                                {
                                    var fillFirst = r.Item1 == null ? 1 : 0;
                                    if (fillFirst == 1)
                                    {
                                        r.Item1 = recs1.First();
                                    }

                                    if (recs1.Count() > fillFirst)
                                    {
                                        var moreRecs = recs1.Skip(fillFirst);
                                        foreach (var moreRec in moreRecs)
                                        {
                                            records.Add(
                                                new ItemSet
                                                {
                                                    Item1 = moreRec,
                                                    Item2 = (TRecord2)r.Item2,
                                                    Item3 = (TRecord3)r.Item3
                                                });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (outer.JoinRightIndex == 2)
                    {
                        var joinKey = (outer.JoinKey as Expression<Func<TRecord1, TRecord3, bool>>).Compile();
                        foreach (var r in records.ToList())
                        {
                            if (outer.Type == JoinType.LEFTOUTER)
                            {
                                var recs3 = records3.Where(r3 => joinKey((TRecord1)r.Item1, r3));
                                if (recs3.Any())
                                {
                                    var fillFirst = r.Item3 == null ? 1 : 0;
                                    if (fillFirst == 1)
                                    {
                                        r.Item3 = recs3.First();
                                    }

                                    if (recs3.Count() > fillFirst)
                                    {
                                        var moreRecs = recs3.Skip(fillFirst);
                                        foreach (var moreRec in moreRecs)
                                        {
                                            records.Add(
                                                new ItemSet
                                                {
                                                    Item1 = (TRecord1)r.Item1,
                                                    Item3 = moreRec,
                                                    Item2 = (TRecord2)r.Item2
                                                });
                                        }
                                    }
                                }
                            }
                            else if (outer.Type == JoinType.RIGHTOUTER)
                            {
                                var recs1 = records1.Where(r1 => joinKey(r1, (TRecord3)r.Item3));
                                if (recs1.Any())
                                {
                                    var fillFirst = r.Item1 == null ? 1 : 0;
                                    if (fillFirst == 1)
                                    {
                                        r.Item1 = recs1.First();
                                    }

                                    if (recs1.Count() > fillFirst)
                                    {
                                        var moreRecs = recs1.Skip(fillFirst);
                                        foreach (var moreRec in moreRecs)
                                        {
                                            records.Add(
                                                new ItemSet
                                                {
                                                    Item1 = moreRec,
                                                    Item3 = (TRecord3)r.Item3,
                                                    Item2 = (TRecord2)r.Item2
                                                });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (outer.JoinLeftIndex == 1)
                {
                    if (outer.JoinRightIndex == 2)
                    {
                        var joinKey = (outer.JoinKey as Expression<Func<TRecord2, TRecord3, bool>>).Compile();
                        foreach (var r in records.ToList())
                        {
                            if (outer.Type == JoinType.LEFTOUTER)
                            {
                                var recs3 = records3.Where(r3 => joinKey((TRecord2)r.Item2, r3));
                                if (recs3.Any())
                                {
                                    var fillFirst = r.Item3 == null ? 1 : 0;
                                    if (fillFirst == 1)
                                    {
                                        r.Item3 = recs3.First();
                                    }

                                    if (recs3.Count() > fillFirst)
                                    {
                                        var moreRecs = recs3.Skip(fillFirst);
                                        foreach (var moreRec in moreRecs)
                                        {
                                            records.Add(
                                                new ItemSet
                                                {
                                                    Item1 = (TRecord1)r.Item1,
                                                    Item3 = moreRec,
                                                    Item2 = (TRecord2)r.Item2
                                                });
                                        }
                                    }
                                }
                            }
                            else if (outer.Type == JoinType.RIGHTOUTER)
                            {
                                var recs2 = records2.Where(r2 => joinKey(r2, (TRecord3)r.Item3));
                                if (recs2.Any())
                                {
                                    var fillFirst = r.Item2 == null ? 1 : 0;
                                    if (fillFirst == 1)
                                    {
                                        r.Item2 = recs2.First();
                                    }

                                    if (recs2.Count() > fillFirst)
                                    {
                                        var moreRecs = recs2.Skip(fillFirst);
                                        foreach (var moreRec in moreRecs)
                                        {
                                            records.Add(
                                                new ItemSet
                                                {
                                                    Item1 = (TRecord1)r.Item1,
                                                    Item3 = (TRecord3)r.Item3,
                                                    Item2 = moreRec
                                                });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return records;
        }
    }
}