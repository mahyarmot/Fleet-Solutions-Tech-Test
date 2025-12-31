# Healthcare API - .NET Coding Challenge Solution

A production-ready RESTful API for managing patient information, built with .NET 8 using Clean Architecture, CQRS, and modern C# patterns.

## Architecture

This solution implements **Clean Architecture** with clear separation of concerns:

NhsTechTest/
- src
  - NhsTechTest.Api               # API controllers and configuratioon
  - NhsTechTest.Domain            # Entities and enterprise business rules
  - NhsTechTest.Application       # Application business rules
  - NhsTechTest.Infrastructure    # Data access
- tests
  - Tests                         # Unit tests
 
### Layer Responsibilities

- **Domain**: Core business entities and repository contracts
- **Application**: Use cases (queries/commands), DTOs, and business logic orchestration
- **Infrastructure**: Data access implementation (in-memory repository)
- **API**: HTTP endpoints, request/response handling, and dependency injection configuration

##  Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Your favorite IDE (Visual Studio 2022, VS Code). I used Version Visual Studio 2022 , Version 17.14.19

### Running the Application
1. **Clone the repository**
```bash
git clone https://github.com/mahyarmot/Fleet-Solutions-Tech-Test.git
cd Fleet-Solutions-Tech-Test
```

2. **Restore dependencies**
```bash
dotnet restore
```

3. **Run the API**
```bash
cd src/NhsTechTest.Api
dotnet run
```

4. **Access Swagger UI**
```
https://localhost:7216
```

The Swagger UI provides interactive API documentation and allows you to test endpoints directly.

## 📋API Endpoints

### Get Patient by ID

```http
GET /api/patients/{id}
```

**Success Response (200 OK):**
```json
{
  "id": 1,
  "nhsNumber": "485 777 3456",
  "name": "Sarah Johnson",
  "dateOfBirth": "1985-03-15T00:00:00Z",
  "age": 39,
  "gpPractice": "North Medical Centre"
}
```

**Not Found Response (404):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Patient.NotFound",
  "status": 404,
  "detail": "Patient with ID 999 was not found"
}
```

**Validation Error (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Patient.InvalidId",
  "status": 400,
  "detail": "Patient ID must be a positive number"
}
```

## Sample Data

The in-memory repository is seeded with 5 example patients:

| ID | NHS Number   | Name              | Date of Birth | GP Practice                |
|----|--------------|-------------------|---------------|----------------------------|
| 1  | 485 777 3456 | Sarah Johnson     | 1985-03-15    | North Medical Centre   |
| 2  | 943 476 5892 | James Anderson    | 1978-07-22    | Wst Health Clinic         |
| 3  | 562 839 1047 | Emily Williams    | 1992-11-08    | Greenfield Surgery         |
| 4  | 721 304 8569 | Mohammed Ali      | 1965-05-30    | Central Medical Practice   |
| 5  | 894 652 7103 | Catherine Brown   | 2001-09-17    | Riverside Medical Centre   |


## 📊 Test Coverage

The solution includes comprehensive tests covering:

- ✅ Happy path scenarios
- ✅ Error cases (not found, validation)
- ✅ Edge cases (boundary values, cancellation)
- ✅ Domain entity validation
- ✅ Repository interaction verification
- ✅ DTO mapping correctness

## DevOps
I have added a CI/CD pipeline to deploy the Swagger UI documentation page to the GitHub Pages. Since there is no server to handle the requests, this page only provides information about the implemented UI. The page is accessible on https://localhost:7216/swagger/index.html.

(Since this was a tricky solution, it made a mess in the commit history of the main branch. Sorry about the spam commits !)

## Technologies Used

- **.NET 8**: Latest LTS version
- **MediatR**: CQRS implementation
- **ErrorOr**: Functional error handling
- **Swashbuckle**: OpenAPI/Swagger documentation
- **xUnit**: Testing framework
- **FluentAssertions**: Expressive test assertions
- **Moq**: Mocking framework

## What I'd Add for Production
If this were a real production system, I would add:

### Infrastructure
- [ ] Entity Framework Core with SQL Server/PostgreSQL
- [ ] Database migrations
- [ ] Health checks (`/health`)
- [ ] Application Insights or similar APM

### API Enhancements
- [ ] API versioning
- [ ] Rate limiting
- [ ] Authentication & Authorization (JWT)
- [ ] Request/Response logging middleware
- [ ] Global exception handling middleware
- [ ] Input validation with FluentValidation
- [ ] Response caching

### Additional Features
- [ ] Pagination for list endpoints
- [ ] Filtering and sorting
- [ ] HATEOAS links
- [ ] Audit logging
- [ ] Soft delete support

### DevOps
- [ ] Docker containerization
- [ ] CI/CD pipeline deploying to a server (Azure / AWS)
- [ ] Infrastructure as Code (Terraform/Bicep)
- [ ] Automated security scanning
