# Profile Configuration API
An ASP.NET Core Web API that manages profile parameters, updates them periodically, and provides RESTful endpoints for access and modification.

- [Overview](#overview)
- [Technologies Used](#technologies-used)
- [Setup Instructions](#setup-instructions)
- [Running the API](#running-the-api)
- [API Endpoints](#api-endpoints)
- [Background Service](#background-service)
- [Testing](#testing)
- [Future Improvements](#future-improvements)

## Overview
This project is an ASP.NET Core Web API for managing profile parameters. It:
- Loads profile parameters at startup.
- Stores parameters in a dictionary.
- Includes a background service that updates parameters every 5 minutes.
- Provides API endpoints to retrieve and modify profile settings.

## Technologies Used
- .NET 7 / .NET 8
- ASP.NET Core Web API
- C#
- Dependency Injection
- Moq (for unit testing)
- xUnit (for unit tests)
- Swagger (API documentation)

## Setup Instructions
### Prerequisites
- git clone https://github.com/lucasbarbosa2/ProfileAndPermissions.git

## API Endpoints

### 🔹 Get All Profiles

**GET** `/api/profiles`

Response
```json
{
  "Admin": {
    "profileName": "Admin",
    "parameters": {
      "CanEdit": "false",
      "CanDelete": "true"
    }
  },
  "User": {
    "profileName": "User",
    "parameters": {
      "CanEdit": "false",
      "CanDelete": "false"
    }
  }
}
```
**POST** `/api/profiles`

Request
```json
{
  "profileName": "Guest",
  "parameters": {
    "CanView": "true"
  }
}
```

Response
```json
{
  "message": "Profile added successfully"
}
```
## Background Service
The background service (`ToggleConfigurationService`) runs every 5 minutes to update profile ('Admin','CanEdit') on and off every 5 minutes.

## Future Improvements
- Improve concurrency handling
- Add authentication & authorization
- Implement caching
- Use appsettings for initial profile setup

