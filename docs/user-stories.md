# Learning Center Platform — REST API Technical Stories

## Overview

This document contains API-focused technical stories intended for frontend or mobile developers integrating with the Learning Center Platform REST API (ASP.NET Core / .NET 10).

Common conventions:
- Base path: `/api/v1`
- Content-Type: `application/json`
- Error responses follow standard RFC 9457 `ProblemDetails` format (`type`, `title`, `status`, `detail`, `instance`).
- Authentication: Standard HTTP Bearer token via `Authorization: Bearer <token>` header.

---

## Bounded Context: Publishing

The Publishing context manages tutorials, attached multimedia assets, and categories across the platform.

### TS-PUB-001 — Create a Tutorial
As a frontend developer, I want to create a tutorial through the API so that I can implement the tutorial creation flow in the instructor UI, persist tutorial metadata, and receive the created tutorial to display or link from course/learning-path screens.

Acceptance criteria:
- **Scenario: Successful create**
  - Given a `POST` request to `/api/v1/tutorials` is received with a body containing: `title`, `summary`, `categoryId`
  - When the API validates and persists the tutorial aggregate
  - Then the API responds `201 Created` with a `Location` header (`/api/v1/tutorials/{id}`) and returns `TutorialResource` (id, title, summary, categoryId, status, assets)
- **Scenario: Category not found**
  - Given a `POST` request to `/api/v1/tutorials` with a non-existent `categoryId`
  - Then the API responds `404 Not Found` with a problem details payload
- **Scenario: Duplicate title or validation error**
  - Given a `POST` request to `/api/v1/tutorials` with missing required fields or duplicate title
  - Then the API responds `400 Bad Request` or `409 Conflict`

---

### TS-PUB-002 — Get a Tutorial by id
As a frontend developer, I want to fetch a tutorial by `{id}` so that I can implement the tutorial detail view, show metadata and any attached assets (videos), and handle not-found states in the UI.

Acceptance criteria:
- **Scenario: Found**
  - Given a `GET` request to `/api/v1/tutorials/{id}` is received
  - When the API finds the tutorial
  - Then the API responds `200 OK` and returns `TutorialResource`
- **Scenario: Not found**
  - Given a `GET` request to `/api/v1/tutorials/{id}` is received for a non-existent `{id}`
  - When the API does not find the tutorial
  - Then the API responds `404 Not Found`

---

### TS-PUB-003 — Get all Tutorials
As a frontend developer, I want to list tutorials so that I can implement tutorial catalogue/listing screens and support category filtering via the category endpoints.

Acceptance criteria:
- Given a `GET` request to `/api/v1/tutorials` is received
- When the API returns tutorials
- Then the API responds `200 OK` with an array of `TutorialResource` items (empty array if no tutorials exist)

---

### TS-PUB-004 — Add a Video Asset to a Tutorial
As a frontend developer, I want to add a video asset to a tutorial so that I can implement an instructor workflow to attach media to tutorials and obtain the updated tutorial resource for the UI.

Acceptance criteria:
- **Scenario: Successful add**
  - Given a `POST` request to `/api/v1/tutorials/{id}/videos` is received with a body containing: `videoUrl`
  - When the API attaches the video asset to the tutorial
  - Then the API responds `201 Created` with `TutorialResource`
- **Scenario: Tutorial not found**
  - Given a `POST` request to `/api/v1/tutorials/{id}/videos` for a non-existent `{id}`
  - Then the API responds `404 Not Found`
- **Scenario: Invalid URL**
  - Given a `POST` request with an invalid or empty `videoUrl`
  - Then the API responds `400 Bad Request`

---

### TS-PUB-005 — Categories: Create, Get by id, Get all, and Tutorials by Category
As a frontend developer, I want to manage categories and obtain tutorials by category so that I can implement category-based browsing and tutorial organization in the UI.

Acceptance criteria:
- **Create category**
  - Given a `POST` request to `/api/v1/categories` is received with a body containing: `name`
  - When the API validates and persists the category
  - Then the API responds `201 Created` with a `Location` header (`/api/v1/categories/{id}`) and returns `CategoryResource`
  - If validation fails, responds `400 Bad Request`
- **Get category by id**
  - Given a `GET` request to `/api/v1/categories/{id}` is received
  - When found, responds `200 OK` with `CategoryResource`
  - If not found, responds `404 Not Found`
- **Get all categories**
  - Given a `GET` request to `/api/v1/categories` is received
  - Then the API responds `200 OK` with an array of `CategoryResource` items
- **Get all tutorials by category**
  - Given a `GET` request to `/api/v1/categories/{categoryId}/tutorials` is received
  - When the category exists, responds `200 OK` with an array of `TutorialResource` items

