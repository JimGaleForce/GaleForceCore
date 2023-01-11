# GaleForceCore

A collection of common and useful classes, and methods, for use in numerous projects.

# GaleForceCore : SimpleSqlBuilder

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

SimpleSqlBuilder has been used and tested against a few larger than average scale projects. However, it is open and welcomes further improvements or features.

