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
        public string Command { get; protected set; } = "SELECT";

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
        /// <param name="types">The types.</param>
        public SimpleSqlBuilder(Type[] types)
        {
            this.Types = types;
        }

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public IEnumerable<Expression<Func<TRecord, object>>> FieldExpressions { get; protected set; } = null;

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
        /// Gets the where condition expression (lambda).
        /// </summary>
        /// <value>The where expression.</value>
        public Expression<Func<object, object, object, bool>> WhereExpression3 { get; protected set; } = null;

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
        public SimpleSqlBuilder<TRecord> Select(IEnumerable<Expression<Func<TRecord, object>>> fields)
        {
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
            return this.Select();
        }

        /// <summary>
        /// Selects this instance.
        /// </summary>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Select()
        {
            this.Command = "SELECT";
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
        /// <typeparam name="TRecord">The type of the t record.</typeparam>
        /// <param name="types">The types.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="isCondition">if set to <c>true</c> [is condition].</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="hideSourceTable">if set to <c>true</c> [hide source table].</param>
        /// <returns>System.String.</returns>
        /// <exception cref="GaleForceCore.Builders.DynamicMethodException">Unable to prebuild SQL string with this method: " + meMethodName</exception>
        /// <exception cref="System.NotSupportedException">Unknown expression type for: " + exp.ToString()</exception>
        protected string ParseExpression<TRecord>(
            Type[] types,
            Expression exp,
            bool isCondition = false,
            IReadOnlyCollection<ParameterExpression> parameters = null,
            bool hideSourceTable = false)
        {
            var sb = new StringBuilder();
            if (exp is BinaryExpression)
            {
                var bExp = exp as BinaryExpression;
                var op = bExp.NodeType;
                var isCompare = op == ExpressionType.Equal || op == ExpressionType.NotEqual;

                var left = this.ParseExpression<TRecord>(types, bExp.Left, isCondition && !isCompare, parameters);
                var right = this.ParseExpression<TRecord>(types, bExp.Right, isCondition && !isCompare, parameters);

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
                        var prefix = types == null || types.Length < 2 || hideSourceTable
                            ? string.Empty
                            : (this.GetMatchingTableName(types, matchingType, parameters, operandMember: operandMember) +
                                ".");
                        var suffix = string.Empty;
                        if (isCondition &&
                            operandMember.Member is PropertyInfo &&
                            SqlHelpers.GetBetterPropTypeName(((PropertyInfo)operandMember.Member).PropertyType)
                                .StartsWith("bool"))
                        {
                            suffix = exp.NodeType == ExpressionType.Not ? " = 0" : " = 1";
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
                var suffix = string.Empty;

                var parmName = pe.Expression is ParameterExpression ? (pe.Expression as ParameterExpression).Name : null;
                var declaringType = pe.Expression.Type.Name;
                var matchingType = this.GetMatchingType(types, declaringType);
                var prefix = types == null || types.Length < 2 || matchingType == null || hideSourceTable
                    ? string.Empty
                    : (this.GetMatchingTableName(types, matchingType, parameters, parmName, operandMember: pe) + ".");

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
                        SqlHelpers.GetBetterPropTypeName(((PropertyInfo)pe.Member).PropertyType).StartsWith("bool"))
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
                var valueStr = value == null ? "NULL" : value.ToString();
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
                else if ((meContainingType.StartsWith("List") || meContainingType.StartsWith("Enumerable")) &&
                    meMethodName == "Contains")
                {
                    var subValue = this.RemoveOuterQuotes(
                        this.ParseExpression<TRecord>(types, me.Arguments[0], parameters: parameters));
                    var obj = this.ParseExpression<TRecord>(types, me.Object ?? me.Arguments[1], parameters: parameters);
                    if (subValue.StartsWith("(") && !obj.StartsWith("("))
                    {
                        return $"{obj} IN {subValue}";
                    }
                    else
                    {
                        return $"{subValue} IN {obj}";
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
        /// <returns>System.String.</returns>
        private string GetMatchingTableName(
            Type[] types,
            Type type,
            IReadOnlyCollection<ParameterExpression> parameters,
            string parmName = null,
            MemberExpression operandMember = null)
        {
            if (type == null)
            {
                return null;
            }

            string tableName = null;
            var tpe = operandMember.Expression as ParameterExpression;
            if (tpe != null)
            {
                var name = tpe.Name;
                var nameIndex = parameters.Select(p => p.Name).ToList().IndexOf(name);
                if (nameIndex > -1)
                {
                    tableName = this.TableNames[nameIndex];
                    return tableName;
                }
            }

            var typeName = type.Name;
            var index = parmName != null
                ? parameters.Select(p => p.Name).ToList().IndexOf(parmName)
                : Array.IndexOf(this.Types.Select(t => t.Name).ToArray(), typeName);
            tableName = this.TableNames != null && this.Types != null && this.Types.Length > 0
                ? this.TableNames[index]
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
}