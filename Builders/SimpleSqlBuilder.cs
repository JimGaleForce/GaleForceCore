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
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// Gets the field names.
        /// </summary>
        /// <value>The fields.</value>
        public List<string> Fields { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the 'order by' list, including 'then by'.
        /// </summary>
        /// <value>The order by list.</value>
        public List<SqlBuilderOrderItem<TRecord>> OrderByList
        {
            get;
            private set;
        } = new List<SqlBuilderOrderItem<TRecord>>();

        /// <summary>
        /// Gets the maximum count for returned items.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; private set; } = int.MaxValue;

        /// <summary>
        /// Gets the command (only SELECT active currently).
        /// </summary>
        /// <value>The command.</value>
        public string Command { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the where condition string.
        /// </summary>
        /// <value>The where string.</value>
        public string WhereString { get; private set; } = null;

        /// <summary>
        /// Gets the field expressions (lambda expressions for each field).
        /// </summary>
        /// <value>The field expressions.</value>
        public IEnumerable<Expression<Func<TRecord, object>>> FieldExpressions
        {
            get;
            private set;
        } = null;

        /// <summary>
        /// Gets the order by expressions (lambda expressions for order-by's and then-by's).
        /// </summary>
        /// <value>The order by expressions.</value>
        public List<Expression<Func<TRecord, object>>> OrderByExpressions
        {
            get;
            private set;
        } = new List<Expression<Func<TRecord, object>>>();

        /// <summary>
        /// Gets the where condition expression (lambda).
        /// </summary>
        /// <value>The where expression.</value>
        public Expression<Func<TRecord, bool>> WhereExpression { get; private set; } = null;

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
            var names = fields.Select(field => this.ParseExpression(field.Body)).ToList();
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
            var name = this.ParseExpression(field.Body);
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
            var name = this.ParseExpression(field.Body);
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
            var name = this.ParseExpression(field.Body);
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
            var name = this.ParseExpression(field.Body);
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
        /// Sets the where condition as an expression (can build, execute).
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>SimpleSqlBuilder&lt;TRecord&gt;.</returns>
        public SimpleSqlBuilder<TRecord> Where(Expression<Func<TRecord, bool>> condition)
        {
            this.WhereExpression = condition;
            this.WhereString = this.ParseExpression(condition.Body);
            return this;
        }

        /// <summary>
        /// Parses different supported expressions to sql-friendly text.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="GaleForceCore.Builders.DynamicMethodException">Unable to prebuild SQL string with this method: " + meMethodName</exception>
        /// <exception cref="System.NotSupportedException">Unknown expression type for: " + exp.ToString()</exception>
        private string ParseExpression(Expression exp)
        {
            var sb = new StringBuilder();
            if (exp is BinaryExpression)
            {
                var bExp = exp as BinaryExpression;
                var left = this.ParseExpression(bExp.Left);
                var right = this.ParseExpression(bExp.Right);

                sb.Append("(");
                sb.Append(left);
                var op = bExp.NodeType;
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

                sb.Append(right);
                sb.Append(")");
                return sb.ToString();
            }
            else if (exp is UnaryExpression)
            {
                var operand = (exp as UnaryExpression).Operand as MemberExpression;
                var declaringType = operand.Member.DeclaringType.Name;
                if (declaringType == typeof(TRecord).Name)
                {
                    return operand.Member.Name;
                }
                else
                {
                    // eval
                    var pe = operand;
                    var ce = operand.Expression as ConstantExpression;
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
            else if (exp is MemberExpression)
            {
                var pe = exp as MemberExpression;
                var ce = pe.Expression as ConstantExpression;
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
                            subValue = this.RemoveOuterQuotes(this.ParseExpression(me.Arguments[0]));
                            obj = this.ParseExpression(me.Object);
                            return $"{obj} LIKE '%{subValue}%'";
                        case "StartsWith":
                            subValue = this.RemoveOuterQuotes(this.ParseExpression(me.Arguments[0]));
                            obj = this.ParseExpression(me.Object);
                            return $"{obj} LIKE '{subValue}%'";
                        case "EndsWith":
                            subValue = this.RemoveOuterQuotes(this.ParseExpression(me.Arguments[0]));
                            obj = this.ParseExpression(me.Object);
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
        private string RemoveOuterQuotes(string value)
        {
            if (value.StartsWith("'") && value.EndsWith("'"))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
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

            sb.Append($" FROM {this.TableName} ");

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
