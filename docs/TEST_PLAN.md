# ANDtr Platform Test Plan & Quality Assurance Matrix

## 1. Overview and Scope

This document specifies the comprehensive manual and functional test matrix for the **ANDtr Medical Device Safety and Recall Notice Platform**. 
Testing covers all workflows across both primary roles:
- **Employee (Compliance Officer)**
- **Customer (Hospital Representative / Health System Procurement)**

---

## 2. Test Execution Matrix (24 Test Cases)

| ID | Module / Feature | Role | Test Description | Expected Result | Priority |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-01** | Accounts / Auth | Public | Attempt access to `/Dashboard` without logging in | User is redirected to `/Accounts/Login?ReturnUrl=%2FDashboard`. | P0 |
| **TC-02** | Accounts / Auth | Public | Login with invalid credentials (`employee@andtr.test` / `WrongPass`) | Validation error: "Invalid login attempt. Please check your email and password." | P1 |
| **TC-03** | Accounts / Auth | Employee | Login with `employee@andtr.test` / `Employee#123` | Authentication succeeds; redirected to `/Dashboard`; top navigation shows "Compliance Portal" with Employee links. | P0 |
| **TC-04** | Accounts / Auth | Customer | Attempt navigating to `/Import` or `/Customers` as Customer | HTTP 403 Forbidden / redirected to `/Accounts/AccessDenied`. | P0 |
| **TC-05** | Dashboard | Employee | View Employee Dashboard at `/Dashboard` | Displays 4 KPI summary cards (Total Notices, Class I Alerts, Active Reviews, Awaiting Reply) and recent review queue. | P1 |
| **TC-06** | Dashboard | Customer | View Customer Dashboard at `/Dashboard` | Displays Customer KPI cards (Shared Reviews, Class I Alerts, Actions Required) and table of authorized shared reviews. | P1 |
| **TC-07** | Ingestion / Import | Employee | Upload valid FDA Excel file (`FDAdataset.xlsx`) via `/Import` | Ingestion completes; displays row count parsed, new notices inserted, and audit log table entry. | P0 |
| **TC-08** | Ingestion / Import | Employee | Re-upload the same FDA Excel file (`FDAdataset.xlsx`) | Idempotent duplicate check flags all 616 rows as existing; 0 new notices added; no duplicate key exceptions thrown. | P0 |
| **TC-09** | Ingestion / Import | Employee | Upload non-Excel file (e.g. `.txt` or empty file) | Client and server validation error: "Please select a valid .xlsx Excel workbook." | P2 |
| **TC-10** | Search & Discovery | Employee | Perform keyword search: "catheter" on `/Search` | Results grid displays matching notices containing "catheter" in Product Description or Reason with highlighted matches. | P1 |
| **TC-11** | Search & Discovery | Employee | Perform synonym search: "pacemaker" on `/Search` | Search matches synonyms including "defibrillator", "cardiac lead", "pulse generator". | P2 |
| **TC-12** | Notice Basket | Employee | Select 2 notices on page 1, advance to page 2, select 1 notice | Floating Selection Basket counter reflects "3 Notices Selected". Checkbox state persists across pagination. | P0 |
| **TC-13** | Notice Filtering | Employee | Filter by Classification: "Class I" and Recalling Firm | Table filters results to only matching Class I records from selected firm; pager updates accurately. | P1 |
| **TC-14** | Reviews Studio | Employee | Click "Create Review with Selected Notices" | Pre-populates review creation form with selected notice IDs; prompts for Title and Initial Risk Score. | P0 |
| **TC-15** | Findings Studio | Employee | Format clinical findings using rich-text toolbar (Bold, Lists, Headings) | Content is styled in real-time inside editor; on Save, findings HTML is stored and rendered cleanly. | P1 |
| **TC-16** | Security / XSS | Employee | Insert script tag `<script>alert('xss')</script>` into findings editor | Ganss.Xss.HtmlSanitizer strips the `<script>` tag server-side before persistence; no script executes. | P0 |
| **TC-17** | Customer Sharing | Employee | Open "Share Review" modal on `/Reviews`, select "St. Jude Hospital", click Share | Review is linked to customer; appears in Active Shares list; customer gains view access. | P0 |
| **TC-18** | Customer Sharing | Employee | Revoke customer access for "St. Jude Hospital" | Share record is flagged `IsActive = false`; customer immediately loses access to the review. | P0 |
| **TC-19** | Customer Portal | Customer | Customer navigates to `/SharedReviews` | Displays all active reviews shared with their specific organization; revoked or unshared reviews are invisible. | P0 |
| **TC-20** | Customer Review | Customer | Customer opens shared review detail `/SharedReviews/Details/{id}` | Displays clinical summary, risk rating badge, associated FDA notices, and clinical findings. | P1 |
| **TC-21** | Discussion Thread | Customer | Customer posts a question: "Does this affect Lot #8921?" | Comment appears in thread; `AwaitingReply` flag is set to `true` on the review share. | P1 |
| **TC-22** | Discussion Thread | Employee | Employee responds: "Lot #8921 is included in the recall scope." | Response is appended to thread; `AwaitingReply` resets to `false`. | P1 |
| **TC-23** | Executive Reports | Employee/Customer | Click "Print Report" on review detail screen | Browser print dialog opens with clean print-optimized layout (`@media print` strips navigation bars and action buttons). | P2 |
| **TC-24** | Executive Reports | Employee | Click "Export CSV" on review or search results | File download triggers with valid RFC 4180 CSV containing all relevant notice and review metadata. | P2 |

---

## 3. Automated Test Coverage

Automated integration and unit tests are located in `tests/ANDtr.Tests/`:
1. **`NoticeSearchServiceTests.cs`**:
   - Keyword filtering in product descriptions and reasons.
   - Classification filter validation (Class I, II, III).
   - Medical synonym query expansion.
   - Pagination calculation and boundary conditions.
2. **`ImportServiceTests.cs`**:
   - Ingestion of ClosedXML Excel streams.
   - Header row detection and carriage-return cleanup.
   - Duplicate recall detection and audit tracking.
3. **`ReviewServiceTests.cs`**:
   - Review creation with linked recall notices.
   - Status transitions (Draft -> Finalized).
   - HTML sanitization against XSS vectors.
4. **`ShareServiceTests.cs`**:
   - Multi-customer sharing link creation.
   - Revocation workflow and authorization verification.
5. **`CommentServiceTests.cs`**:
   - Threaded comment persistence.
   - Automatic `AwaitingReply` state resolution.
