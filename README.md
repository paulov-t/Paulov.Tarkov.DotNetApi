<div align=center style="text-align: center">
<h1 style="text-align: center"> Paulov.Tarkov.DotNetApi </h1>

</div>

---

## About the Project
This is a personal project to develop a .NET Web & Api against a real world example. In this case Escape from Tarkov server side endpoints.

## Status
Under development, can get to the main menu and play a match with some bots.

## The project ruleset
- Use the Model-View-Controller design
- Use Swagger Open Api
- Use username & hashed password authorization - **NOT YET IMPLEMENTED**
- Use web token authorization - **NOT YET IMPLEMENTED**
- Create a DOTNET Web Api that will work in the free Azure Web Service. 
  - The free tier of the Azure Web Service has a very low: memory size threshold (1GB), hard disk space threshold (1GB) and CPU threshold (60 mins per day). If you exceed the threshold, the app will not start or crash. 
  - The main aim is to keep the footprint very low 
  - Do not load anything permanently into memory
  - Do not have large loose files in the project

## Database Support
This app can support the following databases:
- MongoDb (MongoDatabaseProvider) - `Documentation TBD`
- GitHub Repository (GitHubDatabaseProvider) - Fork [the database repo](https://github.com/paulov-t/Paulov.Tarkov.Db/) and create an Auth Token to use 
- JSON File Collection in the app binary directory (JsonFileCollectionDatabaseProvider) - Download [the database repo](https://github.com/paulov-t/Paulov.Tarkov.Db/) and place the loose files in the binary directory
- Ultra Compressed Zip called Database.zip in the app binary directory (MicrosoftCompressionZipDatabaseProvider)  - Download [the database repo](https://github.com/paulov-t/Paulov.Tarkov.Db/) as a zip and place in the binary directory

## Disclaimer
- This is a purely a for fun and personal learning for me against a real world scenario
- This is not designed to replace official Tarkov. Please play official Tarkov!
- This is not designed to replace [SP-Tarkov](https://github.com/sp-tarkov)
- If you use the Url's below to play the game, please remember that your data could be deleted at any time.

## Current running live example Website and Api
- [Dev-Test](https://paulovtarkovdotnetapi-linux-dev.azurewebsites.net)
- [Swagger Api UI](https://paulovtarkovdotnetapi-linux-dev.azurewebsites.net/swagger/index.html)
- [Ammo Table - including custom rating calculation](https://paulovtarkovdotnetapi-linux-dev.azurewebsites.net/ammo)
- [Item Table - including custom rating calculation](https://paulovtarkovdotnetapi-linux-dev.azurewebsites.net/items)

## Installation

### Requirements

This project has been built in [Visual Studio Community Edition](https://visualstudio.microsoft.com/vs/community/) using [.NET 8](https://dotnet.microsoft.com/en-us/)

## License

- This project is licensed under the Attribution-NonCommercial-NoDerivatives 4.0 International License. See [LICENSE](LICENSE.md)
- This project uses SP-Tarkov's Database. SP-Tarkov is licensed under NCSA Open Source. [LICENSE](https://github.com/sp-tarkov/server/blob/master/LICENSE.md)


<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/paulov-t/Paulov.Tarkov.DotNetApi.svg?style=for-the-badge

[forks-shield]: https://img.shields.io/github/forks/paulov-t/Paulov.Tarkov.DotNetApi.svg?style=for-the-badge&color=%234c1

[forks-url]: https://github.com/paulov-t/Paulov.Tarkov.DotNetApi/network/members

[stars-shield]: https://img.shields.io/github/stars/paulov-t/Paulov.Tarkov.DotNetApi?style=for-the-badge&color=%234c1

[stars-url]: https://github.com/paulov-t/Paulov.Tarkov.DotNetApi/stargazers

[downloads-total-shield]: https://img.shields.io/github/downloads/paulov-t/Paulov.Tarkov.DotNetApi/total?style=for-the-badge

[downloads-latest-shield]: https://img.shields.io/github/downloads/paulov-t/Paulov.Tarkov.DotNetApi/latest/total?style=for-the-badge
