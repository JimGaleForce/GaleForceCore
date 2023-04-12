# GaleForceCore

A collection of common and useful classes, and methods, for use in numerous projects.

# GaleForceCore : SimpleSqlBuilder

A powerful SQL builder library for C# applications, capable of generating SQL queries with ease. The library provides a set of classes and methods that allow you to build SQL queries programmatically, without having to write raw SQL strings.

## Features
- Fluent API for building SELECT, INSERT, UPDATE, DELETE, and MERGE queries
- Supports multiple join types (INNER, LEFT OUTER, RIGHT OUTER)
- Conditional filtering (AND, OR, NOT)
- Parameterized queries
- Supports C# expressions as query conditions
- Supports multiple input and output types
- Execution of built queries against in-memory data collections

SimpleSqlBuilder is an open-source project that provides a simple and efficient way to interact with SQL Server databases. It is similar to Entity Framework Core, but is specifically designed for use with SQL Server and focuses on handling simple queries.

With SimpleSqlBuilder, you can perform Select, Delete, Update, and Insert commands, and it also supports join up to 5 tables using inner join, left outer join and right outer join. The library also supports automatic mapping of fields, and it provides a LINQ-like syntax for selecting which fields to include in your queries. This can give you the benefit of compile-time syntax checking, instead of runtime errors.

SimpleSqlBuilder also offers batching capabilities for Insert and Update commands, which can greatly improve performance when working with large datasets. Additionally, it allows for easy testing by providing the ability to pass in either a connection string to SQL Server or a set of data tables. This means that you can test your queries without making any changes to your code.

This library is built with flexibility and simplicity in mind, allowing for ease of use for developers with little to no SQL experience. With SimpleSqlBuilder, you can write efficient SQL queries using an OOP syntax, without having to worry about writing raw SQL.

Overall, SimpleSqlBuilder is an ideal solution for developers working with SQL Server who are looking for an easy and efficient way to interact with their databases. Its specific focus on SQL Server, simple querying capabilities and support for testing makes it a great alternative for EF Core if you use SQL Server as a primary database.

SimpleSqlBuilder library is a .NET Standard 2.0 library that requires **no dependencies** for testing. When interacting with SQL Server, it requires the separate .NET Standard 2.1 library, `System.Data.SqlClient`, which has a **small footprint**.

With its Linq to SQL conversion, it can translate a number of functions such as:
- `Contains`
- `StartsWith`
- `EndsWith`
- `IsNullOrEmpty`
- `IsNullOrWhiteSpace`
- `Trim`
- `Equals`
- `IndexOf`
- `CHARINDEX`
- `ToLower`
- `ToUpper`
- `TrimStart`
- `TrimEnd`
- `Substring`
- `SUBSTRING`

Therefore, code like this works for SQL and testing:
```c#
var actual = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
                .Select(s => s.String1)
                .Where(s => s.String1.ToLower() == "string123")
                .Build();
```

## Installation

