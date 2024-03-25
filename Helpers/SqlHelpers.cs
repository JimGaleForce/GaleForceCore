//-----------------------------------------------------------------------
// <copyright file="SqlHelpers.cs" company="Gale-Force">
// Copyright (C) Gale-Force Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public static string CleanStringForEquals(string text)
        {
            return text.Replace("'", "''");
        }

        public static string CleanStringForLike(string text)
        {
            return text.Replace("\\", "\\\\").Replace("%", "\\%"); //.Replace("_", "\\_");
        }

        /// <summary>
        /// Cleans the text for backslashes, wildcards, and single quotes
        /// </summary>
        /// <param name="fields">The text to clean.</param>
        public static string CleanStringX(string text)
        {
            return text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("'", "''");
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
            if (type == null)
            {
                return "null";
            }

            var name = GetBetterPropTypeName(type);

            switch (name)
            {
                case "string":
                    value = "'" + CleanStringForEquals(value.ToString()) + "'";
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
                    else if (type.Name.StartsWith("List") ||
                        type.Name.StartsWith("String[]") ||
                        type.Name.StartsWith("Int32[]") ||
                        type.Name.StartsWith("Byte[]") ||
                        type.Name.StartsWith("Enumerable"))
                    {
                        var listOfInts = value as List<int>;
                        if (listOfInts != null)
                        {
                            return "(" + string.Join(",", listOfInts) + ")";
                        }

                        var arrayOfInts = value as int[];
                        if (arrayOfInts != null)
                        {
                            return "(" + string.Join(",", arrayOfInts) + ")";
                        }

                        var arrayOfBytes = value as byte[];
                        if (arrayOfBytes != null)
                        {
                            return "0x" + BitConverter.ToString(arrayOfBytes).Replace("-", "");
                        }

                        var listOfStrings = value as List<string>;
                        if (listOfStrings != null)
                        {
                            return "('" + string.Join("','", listOfStrings.Select(s => CleanStringForEquals(s))) + "')";
                        }
                        else
                        {
                            var listOfStrings2 = value as string[];
                            if (listOfStrings2 != null)
                            {
                                return "('" +
                                    string.Join("','", listOfStrings2.Select(s => CleanStringForEquals(s))) +
                                    "')";
                            }
                        }
                    }

#if RELEASE
                    break;
#else
                    throw new ArgumentException($"Unsupported object type '{type}', cannot convert to SQL value.");
#endif
            }

            return (value ?? "null").ToString();
        }
    }
}
