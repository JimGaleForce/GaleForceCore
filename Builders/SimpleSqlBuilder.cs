//-----------------------------------------------------------------------
// <copyright file="SimpleSqlBuilder.cs" company="Gale-Force">
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
    public class SimpleSqlBuilder<TRecord>
    {
        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        /// <value>The types.</value>
        public Type[] Types { get; protected set; }

        /// <summary>
        /// Gets the name of the tables, when multiple.
        /// </summary>
        /// <value>The name of the table.</value>
        public string[] TableNames { get; protected set; }

        /// <summary>
        /// As fields
        /// </summary>
        public Dictionary<object, Expression<Func<TRecord, object>>> AsFields
        {
            get;
            protected set;
        } = new Dictionary<object, Expression<Func<TRecord, object>>>();

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; protected set; }

        /// <summary>
        /// Gets the name of the table to merge into.
        /// </summary>
        /// <value>The name of the table.</value>
        public string MergeIntoTableName { get; protected set; }

        /// <summary>
        /// Gets the field names.
        /// </summary>
        /// <value>The fields.</value>
        public List<string> Fields { get; protected set; } = new List<string>();

        /// <summary>
        /// Gets the update field names.
        /// </summary>
        /// <value>The fields.</value>
        public List<string> Updates { get; protected set; } = new List<string>();

        /// <summary>
        /// Gets the insert field names.
        /// </summary>
        /// <value>The fields.</value>
        public List<string> Inserts { get; protected set; } = new List<string>();

        /// <summary>
        /// Gets the values field names.
        /// </summary>
        /// <value>The fields.</value>
        public List<string> Valueset { get; protected set; } = new List<string>();

        /// <summary>
        /// Gets the maximum count for returned items.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; protected set; } = int.MaxValue;

        /// <summary>
        /// Gets the command (only SELECT active currently).
        /// </summary>
        /// <value>The command.</value>
        public string Command { get; protected set; } = "SELECT";

        /// <summary>
        /// Gets the where condition string.
        /// </summary>
        /// <value>The where string.</value>
        public string WhereString { get; protected set; } = null;

        /// <summary>
        /// Gets or sets the join key.
        /// </summary>
        /// <value>The join key.</value>
        public Expression<Func<TRecord, object>> MatchKey1 { get; protected set; }

        /// <summary>
        /// Gets or sets the match key STR1.
        /// </summary>
        public string MatchKeyStr1 { get; protected set; }

        /// <summary>
        /// Gets or sets the join key.
        /// </summary>
        /// <value>The join key.</value>
        public Expression<Func<TRecord, TRecord, bool>> MatchKey2 { get; protected set; }

        /// <summary>
        /// Gets or sets the match key STR2.
        /// </summary>
        public string MatchKeyStr2 { get; protected set; }

        /// <summary>
        /// Gets or sets the join phrase.
        /// </summary>
        /// <value>The join phrase.</value>
        public string MatchPhrase { get; protected set; }

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; protected set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the source data.
        /// </summary>
        public IEnumerable<TRecord> SourceData { get; protected set; } = new List<TRecord>();

        /// <summary>
        /// Gets the 'order by' list, including 'then by'.
        /// </summary>
        /// <value>The order by list.</value>
        public List<SqlBuilderOrderItem<TRecord>> OrderByList
        {
            get;
            protected set;
        } = new List<SqlBuilderOrderItem<TRecord>>();

        /// <summary>
        /// Gets or sets the order by strings.
        /// </summary>
        public List<string> OrderByStrings { get; protected set; } = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        public SimpleSqlBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public SimpleSqlBuilder(string tableName)
        {
            this.From(tableName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        public SimpleSqlBuilder(Type[] types)
        {
            this.Types = types;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="tableNames">The table names.</param>
        public SimpleSqlBuilder(string[] tableNames)
        {
            this.From(tableNames);
        }

        /// <summary>
        /// Sets the option.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> SetOption(string name, object value)
        {
            this.Metadata[name] = value;
            return this;
        }

        /// <summary>
        /// Gets the option.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>T.</returns>
        public T GetOption<T>(string name, T defaultValue)
        {
            return this.Metadata.ContainsKey(name) ? (T)this.Metadata[name] : defaultValue;
        }

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public IEnumerable<Expression<Func<TRecord, object>>> FieldExpressions { get; protected set; } = null;

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public IEnumerable<Expression<Func<TRecord, object>>> UpdateExpressions { get; protected set; } = null;

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public IEnumerable<Expression<Func<TRecord, object>>> InsertExpressions { get; protected set; } = null;

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public IEnumerable<Expression<Func<TRecord, object>>> ValueExpressions { get; protected set; } = null;

        /// <summary>
        /// Gets the order by expressions (lambda expressions for order-by's and then-by's).
        /// </summary>
        /// <value>The order by expressions.</value>
        public List<Expression<Func<TRecord, object>>> OrderByExpressions
        {
            get;
            protected set;
        } = new List<Expression<Func<TRecord, object>>>();

        /// <summary>
        /// Gets the where condition expression (lambda).
        /// </summary>
        /// <value>The where expression.</value>
        public Expression<Func<TRecord, bool>> WhereExpression { get; protected set; } = null;

        /// <summary>
        /// Gets the where condition expression (lambda).
        /// </summary>
        /// <value>The where expression.</value>
        public Expression<Func<object, object, bool>> WhereExpression2 { get; protected set; } = null;

        /// <summary>
        /// Gets the where condition string.
        /// </summary>
        /// <value>The where string.</value>
        public string WhereString2 { get; protected set; } = null;

        /// <summary>
        /// Gets the where condition expression (lambda).
        /// </summary>
        /// <value>The where expression.</value>
        public Expression<Func<object, object, object, bool>> WhereExpression3 { get; protected set; } = null;

        /// <summary>
        /// Gets or sets the when matched expression.
        /// </summary>
        public Expression<Action<SimpleSqlBuilder<TRecord>>> WhenMatchedExpression { get; protected set; } = null;

        /// <summary>
        /// Gets or sets the when not matched expression.
        /// </summary>
        public Expression<Action<SimpleSqlBuilder<TRecord>>> WhenNotMatchedExpression { get; protected set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether [are table names required].
        /// </summary>
        public bool AreTableNamesRequired { get; set; }

        /// <summary>
        /// Gets or sets the distinct on expression.
        /// </summary>
        public Expression<Func<TRecord, object>> DistinctOnExpression { get; set; }

        /// <summary>
        /// Gets or sets the distinct on string.
        /// </summary>
        public string DistinctOnStr { get; set; }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> From(string tableName)
        {
            this.TableName = tableName;
            return this;
        }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableNameSource">The table name source.</param>
        /// <param name="tableNameTarget">The table name target.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> From(string tableNameSource, string tableNameTarget)
        {
            this.TableNames = new string[] { tableNameSource, tableNameTarget };
            return this;
        }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableNames">The table names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> From(string[] tableNames)
        {
            this.TableNames = tableNames;
            return this;
        }

        /// <summary>
        /// Uses the table names.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> UseTableNames()
        {
            this.AreTableNamesRequired = true;
            return this;
        }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> MergeInto(string tableName)
        {
            this.Command = "MERGE";
            this.MergeIntoTableName = tableName;
            return this;
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as an expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select(Expression<Func<TRecord, object>> field)
        {
            return this.Select(new Expression<Func<TRecord, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select(params Expression<Func<TRecord, object>>[] fields)
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
        public SimpleSqlBuilder<TRecord> Select(IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            var names = fields.Select(
                field => this.ParseExpression(this.Types, field.Body, parameters: field.Parameters))
                .ToList();
            return this.Select(names);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as a field names (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select(string fieldName)
        {
            return this.Select(new string[] { fieldName });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select(IEnumerable<string> fieldNames)
        {
            this.Fields.AddRange(fieldNames);
            return this.Select();
        }

        /// <summary>
        /// Begins the Select command.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select()
        {
            this.Command = "SELECT";
            return this;
        }

        /// <summary>
        /// Begins the Delete command.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Delete()
        {
            this.Command = "DELETE";
            return this;
        }

        /// <summary>
        /// Allows the delete command to delete duplicates based on this field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ExceptDistinctBy(Expression<Func<TRecord, object>> field)
        {
            this.DistinctOnExpression = field;
            this.DistinctOnStr = this.ParseExpression(
                this.Types,
                field.Body,
                false,
                parameters: field.Parameters);

            return this;
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="records">The records.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(
            IEnumerable<TRecord> records,
            params Expression<Func<TRecord, object>>[] fields)
        {
            this.Update(records);
            return this.Update(fields);
        }

        /// <summary>
        /// Updates the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(
            TRecord record,
            params Expression<Func<TRecord, object>>[] fields)
        {
            this.Update(new List<TRecord> { record });
            return this.Update(fields);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="records">The records.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(
            IEnumerable<TRecord> records,
            IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            this.Update(records);
            return this.Update(fields);
        }

        /// <summary>
        /// Updates the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(
            TRecord record,
            IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            this.Update(new List<TRecord> { record });
            return this.Update(fields);
        }

        /// <summary>
        /// Updates the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(TRecord record)
        {
            return this.Update(new List<TRecord> { record });
        }

        /// <summary>
        /// Updates the specified records.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(IEnumerable<TRecord> records)
        {
            this.SourceData = records;
            return this.Update();
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as an expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(Expression<Func<TRecord, object>> field)
        {
            return this.Update(new Expression<Func<TRecord, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(params Expression<Func<TRecord, object>>[] fields)
        {
            this.UpdateExpressions = fields;
            return this.Update(this.UpdateExpressions);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            var names = fields.Select(
                field => this.ParseExpression(this.Types, field.Body, parameters: field.Parameters))
                .ToList();
            return this.Update(names);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as a field names (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(string fieldName)
        {
            return this.Update(new string[] { fieldName });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(IEnumerable<string> fieldNames)
        {
            this.Updates.AddRange(fieldNames);
            return this.Update();
        }

        /// <summary>
        /// Selects this instance.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update()
        {
            this.Command = "UPDATE";
            return this;
        }

        /// <summary>
        /// Inserts the specified records.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(
            IEnumerable<TRecord> records,
            IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            this.Insert(records);
            return this.Insert(fields);
        }

        /// <summary>
        /// Inserts the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(
            TRecord record)
        {
            return this.Insert(new List<TRecord> { record });
        }

        /// <summary>
        /// Inserts the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(
            TRecord record,
            IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            this.Insert(new List<TRecord> { record });
            return this.Insert(fields);
        }

        /// <summary>
        /// Inserts the specified records.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(
            IEnumerable<TRecord> records,
            params Expression<Func<TRecord, object>>[] fields)
        {
            this.Insert(records);
            return this.Insert(fields);
        }

        /// <summary>
        /// Inserts the specified records.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(IEnumerable<TRecord> records)
        {
            this.SourceData = records;
            return this.Insert();
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as an expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(Expression<Func<TRecord, object>> field)
        {
            return this.Insert(new Expression<Func<TRecord, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(params Expression<Func<TRecord, object>>[] fields)
        {
            this.InsertExpressions = fields;
            return this.Insert(this.InsertExpressions);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            var names = fields.Select(
                field => this.ParseExpression(this.Types, field.Body, parameters: field.Parameters))
                .ToList();
            return this.Insert(names);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as a field names (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(string fieldName)
        {
            return this.Insert(new string[] { fieldName });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(IEnumerable<string> fieldNames)
        {
            this.Inserts.AddRange(fieldNames);
            return this.Insert();
        }

        /// <summary>
        /// Selects this instance.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert()
        {
            this.Command = "INSERT";
            return this;
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as an expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Values(Expression<Func<TRecord, object>> field)
        {
            return this.Values(new Expression<Func<TRecord, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Values(params Expression<Func<TRecord, object>>[] fields)
        {
            this.ValueExpressions = fields;
            return this.Values(this.ValueExpressions);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Values(IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            var names = fields.Select(
                field => this.ParseExpression(
                    this.Types,
                    field.Body,
                    parameters: field.Parameters,
                    tableNames: this.AreTableNamesRequired ? this.TableNames : null))
                .ToList();
            return this.Values(names);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as a field names (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Values(string fieldName)
        {
            return this.Values(new string[] { fieldName });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Values(IEnumerable<string> fieldNames)
        {
            this.Valueset.AddRange(fieldNames);
            return this;
        }

        /// <summary>
        /// Adds an order-by expression ascending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> OrderBy(Expression<Func<TRecord, object>> field)
        {
            var name = this.ParseExpression(this.Types, field.Body, parameters: field.Parameters);
            return this.OrderBy(name, true, field);
        }

        /// <summary>
        /// Adds an order-by fieldname ascending (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> OrderBy(string fieldName)
        {
            return this.OrderBy(fieldName, true);
        }

        /// <summary>
        /// Adds an order-by fieldname and expression, with asc/desc.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <param name="expression">The expression.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        private SimpleSqlBuilder<TRecord> OrderBy(
            string fieldName,
            bool isAscending,
            Expression<Func<TRecord, object>> expression = null)
        {
            this.OrderByList.Clear();
            return this.ThenBy(fieldName, isAscending, expression);
        }

        /// <summary>
        /// Adds an then-by expression ascending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ThenBy(Expression<Func<TRecord, object>> field)
        {
            var name = this.ParseExpression(this.Types, field.Body);
            return this.ThenBy(name, true, field);
        }

        /// <summary>
        /// Adds an then-by fieldname ascending (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ThenBy(string fieldName)
        {
            return this.ThenBy(fieldName, true);
        }

        /// <summary>
        /// Adds an then-by fieldname and expression, with asc/desc.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="isAscending">if set to <c>true</c> [is ascending].</param>
        /// <param name="expression">The expression.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        private SimpleSqlBuilder<TRecord> ThenBy(
            string fieldName,
            bool isAscending,
            Expression<Func<TRecord, object>> expression = null)
        {
            this.OrderByList
                .Add(
                    new SqlBuilderOrderItem<TRecord>
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
        public SimpleSqlBuilder<TRecord> OrderByDescending(Expression<Func<TRecord, object>> field)
        {
            var name = this.ParseExpression(this.Types, field.Body, parameters: field.Parameters);
            return this.OrderBy(name, false, field);
        }

        /// <summary>
        /// Adds an order-by fieldname descending (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> OrderByDescending(string fieldName)
        {
            return this.OrderBy(fieldName, false);
        }

        /// <summary>
        /// Adds an then-by expression descending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ThenByDescending(Expression<Func<TRecord, object>> field)
        {
            var name = this.ParseExpression(this.Types, field.Body, parameters: field.Parameters);
            return this.ThenBy(name, false, field);
        }

        /// <summary>
        /// Adds an then-by fieldname descending (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ThenByDescending(string fieldName)
        {
            return this.ThenBy(fieldName, false);
        }

        /// <summary>
        /// Sets the where condition as an expression (can build, execute).
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Where(Expression<Func<TRecord, bool>> condition)
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
        /// Matches the specified match key.
        /// </summary>
        /// <param name="matchKey">The match key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Match(
            Expression<Func<TRecord, object>> matchKey)
        {
            this.MatchKey1 = matchKey;
            this.MatchKeyStr1 = this.ParseExpression(
                this.Types,
                matchKey.Body,
                parameters: matchKey.Parameters);
            return this;
        }

        /// <summary>
        /// Matches the specified match key.
        /// </summary>
        /// <param name="matchKey">The match key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Match(
            Expression<Func<TRecord, TRecord, bool>> matchKey)
        {
            this.MatchKey2 = matchKey;
            this.MatchKeyStr2 = this.ParseExpression(
                this.Types,
                matchKey.Body,
                true,
                parameters: matchKey.Parameters);
            return this;
        }

        /// <summary>
        /// Sets the maximum count for returned records.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Take(int count)
        {
            this.Count = count;
            return this;
        }

        /// <summary>
        /// Whens the matched.
        /// </summary>
        /// <param name="ssBuilderMatched">The ss builder matched.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> WhenMatched(Expression<Action<SimpleSqlBuilder<TRecord>>> ssBuilderMatched)
        {
            this.WhenMatchedExpression = ssBuilderMatched;
            return this;
        }

        /// <summary>
        /// Whens the not matched.
        /// </summary>
        /// <param name="ssBuilderNotMatched">The ss builder not matched.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> WhenNotMatched(
            Expression<Action<SimpleSqlBuilder<TRecord>>> ssBuilderNotMatched)
        {
            this.WhenNotMatchedExpression = ssBuilderNotMatched;
            return this;
        }

        /// <summary>
        /// Parses different supported expressions to sql-friendly text.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="isCondition">if set to <c>true</c> [is condition].</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="hideSourceTable">if set to <c>true</c> [hide source table].</param>
        /// <param name="tableNames">The table names.</param>
        /// <param name="evalInfo">The eval information.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="GaleForceCore.Builders.DynamicMethodException">Unable to prebuild SQL string with this method: " + meMethodName</exception>
        /// <exception cref="System.NotSupportedException">Unknown expression type for: " + exp.ToString()</exception>
        protected string ParseExpression(
            Type[] types,
            Expression exp,
            bool isCondition = false,
            IReadOnlyCollection<ParameterExpression> parameters = null,
            bool hideSourceTable = false,
            string[] tableNames = null,
            EvalInfo evalInfo = null)
        {
            var sb = new StringBuilder();
            if (exp is BinaryExpression)
            {
                var bExp = exp as BinaryExpression;
                evalInfo?.Register(bExp, typeof(BinaryExpression));
                var op = bExp.NodeType;
                var isCompare = op == ExpressionType.Equal || op == ExpressionType.NotEqual;

                var left = this.ParseExpression(
                    types,
                    bExp.Left,
                    isCondition && !isCompare,
                    parameters,
                    tableNames: tableNames,
                    evalInfo: evalInfo);
                var right = this.ParseExpression(
                    types,
                    bExp.Right,
                    isCondition && !isCompare,
                    parameters,
                    tableNames: tableNames,
                    evalInfo: evalInfo);

                var prefixLeft = "(";
                if (bExp.Left.NodeType == ExpressionType.Not)
                {
                    prefixLeft += "NOT ";
                }

                var addLeft = left;
                var addCenter = string.Empty;
                var prefixRight = string.Empty;
                var addRight = right;
                var suffixRight = ")";
                var isFunction = false;

                switch (op)
                {
                    case ExpressionType.AndAlso:
                        addCenter = " AND ";
                        break;
                    case ExpressionType.OrElse:
                        addCenter = " OR ";
                        break;
                    case ExpressionType.GreaterThan:
                        addCenter = " > ";
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        addCenter = " >= ";
                        break;
                    case ExpressionType.LessThan:
                        addCenter = " < ";
                        break;
                    case ExpressionType.LessThanOrEqual:
                        addCenter = " <= ";
                        break;
                    case ExpressionType.NotEqual:
                        addCenter = right == "NULL" ? " IS NOT " : " != ";
                        break;
                    case ExpressionType.Add:
                        if (bExp.Left.Type.Name == "String" && bExp.Right.Type.Name == "String")
                        {
                            prefixLeft = "CONCAT(";
                            addCenter = ",";
                            isFunction = true;
                        }
                        else
                        {
                            addCenter = " + ";
                        }
                        break;
                    case ExpressionType.Subtract:
                        addCenter = " - ";
                        break;
                    case ExpressionType.Multiply:
                        addCenter = " * ";
                        break;
                    case ExpressionType.Divide:
                        addCenter = " / ";
                        break;
                    case ExpressionType.Equal:
                        addCenter = right == "NULL" ? " IS " : " = ";
                        break;
                }

                if (bExp.Right.NodeType == ExpressionType.Not && bExp.Right.Type.Name != "Boolean")
                {
                    if (isFunction)
                    {
                        prefixLeft = "NOT " + prefixLeft;
                    }
                    else
                    {
                        prefixRight = "NOT ";
                    }
                }

                sb.Append(prefixLeft);
                sb.Append(addLeft);
                sb.Append(addCenter);
                sb.Append(prefixRight);
                sb.Append(addRight);
                sb.Append(suffixRight);

                var value = sb.ToString();
                var parent = evalInfo?.Register(bExp, typeof(BinaryExpression), value);
                evalInfo?.RegisterChildren(parent, bExp.Left, bExp.Right);

                return value;
            }
            else if (exp is UnaryExpression)
            {
                var uParent = evalInfo?.Register(exp, typeof(UnaryExpression));
                var operand = (exp as UnaryExpression).Operand;
                if (operand is MemberExpression)
                {
                    var operandMember = (exp as UnaryExpression).Operand as MemberExpression;
                    var uParent2 = evalInfo?.Register(operandMember, typeof(MemberExpression));
                    evalInfo?.RegisterChildren(uParent, operand);
                    var declaringType = operandMember.Member.DeclaringType.Name;

                    var matchingType = this.GetMatchingType(types, declaringType);
                    if (matchingType != null)
                    {
                        var ignoreTableName = 
                            (hideSourceTable ||
                            (tableNames == null && (types == null || types.Length < 2)));
                        var prefix = ignoreTableName
                            ? string.Empty
                            : (this.GetMatchingTableName(
                                    types,
                                    matchingType,
                                    parameters,
                                    operandMember: operandMember,
                                    tableNames: tableNames) +
                                ".");
                        var suffix = string.Empty;
                        if (isCondition &&
                            operandMember.Member is PropertyInfo &&
                            SqlHelpers.GetBetterPropTypeName(((PropertyInfo)operandMember.Member).PropertyType)
                                .StartsWith("bool"))
                        {
                            suffix = exp.NodeType == ExpressionType.Not ? " = 0" : " = 1";
                        }

                        var value = prefix + operandMember.Member.Name + suffix;
                        evalInfo?.Register(operand, typeof(MemberExpression), value);
                        return value;
                    }
                    else
                    {
                        // eval
                        var pe = operandMember;
                        var ce = operandMember.Expression as ConstantExpression;

                        evalInfo?.Register(ce, typeof(ConstantExpression));
                        evalInfo?.RegisterChildren(uParent2, ce);

                        if (ce != null)
                        {
                            if (pe.Member is FieldInfo)
                            {
                                object container = ce.Value;
                                object value = ((FieldInfo)pe.Member).GetValue(container);
                                var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                                evalInfo?.Register(operand, typeof(MemberExpression), sqlValue);
                                return sqlValue;
                            }
                            else if (pe.Member is PropertyInfo)
                            {
                                object container = ce.Value;
                                object value = ((PropertyInfo)pe.Member).GetValue(container);
                                var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                                evalInfo?.Register(operand, typeof(MemberExpression), sqlValue);
                                return sqlValue;
                            }
                        }

                        evalInfo?.Register(pe, typeof(MemberExpression), pe.Member.Name);
                        return pe.Member.Name;
                    }
                }
                else
                {
                    var value = this.ParseExpression(
                        types,
                        operand,
                        isCondition,
                        parameters,
                        tableNames: tableNames,
                        evalInfo: evalInfo);

                    evalInfo?.Register(operand, typeof(MemberExpression), value);
                    return value;
                }
            }
            else if (exp is MemberExpression)
            {
                if (exp.ToString().StartsWith("value("))
                {
                    var value = Expression.Lambda(exp).Compile().DynamicInvoke();
                    var xValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                    evalInfo?.Register(exp, typeof(ConstantExpression), xValue);
                    return xValue;
                }

                var pe = exp as MemberExpression;
                evalInfo?.Register(pe, typeof(MemberExpression));

                var ce = pe.Expression as ConstantExpression;
                var suffix = string.Empty;

                var parmName = pe.Expression is ParameterExpression ? (pe.Expression as ParameterExpression).Name : null;
                var declaringType = pe.Expression.Type.Name;
                var matchingType = this.GetMatchingType(types, declaringType);
                var ignoreTableName = 
                    (hideSourceTable ||
                    (tableNames == null && (types == null || types.Length < 2 || matchingType == null)));
                var prefix = ignoreTableName
                    ? string.Empty
                    : (this.GetMatchingTableName(
                            types,
                            matchingType,
                            parameters,
                            parmName,
                            operandMember: pe,
                            tableNames: tableNames) +
                        ".");

                if (ce != null)
                {
                    if (pe.Member is FieldInfo)
                    {
                        object container = ce.Value;
                        object value = ((FieldInfo)pe.Member).GetValue(container);
                        var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                        evalInfo?.Register(pe, typeof(MemberExpression), sqlValue);
                        return sqlValue;
                    }
                    else if (pe.Member is PropertyInfo)
                    {
                        object container = ce.Value;
                        object value = ((PropertyInfo)pe.Member).GetValue(container);
                        var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                        evalInfo?.Register(pe, typeof(MemberExpression), sqlValue);
                        return sqlValue;
                    }
                }
                else if (isCondition)
                {
                    if (pe.Member is PropertyInfo &&
                        SqlHelpers.GetBetterPropTypeName(((PropertyInfo)pe.Member).PropertyType).StartsWith("bool"))
                    {
                        suffix = " = 1";
                    }
                }

                // special case to get name of .Value references
                var memberName = pe.Member.Name;

                var isNullable = pe.Member.DeclaringType.Name.StartsWith("Nullable");
                var isValueType = (pe.Member as PropertyInfo)?.PropertyType.BaseType.Name == "ValueType";
                if (isValueType && isNullable)
                {
                    memberName = (pe.Expression as MemberExpression)?.Member.Name;
                }

                var value2 = prefix + memberName + suffix;
                evalInfo?.Register(pe, typeof(MemberExpression), value2);
                return value2;
            }
            else if (exp is ConstantExpression)
            {
                var value = (exp as ConstantExpression).Value;
                var valueStr = value == null ? "NULL" : value.ToString();
                if (value is string || value is DateTime)
                {
                    valueStr = "'" + valueStr + "'";
                }

                evalInfo?.Register(exp, typeof(ConstantExpression), valueStr);

                return valueStr;
            }
            else if (exp is ConditionalExpression)
            {
                var ce = exp as ConditionalExpression;
                var test = ce.Test;
                var ei = evalInfo ?? new EvalInfo();
                var testExp = this.ParseExpression(
                    types,
                    test,
                    tableNames: tableNames,
                    evalInfo: ei);

                if (this.GetOption<bool>("EarlyConditionalEval", true))
                {
                    if (ei.HasOnlyConstants(test))
                    {
                        var testResult = (bool) Expression.Lambda(test).Compile().DynamicInvoke();
                        testExp = this.ParseExpression(
                            types,
                            testResult ? ce.IfTrue : ce.IfFalse,
                            tableNames: tableNames,
                            evalInfo: ei,
                            isCondition: true);

                        evalInfo?.Register(ce, typeof(ConstantExpression), testExp);
                        return testExp;
                    }
                }

                var expTrue = this.ParseExpression(
                    types,
                    ce.IfTrue,
                    tableNames: tableNames,
                    evalInfo: ei);

                var expFalse = this.ParseExpression(
                    types,
                    ce.IfFalse,
                    tableNames: tableNames,
                    evalInfo: ei);

                var value = $"1 = IIF({testExp},IIF({expTrue},1,0),IIF({expFalse},1,0))";

                var parent = evalInfo?.Register(ce, typeof(ConstantExpression), value);
                evalInfo?.RegisterChildren(parent, ce.IfTrue, ce.IfFalse);

                return value;
            }
            else if (exp is MethodCallExpression)
            {
                var me = exp as MethodCallExpression;

                var meMethodName = me.Method.Name;
                var meContainingType = me.Method.DeclaringType.Name;

                // special case methods
                // todo: make sure this only acts on fields from the type, not others
                if (meContainingType == "String")
                {
                    string subValue;
                    string obj;
                    EvalNode parent = null;

                    string[] stringMethods = new string[] { "Contains", "StartsWith", "EndWidth" };
                    var isStringContainer = stringMethods.Contains(meMethodName);

                    if (isStringContainer)
                    {
                        subValue = this.RemoveOuterQuotes(
                            this.ParseExpression(
                                types,
                                me.Arguments[0],
                                parameters: parameters,
                                tableNames: tableNames,
                                evalInfo: evalInfo));

                        obj = this.ParseExpression(
                            types,
                            me.Object,
                            parameters: parameters,
                            tableNames: tableNames);

                        string value = null;
                        switch (meMethodName)
                        {
                            case "Contains":
                                value = $"{obj} LIKE '%{subValue}%'";
                                break;
                            case "StartsWith":
                                value = $"{obj} LIKE '{subValue}%'";
                                break;
                            case "EndsWith":
                                value = $"{obj} LIKE '%{subValue}'";
                                break;
                        }

                        parent = evalInfo?.Register(me, typeof(MethodCallExpression), value);
                        evalInfo?.RegisterChildren(parent, me.Arguments[0], me.Object);
                        return value;
                    }
                }
                else if ((meContainingType.StartsWith("List") || meContainingType.StartsWith("Enumerable")) &&
                    meMethodName == "Contains")
                {
                    var subValue = this.RemoveOuterQuotes(
                        this.ParseExpression(
                            types,
                            me.Arguments[0],
                            parameters: parameters,
                            tableNames: tableNames,
                            evalInfo: evalInfo));
                    var obj = this.ParseExpression(
                        types,
                        me.Object ?? me.Arguments[1],
                        parameters: parameters,
                        tableNames: tableNames,
                        evalInfo: evalInfo);

                    string value;
                    if (subValue.StartsWith("(") && !obj.StartsWith("("))
                    {
                        value = $"{obj} IN {subValue}";
                    }
                    else
                    {
                        value = $"{subValue} IN {obj}";
                    }

                    var parent = evalInfo?.Register(me, typeof(MethodCallExpression), value);
                    evalInfo?.RegisterChildren(parent, me.Arguments[0], me.Object ?? me.Arguments[1]);

                    return value;
                }

                try
                {
                    object value = Expression.Lambda(me).Compile().DynamicInvoke();
                    var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                    evalInfo?.Register(me, typeof(MethodCallExpression), sqlValue);
                    return sqlValue;
                }
                catch (InvalidOperationException ioe)
                {
                    throw new DynamicMethodException(
                        "Unable to prebuild SQL string with this method: " + meMethodName,
                        ioe);
                }
            }
            else
            {
                throw new NotSupportedException("Unknown expression type for: " + exp.ToString());
            }
        }

        /// <summary>
        /// Removes the outer quotes.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        protected string RemoveOuterQuotes(string value)
        {
            if (value.StartsWith("'") && value.EndsWith("'"))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        /// <summary>
        /// Gets the type of the matching.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="declaringTypeName">Name of the declaring type.</param>
        /// <returns>Type.</returns>
        private Type GetMatchingType(Type[] types, string declaringTypeName)
        {
            return types == null || types.Length == 0
                ? (declaringTypeName == typeof(TRecord).Name ? typeof(TRecord) : null)
                : types.FirstOrDefault(t => t.Name == declaringTypeName);
        }

        /// <summary>
        /// Gets the name of the matching table.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="parmName">Name of the parm.</param>
        /// <param name="operandMember">The operand member.</param>
        /// <param name="tableNames">The table names.</param>
        /// <returns>System.String.</returns>
        private string GetMatchingTableName(
            Type[] types,
            Type type,
            IReadOnlyCollection<ParameterExpression> parameters,
            string parmName = null,
            MemberExpression operandMember = null,
            string[] tableNames = null)
        {
            if (type == null)
            {
                return null;
            }

            tableNames = tableNames ?? this.TableNames;

            string tableName = null;
            var tpe = operandMember.Expression as ParameterExpression;
            if (tpe != null)
            {
                var name = tpe.Name;
                var nameIndex = parameters.Select(p => p.Name).ToList().IndexOf(name);
                if (nameIndex > -1)
                {
                    tableName = tableNames[nameIndex];
                    return tableName;
                }
            }

            var typeName = type.Name;
            var index = parmName != null
                ? parameters.Select(p => p.Name).ToList().IndexOf(parmName)
                : Array.IndexOf(this.Types.Select(t => t.Name).ToArray(), typeName);
            tableName = tableNames != null && this.Types != null && this.Types.Length > 0
                ? tableNames[index]
                : typeName;

            return tableName;
        }

        /// <summary>
        /// Injects the inner clauses.
        /// </summary>
        /// <param name="sb">The sb.</param>
        public virtual void InjectInnerClauses(StringBuilder sb)
        {
            return;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns>System.String.</returns>
        public string Build()
        {
            switch (this.Command)
            {
                case "SELECT":
                    return this.BuildSelect();
                case "UPDATE":
                    return this.BuildUpdate();
                case "INSERT":
                    return this.BuildInsert();
                case "DELETE":
                    return this.BuildDelete();
                case "MERGE":
                    return this.BuildMerge();
            }

            return null;
        }

        /// <summary>
        /// Builds the sql-server friendly string for DELETE.
        /// </summary>
        /// <returns>System.String.</returns>
        private string BuildDelete()
        {
            var sb = new StringBuilder();

            var tableName = this.TableNames != null && this.TableNames.Length > 0 ? this.TableNames[0] : this.TableName;

            if (this.DistinctOnStr != null)
            {
                sb.Append(
                    $"with ctr AS (SELECT {this.DistinctOnStr}, row_number() over (partition by {this.DistinctOnStr} order by {this.DistinctOnStr}) as Temp from {tableName}) ");
            }

            sb.Append(this.Command);
            sb.Append(" ");

            sb.Append($"FROM {tableName} ");

            // this.InjectInnerClauses(sb);

            if (!string.IsNullOrEmpty(this.WhereString))
            {
                sb.Append($"WHERE {this.WhereString} ");
                if (this.DistinctOnStr != null)
                {
                    sb.Append("AND Temp > 1 ");
                }
            }
            else
            {
                if (this.DistinctOnStr != null)
                {
                    sb.Append("WHERE Temp > 1 ");
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Joineds the where string.
        /// </summary>
        /// <returns>System.String.</returns>
        private string JoinedWhereString()
        {
            if (!string.IsNullOrEmpty(this.WhereString) && !string.IsNullOrEmpty(this.WhereString2))
            {
                return $"WHERE {this.WhereString} AND {this.WhereString2} ";
            }
            else if (!string.IsNullOrEmpty(this.WhereString))
            {
                return $"WHERE {this.WhereString} ";
            }
            else if (!string.IsNullOrEmpty(this.WhereString2))
            {
                return $"WHERE {this.WhereString2} ";
            }

            return "";
        }

        /// <summary>
        /// Builds the sql-server friendly string.
        /// </summary>
        /// <returns>System.String.</returns>
        private string BuildSelect()
        {
            var sb = new StringBuilder();
            sb.Append(this.Command);
            sb.Append(" ");

            if (this.Count < int.MaxValue)
            {
                sb.Append($"TOP {this.Count} ");
            }

            if (this.Fields.Count > 0)
            {
                sb.Append(string.Join(",", this.Fields));
            }
            else
            {
                sb.Append("*");
            }

            if (this.TableNames != null && this.TableNames.Length > 0)
            {
                sb.Append($" FROM {this.TableNames[0]} ");
            }
            else
            {
                sb.Append($" FROM {this.TableName} ");
            }

            this.InjectInnerClauses(sb);

            sb.Append(this.JoinedWhereString());

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

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Builds the sql-server friendly string.
        /// </summary>
        /// <returns>System.String.</returns>
        private string BuildUpdate()
        {
            var sb = new StringBuilder();
            var props = typeof(TRecord).GetProperties();

            List<string> matchFields = new List<string>();
            if (!string.IsNullOrEmpty(this.MatchKeyStr1))
            {
                matchFields.Add(this.MatchKeyStr1);
            }

            var fields = this.Updates;
            if (fields.Count() == 0)
            {
                // get all
                fields = this.GetFields();
            }

            var records = this.SourceData;
            if (records == null || records.Count() == 0)
            {
                // this is an update segment for an outside instruction, do field to field or field to value
                var setValues = this.CreateFieldEqualsExpressionValues(fields, this.Valueset, this.TableNames);
                sb.Append(this.Command);
                sb.Append(" SET ");
                sb.Append(string.Join(", ", setValues));
                return sb.ToString().Trim();
            }

            var count = 0;
            foreach (var record in records)
            {
                if (count > 0)
                {
                    sb.AppendLine(";");
                }

                sb.Append(this.Command);

                if (this.TableNames != null && this.TableNames.Length > 0)
                {
                    sb.Append($" {this.TableNames[0]} ");
                }
                else
                {
                    sb.Append($" {this.TableName} ");
                }

                sb.Append("SET ");

                var setValues = this.CreateFieldEqualsValues(fields, record, props);
                sb.Append(string.Join(", ", setValues));

                var matchValues = matchFields.Count() > 0
                    ? (string.Join(" AND ", this.CreateFieldEqualsValues(matchFields, record, props)) +
                        (!string.IsNullOrEmpty(this.WhereString) ? " AND " : ""))
                    : "";

                if (!string.IsNullOrEmpty(this.WhereString) || !string.IsNullOrEmpty(matchValues))
                {
                    sb.Append($" WHERE {matchValues}{this.WhereString}");
                }

                count++;
            }

            if (count > 1)
            {
                sb.AppendLine(";");
                sb.AppendLine("GO;");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Builds the sql-server friendly string.
        /// </summary>
        /// <returns>System.String.</returns>
        private string BuildInsert()
        {
            var sb = new StringBuilder();
            var props = typeof(TRecord).GetProperties();

            var fields = this.Inserts;
            if (fields.Count() == 0)
            {
                // get all
                fields = this.GetFields();
            }

            var fieldString = string.Join(",", fields);

            var records = this.SourceData;
            if (records == null || records.Count() == 0)
            {
                var prefix0 = this.Command +
                    $" ({fieldString}) VALUES ";

                // this is an update segment for an outside instruction, do field to field or field to value
                var setValues = this.CreateFieldExpressionValues(fields, this.Valueset, this.TableNames);
                sb.Append(prefix0);
                sb.Append("(");
                sb.Append(string.Join(", ", setValues));
                sb.Append(")");
                return sb.ToString().Trim();
            }

            var prefix = this.Command +
                " INTO "
                +
                ((this.TableNames != null && this.TableNames.Length > 0) ? this.TableNames[0] : this.TableName)
                +
                $" ({fieldString}) VALUES ";

            var hasValues = this.Valueset.Count() > 0;
            var values = hasValues ? this.ValueExpressions.Select(v => v.Compile()).ToList() : null;

            var count = 0;
            foreach (var record in records)
            {
                if (count > 0)
                {
                    sb.AppendLine(";");
                }

                sb.Append(prefix);
                var setValues = this.CreateFieldValues(fields, record, props, values);
                sb.Append("(" + string.Join(",", setValues) + ")");

                count++;
            }

            if (count > 1)
            {
                sb.AppendLine(";");
                sb.AppendLine("GO;");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Builds the sql-server friendly string.
        /// </summary>
        /// <returns>System.String.</returns>
        private string BuildMerge()
        {
            var sb = new StringBuilder();
            var props = typeof(TRecord).GetProperties();

            List<string> matchFields = new List<string>();
            if (!string.IsNullOrEmpty(this.MatchKeyStr1))
            {
                matchFields.Add(this.MatchKeyStr1);
            }

            sb.Append(
                this.Command +
                    $" INTO {this.MergeIntoTableName} AS Target USING "
                +
                    ((this.TableNames != null && this.TableNames.Length > 0) ? this.TableNames[0] : this.TableName)
                +
                    $" AS Source ON ");

            if (this.MatchKeyStr1 != null)
            {
                sb.Append($"Source.{this.MatchKeyStr1} = Target.{this.MatchKeyStr1} ");
            }
            else if (this.MatchKey2 != null)
            {
                var matchKeyStr2 = this.ParseExpression(
                    this.Types,
                    this.MatchKey2.Body,
                    true,
                    parameters: this.MatchKey2.Parameters,
                    tableNames: new[] { "Source", "Target" });

                sb.Append(matchKeyStr2 + " ");
            }

            if (this.WhenMatchedExpression != null)
            {
                var ssWhenMatched = new SimpleSqlBuilder<TRecord>().From("Source", "Target").UseTableNames();
                this.WhenMatchedExpression.Compile().Invoke(ssWhenMatched);
                var ssWhenMatchedBuild = ssWhenMatched.Build();
                sb.Append("WHEN MATCHED THEN ");
                sb.Append(ssWhenMatchedBuild);
                sb.Append(" ");
            }

            if (this.WhenNotMatchedExpression != null)
            {
                var ssNotWhenMatched = new SimpleSqlBuilder<TRecord>().From("Source", "Target").UseTableNames();
                this.WhenNotMatchedExpression.Compile().Invoke(ssNotWhenMatched);
                var ssWhenNotMatchedBuild = ssNotWhenMatched.Build();
                sb.Append("WHEN NOT MATCHED THEN ");
                sb.Append(ssWhenNotMatchedBuild);
                sb.Append(" ");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Creates the field values.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="record">The record.</param>
        /// <param name="props">The props.</param>
        /// <param name="values">The values.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        private List<string> CreateFieldValues(
            IEnumerable<string> fields,
            TRecord record,
            PropertyInfo[] props,
            List<Func<TRecord, object>> values = null)
        {
            var list = new List<string>();
            var fieldIndex = 0;
            foreach (var field in fields)
            {
                list.Add($"{this.GetAsSqlValue(record, field, props, values == null ? null : values[fieldIndex])}");
                fieldIndex++;
            }

            return list;
        }

        /// <summary>
        /// Creates the field equals values.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="record">The record.</param>
        /// <param name="props">The props.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        private List<string> CreateFieldEqualsValues(IEnumerable<string> fields, TRecord record, PropertyInfo[] props)
        {
            var list = new List<string>();
            foreach (var field in fields)
            {
                list.Add($"{field} = {this.GetAsSqlValue(record, field, props)}");
            }

            return list;
        }

        /// <summary>
        /// Creates the field equals expression values.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="values">The values.</param>
        /// <param name="tableNames">The table names.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        private List<string> CreateFieldEqualsExpressionValues(
            IEnumerable<string> fields,
            List<string> values = null,
            string[] tableNames = null)
        {
            var list = new List<string>();
            var fieldIndex = 0;
            var hasValues = values != null && values.Any();
            foreach (var field in fields)
            {
                var value = hasValues ? values[fieldIndex] : field;
                if (fields.Contains(value))
                {
                    value = tableNames[0] + "." + value;
                }

                list.Add($"{tableNames[1]}.{field} = {value}");
                fieldIndex++;
            }

            return list;
        }

        /// <summary>
        /// Creates the field expression values.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="values">The values.</param>
        /// <param name="tableNames">The table names.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        private List<string> CreateFieldExpressionValues(
            IEnumerable<string> fields,
            List<string> values = null,
            string[] tableNames = null)
        {
            var list = new List<string>();
            var fieldIndex = 0;
            var hasValues = values != null && values.Any();
            foreach (var field in fields)
            {
                var value = hasValues ? values[fieldIndex] : field;
                if (fields.Contains(value))
                {
                    value = tableNames[0] + "." + value;
                }

                list.Add($"{value}");
                fieldIndex++;
            }

            return list;
        }

        /// <summary>
        /// Executes the specified records.
        /// </summary>
        /// <param name="source">The records.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<TRecord> Execute(IEnumerable<TRecord> source)
        {
            switch (this.Command)
            {
                case "SELECT":
                    return this.ExecuteSelect(source);
                default:
                    throw new NotImplementedException($"{this.Command} is not supported as a query.");
            }
        }

        /// <summary>
        /// Executes the specified source.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="overrideSource">The override source.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int ExecuteNonQuery(List<TRecord> target, IEnumerable<TRecord> overrideSource = null)
        {
            switch (this.Command)
            {
                case "UPDATE":
                    return this.ExecuteUpdate(target, overrideSource);
                case "INSERT":
                    return this.ExecuteInsert(target, overrideSource);
                case "MERGE":
                    return this.ExecuteMerge(target, overrideSource);
                case "DELETE":
                    return this.ExecuteDelete(target);
                default:
                    throw new NotImplementedException($"{this.Command} is not supported as a non-query.");
            }
        }

        /// <summary>
        /// Executes the expressions within this builder upon these records, returning the results.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        private IEnumerable<TRecord> ExecuteSelect(IEnumerable<TRecord> records)
        {
            var result = new List<TRecord>();

            // todo: reform to execute on string fields, not only expressions

            var current = records;
            if (this.WhereExpression != null)
            {
                current = current.Where(this.WhereExpression.Compile());
            }

            if (this.OrderByList.Count > 0)
            {
                for (var i = this.OrderByList.Count - 1; i > -1; i--)
                {
                    var orderBy = this.OrderByList[i];
                    if (orderBy.IsAscending)
                    {
                        current = current.OrderBy(orderBy.Expression.Compile());
                    }
                    else
                    {
                        current = current.OrderByDescending(orderBy.Expression.Compile());
                    }
                }
            }

            if (this.Count < int.MaxValue)
            {
                current = current.Take(this.Count);
            }

            var type = typeof(TRecord);
            var props = type.GetProperties();
            var fields = this.FieldList(this.Fields);

            foreach (var record in current)
            {
                var newRecord = (TRecord)Activator.CreateInstance(type);
                foreach (var field in fields)
                {
                    var prop = props.FirstOrDefault(p => p.Name == field);
                    prop.SetValue(newRecord, prop.GetValue(record));
                }

                result.Add(newRecord);
            }

            return result;
        }

        /// <summary>
        /// Executes the expressions within this builder upon these records, returning the results.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="overrideSource">The override source.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        public int ExecuteUpdate(IEnumerable<TRecord> target, IEnumerable<TRecord> overrideSource = null)
        {
            var result = new List<TRecord>();

            // todo: reform to execute on string fields, not only expressions

            var current = target;
            if (this.WhereExpression != null)
            {
                current = current.Where(this.WhereExpression.Compile());
            }

            var type = typeof(TRecord);
            var props = type.GetProperties();

            var keyProp = props.FirstOrDefault(p => p.Name == this.MatchKeyStr1);

            var hasValues = this.Valueset.Count() > 0;
            var values = hasValues ? this.ValueExpressions.Select(v => v.Compile()).ToList() : null;

            // assumes 1key
            int count = 0;
            foreach (var record in overrideSource ?? this.SourceData)
            {
                var sourceKey = keyProp?.GetValue(record);
                var targets = keyProp == null
                    ? current
                    : current.Where(t => object.Equals(keyProp.GetValue(t), sourceKey)).ToList();

                foreach (var target1 in targets)
                {
                    var fieldIndex = 0;
                    foreach (var field in this.Updates)
                    {
                        var prop = props.FirstOrDefault(p => p.Name == field);
                        var value = hasValues ? values[fieldIndex].Invoke(record) : prop.GetValue(record);
                        prop.SetValue(target1, value);
                        fieldIndex++;
                    }

                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Get a field list, or default to all fields
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        public List<string> FieldList(List<string> list)
        {
            return list != null && list.Count() > 0
                ? list
                : typeof(TRecord).GetProperties().Select(p => p.Name).ToList();
        }

        /// <summary>
        /// Executes the expressions within this builder upon these records, returning the results.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="overrideSource">The override source.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        public int ExecuteInsert(List<TRecord> target, IEnumerable<TRecord> overrideSource = null)
        {
            // todo: reform to execute on string fields, not only expressions

            var type = typeof(TRecord);
            var props = type.GetProperties();

            var hasValues = this.Valueset.Count() > 0;
            var values = hasValues ? this.ValueExpressions.Select(v => v.Compile()).ToList() : null;

            var fields = this.FieldList(this.Inserts);

            var count = 0;
            foreach (var record in overrideSource ?? this.SourceData)
            {
                var newRecord = (TRecord)Activator.CreateInstance(type);
                var fieldIndex = 0;
                foreach (var field in fields)
                {
                    var prop = props.FirstOrDefault(p => p.Name == field);
                    var value = hasValues ? values[fieldIndex].Invoke(record) : prop.GetValue(record);
                    prop.SetValue(newRecord, value);
                    fieldIndex++;
                }

                target.Add(newRecord);
                count++;
            }

            return count;
        }

        /// <summary>
        /// Executes the merge.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.Exception">source record matches multiple targets (record #{index})</exception>
        public int ExecuteMerge(List<TRecord> target, IEnumerable<TRecord> source)
        {
            Func<TRecord, TRecord, bool> match;
            if (this.MatchKeyStr1 != null)
            {
                var prop = typeof(TRecord).GetProperty(this.MatchKeyStr1);
                match = (s, t) => prop.GetValue(s).Equals(prop.GetValue(t));
            }
            else
            {
                match = this.MatchKey2.Compile();
            }

            var whenMatchedSSB = new SimpleSqlBuilder<TRecord>();
            this.WhenMatchedExpression?.Compile().Invoke(whenMatchedSSB);

            var whenNotMatchedSSB = new SimpleSqlBuilder<TRecord>();
            this.WhenNotMatchedExpression?.Compile().Invoke(whenNotMatchedSSB);

            var index = 0;
            var count = 0;
            foreach (var s in source)
            {
                index++;
                var targets = target.Where(t => match(s, t)).ToList();
                if (targets.Count() > 1)
                {
                    throw new Exception($"source record matches multiple targets (record #{index})");
                }

                if (whenMatchedSSB != null && targets.Any())
                {
                    // matched
                    whenMatchedSSB.ExecuteNonQuery(targets, new List<TRecord> { s });
                    count++;
                }
                else if (whenNotMatchedSSB != null && !targets.Any())
                {
                    // not matched
                    whenNotMatchedSSB.ExecuteNonQuery(target, new List<TRecord> { s });
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Executes the delete command against records.
        /// </summary>
        /// <param name="target">The target set of records.</param>
        /// <returns>Count of deleted records.</returns>
        public int ExecuteDelete(List<TRecord> target)
        {
            var current = target;

            if (this.DistinctOnStr != null)
            {
                var distExp = this.DistinctOnExpression.Compile();
                current = current.GroupBy(r => distExp(r)).SelectMany(r => r.Skip(1)).ToList();
            }

            if (this.WhereExpression != null)
            {
                current = current.Where(this.WhereExpression.Compile()).ToList();
            }

            var count = target.Count();

            // issue: if the test records were duplicated, then they are also dups here and will be removed,
            // even the source records.
            target.RemoveAll(t => current.Contains(t));
            return count - target.Count();
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <returns>List&lt;System.String&gt;.</returns>
        private List<string> GetFields()
        {
            return typeof(TRecord).GetProperties().Select(p => p.Name).ToList();
        }

        /// <summary>
        /// Gets as SQL value.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="props">The props.</param>
        /// <param name="valueExpression">The value expression.</param>
        /// <returns>System.String.</returns>
        private string GetAsSqlValue(
            TRecord record,
            string fieldName,
            PropertyInfo[] props,
            Func<TRecord, object> valueExpression = null)
        {
            var prop = props.First(p => p.Name == fieldName);
            var value = valueExpression != null ? valueExpression.Invoke(record) : prop.GetValue(record);
            return SqlHelpers.GetAsSQLValue(value.GetType(), value);
        }
    }
}