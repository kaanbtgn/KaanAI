# KaanAI - Chatbot Web API

A .NET 9 Web API project that provides chatbot functionality using clean architecture, code-first approach with SQL Server, and generic repository with unit of work pattern.

## Architecture

This project follows Clean Architecture principles with the following layers:

### 1. **KaanAI.API** (Presentation Layer)
- Controllers that handle HTTP requests
- API endpoints for chat functionality
- Dependency injection configuration
- Swagger/OpenAPI documentation

### 2. **KaanAI.Application** (Application Layer)
- Business logic implementation
- Service classes that orchestrate domain operations
- DTOs for data transfer between layers

### 3. **KaanAI.Application.Abstraction** (Application Contracts)
- Interfaces for repositories and services
- DTOs for API requests and responses
- Contracts that define the application's public API

### 4. **KaanAI.Domain** (Domain Layer)
- Entity models (ChatSession, Question, Answer, ErrorLog)
- Domain business rules and validations
- Core business entities

### 5. **KaanAI.Persistance** (Infrastructure Layer)
- Entity Framework DbContext
- Repository implementations
- Unit of Work pattern implementation
- Database migrations

## Features

- **Chat Sessions Management**: Create and manage chat sessions
- **Message Handling**: Add questions and answers to sessions
- **User Sessions**: Retrieve all sessions for a specific user
- **Clean Architecture**: Separation of concerns with proper layering
- **Generic Repository**: Reusable data access pattern
- **Unit of Work**: Transaction management and consistency
- **Code-First**: Database schema generated from entity models
- **SQL Server**: Microsoft SQL Server database backend

## API Endpoints

### Chat Sessions
- `POST /api/chat/sessions` - Create a new chat session
- `GET /api/chat/sessions/{id}` - Get a specific chat session with messages
- `GET /api/chat/sessions/user/{userId}` - Get all sessions for a user

### Messages
- `POST /api/chat/questions` - Add a question to a session
- `POST /api/chat/answers` - Add an answer to a session

### Alternative Session Creation
- `POST /api/session` - Create a session (alternative endpoint)

## Database Schema

### ChatSession
- `Id` (Primary Key)
- `CreatedAt` (DateTime)
- `CreatedBy` (string, max 100 chars)
- `UpdatedAt` (DateTime)

### Question
- `Id` (Primary Key)
- `Content` (string, max 4000 chars)
- `AskedAt` (DateTime)
- `SessionId` (Foreign Key to ChatSession)

### Answer
- `Id` (Primary Key)
- `AnswerText` (string, max 4000 chars)
- `AnsweredAt` (DateTime)
- `SessionId` (Foreign Key to ChatSession)

### ErrorLog
- `Id` (Primary Key)
- `ErrorMessage` (string, max 4000 chars)
- `StackTrace` (string, max 8000 chars)
- `CreatedAt` (DateTime)

## Setup Instructions

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code

### Database Setup
1. Update the connection string in `KaanAI.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=KaanAIDb;User Id=sa;Password=myPassword123;TrustServerCertificate=True;"
  }
}
```

2. Run Entity Framework migrations:
```bash
cd KaanAI.API
dotnet ef database update
```

### Running the Application
1. Navigate to the API project:
```bash
cd KaanAI.API
```

2. Run the application:
```bash
dotnet run
```

3. Access Swagger UI at: `https://localhost:7001/swagger`

## Testing the API

Use the provided `KaanAI.API.http` file in Visual Studio or VS Code to test the endpoints:

1. **Create a session**:
```http
POST https://localhost:7001/api/chat/sessions
Content-Type: application/json

{
  "createdBy": "testuser@example.com"
}
```

2. **Add a question**:
```http
POST https://localhost:7001/api/chat/questions
Content-Type: application/json

{
  "sessionId": 1,
  "content": "What is the capital of France?"
}
```

3. **Add an answer**:
```http
POST https://localhost:7001/api/chat/answers
Content-Type: application/json

{
  "sessionId": 1,
  "answerText": "The capital of France is Paris."
}
```

4. **Get session details**:
```http
GET https://localhost:7001/api/chat/sessions/1
```

## Project Structure

```
KaanAI/
├── KaanAI.API/                    # Web API layer
│   ├── Controllers/               # API controllers
│   ├── Program.cs                 # Application startup
│   └── appsettings.json          # Configuration
├── KaanAI.Application/            # Application layer
│   └── src/Services/             # Business logic services
├── KaanAI.Application.Abstraction/ # Application contracts
│   └── src/                      # Interfaces and DTOs
├── KaanAI.Domain/                # Domain layer
│   └── src/                      # Entity models
└── KaanAI.Persistance/           # Infrastructure layer
    └── src/                      # Data access and EF context
```

## Design Patterns Used

1. **Repository Pattern**: Generic repository for data access
2. **Unit of Work Pattern**: Transaction management
3. **Dependency Injection**: Service registration and resolution
4. **DTO Pattern**: Data transfer objects for API communication
5. **Clean Architecture**: Separation of concerns across layers

## Future Enhancements

- Authentication and authorization
- Real-time chat using SignalR
- AI integration for automated responses
- Message encryption
- File upload support
- User management system
- Chat session archiving
- Analytics and reporting

## Contributing

1. Follow the existing code structure and naming conventions
2. Add appropriate unit tests for new features
3. Update documentation for any API changes
4. Ensure all database migrations are properly created

## License

This project is licensed under the MIT License. 