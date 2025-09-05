# Clew
![GitHub License](https://img.shields.io/github/license/Det-rovv/Clew?style=for-the-badge&color=blue)
![GitHub Repo stars](https://img.shields.io/github/stars/Det-rovv/Clew?style=for-the-badge)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/Det-rovv/Clew?style=for-the-badge&color=blue)

A web service designed to simplify and automate downloading modpacks. It provides a unified API that allows you to flexibly download content from Modrinth and Curseforge (more sources will be added soon)
## Key Features
- **Unified API** - get everything you need through a single interface
- **Automatic dependency resolution** - you won't have to worry about dependencies
- **Flexible request configuration** - fine-tune how your request should be executed
## Table of Contents
*   [Key Features](#key-features)
*   [Getting Started](#getting-started)
    *   [Setup](#setup)
        *   [Dotnet CLI](#dotnet-cli)
        *   [Docker](#docker)
    *   [Configuration](#configuration)
*   [Usage](#usage)
*   [Roadmap](#roadmap)
## Getting started
### Setup
#### Dotnet CLI
Requirements:
- .NET 9 SDK
1. Check your dotnet version, it should be `9.*.*`. Install it if you don't have it.
```shell
dotnet --version
```
2. Clone the repository:
```shell
git clone https://github.com/Det-rovv/Clew
cd ./Clew/
```
3. In the Clew.Api project you can find the appsettings.Example.json file, it contains a configuration template that you can use to configure the application. 
4. Create an appsettings.{Environment}.json file in the same directory and configure it as shown below
5. Build and run the Api project via the dotnet cli:
```shell
dotnet restore
dotnet run --project ./src/Clew.Api/
```
#### Docker
1. Clone the repository:
```shell
git clone https://github.com/Det-rovv/Clew
cd ./Clew/
```
2. Copy .env.example to .env and configure it as shown below
3. Build image
```shell
docker build -t clew -f ./src/Clew.Api/Dockerfile .
```
4. Run container
```shell
docker run --env-file .env -p 8080:8080 clew
```
### Configuration
Both options, .env and appsettings.json have the same structure, but here I will show the configuration using the example of json, as it is more readable.
Here is an example of the application configuration:
```json
{
  "ModrinthSettings": {
    "ApiName": "modrinth",
    "BaseUrl": "https://api.modrinth.com/v2/",

    "MaxRequestsPerMinute": 300,
    "MaxItemsPerRequest": 800,
    "BatchRequestItemsCountThreshold": 10
  },

  "CurseForgeSettings": {
    "ApiName": "curseforge",
    "BaseUrl": "https://api.curseforge.com/v1/",
    
    "ApiKey": "YOUR_API_KEY"
  },

  "ConcurrencySettings": {
    "ItemsPerThread": {
      "ProjectDataFetching": 2,
      "ProjectDataProcessing": 3
    }
  },

  "ContentSourceNamingsSettings": {
    "Platforms": {
      "neoforge": {
        "modrinth": "neoforge",
        "curseforge": "NeoForge"
      },
      "fabric": {
        "modrinth": "fabric",
        "curseforge": "Fabric"
      },
      "iris": {
        "modrinth": "iris",
        "curseforge": "Iris"
      },
      "optifine": {
        "modrinth": "optifine",
        "curseforge": "OptiFine"
      }
    },

    "ReleaseChannels": {
      "release": {
        "modrinth": "release",
        "curseforge": "1"
      },
      "beta": {
        "modrinth": "beta",
        "curseforge": "2"
      },
      "alpha": {
        "modrinth": "alpha",
        "curseforge": "5"
      }
    },
          
    "RelationTypes": {
      "RequiredDependency": {
        "modrinth": "required",
        "curseforge": "3"
      },
      "OptionalDependency": {
        "modrinth": "optional",
        "curseforge": "2"
      },
      "Incompatible": {
        "modrinth": "incompatible",
        "curseforge": "5"
      }
    }
  }
}
```
Settings explanation:

| Key                     | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
|-------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|{Source}Settings.ApiName | The name of the content source to be accessed when requesting Clew                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
|{Source}Settings.BaseUrl | The api url of a content source that all requests routes are based on.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
|ModrinthSettings.MaxRequestsPerMinute| Limit on the number of requests to the modrinth api per minute, by default 300                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
|ModrinthSettings.MaxItemsPerRequest| Limit on the number of items within a single batch request to the modrinth api. At the time of writing the README, the recommended value is 800                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
|ModrinthSettings.BatchRequestItemsCountThreshold| The threshold value for the number of projects, after crossing which the batch strategy for resolving the list of projects will be used.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
|CurseForgeSettings.ApiKey| Api key required for api requests                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
|ConcurrencySettings.ItemsPerThread.ProjectDataFetching| The number of projects that one thread is allocated to request information about in the pipeline strategy for resolving project lists.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
|ConcurrencySettings.ItemsPerThread.ProjectDataProcessing| The number of projects for which one thread is allocated in the pipeline strategy for resolving project lists.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
|ContentSourceNamingsSettings.{TranslatableNameCategory}.{CommonName}.{ContentSourceName}.{SpecificName}| This application uses a system for translating specific names for various content sources into a unified one that the client will use in communication with the Clew API. For example, in the Modrinth API, the relationship between projects denoting the required dependency is called "required", and in the CurseForge API: "3". To describe how these names should be translated, there is this configuration section<br>So:<br>**"TranslatableNameCategory"** is a category of translatable names, for example, **"Platforms"** are platforms for which project versions are released, for example, "fabric" for mods, and "iris" for shaders<br>**"CommonName"** is the name that the user will specify when accessing the Clew API<br>**"ContentSourceName"** is the name of the content source for which this translation is intended<br>Important: it must match this configuration value for this source "{Source}Settings.ApiName"<br>**"SpecificName"** is a specific name for any source, for example, in the example above it is "3" for CurseForge |
## Usage
Currently the service has only one endpoint: `/projects/resolve/list`. It handles POST requests containing an object of the following format in the body:
```json
{
  "defaultGameVersions": [ "1.21.1" ],
  "defaultPlatforms": ["fabric"],
  "defaultReleaseChannel": "atLeastBeta",
  "excludedProjects": [
    {
      "contentSourceName": "modrinth",
      "id": "fabric-api"
    }
  ],
  "projects": [
    {
      "contentSourceName": "modrinth",
      "id": "sodium",
      "gameVersions": [ "1.21" ],
      "platforms": [ "neoforge" ],
      "releaseChannel": "any"
    }
  ]
}
```
- "defaultGameVersions" - default versions of the game for the entire modpack
- "defaultPlatforms" - default platforms for the entire modpack (e.g. fabric, iris)
- "defaultReleaseChannel" - which release types are valid for your modpack ("any", "atLeastBeta", "releaseOnly")
- "excludedProjects" - projects that should be ignored if they are dependencies of any mods
- "contentSourceName" - content source name, must match the one specified in the configuration
- "id" - unique identifier of the project in this source
- "projects" - list of projects you want to download
- "gameVersions" - game versions for this project, has a higher priority than the default versions
- "platforms" - platforms for this project, has a higher priority than the default platforms
- "releaseChannel" - release channel for this project, overrides the default value

### Example Request
`POST /projects/resolve/list`

**Body:**
```json
{
  "defaultGameVersions": [
    "1.21.1", "1.21"
  ],
  "defaultPlatforms": [
    "neoforge", "fabric"
  ],
  "defaultReleaseChannel": "atLeastBeta",
  "excludedProjects": [
    {
      "contentSourceName": "modrinth",
      "id": "fabric-api"
    },
    {
      "contentSourceName": "curseforge",
      "id": "306612"
    }
  ],
  "projects": [
    {
      "contentSourceName": "modrinth",
      "id": "connector"
    },
    {
      "contentSourceName": "modrinth",
      "id": "malum"
    },
    {
      "contentSourceName": "modrinth",
      "id": "subtle-effects"
    },
    {
      "contentSourceName": "modrinth",
      "id": "iris"
    },
    {
      "contentSourceName": "modrinth",
      "id": "sodium-extra"
    },
    {
      "contentSourceName": "modrinth",
      "id": "botania",
      "gameVersions": [ "1.20.1" ]
    },
    {
      "contentSourceName": "modrinth",
      "id": "complementary-reimagined",
      "platforms": [ "iris" ]
    },
    {
      "contentSourceName": "curseforge",
      "id": "422301"
    },
    {
      "contentSourceName": "curseforge",
      "id": "233564"
    }
  ]
}
```
**Response:**
```json
{
  "initialProjectsDownloadUrls": [
    "https://cdn.modrinth.com/data/4q8UOK1d/versions/1F9HgTm5/SubtleEffects-neoforge-1.21.1-1.12.1.jar",
    "https://cdn.modrinth.com/data/u58R1TMW/versions/leZwcwvX/connector-2.0.0-beta.9%2B1.21.1-full.jar",
    "https://cdn.modrinth.com/data/YL57xq9U/versions/t3ruzodq/iris-neoforge-1.8.12%2Bmc1.21.1.jar",
    "https://cdn.modrinth.com/data/jgzwYsAN/versions/DPwO66EL/malum-1.21.1-1.7.3.1.jar",
    "https://cdn.modrinth.com/data/PtjYWJkn/versions/pFmw1eci/sodium-extra-neoforge-0.6.0%2Bmc1.21.1.jar",
    "https://cdn.modrinth.com/data/pfjLUfGv/versions/X2tY0LhB/Botania-1.20.1-450-FABRIC.jar",
    "https://cdn.modrinth.com/data/HVnmMxH1/versions/sAAjYvFB/ComplementaryReimagined_r5.5.1.zip",
    "https://edge.forgecdn.net/files/6962/762/sophisticatedbackpacks-1.21.1-3.25.2.1338.jar",
    "https://edge.forgecdn.net/files/6664/367/TechReborn-5.11.19.jar"
  ],
  "dependenciesDownloadUrls": [
    "https://cdn.modrinth.com/data/AANobbMI/versions/Pb3OXVqC/sodium-neoforge-0.6.13%2Bmc1.21.1.jar",
    "https://cdn.modrinth.com/data/Aqlf1Shp/versions/vQbH2z4u/forgified-fabric-api-0.115.6%2B2.1.1%2B1.21.1.jar",
    "https://cdn.modrinth.com/data/hYykXjDp/versions/ZWbJxzBB/fzzy_config-0.7.2%2B1.21%2Bneoforge.jar",
    "https://cdn.modrinth.com/data/bN3xUWdo/versions/46jCo03u/lodestone-1.21.1-1.7.1.jar",
    "https://cdn.modrinth.com/data/vvuO3ImH/versions/yohfFbgD/curios-neoforge-9.5.1%2B1.21.1.jar",
    "https://cdn.modrinth.com/data/nU0bVIaL/versions/h6hKI2ob/Patchouli-1.21.1-92-NEOFORGE.jar",
    "https://cdn.modrinth.com/data/ordsPcFz/versions/4qCjWixP/kotlinforforge-5.9.0-all.jar",
    "https://cdn.modrinth.com/data/5aaWibi9/versions/JagCscwi/trinkets-3.10.0.jar",
    "https://edge.forgecdn.net/files/6965/336/sophisticatedcore-1.21.1-1.3.70.1131.jar"
  ]
}
```
## Roadmap
This is a WIP project, here are a few things I would like to see implemented
- Other sources (including Guthub)
- Filtering releases by regular expressions
- Deploy service to a public hosting environment