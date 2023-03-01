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
        /// Gets or sets the type of the syntax.
        /// </summary>
        public SimpleSqlBuilderType SyntaxType { get; protected set; } = SimpleSqlBuilderType.SQLServer;

        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        /// <value>The types.</value>
        public Type[] Types { get; protected set; }

        /// <summary>
        /// Gets the name (potentially generated if matching previous tables) of the tables, when
        /// multiple.
        /// </summary>
        /// <value>The name of the table.</value>
        public string[] TableNames { get; protected set; }

        /// <summary>
        /// Gets the actual name of the tables, when multiple.
        /// </summary>
        /// <value>The name of the table.</value>
        public string[] TableNamesActual { get; protected set; }

        /// <summary>
        /// As fields
        /// </summary>
        public Dictionary<object, Expression<Func<TRecord, object>>> AsFields
        {
            get;
            protected set;
        } = new Dictionary<object, Expression<Func<TRecord, object>>>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is distinctive (uses DISTINCT).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is distinct; otherwise, <c>false</c>.
        /// </value>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// The table name.
        /// </summary>
        private string tableName;

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName
        {
            get { return _OverrideTableName ?? tableName; }
            protected set { tableName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the override table.
        /// </summary>
        public string _OverrideTableName { get; protected set; }

        /// <summary>
        /// Gets the name of the table to merge into.
        /// </summary>
        /// <value>The name of the table.</value>
        public string MergeIntoTableName { get; protected set; }

        /// <summary>
        /// The fields
        /// </summary>
        private List<string> fields = new List<string>();

        /// <summary>
        /// Gets the field names.
        /// </summary>
        /// <value>The fields.</value>
        public List<string> Fields
        {
            get { return _OverrideFields ?? fields; }
            protected set { fields = value; }
        }

        /// <summary>
        /// Gets or sets the override fields.
        /// </summary>
        public List<string> _OverrideFields { get; protected set; }

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
        /// The command
        /// </summary>
        private string command = "SELECT";

        /// <summary>
        /// Gets the command (only SELECT active currently).
        /// </summary>
        /// <value>The command.</value>
        public string Command
        {
            get { return _OverrideCommand ?? command; }
            protected set { command = value; }
        }

        /// <summary>
        /// Gets or sets the override command.
        /// </summary>
        public string _OverrideCommand { get; protected set; }

        /// <summary>
        /// Gets the where condition string.
        /// </summary>
        /// <value>The where string.</value>
        public List<string> WhereString { get; protected set; } = new List<string>();

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
        public IEnumerable<TRecord> SourceData { get; protected set; } = null;

        /// <summary>
        /// Gets or sets the index of the parameter.
        /// </summary>
        protected int ParamIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        protected Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

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

        public bool IsTracing { get; protected set; } = false;

        public StringBuilder Trace { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        public SimpleSqlBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SimpleSqlBuilder(SimpleSqlBuilderOptions options)
        {
            SetOptions(options);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public SimpleSqlBuilder(string tableName)
        {
            From(tableName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="tableName">Name of the table.</param>
        public SimpleSqlBuilder(SimpleSqlBuilderOptions options, string tableName)
        {
            SetOptions(options);
            From(tableName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="types">The types.</param>
        public SimpleSqlBuilder(SimpleSqlBuilderOptions options, Type[] types)
        {
            SetOptions(options);
            Types = types;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        public SimpleSqlBuilder(Type[] types)
        {
            Types = types;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="tableNames">The table names.</param>
        public SimpleSqlBuilder(params string[] tableNames)
        {
            From(tableNames);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlBuilder{TRecord}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="tableNames">The table names.</param>
        public SimpleSqlBuilder(SimpleSqlBuilderOptions options, params string[] tableNames)
        {
            SetOptions(options);
            From(tableNames);
        }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public SimpleSqlBuilderOptions Options { get; protected set; } = new SimpleSqlBuilderOptions();

        public SimpleSqlBuilder<TRecord> TraceTo(StringBuilder sb)
        {
            Trace = sb;
            IsTracing = true;
            return this;
        }

        /// <summary>
        /// Sets the options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> SetOptions(SimpleSqlBuilderOptions options)
        {
            Options = options;
            if (options.Metadata != null)
            {
                Metadata = options.Metadata;
            }

            return this;
        }

        /// <summary>
        /// Sets an option's value by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> SetOption(string name, object value)
        {
            Metadata[name] = value;
            return this;
        }

        /// <summary>
        /// Sets the syntax type to build with - currently only builds for SQLServer.
        /// </summary>
        /// <param name="syntaxType">Type of the syntax.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> For(SimpleSqlBuilderType syntaxType)
        {
            SyntaxType = syntaxType;
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
            return Metadata.ContainsKey(name) ? (T)Metadata[name] : defaultValue;
        }

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public List<Expression<Func<TRecord, object>>> FieldExpressions
        {
            get;
            protected set;
        } = new List<Expression<Func<TRecord, object>>>();

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
        public List<Expression<Func<TRecord, bool>>> WhereExpression
        {
            get;
            protected set;
        } = new List<Expression<Func<TRecord, bool>>>();

        /// <summary>
        /// Gets the where condition string.
        /// </summary>
        /// <value>The where string.</value>
        public List<string> WhereString2 { get; protected set; } = new List<string>();

        /// <summary>
        /// Gets the where condition string.
        /// </summary>
        /// <value>The where string.</value>
        public List<string> WhereString3 { get; protected set; } = new List<string>();

        /// <summary>
        /// Gets the where condition string.
        /// </summary>
        /// <value>The where string.</value>
        public List<string> WhereString4 { get; protected set; } = new List<string>();

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
        /// Gets or sets the source builder.
        /// </summary>
        public Expression<Action<SimpleSqlBuilder<TRecord>>> SourceBuilder { get; set; }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> From(string tableName)
        {
            TableName = tableName;
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
            TableNames = new string[] { tableNameSource, tableNameTarget };
            return this;
        }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableNames">The table names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> From(string[] tableNames)
        {
            TableNames = tableNames;
            var newTableNames = new List<string>();
            var actualTableNames = new List<string>();
            int index = 1;
            foreach (string tableName in tableNames)
            {
                if (actualTableNames.Contains(tableName))
                {
                    newTableNames.Add(tableName + "__" + index);
                }
                else
                {
                    newTableNames.Add(tableName);
                }

                actualTableNames.Add(tableName);
            }

            TableNames = newTableNames.ToArray();
            TableNamesActual = actualTableNames.ToArray();

            return this;
        }

        /// <summary>
        /// Uses the table names.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> UseTableNames()
        {
            AreTableNamesRequired = true;
            return this;
        }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> MergeInto(string tableName)
        {
            Command = "MERGE";
            MergeIntoTableName = tableName;
            return this;
        }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="isChained">if set to <c>true</c> [is chained].</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> IsChained(bool isChained = true)
        {
            Metadata["Chained"] = isChained;
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
            CheckDowncast("Select");
            return Select(new Expression<Func<TRecord, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select(params Expression<Func<TRecord, object>>[] fields)
        {
            CheckDowncast("Select");
            FieldExpressions.AddRange(fields);
            return Select(FieldExpressions);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select(IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            CheckDowncast("Select");
            var names = fields.Select(field => ParseExpression(Types, field.Body, parameters: field.Parameters))
                .ToList();
            return Select(names);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as a field names (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select(string fieldName)
        {
            return Select(new string[] { fieldName });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select(IEnumerable<string> fieldNames)
        {
            Fields.AddRange(fieldNames);
            return Select();
        }

        /// <summary>
        /// Begins the Select command.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select()
        {
            Command = "SELECT";
            return this;
        }

        /// <summary>
        /// Begins the Delete command.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Delete()
        {
            CheckDowncast("Delete");
            Command = "DELETE";
            return this;
        }

        /// <summary>
        /// Makes this instance distinctive (uses DISTINCT).
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Distinct()
        {
            CheckDowncast("Distinct");
            IsDistinct = true;
            return this;
        }

        /// <summary>
        /// Allows the delete command to delete duplicates based on this field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ExceptDistinctBy(Expression<Func<TRecord, object>> field)
        {
            CheckDowncast("ExceptDistinctBy");
            DistinctOnExpression = field;
            DistinctOnStr = ParseExpression(Types, field.Body, false, parameters: field.Parameters);

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
            Update(records);
            return Update(fields);
        }

        /// <summary>
        /// Updates the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(TRecord record, params Expression<Func<TRecord, object>>[] fields)
        {
            Update(new List<TRecord> { record });
            return Update(fields);
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
            Update(records);
            return Update(fields);
        }

        /// <summary>
        /// Updates the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(TRecord record, IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            Update(new List<TRecord> { record });
            return Update(fields);
        }

        /// <summary>
        /// Updates the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(TRecord record)
        {
            return Update(new List<TRecord> { record });
        }

        /// <summary>
        /// Updates the specified records.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(IEnumerable<TRecord> records)
        {
            SourceData = records;
            return Update();
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as an expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(Expression<Func<TRecord, object>> field)
        {
            return Update(new Expression<Func<TRecord, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(params Expression<Func<TRecord, object>>[] fields)
        {
            UpdateExpressions = fields;
            return Update(UpdateExpressions);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            var names = fields.Select(field => ParseExpression(Types, field.Body, parameters: field.Parameters))
                .ToList();
            return Update(names);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as a field names (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(string fieldName)
        {
            return Update(new string[] { fieldName });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update(IEnumerable<string> fieldNames)
        {
            Updates.AddRange(fieldNames);
            return Update();
        }

        /// <summary>
        /// Selects this instance.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Update()
        {
            CheckDowncast("Update");
            Command = "UPDATE";
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
            Insert(records);
            return Insert(fields);
        }

        /// <summary>
        /// Inserts the specified records.
        /// </summary>
        /// <param name="selectSource">The select source.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(
            Expression<Action<SimpleSqlBuilder<TRecord>>> selectSource,
            IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            Insert(selectSource);
            return Insert(fields);
        }

        /// <summary>
        /// Inserts the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(TRecord record)
        {
            return Insert(new List<TRecord> { record });
        }

        /// <summary>
        /// Inserts the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(TRecord record, IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            Insert(new List<TRecord> { record });
            return Insert(fields);
        }

        /// <summary>
        /// Inserts the specified records.
        /// </summary>
        /// <param name="selectSource">The select source.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(
            Expression<Action<SimpleSqlBuilder<TRecord>>> selectSource,
            params Expression<Func<TRecord, object>>[] fields)
        {
            Insert(selectSource);
            return Insert(fields);
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
            Insert(records);
            return Insert(fields);
        }

        /// <summary>
        /// Inserts the specified records.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(IEnumerable<TRecord> records)
        {
            SourceData = records;
            return Insert();
        }

        /// <summary>
        /// Inserts the specified records.
        /// </summary>
        /// <param name="selectSource">The select source.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(Expression<Action<SimpleSqlBuilder<TRecord>>> selectSource)
        {
            SourceBuilder = selectSource;
            return Insert();
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as an expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(Expression<Func<TRecord, object>> field)
        {
            return Insert(new Expression<Func<TRecord, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(params Expression<Func<TRecord, object>>[] fields)
        {
            InsertExpressions = fields;
            return Insert(InsertExpressions);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as expressions (can build,
        /// execute).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            var names = fields.Select(field => ParseExpression(Types, field.Body, parameters: field.Parameters))
                .ToList();
            return Insert(names);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as a field names (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(string fieldName)
        {
            return Insert(new string[] { fieldName });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert(IEnumerable<string> fieldNames)
        {
            Inserts.AddRange(fieldNames);
            return Insert();
        }

        /// <summary>
        /// Selects this instance.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Insert()
        {
            CheckDowncast("Insert");
            Command = "INSERT";
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
            return Values(new Expression<Func<TRecord, object>>[] { field });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Values(params Expression<Func<TRecord, object>>[] fields)
        {
            ValueExpressions = fields;
            return Values(ValueExpressions);
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
                field => ParseExpression(
                    Types,
                    field.Body,
                    parameters: field.Parameters,
                    tableNames: AreTableNamesRequired ? TableNames : null))
                .ToList();
            return Values(names);
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying a field as a field names (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Values(string fieldName)
        {
            return Values(new string[] { fieldName });
        }

        /// <summary>
        /// Chooses SELECT as the command, specifying the fields as field names (can only build).
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Values(IEnumerable<string> fieldNames)
        {
            CheckDowncast("Values");
            Valueset.AddRange(fieldNames);
            return this;
        }

        /// <summary>
        /// Adds an order-by expression ascending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> OrderBy(Expression<Func<TRecord, object>> field)
        {
            var name = ParseExpression(Types, field.Body, parameters: field.Parameters);
            return OrderBy(name, true, field);
        }

        /// <summary>
        /// Adds an order-by fieldname ascending (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> OrderBy(string fieldName)
        {
            return OrderBy(fieldName, true);
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
            CheckDowncast("OrderBy");
            OrderByList.Clear();
            return ThenBy(fieldName, isAscending, expression);
        }

        /// <summary>
        /// Adds an then-by expression ascending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ThenBy(Expression<Func<TRecord, object>> field)
        {
            var name = ParseExpression(Types, field.Body);
            return ThenBy(name, true, field);
        }

        /// <summary>
        /// Adds an then-by fieldname ascending (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ThenBy(string fieldName)
        {
            return ThenBy(fieldName, true);
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
            CheckDowncast("ThenBy");
            OrderByList
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
            CheckDowncast("OrderByDescending");
            var name = ParseExpression(Types, field.Body, parameters: field.Parameters);
            return OrderBy(name, false, field);
        }

        /// <summary>
        /// Adds an order-by fieldname descending (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> OrderByDescending(string fieldName)
        {
            return OrderBy(fieldName, false);
        }

        /// <summary>
        /// Adds an then-by expression descending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ThenByDescending(Expression<Func<TRecord, object>> field)
        {
            CheckDowncast("ThenByDescending");
            var name = ParseExpression(Types, field.Body, parameters: field.Parameters);
            return ThenBy(name, false, field);
        }

        /// <summary>
        /// Adds an then-by fieldname descending (can only build).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ThenByDescending(string fieldName)
        {
            return ThenBy(fieldName, false);
        }

        /// <summary>
        /// Sets the where condition as an expression (can build, execute).
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Where(Expression<Func<TRecord, bool>> condition)
        {
            WhereCheck();
            WhereExpression.Add(condition);
            WhereString
                .Add(ParseExpression(Types, condition.Body, true, parameters: condition.Parameters));
            return this;
        }

        /// <summary>
        /// Checks if the Where clause is on an INSERT command, and throws.
        /// </summary>
        /// <exception cref="GaleForceCore.Builders.IncompatibleClauseException">Where clause is not available on an INSERT command</exception>
        /// <exception cref="T:GaleForceCore.Builders.IncompatibleClauseException">Where clause is not available on an INSERT command</exception>
        protected void WhereCheck()
        {
            if (Command == "INSERT")
            {
                throw new IncompatibleClauseException("Where clause is not available on an INSERT command");
            }
        }

        /// <summary>
        /// Clears the accumulative where clauses.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> ClearWhere()
        {
            WhereExpression.Clear();
            WhereString.Clear();
            return this;
        }

        /// <summary>
        /// Optionally add a clause when a condition is true.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="trueClause">The clause to eval if true.</param>
        /// <param name="falseClause">The clause to eval if false.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> If(
            bool condition,
            Action<SimpleSqlBuilder<TRecord>> trueClause = null,
            Action<SimpleSqlBuilder<TRecord>> falseClause = null)
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
        /// Matches the specified match key.
        /// </summary>
        /// <param name="matchKey">The match key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Match(Expression<Func<TRecord, object>> matchKey)
        {
            MatchKey1 = matchKey;
            MatchKeyStr1 = ParseExpression(Types, matchKey.Body, parameters: matchKey.Parameters);
            return this;
        }

        /// <summary>
        /// Matches the specified match key.
        /// </summary>
        /// <param name="matchKey">The match key.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Match(Expression<Func<TRecord, TRecord, bool>> matchKey)
        {
            MatchKey2 = matchKey;
            MatchKeyStr2 = ParseExpression(Types, matchKey.Body, true, parameters: matchKey.Parameters);
            return this;
        }

        /// <summary>
        /// Sets the maximum count for returned records.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Take(int count)
        {
            Count = count;
            return this;
        }

        /// <summary>
        /// Whens the matched.
        /// </summary>
        /// <param name="ssBuilderMatched">The ss builder matched.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> WhenMatched(Expression<Action<SimpleSqlBuilder<TRecord>>> ssBuilderMatched)
        {
            WhenMatchedExpression = ssBuilderMatched;
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
            WhenNotMatchedExpression = ssBuilderNotMatched;
            return this;
        }

        /// <summary>
        /// Overrides the name of the table.
        /// </summary>
        /// <param name="tempTableName">Name of the temporary table.</param>
        public void OverrideTableName(string tempTableName)
        {
            _OverrideTableName = tempTableName;
        }

        /// <summary>
        /// Overrides the command.
        /// </summary>
        /// <param name="tempCommand">The temporary command.</param>
        public void OverrideCommand(string tempCommand)
        {
            _OverrideCommand = tempCommand;
        }

        /// <summary>
        /// Overrides the fields.
        /// </summary>
        /// <param name="fields">The fields.</param>
        public void OverrideFields(IEnumerable<string> fields)
        {
            _OverrideFields = fields == null ? null : fields.ToList();
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
        /// <exception cref="GaleForceCore.Builders.UnsupportedOperandException">null</exception>
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

                var left = ParseExpression(
                    types,
                    bExp.Left,
                    isCondition && !isCompare,
                    parameters,
                    tableNames: tableNames,
                    evalInfo: evalInfo);
                var right = ParseExpression(
                    types,
                    bExp.Right,
                    isCondition && !isCompare,
                    parameters,
                    tableNames: tableNames,
                    evalInfo: evalInfo);

                var prefixLeft = "(";

                // if (bExp.Left.NodeType == ExpressionType.Not)
                // {
                // prefixLeft += "NOT ";
                // }

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
                            if (bExp.Left.Type.Name == "String" && bExp.Right.Type.Name != "String")
                            {
                                addRight = $"STR({right.Trim()}) ";
                            }
                            else if (bExp.Left.Type.Name != "String" && bExp.Right.Type.Name == "String")
                            {
                                addLeft = $"STR({left.Trim()}) ";
                            }
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
                    case ExpressionType.Modulo:
                        addCenter = " % ";
                        break;
                    default:
                        throw new UnsupportedOperandException(op.ToString() + " is an unknown operand", null);
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
                var wvalue2 = WrappedValue(value, bExp, typeof(BinaryExpression));
                var parent = evalInfo?.Register(bExp, typeof(BinaryExpression), wvalue2);
                evalInfo?.RegisterChildren(parent, bExp.Left, bExp.Right);

                return wvalue2;
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

                    var matchingType = GetMatchingType(types, declaringType);
                    if (matchingType != null)
                    {
                        var ignoreTableName = 
                            (hideSourceTable || (tableNames == null && (types == null || types.Length < 2)));
                        var prefix = ignoreTableName
                            ? string.Empty
                            : (GetMatchingTableName(
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
                        var wvalue2 = WrappedValue(value, operand, typeof(MemberExpression));
                        evalInfo?.Register(operand, typeof(MemberExpression), wvalue2);
                        return wvalue2;
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
                                var wvalue2 = WrappedValue(sqlValue, operand, typeof(MemberExpression));
                                evalInfo?.Register(operand, typeof(MemberExpression), wvalue2);
                                return wvalue2;
                            }
                            else if (pe.Member is PropertyInfo)
                            {
                                object container = ce.Value;
                                object value = ((PropertyInfo)pe.Member).GetValue(container);
                                var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                                var wvalue2 = WrappedValue(sqlValue, operand, typeof(MemberExpression));
                                evalInfo?.Register(operand, typeof(MemberExpression), wvalue2);
                                return wvalue2;
                            }
                        }
                        else if (operand.NodeType == ExpressionType.MemberAccess)
                        {
                            var value = ParseExpression(
                                types,
                                operand,
                                isCondition,
                                parameters,
                                tableNames: tableNames,
                                evalInfo: evalInfo);

                            var wvalue2 = WrappedValue(value, operand, typeof(MemberExpression));
                            evalInfo?.Register(operand, typeof(MemberExpression), wvalue2);
                            return wvalue2;
                        }

                        var wvalue = WrappedValue(pe.Member.Name, pe, typeof(MemberExpression));
                        evalInfo?.Register(pe, typeof(MemberExpression), wvalue);
                        return wvalue;
                    }
                }
                else
                {
                    var value = ParseExpression(
                        types,
                        operand,
                        isCondition,
                        parameters,
                        tableNames: tableNames,
                        evalInfo: evalInfo);

                    if (exp.NodeType == ExpressionType.Not)
                    {
                        value = "NOT " + value;
                    }

                    var wvalue = WrappedValue(value, operand, typeof(MemberExpression));
                    evalInfo?.Register(operand, typeof(MemberExpression), wvalue);
                    return wvalue;
                }
            }
            else if (exp is MemberExpression)
            {
                if (exp.ToString().StartsWith("value("))
                {
                    var value = Expression.Lambda(exp).Compile().DynamicInvoke();
                    if (IsTracing)
                    {
                        Trace.AppendLine("(eval:" + exp.ToString() + "=" + value.ToString());
                    }

                    var xValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                    var wvalue = WrappedValue(xValue, exp, typeof(ConstantExpression));
                    evalInfo?.Register(exp, typeof(ConstantExpression), wvalue);
                    return wvalue;
                }

                var pe = exp as MemberExpression;
                evalInfo?.Register(pe, typeof(MemberExpression));

                var ce = pe.Expression as ConstantExpression;
                var suffix = string.Empty;

                var parmName = pe.Expression is ParameterExpression ? (pe.Expression as ParameterExpression).Name : null;
                var declaringType = pe.Expression.Type.Name;
                var matchingType = GetMatchingType(types, declaringType);
                var ignoreTableName = 
                    (hideSourceTable ||
                    (tableNames == null && (types == null || types.Length < 2 || matchingType == null)));
                var prefix = ignoreTableName
                    ? string.Empty
                    : (GetMatchingTableName(
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
                        var wvalue3 = WrappedValue(sqlValue, pe, typeof(MemberExpression));
                        evalInfo?.Register(pe, typeof(MemberExpression), wvalue3);
                        return wvalue3;
                    }
                    else if (pe.Member is PropertyInfo)
                    {
                        object container = ce.Value;
                        object value = ((PropertyInfo)pe.Member).GetValue(container);
                        var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                        var wvalue3 = WrappedValue(sqlValue, pe, typeof(MemberExpression));
                        evalInfo?.Register(pe, typeof(MemberExpression), wvalue3);
                        return wvalue3;
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

                var value2 = prefix + FinalField(memberName) + suffix;
                var wvalue2 = WrappedValue(value2, pe, typeof(MemberExpression));
                evalInfo?.Register(pe, typeof(MemberExpression), wvalue2);
                return wvalue2;
            }
            else if (exp is ConstantExpression)
            {
                var cexp = (exp as ConstantExpression);
                var value = cexp.Value;
                var valueStr = value == null ? "NULL" : value.ToString();
                if (value is string || value is DateTime)
                {
                    valueStr = "'" + SqlHelpers.CleanString(valueStr) + "'";
                }
                else if (value is bool)
                {
                    valueStr = ((bool) value) ? "1" : "0";
                }

                var wvalue2 = WrappedValue(valueStr, exp, typeof(ConstantExpression));
                evalInfo?.Register(exp, typeof(ConstantExpression), wvalue2);

                return wvalue2;
            }
            else if (exp is ConditionalExpression)
            {
                var ce = exp as ConditionalExpression;
                var test = ce.Test;
                var ei = evalInfo ?? new EvalInfo();
                var testExp = ParseExpression(types, test, parameters: parameters, tableNames: tableNames, evalInfo: ei);

                if (this.GetOption<bool>("EarlyConditionalEval", true))
                {
                    if (ei.HasOnlyConstants(test))
                    {
                        var testResult = (bool) Expression.Lambda(test).Compile().DynamicInvoke();
                        testExp = ParseExpression(
                            types,
                            testResult ? ce.IfTrue : ce.IfFalse,
                            tableNames: tableNames,
                            evalInfo: ei,
                            hideSourceTable: hideSourceTable,
                            parameters: parameters,
                            isCondition: true);

                        if (IsTracing)
                        {
                            Trace
                                .AppendLine(
                                    "(eval:" +
                                        test.ToString() +
                                        "=" +
                                        testResult.ToString() +
                                        " -> " +
                                        testExp.ToString());
                        }

                        var wvalue2 = WrappedValue(testExp, ce, typeof(ConstantExpression));
                        evalInfo?.Register(ce, typeof(ConstantExpression), wvalue2);
                        return wvalue2;
                    }
                }

                var expTrue = ParseExpression(
                    types,
                    ce.IfTrue,
                    tableNames: tableNames,
                    hideSourceTable: hideSourceTable,
                    parameters: parameters,
                    evalInfo: ei,
                    isCondition: true);

                var expFalse = ParseExpression(
                    types,
                    ce.IfFalse,
                    tableNames: tableNames,
                    hideSourceTable: hideSourceTable,
                    parameters: parameters,
                    evalInfo: ei,
                    isCondition: true);

                var value = $"1 = IIF({testExp},IIF({expTrue},1,0),IIF({expFalse},1,0))";
                var wvalue = WrappedValue(value, ce, typeof(ConstantExpression));

                var parent = evalInfo?.Register(ce, typeof(ConstantExpression), wvalue);
                evalInfo?.RegisterChildren(parent, ce.IfTrue, ce.IfFalse);

                return wvalue;
            }
            else if (exp is MethodCallExpression)
            {
                var me = exp as MethodCallExpression;

                var meMethodName = me.Method.Name;
                var meContainingType = me.Method.DeclaringType.Name;
                var meFullTypeName = me.Method.DeclaringType.FullName;

                // special case methods
                // todo: make sure this only acts on fields from the type, not others
                if (meContainingType == "String" || meFullTypeName == "GaleForceCore.Helpers.StringHelper")
                {
                    string origValue = null;
                    int? origValueInt = null;
                    string subValue = null;
                    string obj;
                    EvalNode parent = null;

                    string[] stringMethods = new string[]
                    {
                        "Contains",
                        "StartsWith",
                        "EndsWith",
                        "IsNullOrEmpty",
                        "IsNullOrWhiteSpace",
                        "Trim",
                        "Equals",
                        "IndexOf",
                        "CHARINDEX",
                        "ToLower",
                        "ToUpper",
                        "TrimStart",
                        "TrimEnd",
                        "Substring",
                        "SUBSTRING"
                    };

                    var isStringContainer = stringMethods.Contains(meMethodName);
                    var isQuoted = false;

                    if (isStringContainer)
                    {
                        var argsAreShifted = meFullTypeName == "GaleForceCore.Helpers.StringHelper";

                        if (me.Arguments.Count() > (argsAreShifted ? 1 : 0))
                        {
                            var argExp = argsAreShifted ? me.Arguments[1] : me.Arguments[0];
                            origValue = 
                                ParseExpression(
                                types,
                                argExp,
                                parameters: parameters,
                                tableNames: tableNames,
                                evalInfo: evalInfo);

                            subValue = RemoveOuterQuotes(origValue);

                            if (argExp.Type.Name == "Int32" && argExp.NodeType == ExpressionType.Constant)
                            {
                                origValueInt = int.Parse(origValue);
                            }

                            isQuoted = subValue != origValue;
                        }

                        obj = me.Object == null && !argsAreShifted
                            ? null
                            : ParseExpression(
                                types,
                                argsAreShifted ? me.Arguments[0] : me.Object,
                                parameters: parameters,
                                tableNames: tableNames);

                        string value = null;
                        switch (meMethodName)
                        {
                            case "Contains":
                                if (isQuoted)
                                {
                                    value = $"{obj} LIKE '%{subValue}%'";
                                }
                                else
                                {
                                    value = $"{obj} LIKE '%'+{origValue}+'%'";
                                }
                                break;
                            case "StartsWith":
                                if (isQuoted)
                                {
                                    value = $"{obj} LIKE '{subValue}%'";
                                }
                                else
                                {
                                    value = $"{obj} LIKE {origValue}+'%'";
                                }
                                break;
                            case "EndsWith":
                                if (isQuoted)
                                {
                                    value = $"{obj} LIKE '%{subValue}'";
                                }
                                else
                                {
                                    value = $"{obj} LIKE '%'+{origValue}";
                                }
                                break;
                            case "IsNullOrEmpty":
                                value = $"ISNULL({origValue}, '') = ''";
                                break;
                            case "IsNullOrWhiteSpace":
                                value = $"TRIM(ISNULL({origValue}, '')) = ''";
                                break;
                            case "Trim":
                                value = $"TRIM({origValue})";
                                break;
                            case "TrimStart":
                                value = $"LTRIM({origValue})";
                                break;
                            case "TrimEnd":
                                value = $"RTRIM({origValue})";
                                break;
                            case "Equals":
                                value = $"{origValue} = {obj}";
                                break;
                            case "IndexOf":
                                value = $"(CHARINDEX({origValue},{obj})-1)";
                                break;
                            case "CHARINDEX":
                                value = $"CHARINDEX({origValue},{obj})";
                                break;
                            case "ToLower":
                                value = $"LOWER({obj})";
                                break;
                            case "ToUpper":
                                value = $"UPPER({obj})";
                                break;
                            case "Substring":
                            case "SUBSTRING":
                                var offset = meMethodName == "Substring" ? 1 : 0;
                                var arg2 = me.Arguments.Count() > (argsAreShifted ? 2 : 1)
                                    ? "," +
                                        ParseExpression(
                                            types,
                                            argsAreShifted ? me.Arguments[2] : me.Arguments[1],
                                            parameters: parameters,
                                            tableNames: tableNames,
                                            evalInfo: evalInfo)
                                    : $",LEN({obj})";

                                var parm2 = origValueInt.HasValue
                                    ? (origValueInt + offset).ToString()
                                    : (origValue + (offset == 1 ? "+1" : string.Empty));
                                value = $"SUBSTRING({obj},{parm2}{arg2})";

                                break;
                        }

                        var wvalue = WrappedValue(value, me, typeof(MethodCallExpression));

                        parent = evalInfo?.Register(me, typeof(MethodCallExpression), value);
                        evalInfo?.RegisterChildren(parent, me.Arguments[0], me.Object);
                        return value;
                    }
                }
                else if ((meContainingType.StartsWith("List") || meContainingType.StartsWith("Enumerable")) &&
                    meMethodName == "Contains")
                {
                    var subValue = RemoveOuterQuotes(
                        ParseExpression(
                            types,
                            me.Arguments[0],
                            parameters: parameters,
                            tableNames: tableNames,
                            evalInfo: evalInfo));
                    var obj = ParseExpression(
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

                    var wvalue = WrappedValue(value, me, typeof(MethodCallExpression));
                    var parent = evalInfo?.Register(me, typeof(MethodCallExpression), wvalue);
                    evalInfo?.RegisterChildren(parent, me.Arguments[0], me.Object ?? me.Arguments[1]);

                    return wvalue;
                }

                try
                {
                    object value = Expression.Lambda(me).Compile().DynamicInvoke();

                    if (IsTracing)
                    {
                        Trace.AppendLine("(eval:" + me.ToString() + "=" + value.ToString());
                    }

                    var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                    var wvalue = WrappedValue(sqlValue, me, typeof(MethodCallExpression));
                    evalInfo?.Register(me, typeof(MethodCallExpression), wvalue);
                    return wvalue;
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
        /// Wrappeds the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="expType">Type of the exp.</param>
        /// <returns>System.String.</returns>
        private string WrappedValue(string value, Expression exp, Type expType)
        {
            if (Options.UseParameters)
            {
                if (exp.Type.Name == "String" && expType.Name == "ConstantExpression" && value.Length > 0)
                {
                    Parameters["Param" + (++ParamIndex)] = value;
                    return "@Param" + ParamIndex;
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
        public Dictionary<string, object> GetParameters()
        {
            return Parameters.ToDictionary(x => x.Key, x => WithoutQuotes(x.Value));
        }

        /// <summary>
        /// Removes quotes from quoted string in value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Object.</returns>
        private object WithoutQuotes(object value)
        {
            var str = value as string;
            if (!string.IsNullOrEmpty(str) && str.Length > 1 && str.StartsWith("'") && str.EndsWith("'"))
            {
                return str.Substring(1, str.Length - 2);
            }

            return value;
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

            tableNames = tableNames ?? TableNames;

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
                : Array.IndexOf(Types.Select(t => t.Name).ToArray(), typeName);
            tableName = tableNames != null && Types != null && Types.Length > 0 ? tableNames[index] : typeName;

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
        /// <param name="declareParamsIfExist">if set to <c>true</c> [declare parameters if exist].</param>
        /// <returns>System.String.</returns>
        public string Build(bool declareParamsIfExist = false)
        {
            switch (Command)
            {
                case "SELECT":
                    return AddParams(BuildSelect(), declareParamsIfExist);
                case "UPDATE":
                    return AddParams(BuildUpdate(), declareParamsIfExist);
                case "INSERT":
                    return AddParams(BuildInsert(), declareParamsIfExist);
                case "DELETE":
                    return AddParams(BuildDelete(), declareParamsIfExist);
                case "MERGE":
                    return AddParams(BuildMerge(), declareParamsIfExist);
            }

            return null;
        }

        /// <summary>
        /// Adds the parameters.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="declareParamsIfExist">if set to <c>true</c> [declare parameters if exist].</param>
        /// <returns>System.String.</returns>
        private string AddParams(string cmd, bool declareParamsIfExist = false)
        {
            if (!declareParamsIfExist || !Options.UseParameters || Parameters.Count() == 0)
            {
                return cmd;
            }

            var sb = new StringBuilder();
            foreach (var kv in Parameters)
            {
                var value = kv.Value.ToString();
                var len = value.StartsWith("'") && value.EndsWith("'") ? (value.Length - 2).ToString() : "MAX";
                sb.AppendLine($"DECLARE @{kv.Key} VARCHAR({len})");
            }

            foreach (var kv in Parameters)
            {
                sb.AppendLine($"SET @{kv.Key} = {kv.Value}");
            }

            sb.AppendLine();
            sb.Append(cmd);
            return sb.ToString();
        }

        /// <summary>
        /// Builds the sql-server friendly string for DELETE.
        /// </summary>
        /// <returns>System.String.</returns>
        private string BuildDelete()
        {
            var sb = new StringBuilder();

            var tableName = TableNames != null && TableNames.Length > 0 ? TableNames[0] : TableName;

            var updatedTableName = tableName;
            if (DistinctOnStr != null)
            {
                var where = JoinedWhereString().Trim();
                if (!string.IsNullOrEmpty(where))
                {
                    where = " " + where;
                }

                sb.Append(

                    // with TokensCTE AS (SELECT ocvId, row_number() over (partition by ocvId order by ocvId) as Temp from Tokens) select * FROM TokensCTE WHERE Temp > 1
                    $"with {tableName}CTE AS (SELECT {DistinctOnStr}, row_number() over (partition by {DistinctOnStr} order by {DistinctOnStr}) as Temp from {tableName}{where}) ");
                updatedTableName = $"{tableName}CTE";
            }

            sb.Append(Command);
            sb.Append(" ");

            sb.Append($"FROM {updatedTableName} ");

            InjectInnerClauses(sb);

            if (DistinctOnStr != null)
            {
                sb.Append("where Temp > 1");
            }
            else
            {
                sb.Append(JoinedWhereString());
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Joins the where strings.
        /// </summary>
        /// <param name="addWhere">The add where.</param>
        /// <param name="prefixWhereSet">The prefix where set.</param>
        /// <returns>System.String.</returns>
        private string JoinedWhereString(string addWhere = null, List<string> prefixWhereSet = null)
        {
            var allwheres = new List<string>();
            if (prefixWhereSet != null)
            {
                allwheres.AddRange(prefixWhereSet);
            }

            allwheres.AddRange(WhereString);
            allwheres.AddRange(WhereString2);
            allwheres.AddRange(WhereString3);
            allwheres.AddRange(WhereString4);
            if (addWhere != null)
            {
                allwheres.Add(addWhere);
            }

            var wheres = allwheres.Where(w => !string.IsNullOrEmpty(w)).ToList();

            if (wheres.Count > 0)
            {
                return $"WHERE {string.Join(" AND ",wheres)} ";
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the final field choice.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>System.String.</returns>
        private string FinalField(string field)
        {
            // TODO: Allow for  attributed replacements - and recognize keywords.

            var lcField = field.ToLowerInvariant();
            if (lcField == "user" || lcField == "from")
            {
                return $"[{field}]";
            }

            return field;
        }

        /// <summary>
        /// Returns the final field choice.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        private List<string> FinalFields(List<string> fields)
        {
            return fields.Select(f => FinalField(f)).ToList();
        }

        /// <summary>
        /// Builds the sql-server friendly string.
        /// </summary>
        /// <returns>System.String.</returns>
        private string BuildSelect()
        {
            var sb = new StringBuilder();
            sb.Append(Command);
            sb.Append(" ");

            if (Count < int.MaxValue)
            {
                sb.Append($"TOP {Count} ");
            }

            if (IsDistinct)
            {
                sb.Append("DISTINCT ");
            }

            if (Fields.Count > 0)
            {
                sb.Append(string.Join(",", FinalFields(Fields)));
            }
            else
            {
                sb.Append("*");
            }

            if (TableNames != null && TableNames.Length > 0)
            {
                sb.Append($" FROM {TableNames[0]} ");
            }
            else
            {
                sb.Append($" FROM {TableName} ");
            }

            InjectInnerClauses(sb);

            sb.Append(JoinedWhereString());

            InjectOrderByClauses(sb);

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Injects the order by clauses.
        /// </summary>
        /// <param name="sb">The sb.</param>
        public virtual void InjectOrderByClauses(StringBuilder sb)
        {
            if (OrderByList.Count > 0)
            {
                sb.Append("ORDER BY ");
                var comma = false;
                foreach (var item in OrderByList)
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

        /// <summary>
        /// Gets the non ignore properties.
        /// </summary>
        /// <typeparam name="TRecordType">The type of the t record type.</typeparam>
        /// <returns>PropertyInfo[].</returns>
        public PropertyInfo[] GetNonIgnoreProperties<TRecordType>()
        {
            return typeof(TRecordType).GetProperties()
                .Where(p => !p.GetCustomAttributes(typeof(IgnoreFieldAttribute), true).Any())
                .ToArray();
        }

        /// <summary>
        /// Builds the sql-server friendly string.
        /// </summary>
        /// <returns>System.String.</returns>
        private string BuildUpdate()
        {
            var records = SourceData;
            if (records != null && records.Count() == 0)
            {
                // no data to insert
                return string.Empty;
            }

            var sb = new StringBuilder();
            var props = GetNonIgnoreProperties<TRecord>();

            List<string> matchFields = new List<string>();
            if (!string.IsNullOrEmpty(MatchKeyStr1))
            {
                matchFields.Add(MatchKeyStr1);
            }

            var fields = Updates;
            if (fields.Count() == 0)
            {
                // get all
                fields = GetFields();
            }

            bool isChained = Metadata.ContainsKey("Chained") && (bool) Metadata["Chained"];

            if (records == null)
            {
                // this is an update segment for an outside instruction, do field to field or field to value
                var setValues = CreateFieldEqualsExpressionValues(fields, Valueset, TableNames);
                sb.Append(Command);

                if (!isChained)
                {
                    if (TableNames != null && TableNames.Length > 0)
                    {
                        sb.Append($" {TableNames[0]}");
                    }
                    else
                    {
                        sb.Append($" {TableName}");
                    }
                }

                sb.Append(" SET ");
                sb.Append(string.Join(", ", setValues));

                var joinedWhere = JoinedWhereString();
                sb.Append(!string.IsNullOrEmpty(joinedWhere) ? $" {joinedWhere.Trim()}" : string.Empty);

                return sb.ToString().Trim();
            }

            var count = 0;
            foreach (var record in records)
            {
                if (count > 0)
                {
                    sb.AppendLine(";");
                }

                sb.Append(Command);

                if (TableNames != null && TableNames.Length > 0)
                {
                    sb.Append($" {TableNames[0]} ");
                }
                else
                {
                    sb.Append($" {TableName} ");
                }

                sb.Append("SET ");

                var setValues = CreateFieldEqualsValues(fields, record, props);
                sb.Append(string.Join(", ", setValues));
                var joinedWhere = JoinedWhereString(
                    prefixWhereSet: matchFields.Count() > 0 ? CreateFieldEqualsValues(matchFields, record, props) : null);
                sb.Append(!string.IsNullOrEmpty(joinedWhere) ? $" {joinedWhere.Trim()}" : string.Empty);

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
        /// <exception cref="GaleForceCore.Logger.StageItem.Exception">
        /// The select list for the INSERT statement contains more items than the insert list. The
        /// number of SELECT values must match the number of INSERT columns.
        /// </exception>
        private string BuildInsert()
        {
            var records = SourceData;
            if (records != null && records.Count() == 0)
            {
                // no data to insert
                return string.Empty;
            }

            var sb = new StringBuilder();
            var props = GetNonIgnoreProperties<TRecord>();

            var fields = Inserts;
            if (fields.Count() == 0)
            {
                // get all
                fields = GetFields();
            }

            var fieldString = string.Join(",", fields);

            bool isChained = Metadata.ContainsKey("Chained") && (bool) Metadata["Chained"];

            if (records == null)
            {
                // todo: standalone, non chained insert values to new record

                sb.Append(Command);

                if (!isChained)
                {
                    if (TableNames != null && TableNames.Length > 0)
                    {
                        sb.Append($" INTO {TableNames[0]}");
                    }
                    else
                    {
                        sb.Append($" INTO {TableName}");
                    }
                }

                if (SourceBuilder != null)
                {
                    var ssSource = new SimpleSqlBuilder<TRecord>().IsChained();
                    SourceBuilder.Compile().Invoke(ssSource);
                    var ssSourceBuild = ssSource.Build();

                    var ssSourceFieldList = ssSource.FieldList(ssSource.Fields);
                    if (ssSourceFieldList.Count() > fields.Count())
                    {
                        throw new Exception(
                            "The select list for the INSERT statement contains more items than the insert list. The number of SELECT values must match the number of INSERT columns.");
                    }

                    if (ssSource.Fields.Any())
                    {
                        fieldString = string.Join(",", ssSource.Fields);
                        sb.Append($" ({fieldString}) ");
                    }
                    else
                    {
                        sb.Append(" ");
                    }

                    sb.Append(ssSourceBuild);

                    return sb.ToString().Trim();
                }
                else
                {
                    sb.Append($" ({fieldString}) VALUES ");

                    // this is an update segment for an outside instruction, do field to field or field to value
                    var setValues = CreateFieldExpressionValues(fields, Valueset, TableNames);
                    sb.Append("(");
                    sb.Append(string.Join(", ", setValues));
                    sb.Append(")");
                    return sb.ToString().Trim();
                }
            }

            var prefix = Command +
                " INTO " +
                ((TableNames != null && TableNames.Length > 0) ? TableNames[0] : TableName) +
                $" ({fieldString}) VALUES ";

            var hasValues = Valueset.Count() > 0;
            var values = hasValues ? ValueExpressions.Select(v => v.Compile()).ToList() : null;

            var count = 0;
            foreach (var record in records)
            {
                if (count > 0)
                {
                    sb.AppendLine(";");
                }

                sb.Append(prefix);
                var setValues = CreateFieldValues(fields, record, props, values);
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
            var props = GetNonIgnoreProperties<TRecord>();

            List<string> matchFields = new List<string>();
            if (!string.IsNullOrEmpty(MatchKeyStr1))
            {
                matchFields.Add(MatchKeyStr1);
            }

            sb.Append(
                Command +
                    $" INTO {MergeIntoTableName} AS Target USING " +
                    ((TableNames != null && TableNames.Length > 0) ? TableNames[0] : TableName) +
                    $" AS Source ON ");

            if (MatchKeyStr1 != null)
            {
                sb.Append($"Source.{MatchKeyStr1} = Target.{MatchKeyStr1} ");
            }
            else if (MatchKey2 != null)
            {
                var matchKeyStr2 = ParseExpression(
                    Types,
                    MatchKey2.Body,
                    true,
                    parameters: MatchKey2.Parameters,
                    tableNames: new[] { "Source", "Target" });

                sb.Append(matchKeyStr2 + " ");
            }

            if (WhenMatchedExpression != null)
            {
                var ssWhenMatched = new SimpleSqlBuilder<TRecord>(Options).From("Source", "Target")
                    .UseTableNames()
                    .IsChained();
                WhenMatchedExpression.Compile().Invoke(ssWhenMatched);
                var ssWhenMatchedBuild = ssWhenMatched.Build();
                sb.Append("WHEN MATCHED THEN ");
                sb.Append(ssWhenMatchedBuild);
                sb.Append(" ");
            }

            if (WhenNotMatchedExpression != null)
            {
                var ssNotWhenMatched = new SimpleSqlBuilder<TRecord>(Options).From("Source", "Target")
                    .UseTableNames()
                    .IsChained();
                WhenNotMatchedExpression.Compile().Invoke(ssNotWhenMatched);
                var ssWhenNotMatchedBuild = ssNotWhenMatched.Build();
                sb.Append("WHEN NOT MATCHED THEN ");
                sb.Append(ssWhenNotMatchedBuild);
                sb.Append(" ");
            }

            return sb.ToString().Trim() + ";";
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
                list.Add($"{GetAsSqlValue(record, field, props, values == null ? null : values[fieldIndex])}");
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
                list.Add($"{field} = {GetAsSqlValue(record, field, props)}");
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

                list.Add($"{tableNames?[1] ?? TableName}.{field} = {value}");
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
        /// Checks the downcast.
        /// </summary>
        /// <param name="clauseName">Name of the clause.</param>
        /// <exception cref="GaleForceCore.Builders.InadvertentDowncastException">
        /// The {clauseName} clause recasts SimpleSqlBuilder as a single table/type, use more
        /// parameters to preserve it.
        /// </exception>
        private void CheckDowncast(string clauseName)
        {
            if (Types?.Count() > 1)
            {
                throw new InadvertentDowncastException(
                    $"The {clauseName} clause recasts SimpleSqlBuilder as a single table/type, use more parameters to preserve it.");
            }
        }

        /// <summary>
        /// Executes the specified records.
        /// </summary>
        /// <param name="source">The records.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<TRecord> Execute(IEnumerable<TRecord> source)
        {
            CheckDowncast("Execute");
            switch (Command)
            {
                case "SELECT":
                    return ExecuteSelect(source);
                default:
                    throw new NotImplementedException(
                        $"{Command} is not supported as a query. Consider ExecuteNonQuery().");
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
            Dictionary<string, SourceData> sources = null;
            if (overrideSource != null)
            {
                sources = new Dictionary<string, SourceData>();
                sources.Add(
                    "__source",
                    new SourceData
                    {
                        Data = (IEnumerable<object>) overrideSource,
                        Name = "__source",
                        SourceType = typeof(TRecord)
                    });
            }

            return ExecuteNonQuery(target, sources);
        }

        /// <summary>
        /// Executes the specified source.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="sources">The sources.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int ExecuteNonQuery(List<TRecord> target, Dictionary<string, SourceData> sources)
        {
            CheckDowncast("ExecuteNonQuery");
            switch (Command)
            {
                case "UPDATE":
                    return ExecuteUpdate(target, sources);
                case "INSERT":
                    return ExecuteInsert(target, sources);
                case "MERGE":
                    return ExecuteMerge(target, sources);
                case "DELETE":
                    return ExecuteDelete(target);
                default:
                    throw new NotImplementedException($"{Command} is not supported as a non-query. Consider Execute().");
            }
        }

        /// <summary>
        /// Executes the expressions within this builder upon these records, returning the results.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="GaleForceCore.Builders.MissingDataTableException">SELECT testing requires a record set</exception>
        public IEnumerable<TRecord> ExecuteSelect(IEnumerable<TRecord> records)
        {
            if (records == null)
            {
                throw new MissingDataTableException("SELECT testing requires a record set");
            }

            var result = new List<TRecord>();
            var current = records;

            if (IsTracing)
            {
                Trace.AppendLine("ExecuteSelect: record count=" + current.Count());
            }

            foreach (var whereExpression in WhereExpression)
            {
                var we = whereExpression.Compile();
                current = current.Where(we);

                if (IsTracing)
                {
                    Trace
                        .AppendLine(
                            "ExecuteSelect: after Where clause: " +
                                whereExpression.ToString() +
                                ": count=" +
                                current.Count());
                }
            }

            if (OrderByList.Count > 0)
            {
                for (var i = OrderByList.Count - 1; i > -1; i--)
                {
                    var orderBy = OrderByList[i];
                    if (orderBy.IsAscending)
                    {
                        var oe = orderBy.Expression.Compile();
                        current = current.OrderBy(oe);
                    }
                    else
                    {
                        var oe = orderBy.Expression.Compile();
                        current = current.OrderByDescending(oe);
                    }
                }
            }

            if (Count < int.MaxValue)
            {
                current = current.Take(Count);
            }

            var type = typeof(TRecord);
            var props = GetNonIgnoreProperties<TRecord>();
            var fields = FieldList(Fields);
            var distinctiveKeys = new List<int>();

            foreach (var record in current)
            {
                var distinctiveKey = 17;
                var newRecord = (TRecord)Activator.CreateInstance(type);
                foreach (var field in fields)
                {
                    var prop = ExceptionHelpers.ThrowIfNull<PropertyInfo, MissingMemberException>(
                        props.FirstOrDefault(p => p.Name == field),
                        $"{field} property missing from {type.Name}");

                    prop.SetValue(newRecord, prop.GetValue(record));
                    if (IsDistinct)
                    {
                        distinctiveKey = (distinctiveKey * 23) + field.GetHashCode();
                        distinctiveKey = (distinctiveKey * 23) + prop.GetValue(record).GetHashCode();
                    }
                }

                if (!IsDistinct || !distinctiveKeys.Contains(distinctiveKey))
                {
                    result.Add(newRecord);
                    if (IsDistinct)
                    {
                        distinctiveKeys.Add(distinctiveKey);
                    }
                }
            }

            if (IsTracing)
            {
                Trace.AppendLine("ExecuteSelect: final count: " + result.Count());
            }

            return result;
        }

        /// <summary>
        /// Executes the expressions within this builder upon these records, returning the results.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="sources">The sources.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="GaleForceCore.Builders.MissingDataTableException">UPDATE testing requires a target record set to populate</exception>
        public int ExecuteUpdate(IEnumerable<TRecord> target, Dictionary<string, SourceData> sources = null)
        {
            if (target == null)
            {
                throw new MissingDataTableException("UPDATE testing requires a target record set to populate");
            }

            var result = new List<TRecord>();

            var current = target;
            foreach (var whereExpression in WhereExpression)
            {
                current = current.Where(whereExpression.Compile());
            }

            var type = typeof(TRecord);
            var props = GetNonIgnoreProperties<TRecord>();

            var keyProp = props.FirstOrDefault(p => p.Name == MatchKeyStr1);

            var hasValues = Valueset.Count() > 0;
            var values = hasValues ? ValueExpressions.Select(v => v.Compile()).ToList() : null;

            // assumes 1key
            int count = 0;

            var resultSet = GetSourceData(sources, "__source");
            if (resultSet != null && resultSet.Count() > 0)
            {
                foreach (var record in resultSet)
                {
                    var sourceKey = keyProp?.GetValue(record);
                    var targets = keyProp == null
                        ? current
                        : current.Where(t => object.Equals(keyProp.GetValue(t), sourceKey)).ToList();

                    foreach (var target1 in targets)
                    {
                        var fieldIndex = 0;
                        foreach (var field in Updates)
                        {
                            var prop = props.FirstOrDefault(p => p.Name == field);
                            var value = hasValues ? values[fieldIndex].Invoke(record) : prop.GetValue(record);
                            prop.SetValue(target1, value);
                            fieldIndex++;
                        }

                        count++;
                    }
                }
            }
            else
            {
                var targets = current;

                foreach (var target1 in targets)
                {
                    var record = target1;
                    var fieldIndex = 0;
                    foreach (var field in Updates)
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
        /// <param name="useAs">if set to <c>true</c> [use as].</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        public List<string> FieldList(List<string> list, bool useAs = false)
        {
            var original = list != null && list.Count() > 0
                ? list
                : GetNonIgnoreProperties<TRecord>().Select(p => p.Name).ToList();

            if (useAs)
            {
                for (var i = 0; i < original.Count(); i++)
                {
                    if (original[i].Contains(" AS "))
                    {
                        original[i] = original[i].Substring(original[i].IndexOf(" AS ") + 4);
                    }
                }
            }

            return original;
        }

        /// <summary>
        /// Gets the source data.
        /// </summary>
        /// <param name="sources">The sources.</param>
        /// <param name="name">The name.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        public IEnumerable<TRecord> GetSourceData(Dictionary<string, SourceData> sources, string name)
        {
            if (sources == null || !sources.ContainsKey(name))
            {
                return SourceData;
            }

            return sources[name].Data as IEnumerable<TRecord>;
        }

        /// <summary>
        /// Executes the expressions within this builder upon these records, returning the results.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="sources">The sources.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        /// <exception cref="GaleForceCore.Builders.MissingDataTableException">INSERT testing requires a record set</exception>
        /// <exception cref="GaleForceCore.Builders.MissingDataTableException">Named table {subFrom} inside SELECT inside requires a data source in sources</exception>
        public int ExecuteInsert(List<TRecord> target, Dictionary<string, SourceData> sources = null)
        {
            if (target == null)
            {
                throw new MissingDataTableException("INSERT testing requires a record set");
            }

            var type = typeof(TRecord);
            var props = GetNonIgnoreProperties<TRecord>();

            var hasValues = Valueset.Count() > 0;
            var values = hasValues ? ValueExpressions.Select(v => v.Compile()).ToList() : null;

            var fields = FieldList(Inserts);

            var count = 0;

            var resultSet = GetSourceData(sources, "__source");
            if (SourceBuilder != null)
            {
                var ssSource = new SimpleSqlBuilder<TRecord>(Options).IsChained();
                SourceBuilder.Compile().Invoke(ssSource);
                var subSources = sources.ToDictionary(kv => kv.Key, kv => kv.Value);
                var subFrom = ssSource.TableName;
                if (!subSources.ContainsKey(subFrom))
                {
                    throw new MissingDataTableException(
                        $"Named table {subFrom} inside SELECT inside requires a data source in sources");
                }

                subSources["__source"] = subSources[subFrom];
                resultSet = ssSource.Execute(subSources[subFrom].Data as IEnumerable<TRecord>);
            }

            foreach (var record in resultSet)
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
        public int ExecuteMerge(List<TRecord> target, IEnumerable<TRecord> source)
        {
            var sources = new Dictionary<string, SourceData>();
            sources.Add(
                "__source",
                new SourceData { Data = (IEnumerable<object>) source, Name = "__source", SourceType = typeof(TRecord) });
            return ExecuteMerge(target, sources);
        }

        /// <summary>
        /// Executes the merge.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="sources">The sources.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="GaleForceCore.Builders.MissingDataTableException">MERGE testing requires a List<TRecord> set</exception>
        /// <exception cref="GaleForceCore.Logger.StageItem.Exception">source record matches multiple targets (record #{index})</exception>
        /// <font color="red">Badly formed XML comment.</font>
        public int ExecuteMerge(List<TRecord> target, Dictionary<string, SourceData> sources)
        {
            if (target == null)
            {
                throw new MissingDataTableException("MERGE testing requires a List<TRecord> set");
            }

            Func<TRecord, TRecord, bool> match;
            if (MatchKeyStr1 != null)
            {
                var prop = typeof(TRecord).GetProperty(MatchKeyStr1);
                match = (s, t) => prop.GetValue(s).Equals(prop.GetValue(t));
            }
            else
            {
                match = MatchKey2.Compile();
            }

            var whenMatchedSSB = WhenMatchedExpression != null ? new SimpleSqlBuilder<TRecord>(Options) : null;
            WhenMatchedExpression?.Compile().Invoke(whenMatchedSSB);

            var whenNotMatchedSSB = WhenNotMatchedExpression != null ? new SimpleSqlBuilder<TRecord>(Options) : null;
            WhenNotMatchedExpression?.Compile().Invoke(whenNotMatchedSSB);

            var index = 0;
            var count = 0;
            var source = GetSourceData(sources, "__source");

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
        /// Executes the delete.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="GaleForceCore.Builders.MissingDataTableException">DELETE testing requires a List<TRecord> set</exception>
        /// <font color="red">Badly formed XML comment.</font>
        public int ExecuteDelete(List<TRecord> target)
        {
            if (target == null)
            {
                throw new MissingDataTableException("DELETE testing requires a List<TRecord> set");
            }

            var current = target;

            foreach (var whereExpression in WhereExpression)
            {
                current = current.Where(whereExpression.Compile()).ToList();
            }

            if (DistinctOnStr != null)
            {
                var distExp = DistinctOnExpression.Compile();
                current = current.GroupBy(r => distExp(r)).SelectMany(r => r.Skip(1)).ToList();
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
            return GetNonIgnoreProperties<TRecord>().Select(p => p.Name).ToList();
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <returns>List&lt;System.String&gt;.</returns>
        private List<string> GetFieldOrAsFieldList()
        {
            var fields = Fields;
            return GetNonIgnoreProperties<TRecord>().Select(p => p.Name).ToList();
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
            return SqlHelpers.GetAsSQLValue(value?.GetType(), value);
        }
    }

    /// <summary>
    /// Class SourceData.
    /// </summary>
    public class SourceData
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        public Type SourceType { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public IEnumerable<object> Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceData"/> class.
        /// </summary>
        public SourceData()
        {
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        /// <returns>SourceData.</returns>
        public static SourceData Create<T>(string name, IEnumerable<T> data)
        {
            return new SourceData { Data = (IEnumerable<object>) data, Name = name, SourceType = typeof(T) };
        }
    }
}