---

## Bounded Context: Profiles

The Profiles context manages learner and instructor profile identities.

### TS-PROF-001 — Create a Profile
As a frontend developer, I want to create a profile through the API so that I can implement profile creation and onboarding flows in the UI, store contact/address data, and receive the created profile to continue onboarding or navigation.

Acceptance criteria:
- **Scenario: Successful create**
  - Given a `POST` request to `/api/v1/profiles` is received with a body containing: `firstName`, `lastName`, `email`, `street`, `number`, `city`, `postalCode`, `country`
  - When the API validates and persists the profile aggregate
  - Then the API responds `201 Created` with a `Location` header (`/api/v1/profiles/{id}`) and returns `ProfileResource` (id, fullName, email, streetAddress)
- **Scenario: Duplicate email**
  - Given a `POST` request with an email that already exists
  - Then the API responds `409 Conflict` or `400 Bad Request`
- **Scenario: Validation error**
  - Given a `POST` request with missing or invalid fields
  - Then the API responds `400 Bad Request`

---

### TS-PROF-002 — Get Profile by id
As a frontend developer, I want to fetch a profile by `{id}` so that I can implement profile detail screens, pre-fill forms when editing, and handle not-found states.

Acceptance criteria:
- Given a `GET` request to `/api/v1/profiles/{id}` is received
- When found, responds `200 OK` with `ProfileResource`
- When not found, responds `404 Not Found`

---

### TS-PROF-003 — Get all Profiles
As a frontend developer, I want to list profiles so that I can implement admin or directory views.

Acceptance criteria:
- Given a `GET` request to `/api/v1/profiles` is received
- Then the API responds `200 OK` with an array of `ProfileResource` items

---

## Bounded Context: IAM (Identity & Access Management)

The IAM context provides RESTful user management and credential-based token authentication.

### TS-IAM-001 — Create User (Sign-up)
As a frontend developer, I want to register a new user through a RESTful endpoint so that I can implement user registration flows, securely store user credentials, and receive the created user identity resource.

Acceptance criteria:
- **Scenario: Successful user registration**
  - Given an anonymous `POST` request to `/api/v1/users` is received with a body containing: `username`, `password`
  - When the API validates credentials and persists the user aggregate
  - Then the API responds `201 Created` with a `Location` header (`/api/v1/users/{id}`) and returns `UserResource` (`id`, `username`)
- **Scenario: Username already taken**
  - Given a `POST` request to `/api/v1/users` with an existing `username`
  - Then the API responds `409 Conflict` (or `400 Bad Request`) with a descriptive problem details error
- **Scenario: Invalid input**
  - Given a `POST` request with a blank username or password
  - Then the API responds `400 Bad Request`

---

### TS-IAM-002 — Create Authentication Token (Sign-in)
As a frontend developer, I want to authenticate user credentials and issue a security token via a RESTful resource endpoint so that I can establish user sessions and make authenticated API calls.

Acceptance criteria:
- **Scenario: Successful authentication**
  - Given an anonymous `POST` request to `/api/v1/tokens` is received with a body containing: `username`, `password`
  - When credentials match an existing user
  - Then the API responds `200 OK` and returns `AuthenticatedUserResource` (`id`, `username`, `token`)
- **Scenario: Invalid credentials**
  - Given a `POST` request to `/api/v1/tokens` with incorrect password or non-existent username
  - Then the API responds `400 Bad Request` or `401 Unauthorized` with an error message
- **Scenario: Legacy endpoint backward compatibility**
  - The legacy `POST /api/v1/authentication/sign-in` and `POST /api/v1/authentication/sign-up` endpoints remain available with `[Obsolete]` annotations for backward compatibility.

---

### TS-IAM-003 — Get User by id
As an authenticated client, I want to fetch user details by `{id}` so that I can verify account existence and retrieve username metadata.

Acceptance criteria:
- Given an authenticated `GET` request to `/api/v1/users/{id}` with a valid JWT Bearer token
- When found, responds `200 OK` with `UserResource` (`id`, `username`)
- When not found, responds `404 Not Found`
- When unauthenticated or invalid token, responds `401 Unauthorized`
- When unmapped route, handled gracefully by middleware

---

### TS-IAM-004 — Get all Users
As an authenticated client, I want to retrieve all registered users so that I can manage user directories.

Acceptance criteria:
- Given an authenticated `GET` request to `/api/v1/users` with a valid JWT Bearer token
- Then the API responds `200 OK` with an array of `UserResource` items
- When unauthenticated or invalid token, responds `401 Unauthorized`
