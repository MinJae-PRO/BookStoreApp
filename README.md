BookStoreApp

BookStoreApp is a simple web application built using Blazor Server and Azure SQL Database. The goal of this project is to demonstrate clean project structure, basic authentication, and proper cloud-based database integration. The focus of the implementation is on code quality, readability, and secure configuration rather than complex business logic.
The application allows users to register, log in, and manage book records. All data is stored in Azure SQL Database and accessed using Entity Framework Core. The application is deployed to Azure App Service.

Tech Stack: 
This project is developed using Blazor Server as the front-end framework and Entity Framework Core for database interaction. Azure SQL Database is used as the persistent storage layer. The application is deployed to Azure App Service and uses SQL Server authentication for database access.

Implemented Features: 
The application includes user authentication functionality such as user registration, login, and logout. User credentials are validated against data stored in Azure SQL Database.
Book management functionality is also implemented. Users can create new book records, view a list of books, access detailed information for a specific book, update book information, and delete existing records.
All database operations are handled through a structured data access layer using Entity Framework Core. The connection string is not hardcoded in the source code. Instead, it is securely configured through Azure App Service settings.

Deployment Information: 
The application is deployed to Azure App Service.
Default domain: 
https://bookstoreapp-jae-gbb7hzgdesd8acdt.australiaeast-01.azurewebsites.net
Example page: 
https://bookstoreapp-jae-gbb7hzgdesd8acdt.australiaeast-01.azurewebsites.net/books
The Azure SQL firewall is configured to allow the App Service outbound IP addresses to access the database server.

Project Purpose:
This project demonstrates structured development practices using Blazor Server and Azure services. It focuses on proper separation of concerns, secure database configuration, and a complete but simple authentication workflow. The main objective is to show clean architecture, maintainable code, and correct cloud deployment setup.
