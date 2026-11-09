# ANDtr — Medical Device Safety & Recall Platform

**Module:** MOD003263 Software Engineering  
**Group:** G4  
**Framework:** ASP.NET Core 8.0 MVC | C# 12 | EF Core 8 | ClosedXML | Bootstrap 5  

---

## 1. Quickstart (How to Run)

To run the application locally:

```bash
# 1. Open terminal in the project folder
cd ANDtr

# 2. Run the application
dotnet run --launch-profile http
```

Open your browser and visit: **http://localhost:5000**  
*(Note: Use `http://`, not `https://`, for local development)*.

---

## 2. Test User Accounts

The database seeds automatically with all 616 recall notices and test accounts on first launch:

| Role | Email | Password | Landing Page |
| :--- | :--- | :--- | :--- |
| **Employee** | `employee@andtr.co.uk` | `Password123!` | Employee Dashboard (`/Dashboard/Employee`) |
| **Customer** | `customer@nhs.uk` | `Password123!` | Customer Portal (`/Dashboard/Customer`) |

---

## 3. Project Structure

The project follows standard ASP.NET Core MVC structure:

```text
ANDtr/
├── Controllers/       # MVC Controllers for all features
├── Models/            # Database entities (EF Core)
├── ViewModels/        # Strongly-typed models for views
├── Views/             # Razor views (.cshtml) organised by controller
│   ├── Accounts/
│   ├── Dashboard/
│   ├── Sharing/
│   ├── Import/
│   ├── Search/
│   ├── Filters/
│   ├── Reviews/
│   ├── Findings/
│   ├── SharedReviews/
│   ├── Notices/
│   ├── Customers/
│   ├── Reports/
│   └── Shared/        # Layouts (_Layout, _EmployeeLayout, _CustomerLayout)
├── Services/          # Business logic and database operations
├── data/              # Initial seed files (FDAdataset.xlsx)
├── docs/              # 15 User Stories specification
├── wwwroot/           # Static files (CSS, JS, images)
└── Program.cs         # App configuration and dependency injection
```

---

## 4. Team Member Contribution Mapping (15 User Stories)

Each team member is responsible for 3 user stories, along with their matching controllers, views, and services:

| Member | User Stories | Assigned Controllers & Services | Assigned Views |
| :--- | :--- | :--- | :--- |
| **Suryanath Sunil** | **US-01:** Login & Lockout<br>**US-02:** Role-Based Dashboards<br>**US-03:** Share Reviews with Customers | `AccountsController.cs`<br>`DashboardController.cs`<br>`SharingController.cs`<br>`ShareService.cs` | `Views/Accounts/`<br>`Views/Dashboard/`<br>`Views/Sharing/` |
| **Azlan Ahmed** | **US-04:** Import FDA Excel File (616 notices)<br>**US-05:** Search Notices by Keyword<br>**US-06:** Synonym Search & Highlighting | `ImportController.cs`<br>`SearchController.cs`<br>`ImportService.cs`<br>`NoticeSearchService.cs` | `Views/Import/`<br>`Views/Search/` |
| **Hassan Naveed** | **US-07:** Filter by Class, Firm & Date<br>**US-08:** Paged Results & Notice Selection Basket<br>**US-09:** Create & Edit Risk Reviews | `FiltersController.cs`<br>`ReviewsController.cs`<br>`ReviewService.cs` | `Views/Filters/`<br>`Views/Reviews/` |
| **Sanjida Afrin** | **US-10:** Clinical Findings Editor<br>**US-11:** Customer Portal Shared Reviews<br>**US-12:** Comments on Reviews | `FindingsController.cs`<br>`SharedReviewsController.cs`<br>`CommentService.cs` | `Views/Findings/`<br>`Views/SharedReviews/` |
| **Tamilore Adekanye** | **US-13:** Full Notice Details View<br>**US-14:** Customer Account Management<br>**US-15:** Print Review Report & Export CSV | `NoticesController.cs`<br>`CustomersController.cs`<br>`ReportsController.cs`<br>`CustomerService.cs`<br>`ReportService.cs` | `Views/Notices/`<br>`Views/Customers/`<br>`Views/Reports/` |

---

## 5. Automated Tests

Run the xUnit test suite (21 unit tests covering all 15 user stories):

```bash
dotnet test tests/ANDtr.Tests/ANDtr.Tests.csproj
```

---

## 6. Coursework Reference
The specification of all 15 User Stories with acceptance criteria is located in:
* `docs/TEAM_USER_STORIES_15.md`
