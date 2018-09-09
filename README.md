# SchemaSync

For a long time I've wanted my own database schema diff/merge app. There are several such tools from ApexSQL, Red Gate, AquaFold, SQL Accessories, and I'm sure others. I've made a couple stabs at this that are part of my [Postulate ORM](https://github.com/adamosoftware/Postulate.Orm) project. My "model merge" feature is for supporting code-first ORM by merging C# model classes to SQL Server objects. This has several limitations: 
- It works only C#-to-database. There's no database-to-database merging.
- It's coupled to Postulate ORM model classes.
- It works only for SQL Server.
- It handles only table-related changes (tables, columns, foreign keys), not stored procedures, views nor other objects.

This library is addressing these limitations to varying degrees. The main goals are:
- Do C#-to-database and data-to-database merges equally well
- Work with any source ORM or database platform through a Provider pattern

So, SchemaSync has a [model layer](https://github.com/adamosoftware/SchemaSync/tree/master/SchemaSync.Library/Models) that virtualizes database metadata -- whether it comes from a physical database or .NET assembly of model classes. The [Database](https://github.com/adamosoftware/SchemaSync/blob/master/SchemaSync.Library/Models/Database.cs) object is the root object from which child objects (Tables, ForeignKeys, Views, Procedures) are collected under.

To support a particular ORM model class assembly or database backend, there's an interface to implement: [IDbAssemblyProvider](https://github.com/adamosoftware/SchemaSync/blob/master/SchemaSync.Library/Interfaces/IDbAssemblyProvider.cs) or [IDbConnectionProvider](https://github.com/adamosoftware/SchemaSync/blob/master/SchemaSync.Library/Interfaces/IDbConnectionProvider.cs).

As I said above, SchemaSync has no particular ORM dependency, but I do want it to work with [Postulate Lite](https://github.com/adamosoftware/Postulate.Lite). So, a [Postulate Provider](https://github.com/adamosoftware/SchemaSync/tree/master/SchemaSync.Postulate) is part of the solution. Likewise, there's a [SQL Server Provider](https://github.com/adamosoftware/SchemaSync/tree/master/SchemaSync.SqlServer).

