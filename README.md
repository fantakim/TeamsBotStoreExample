# Sample: Maintaining Notification Connection via Azure Blob Storage

This sample demonstrates how to maintain a notification connection through Microsoft Teams using Azure Blob Storage. 
Below are prerequisites for using the sample and an explanation of key components.

## Prerequisites
- Microsoft Teams installed and an account is required.
- .NET SDK version 6.0 needs to be installed.
- Visual Studio .NET 2022 should be installed.

## Key Components
### 1. BlobStore.cs
The `BlobStore.cs` file provides essential functionality for connecting to Azure Blob Storage to maintain a notification connection.

### 2. Connection String
For local testing, the "StoreConnectionString" in the `appsettings.json` file represents the connection string value for Azurite. Currently, it is set to "UseDevelopmentStorage=true."

## Local Testing
1. Build and run the project.
2. Refer to "GettingStarted.txt" for the basic operational instructions.

## Reference Documentation
For more detailed information and usage guidelines, consult "GettingStarted.txt."
