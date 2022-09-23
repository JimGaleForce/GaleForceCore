//-----------------------------------------------------------------------
// <copyright file="SqlHelpers.cs" company="Gale-Force">
// Copyright (C) Gale-Force Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Class SqlHelpers.
    /// </summary>
    public class SqlHelpers
    {
        /// <summary>
        /// Gets the name of the better property type.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <returns>System.String.</returns>
        public static string GetBetterPropTypeName(PropertyInfo prop)
        {
            return GetBetterPropTypeName(prop.PropertyType);
        }

        /// <summary>
        /// Gets the name of the better property type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        public static string GetBetterPropTypeName(Type type)
        {
            var typeName = type.Name;
            var isNullable = typeName == "Nullable`1";
            if (isNullable)
            {
                typeName = type.GetGenericArguments()[0].Name;
            }

            return GetBetterPropTypeName(typeName, isNullable);
        }

        /// <summary>
        /// Gets the name of the better property type.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
        /// <returns>System.String.</returns>
        public static string GetBetterPropTypeName(string typeName, bool isNullable)
        {
            switch (typeName)
            {
                case "String":
                    typeName = "string";
                    break;
                case "Int32":
                    typeName = "int";
                    break;
                case "Boolean":
                    typeName = "bool";
                    break;
                case "Double":
                case "System.Double":
                    typeName = "double";
                    break;
                case "Float":
                case "System.Float":
                    typeName = "float";
                    break;
            }

            return typeName + (isNullable ? "?" : string.Empty);
        }

        /// <summary>
        /// Gets a SQL-friendly value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.ArgumentException">Unsupported object type '{type}', cannot convert to SQL value.</exception>
        public static string GetAsSQLValue(Type type, object value)
        {
            var name = GetBetterPropTypeName(type);

            switch (name)
            {
                case "string":
                    value = "'" + value + "'";
                    break;
                case "DateTime":
                    if (value is DateTime)
                    {
                        value = "'" + ((DateTime)value).ToString("o") + "'";
                    }
                    else if (value is string)
                    {
                        value = "'" + DateTime.Parse(value.ToString()).ToString("o") + "'";
                    }

                    break;
                case "DateTime?":
                    if (value is DateTime?)
                    {
                        if (((DateTime?)value).HasValue)
                        {
                            value = "'" + ((DateTime?)value).Value.ToString("o") + "'";
                        }
                    }
                    else if (value is string)
                    {
                        value = "'" + DateTime.Parse(value.ToString()).ToString("o") + "'";
                    }

                    break;
                case "bool":
                    value = ((bool)value) ? 1 : 0;
                    break;
                case "bool?":
                    if (((bool?)value).HasValue)
                    {
                        value = ((bool)value) ? 1 : 0;
                    }

                    break;
                case "int":
                case "double":
                case "float":
                case "int?":
                case "double?":
                case "float?":

                    // correct format
                    break;

                default:
                    if (type.BaseType.Name == "Enum")
                    {
                        value = (int)Enum.Parse(type, value.ToString());
                        break;
                    }
                    else if (type.Name.StartsWith("List"))
                    {
                        var listOfInts = value as List<int>;
                        if (listOfInts != null)
                        {
                            return "(" + string.Join(",", listOfInts) + ")";
                        }

                        var listOfStrings = value as List<string>;
                        if (listOfStrings != null)
                        {
                            return "('" + string.Join("','", listOfStrings) + "')";
                        }
                    }

#if RELEASE
                    break;
#else
                    throw new ArgumentException(
                        $"Unsupported object type '{type}', cannot convert to SQL value.");
#endif
            }

            return (value ?? "null").ToString();
        }
    }
}
