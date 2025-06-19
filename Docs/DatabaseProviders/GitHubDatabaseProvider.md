# GitHubDatabaseProvider

The GitHub Database Provider uses a GitHub repository as the Database. This requires the following settings in your appsettings.json:

```
- "DatabaseProvider": "GitHubDatabaseProvider"
- "GitHubDatabaseUrl": "{YOUR_GITHUB_DB_URL}"
- "GitHubAuthToken": {YOUR_GITHUB_SECRET_AUTH_TOKEN}
```

## GitHubDatabaseUrl

You need to replace {YOUR_GITHUB_DB_URL} with the repository with the database files. An example of this is Paulov.Tarkov.Db. It is HIGHLY recommended it is a Repository you own otherwise you will reach a limit of access to the files (you can simply fork Paulov.Tarkov.Db).

## GitHubAuthToken

It is highly recommended to provide a Auth Token. If you do not do this GitHub will stop allowing access to the files. 

To do this:
- Open GitHub and Login
- Select your profile icon and select Settings
- Select Developer Settings
- Select Generate new token
- Name your token TarkovDbRead
- Set it to never expire
- Select "only select repositories"
- Select your database repository
- Select "repository permissions" and provide "read-access" to "Contents"
- Select Generate Token
- Store your generated token on your PC to use later

### Applying the Auth Token in Visual Studio Locally

With the solution open in Visual Studio, right-click Web App project and select "Manage user-secrets".

In the json file, add "GitHubAuthToken": {PASTE YOUR GENERATED TOKEN HERE}

### Applying the Auth Token in Deployment to AZURE from GitHub

**To do this you will need a fork of the repository with your own GitHub Actions OR a repository you own that can deploy this repository**

TODO: WRITE DOCS ON THIS!