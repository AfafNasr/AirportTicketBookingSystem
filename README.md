# ✈️ Airport Ticket Booking System

A console-based Airport Ticket Booking System developed using **C#** and **.NET**, following **Clean Architecture** principles and **SOLID** design practices.

The system provides separate functionalities for **Passengers** and **Managers**, supports secure authentication, flight management, booking operations, CSV flight import, and JSON-based data persistence.

---

# Project Architecture

The project is organized into four main layers:

```
ConsoleApp
    │
    ▼
Application
    │
    ▼
Domain
    ▲
    │
Infrastructure
```

- **Domain** → Core business entities and enums.
- **Application** → Business logic, services, DTOs, and interfaces.
- **Infrastructure** → JSON repositories, file storage, password hashing, and data seeding.
- **ConsoleApp** → User interface and application workflow.

---

# Features

## Passenger

- Register a new account.
- Secure login.
- Browse all available flights.
- Search flights using multiple filters.
- Book a flight.
- View personal bookings.
- Modify an existing booking.
- Cancel a booking.

---

## Manager

- Import flights from a CSV file.
- Validate imported flight data.
- Display detailed validation errors.
- Filter bookings using multiple criteria:
  - Flight
  - Passenger
  - Price
  - Departure Country
  - Destination Country
  - Departure Date
  - Departure Airport
  - Arrival Airport
  - Travel Class
- Display dynamically generated flight validation metadata.

---

# Data Storage

The project uses **JSON files** as the persistence layer.

Stored data includes:

- Users
- Passenger Profiles
- Flights
- Bookings

---

# Security

Passwords are never stored as plain text.

The system hashes passwords using **SHA-256** before saving them.

---

# Running the Project

1. Clone the repository.
2. Open the solution using Visual Studio.
3. Build the solution.
4. Run **AirportTicketBookingSystem.ConsoleApp**.

---
