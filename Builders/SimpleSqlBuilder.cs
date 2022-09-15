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

    public class SimpleSqlBuilder<TRecord>
    {
        public Type[] Types { get; protected set; }

        /// <summary>
        /// Gets the name of the tables, when multiple.
        /// </summary>
        /// <value>The name of the table.</value>
        public string[] TableNames { get; protected set; }

        public Dictionary<object, Expression<Func<TRecord, object>>> AsFields = new Dictionary<object, Expression<Func<TRecord, object>>>();

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; protected set; }

        /// <summary>
        /// Gets the field names.
        /// </summary>
        /// <value>The fields.</value>
        public List<string> Fields { get; protected set; } = new List<string>();

        /// <summary>
        /// Gets the maximum count for returned items.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; protected set; } = int.MaxValue;

        /// <summary>
        /// Gets the command (only SELECT active currently).
        /// </summary>
        /// <value>The command.</value>
        public string Command { get; protected set; } = string.Empty;

        /// <summary>
        /// Gets the where condition string.
        /// </summary>
        /// <value>The where string.</value>
        public string WhereString { get; protected set; } = null;

        /// <summary>
        /// Gets the 'order by' list, including 'then by'.
        /// </summary>
        /// <value>The order by list.</value>
        public List<SqlBuilderOrderItem<TRecord>> OrderByList
        {
            get;
            protected set;
        } = new List<SqlBuilderOrderItem<TRecord>>();

        public SimpleSqlBuilder()
        {
        }

        public SimpleSqlBuilder(Type[] types)
        {
            this.Types = types;
        }

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public IEnumerable<Expression<Func<TRecord, object>>> FieldExpressions
        {
            get;
            protected set;
        } = null;

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
        public SimpleSqlBuilder<TRecord> Select(
            IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
            this.Command = "SELECT";
            var names = fields.Select(
                field => this.ParseExpression<TRecord>(this.Types, field.Body, parameters: field.Parameters))
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
            return this;
        }

        /// <summary>
        /// Adds an order-by expression ascending (can build, execute).
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> OrderBy(Expression<Func<TRecord, object>> field)
        {
            var name = this.ParseExpression<TRecord>(this.Types, field.Body, parameters: field.Parameters);
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
            var name = this.ParseExpression<TRecord>(this.Types, field.Body);
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
            var name = this.ParseExpression<TRecord>(this.Types, field.Body, parameters: field.Parameters);
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
            var name = this.ParseExpression<TRecord>(this.Types, field.Body, parameters: field.Parameters);
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
            this.WhereString = this.ParseExpression<TRecord>(
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
        public SimpleSqlBuilder<TRecord> Take(int count)
        {
            this.Count = count;
            return this;
        }

        /// <summary>
        /// Parses different supported expressions to sql-friendly text.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="GaleForceCore.Builders.DynamicMethodException">Unable to prebuild SQL string with this method: " + meMethodName</exception>
        /// <exception cref="System.NotSupportedException">Unknown expression type for: " + exp.ToString()</exception>
        protected string ParseExpression<TRecord>(
            Type[] types,
            Expression exp,
            bool isCondition = false,
            IReadOnlyCollection<ParameterExpression> parameters = null)
        {
            var sb = new StringBuilder();
            if (exp is BinaryExpression)
            {
                var bExp = exp as BinaryExpression;
                var op = bExp.NodeType;
                var isCompare = op == ExpressionType.Equal || op == ExpressionType.NotEqual;

                var left = this.ParseExpression<TRecord>(types, bExp.Left, isCondition && !isCompare, parameters);
                var right = this.ParseExpression<TRecord>(types, bExp.Right, isCondition && !isCompare, parameters);

                sb.Append("(");
                if (bExp.Left.NodeType == ExpressionType.Not)
                {
                    sb.Append("NOT ");
                }

                sb.Append(left);
                switch (op)
                {
                    case ExpressionType.AndAlso:
                        sb.Append(" AND ");
                        break;
                    case ExpressionType.OrElse:
                        sb.Append(" OR ");
                        break;
                    case ExpressionType.GreaterThan:
                        sb.Append(" > ");
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        sb.Append(" >= ");
                        break;
                    case ExpressionType.LessThan:
                        sb.Append(" < ");
                        break;
                    case ExpressionType.LessThanOrEqual:
                        sb.Append(" <= ");
                        break;
                    case ExpressionType.NotEqual:
                        sb.Append(" != ");
                        break;
                    case ExpressionType.Add:
                        sb.Append(" + ");
                        break;
                    case ExpressionType.Subtract:
                        sb.Append(" - ");
                        break;
                    case ExpressionType.Multiply:
                        sb.Append(" * ");
                        break;
                    case ExpressionType.Divide:
                        sb.Append(" / ");
                        break;
                    case ExpressionType.Equal:
                        sb.Append(" = ");
                        break;
                }

                if (bExp.Right.NodeType == ExpressionType.Not)
                {
                    sb.Append("NOT ");
                }

                sb.Append(right);
                sb.Append(")");
                return sb.ToString();
            }
            else if (exp is UnaryExpression)
            {
                var operand = (exp as UnaryExpression).Operand;
                if (operand is MemberExpression)
                {
                    var operandMember = (exp as UnaryExpression).Operand as MemberExpression;
                    var declaringType = operandMember.Member.DeclaringType.Name;

                    var matchingType = this.GetMatchingType(types, declaringType);
                    if (matchingType != null)
                    {
                        var prefix = types == null || types.Length < 2
                            ? ""
                            : (this.GetMatchingTableName(types, matchingType, parameters) + ".");
                        var suffix = "";
                        if (isCondition &&
                            operandMember.Member is PropertyInfo &&
                            SqlHelpers.GetBetterPropTypeName(((PropertyInfo)operandMember.Member).PropertyType)
                                .StartsWith("bool"))
                        {
                            suffix = " = 1";
                        }

                        return prefix + operandMember.Member.Name + suffix;
                    }
                    else
                    {
                        // eval
                        var pe = operandMember;
                        var ce = operandMember.Expression as ConstantExpression;
                        if (ce != null)
                        {
                            if (pe.Member is FieldInfo)
                            {
                                object container = ce.Value;
                                object value = ((FieldInfo)pe.Member).GetValue(container);
                                var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                                return sqlValue;
                            }
                            else if (pe.Member is PropertyInfo)
                            {
                                object container = ce.Value;
                                object value = ((PropertyInfo)pe.Member).GetValue(container);
                                var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                                return sqlValue;
                            }
                        }

                        //
                        return pe.Member.Name;
                    }
                }
                else
                {
                    return this.ParseExpression<TRecord>(types, operand, isCondition, parameters);
                }
            }
            else if (exp is MemberExpression)
            {
                if (exp.ToString().StartsWith("value("))
                {
                    var value = Expression.Lambda(exp).Compile().DynamicInvoke();
                    return SqlHelpers.GetAsSQLValue(value.GetType(), value);
                }

                var pe = exp as MemberExpression;

                var ce = pe.Expression as ConstantExpression;
                var suffix = "";

                var parmName = pe.Expression is ParameterExpression ? (pe.Expression as ParameterExpression).Name : null;
                var declaringType = pe.Expression.Type.Name;
                var matchingType = this.GetMatchingType(types, declaringType);
                var prefix = types == null || types.Length < 2 || matchingType == null
                    ? ""
                    : (this.GetMatchingTableName(types, matchingType, parameters, parmName) + ".");

                if (ce != null)
                {
                    if (pe.Member is FieldInfo)
                    {
                        object container = ce.Value;
                        object value = ((FieldInfo)pe.Member).GetValue(container);
                        var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                        return sqlValue;
                    }
                    else if (pe.Member is PropertyInfo)
                    {
                        object container = ce.Value;
                        object value = ((PropertyInfo)pe.Member).GetValue(container);
                        var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
                        return sqlValue;
                    }
                }
                else if (isCondition)
                {
                    if (pe.Member is PropertyInfo &&
                        SqlHelpers.GetBetterPropTypeName(((PropertyInfo)pe.Member).PropertyType)
                            .StartsWith("bool"))
                    {
                        suffix = " = 1";
                    }
                }

                //
                return prefix + pe.Member.Name + suffix;
            }
            else if (exp is ConstantExpression)
            {
                var value = (exp as ConstantExpression).Value;
                var valueStr = value.ToString();
                if (value is string || value is DateTime)
                {
                    valueStr = "'" + valueStr + "'";
                }

                return valueStr;
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
                    switch (meMethodName)
                    {
                        case "Contains":
                            subValue = this.RemoveOuterQuotes(
                                this.ParseExpression<TRecord>(types, me.Arguments[0], parameters: parameters));
                            obj = this.ParseExpression<TRecord>(types, me.Object, parameters: parameters);
                            return $"{obj} LIKE '%{subValue}%'";
                        case "StartsWith":
                            subValue = this.RemoveOuterQuotes(
                                this.ParseExpression<TRecord>(types, me.Arguments[0], parameters: parameters));
                            obj = this.ParseExpression<TRecord>(types, me.Object, parameters: parameters);
                            return $"{obj} LIKE '{subValue}%'";
                        case "EndsWith":
                            subValue = this.RemoveOuterQuotes(
                                this.ParseExpression<TRecord>(types, me.Arguments[0], parameters: parameters));
                            obj = this.ParseExpression<TRecord>(types, me.Object, parameters: parameters);
                            return $"{obj} LIKE '%{subValue}'";
                    }
                }

                try
                {
                    object value = Expression.Lambda(me).Compile().DynamicInvoke();
                    var sqlValue = SqlHelpers.GetAsSQLValue(value.GetType(), value);
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

        private Type GetMatchingType(
            Type[] types,
            string declaringTypeName)
        {
            return types == null || types.Length == 0
                ? (declaringTypeName == typeof(TRecord).Name ? typeof(TRecord) : null)
                : types.FirstOrDefault(t => t.Name == declaringTypeName);
        }

        private string GetMatchingTableName(
            Type[] types,
            Type type,
            IReadOnlyCollection<ParameterExpression> parameters,
            string parmName = null)
        {
            if (type == null)
            {
                return null;
            }

            var typeName = type.Name;
            var index = parmName != null
                ? parameters.Select(p => p.Name).ToList().IndexOf(parmName)
                : Array.IndexOf(this.Types.Select(t => t.Name).ToArray(), typeName);
            var tableName = this.TableNames != null && this.Types != null && this.Types.Length > 0
                ?
                this.TableNames[index]
                : typeName;

            return tableName;
        }

        public virtual void InjectInnerClauses(StringBuilder sb)
        {
            return;
        }

        /// <summary>
        /// Builds the sql-server friendly string.
        /// </summary>
        /// <returns>System.String.</returns>
        public string Build()
        {
            var sb = new StringBuilder();
            sb.Append(this.Command);
            sb.Append(" ");

            if (this.Count < int.MaxValue)
            {
                sb.Append($"TOP {this.Count} ");
            }

            // assuming SELECT atm
            // todo: do other commands when needed
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

            if (!string.IsNullOrEmpty(this.WhereString))
            {
                sb.Append($"WHERE {this.WhereString} ");
            }

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
        /// Executes the expressions within this builder upon these records, returning the results.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <returns>IEnumerable&lt;TRecord&gt;.</returns>
        public IEnumerable<TRecord> Execute(IEnumerable<TRecord> records)
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

            foreach (var record in current)
            {
                var newRecord = (TRecord)Activator.CreateInstance(type);
                foreach (var field in this.Fields)
                {
                    var prop = props.FirstOrDefault(p => p.Name == field);
                    prop.SetValue(newRecord, prop.GetValue(record));
                }

                result.Add(newRecord);
            }

            return result;
        }
    }

    /// <summary>
    /// Class SimpleSqlBuilder.
    /// </summary>
    /// <typeparam name="TRecord">The type of the t record.</typeparam>
    public class SimpleSqlBuilder<TRecord, TRecord1, TRecord2> : SimpleSqlBuilder<TRecord>
    {
        public Expression<Func<TRecord1, TRecord2, object>> JoinKey { get; protected set; }

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public new IEnumerable<Expression<Func<TRecord1, TRecord2, object>>> FieldExpressions
        {
            get;
            protected set;
        } = null;

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
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Select(
            Expression<Func<TRecord1, TRecord2, object>> field)
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
            base.Select(names);
            return this;
        }

        private string IncludeAs(Expression<Func<TRecord1, TRecord2, object>> field)
        {
            var expString = this.ParseExpression<TRecord>(this.Types, field.Body, parameters: field.Parameters);
            if (this.AsFields != null && this.AsFields.ContainsKey(field))
            {
                var asStr = this.ParseExpression<TRecord>(
                    this.Types,
                    this.AsFields[field].Body,
                    parameters: field.Parameters);
                return expString + " AS " + asStr;
            }

            return expString;
        }

        /// <summary>
        /// Adds the table name for the builder.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> From(string tableName1, string tableName2)
        {
            this.TableNames = new string[] { tableName1, tableName2 };
            return this;
        }

        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> SelectAs(
            Expression<Func<TRecord, object>> asField,
            Expression<Func<TRecord1, TRecord2, object>> field)
        {
            this.AsFields[field] = asField;
            return this.Select(new Expression<Func<TRecord1, TRecord2, object>>[] { field });
        }

        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> InnerJoinOn(
            Expression<Func<TRecord1, TRecord2, object>> joinKey)
        {
            this.JoinKey = joinKey;
            return this;
        }

        public override void InjectInnerClauses(StringBuilder sb)
        {
            if (this.JoinKey != null)
            {
                var keys = this.ParseExpression<TRecord>(
                    this.Types,
                    this.JoinKey.Body,
                    true,
                    parameters: this.JoinKey.Parameters);
                sb.Append("INNER JOIN " + this.TableNames[1] + " ON " + keys + " ");
            }
        }

        public SimpleSqlBuilder<TRecord, TRecord1, TRecord2> Where(Expression<Func<TRecord1, TRecord2, bool>> condition)
        {
            this.WhereExpression2 = condition as Expression<Func<object, object, bool>>;
            this.WhereString = this.ParseExpression<TRecord>(
                this.Types,
                condition.Body,
                true,
                parameters: condition.Parameters);
            return this;
        }
    }
}