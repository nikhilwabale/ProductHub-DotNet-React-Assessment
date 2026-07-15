# High-Level Architecture

- **Web:** Optional React client; stores the short-lived token and calls versioned JSON endpoints.
- **API:** HTTP concerns, Swagger, authentication, authorization, middleware and versioning.
- **Application:** DTOs, validation, mapping, service orchestration and contracts.
- **Domain:** Product, Item, user/token models, events, enums and domain exceptions.
- **Infrastructure:** EF Core SQL Server mappings, repositories, Unit of Work, JWT generation, seeding and logging integration.
- **Tests:** Service unit tests, infrastructure persistence tests and API integration tests.

Dependency direction keeps Domain independent and infrastructure replaceable.