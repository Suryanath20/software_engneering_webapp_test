# ANDtr Database Entity-Relationship (ER) Specification

## 1. Entity-Relationship Diagram

```mermaid
erDiagram
    ApplicationUser ||--o{ RiskReview : "authors"
    ApplicationUser ||--o{ ReviewShare : "receives access"
    ApplicationUser ||--o{ Comment : "writes"
    ApplicationUser ||--o{ ImportRecord : "executes"
    
    RiskReview ||--|{ ReviewNotice : "contains"
    RecallNotice ||--o{ ReviewNotice : "included in"
    
    RiskReview ||--o{ ReviewShare : "shared via"
    RiskReview ||--o{ Comment : "commented on"
    
    Comment ||--o{ Comment : "replies to"

    ApplicationUser {
        string Id PK
        string UserName
        string Email
        string FullName
        string Organization
        int UserType "0=Employee, 1=Customer"
        bool IsActive
        datetime CreatedAt
        datetime LastLoginAt
    }

    RecallNotice {
        int Id PK
        string RecallNumber UK "Indexed"
        string ProductDescription
        string ReasonForRecall
        string Classification "Class I, II, III (Indexed)"
        string RecallingFirm "Indexed"
        string DistributionPattern
        string CodeInfo
        string EventId
        string Status "Ongoing, Terminated, Completed"
        datetime CenterClassificationDate
        datetime TerminationDate
        datetime CreatedAt
    }

    RiskReview {
        int Id PK
        string Title
        int RiskScore "0=Low, 1=Moderate, 2=High, 3=Critical"
        int Status "0=Draft, 1=InProgress, 2=InReview, 3=Finalized"
        string AuthorId FK
        string Summary
        string ClinicalFindingsHtml
        datetime CreatedAt
        datetime UpdatedAt
        datetime FinalizedAt
    }

    ReviewNotice {
        int RiskReviewId PK, FK
        int RecallNoticeId PK, FK
    }

    ReviewShare {
        int Id PK
        int RiskReviewId FK
        string CustomerId FK
        datetime SharedAt
        bool IsActive "Indexed"
        datetime RevokedAt
        string Notes
        bool AwaitingReply
    }

    Comment {
        int Id PK
        int RiskReviewId FK
        string AuthorId FK
        string Message
        datetime CreatedAt
        bool IsHospitalQuery
        int ParentCommentId FK "Nullable"
    }

    ImportRecord {
        int Id PK
        string FileName
        int TotalRows
        int NewNoticesAdded
        int DuplicateNoticesIgnored
        string ImportedByUserId FK
        datetime ImportedAt
        string Notes
    }
```

---

## 2. Table Specifications & Indexes

### 2.1 `RecallNotices`
Stores all FDA recall records extracted from external datasets or live feeds.
- **Primary Key:** `Id` (INT, Auto Increment)
- **Indexes:**
  - `IX_RecallNotices_RecallNumber` (UNIQUE) — Guarantees fast lookup and prevents duplicate recall imports.
  - `IX_RecallNotices_Classification` — Optimizes filtering on Class I, II, and III.
  - `IX_RecallNotices_RecallingFirm` — Optimizes firm-specific queries and aggregations.
  - `IX_RecallNotices_CenterClassificationDate` — Enables efficient date range searches and sorting.

### 2.2 `RiskReviews`
Clinical evaluations and regulatory assessments authored by ANDtr employees.
- **Primary Key:** `Id` (INT, Auto Increment)
- **Foreign Keys:**
  - `AuthorId` -> `AspNetUsers(Id)` (ON DELETE RESTRICT)
- **Indexes:**
  - `IX_RiskReviews_Status` — Filters dashboard lists (Draft, Finalized, etc.).
  - `IX_RiskReviews_RiskScore` — Filters reviews by severity level.
  - `IX_RiskReviews_CreatedAt` — Default reverse-chronological ordering.

### 2.3 `ReviewNotices`
Many-to-many join table associating recall notices with risk reviews.
- **Composite Primary Key:** (`RiskReviewId`, `RecallNoticeId`)
- **Foreign Keys:**
  - `RiskReviewId` -> `RiskReviews(Id)` (ON DELETE CASCADE)
  - `RecallNoticeId` -> `RecallNotices(Id)` (ON DELETE RESTRICT)

### 2.4 `ReviewShares`
Grants read access and tracks active collaboration states between ANDtr reviews and external customer accounts.
- **Primary Key:** `Id` (INT, Auto Increment)
- **Foreign Keys:**
  - `RiskReviewId` -> `RiskReviews(Id)` (ON DELETE CASCADE)
  - `CustomerId` -> `AspNetUsers(Id)` (ON DELETE RESTRICT)
- **Indexes:**
  - `IX_ReviewShares_RiskReviewId_CustomerId` — Prevents duplicate share links.
  - `IX_ReviewShares_CustomerId_IsActive` — Fast lookup for customer dashboard active views.

### 2.5 `Comments`
Threaded discussion records on risk reviews between customers and employees.
- **Primary Key:** `Id` (INT, Auto Increment)
- **Foreign Keys:**
  - `RiskReviewId` -> `RiskReviews(Id)` (ON DELETE CASCADE)
  - `AuthorId` -> `AspNetUsers(Id)` (ON DELETE RESTRICT)
  - `ParentCommentId` -> `Comments(Id)` (ON DELETE NO ACTION / Nullable)

### 2.6 `ImportRecords`
Auditing entity logging every Excel import execution, user who performed it, and metrics.
- **Primary Key:** `Id` (INT, Auto Increment)
- **Foreign Keys:**
  - `ImportedByUserId` -> `AspNetUsers(Id)` (ON DELETE RESTRICT)
