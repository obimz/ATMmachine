# ATM — Simple Console ATM Application

This is a small C# console ATM application (learning / demo project) that lets users
create accounts, login, view balance, deposit, withdraw and transfer funds between
customers stored in memory.

## Project structure

- ATM/ — main C# project containing the console app
  - Customer.cs — customer model and balance operations
  - Authentication.cs — in-memory customer store and account creation/login helpers
  - ATMactions.cs — console UI flow and actions (login, menu, deposit, withdraw, transfer)
  - ScreenDisplay.cs — small UI helper for printing menus and messages

## Requirements

- .NET SDK (6.0+ or the version used by the project)
- Visual Studio 2022/2026 or VS Code with C# extension

## Build and run

From the repository root you can use the CLI:

1. dotnet build
2. dotnet run --project ATM

Or open the solution in Visual Studio and press Run (F5) or Build (Ctrl+Shift+B).

## Usage

1. Start the app and follow prompts on the console.
2. At onboarding you can choose to login as an existing user or create a new user.
3. After login you can choose from the menu: show balance, deposit, withdraw, transfer, view transaction history, or exit.


---
Created for learning and demonstration purposes.
