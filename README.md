Pension Contribution Management System

An ASP.NET Core API application for managing pension contributions, user authentication, employer management, benefit calculations, and reporting. This system includes SMS notifications and follows event-driven principles for handling contributions and benefit updates.

Prerequisites

.NET SDK 7.0

SQL Server

Visual Studio or another C# IDE

Setup and Run Instructions

1. Clone the Repository

Clone the project to your local machine and navigate to the project directory:

git clone https://github.com/pecomen/Pension-Contribution-Management-System.git
cd Pension-Contribution-Management-System

2. Configure the Database Connection

Open appsettings.json in the PensionContributionManagementSystem.Api project and update the connection string:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PensionDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}

3. Apply Database Migrations

Open a terminal or command prompt, navigate to the PensionContributionManagementSystem.Api directory, and run the following commands:

cd PensionContributionManagementSystem.Api
dotnet ef database update

4. Run the Application

You can run the application using your IDE or via the command line.

Using Visual Studio

Open the solution file (PensionContributionManagementSystem.sln) in Visual Studio.

Set PensionContributionManagementSystem.Api as the startup project.

Press F5 to build and run the application.

Using Command Line

Navigate to the PensionContributionManagementSystem.Api directory and run:

dotnet run

5. Access the Application

API Base URL: https://localhost:5001/ or http://localhost:5000/

Swagger UI: https://localhost:5001/swagger

API Endpoints

AuthController (User Authentication)

POST /auth/register → Register a new user.

POST /auth/login → Authenticate and obtain a token.

EmployerController (Employer Management)

POST /api/employers → Register a new employer.

GET /api/employers/{employerId} → Get employer details with associated members.

MemberController (Member Management)

GET /api/members/{memberId} → Retrieve member details.

PUT /api/members/{memberId} → Update member details.

DELETE /api/members/{memberId} → Soft delete a member.

ContributionController (Pension Contributions)

POST /api/contributions → Add a new contribution.

GET /api/contributions/{memberId} → Get contributions for a specific member.

GET /api/contributions/transactions/{memberId} → Retrieve transaction history.

BenefitController (Pension Benefits)

GET /api/benefits/{memberId} → Calculate benefits for a member.

PUT /api/benefits/update-eligibility → Update benefit eligibility statuses.

ReportController (Pension Reports)

POST /api/reports/generate-validation-report → Generate a contribution validation report.

POST /api/reports/generate-member-statements → Generate pension statements for members.

Event Publishing

This system supports an event-driven architecture with the following events:

contribution_added → Triggered after a successful contribution is recorded.

benefit_calculated → Triggered when a member's benefits are calculated.

eligibility_updated → Triggered when benefit eligibility statuses are updated.

Troubleshooting

Database Connection Issues

Ensure SQL Server is running and the connection string in appsettings.json is correct.

Try connecting to the database using SQL Server Management Studio (SSMS) to verify access.

CORS Issues

Ensure CORS is properly configured in Program.cs.

You can modify the CORS policy to allow requests from specific origins.

Port Conflicts

If ports 5000 or 5001 are already in use, modify launchSettings.json or specify a different port when running the app:

dotnet run --urls=http://localhost:5050

Assumptions

The database is initialized with necessary tables and schema.

Environment variables for database connections and JWT authentication are set up.

The default ports for SQL Server and the API are being used. Adjust in configurations if needed.

License

This project is licensed under the MIT License.

Contributor

Your Name - pecomen