The library can be installed from [NuGet](https://www.nuget.org/packages/GaleForceCore/). To install using the NuGet Package Manager, run the following command:

```
PM> Install-Package GaleForceCore
```

## Usage

To use the library, you first need to create an instance of the `SimpleSqlBuilder` class, passing the table name as an argument. You can then chain various methods to build your SQL query. Finally, call the `Build()` method to generate the SQL string.

```csharp
using GaleForceCore.Builders;
using GaleForceCore.Helpers;
```

### Building a Select query

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Select(s => s.Int1, s => s.String1)
    .Where(s => s.Int1 > 10 && s.String1.Contains("ABC"))
    .OrderBy(s => s.Int1)
    .Build();
```

### Building an Insert query

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Insert(s => s.Int1, s => s.String1)
    .Values(1, "ABC")
    .Build();
```

### Building an Update query

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Update(s => s.Int1)
    .Values(10)
    .Where(s => s.String1 == "ABC")
    .Build();
```

### Building a Delete query

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Delete()
    .Where(s => s.Int1 > 10)
    .Build();
```

### Building a Merge query

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .MergeInto(SqlTestRecord.TableName)
    .Match((s, t) => s.Int1 == t.Int1)
    .WhenMatched(s => s.Update(s => s.Int2))
    .WhenNotMatched(s => s.Insert(s => s.Int1, s => s.Int2))
    .Build();
```

## Executing queries

The library also includes methods to execute the built queries against in-memory data collections.

```csharp
var data = GetData(); // Method that returns a List of SqlTestRecord objects
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Select(s => s.Int1, s => s.String1)
    .Where(s => s.Int1 > 10 && s.String1.Contains("ABC"));

var results = builder.Execute(data);
```

## Select Statement Options and Complex Usage

The following examples demonstrate various options and complex usages for building a SELECT statement using the GaleForceCore library. These examples are discovered from the unit tests included in the source code.

### Basic Select Statement

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Select(s => s.Int1, s => s.String1)
    .Build();
```

### Select with a Where Clause

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Select(s => s.Int1, s => s.String1)
    .Where(s => s.Int1 > 10 && s.String1.Contains("ABC"))
    .Build();
```

### Select with a Where Clause and a Parameterized Query

```csharp
var options = new SimpleSqlBuilderOptions { UseParameters = true };
var builder = new SimpleSqlBuilder<SqlTestRecord>(options, SqlTestRecord.TableName)
    .Select(s => s.Int1, s => s.String1)
    .Where(s => s.Int1 > 10 && s.String1.Contains("ABC"))
    .Build();
```

### Select with Multiple Where Clauses

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Select(s => s.Int1)
    .Where(s => s.Bool1)
    .Where(s => s.Int2 % 2 == 1)
    .Build();
```

### Select with OrderBy and OrderByDescending

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Select(s => s.Int1, s => s.String1)
    .Where(s => s.Int1 > 10 && s.String1.Contains("ABC"))
    .OrderBy(s => s.Int1)
    .ThenByDescending(s => s.String1)
    .Build();
```

### Select with Top Records (Take)

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Select(s => s.Int1, s => s.String1)
    .Where(s => s.Int1 > 10 && s.String1.Contains("ABC"))
    .Take(10)
    .Build();
```

### Select with Distinct

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord>(SqlTestRecord.TableName)
    .Select(s => s.Int2)
    .Distinct()
    .Build();
```

### Select with Inner Join

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
    .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
    .Select((s1, s2) => s1.Int1, (s1, s2) => s2.Int2, (s1, s2) => s1.String1)
    .InnerJoinOn((s1, s2) => s1.Int1 == s2.Int1)
    .Build();
```

### Select with Left Outer Join

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
    .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
    .Select((s1, s2) => s1.Int1, (s1, s2) => s2.Int2, (s1, s2) => s1.String1)
    .LeftOuterJoinOn((s1, s2) => s1.Int1 == s2.Int1)
    .Build();
```

### Select with Right Outer Join

```csharp
var builder = new SimpleSqlBuilder<SqlTestRecord, SqlTestRecord, SqlTestRecord>()
    .From(SqlTestRecord.TableName, SqlTestRecord.TableName)
    .Select((s1, s2) => s1.Int1, (s1, s2) => s2.Int2, (s1, s2) => s1.String1)
    .RightOuterJoinOn((s1, s2) => s1.Int1 == s2.Int1)
    .Build();
```

### Complex Query with Multiple Joins and Conditions

```csharp
var options = new SimpleSqlBuilderOptions { UseParameters = false };
var builder = new SimpleSqlBuilder<TargetRecord, SourceRecord1, SourceRecord2, SourceRecord2, SourceRecord3>(options)
    .From(SourceRecord1.TableName, SourceRecord2.TableName, SourceRecord2.TableName, SourceRecord3.TableName)
    .Select((s1, s2a, s2b, s3) => s1.Int1, (s1, s2a, s2b, s3) => s2a.Int2, (s1, s2a, s2b, s3) => s3.String1)
    .SelectAs(z => z.SameString, (s1, s2a, s2b, s3) => s3.SameString)
    .InnerJoin12On((s1, s2a) => s1.SameString == s2a.SameString.Substring(s2a.SameString.IndexOf(":") + 1, 100) && s2a.Bool2.Value)
    .InnerJoin13On((s1, s2b) => s1.Int1 == s2b.Int2)
    .InnerJoin14On((s1, s3) => s1.Int1 == s3.Int3)
    .Where((s1, s2a, s2b, s3) => s2b.String2 == "text" && !s3.String3.EndsWith(":bad"))
    .Build();
```

These examples showcase the various options and complex use cases that can be achieved using the GaleForceCore SQL Builder library for constructing SELECT statements.

SimpleSqlBuilder has been used and tested against a few larger than average scale projects. However, it is open and welcomes further improvements or features.







