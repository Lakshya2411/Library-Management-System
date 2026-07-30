# Lakshya Library Management System

A premium, fully functional Library Management System built using **C# ASP.NET Core MVC**, **HTML5/CSS3**, **JavaScript**, and **MySQL**.

The system utilizes a custom-designed **glassmorphic dark slate UI** with glowing accent gradients, responsive layouts, FontAwesome icons, and interactive visual charts built using **Chart.js**.

---

## Key Features

1. **Analytical Dashboard**:
   - Dynamic metric counters (Total Books, Available Stock, Members Registry, Active Loans).
   - Interactive charts illustrating **Genre Distribution** and **Borrowing Trends**.
   - Immediate highlights panel listing overdue logs and recent transaction feeds.
2. **Book Catalog Management**:
   - Browse catalog listing with text query searches (ISBN/Title/Author) and genre dropdown filters.
   - Catalog CRUD forms (Add Books, Edit properties, view complete inventory profiles).
   - Availability progress meters and historical logs detail grid for each book.
3. **Member Registry**:
   - Register member profiles with email duplication protections.
   - Direct toggles to suspend or activate reader accounts.
   - View member cards alongside loan charts and aggregate paid/unpaid fine tallies.
4. **Checkout & Returns Logging**:
   - Check out active copies to registered readers.
   - Return transaction processes with stock updates and dynamic overdue fine calculations (assessed at $1.00 per day overdue).
   - Fine payment collection actions.

---

## Technical Stack

* **Backend Framework**: .NET 8.0 (ASP.NET Core MVC)
* **ORM / Database Provider**: Entity Framework Core + Pomelo MySQL Connector
* **Database Engine**: MySQL Server 8.0
* **Frontend styles**: Custom CSS3 styling + Bootstrap 5.0 base
* **Script / CDN integrations**: Chart.js, jQuery, Bootstrap JS, FontAwesome Icons

---

## Prerequisites

Ensure you have the following installed on your machine:

1. **.NET 8.0 SDK** or later
2. **MySQL Server 8.0** (running locally)

---

## Getting Started

### 1. Database Configuration

The application is configured to connect to your local MySQL instance. You can modify connection strings in [appsettings.json](file:///c:/Users/forrl/Library%20Management%20System/appsettings.json):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=library_db;User=root;Password=LAK242004;"
}
```

> [!NOTE]
> On startup, the application calls `db.Database.EnsureCreated()`. This **automatically creates the MySQL database** (`library_db`), constructs the tables, and seeds them with mock records (books, members, and transactions) so the application is ready to use immediately.

### 2. Run the Application

Open your terminal or command prompt and execute the following:

1. Navigate to the project root folder:
   ```bash
   cd "C:\Users\forrl\Library Management System"
   ```

2. Compile and run the project:
   ```bash
   dotnet run --launch-profile "http"
   ```

3. Open your browser and navigate to:
   👉 **[http://localhost:5075/](http://localhost:5075/)**
