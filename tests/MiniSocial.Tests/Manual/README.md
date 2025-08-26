# Manual Testing

This folder contains files for manual testing and exploration of the API.

## Api.http

This file contains HTTP requests that can be executed directly in VS Code using the REST Client extension.

### Requirements
- Install the "REST Client" extension in VS Code
- Ensure the API is running locally (`dotnet run` from the MiniSocial.Api project)

### Usage
1. Open `Api.http` in VS Code
2. Click "Send Request" above any HTTP request
3. View responses in the split pane

### Available Tests
- **User Registration**: Test various registration scenarios
  - Valid user creation
  - Duplicate username handling
  - Duplicate email handling
  - Invalid data validation

### Note
These manual tests complement the automated integration tests in the `Integration` folder. The automated tests run with `dotnet test`, while these HTTP files are for manual exploration and ad-hoc testing during development.
