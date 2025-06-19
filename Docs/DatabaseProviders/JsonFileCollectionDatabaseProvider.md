# JsonFileCollectionDatabaseProvider

The Json File Collection Database Provider uses a folder within application binary directory to provide the Database. This requires the following settings in your appsettings.json:

```
- "DatabaseProvider": "JsonFileCollectionDatabaseProvider"
```

### Usage in Visual Studio Locally

- Download and build the project to create an output binary (/bin)
- Download the files from a Database source (e.g. [Paulov.Tarkov.Db](https://github.com/paulov-t/Paulov.Tarkov.db))
- Place the loose files in the compiled output binary
