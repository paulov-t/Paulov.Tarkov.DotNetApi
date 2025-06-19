# MicrosoftCompressionZipDatabaseProvider

The Compressed Database Provider uses a .zip within application binary directory to provide the Database. This requires the following settings in your appsettings.json:

```
- "DatabaseProvider": "MicrosoftCompressionZipDatabaseProvider"
```

### Usage in Visual Studio Locally

- Download and build the project to create an output binary (/bin)
- Download the files as a zip from a Database source (e.g. [Paulov.Tarkov.Db](https://github.com/paulov-t/Paulov.Tarkov.db))
- Place the zip in the compiled output binary and rename to `database.zip`