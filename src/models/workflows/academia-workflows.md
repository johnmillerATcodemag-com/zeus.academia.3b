# Zeus Academia — Workflow Catalogue

Derived from the ORM domain model (`models/orm/academia.txt`).
Domain entities: **Academic**, **Rank**, **Degree**, **University**, **AccessLevel**, **Extension**.

---

## 1. Academic Lifecycle

| #   | Workflow                    | Trigger             | Key Rules                                                                                                                    |
| --- | --------------------------- | ------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| 1.1 | **Register Academic**       | HR onboarding       | Requires empNr (6-char), EmpName, Rank, Extension (unique), ≥1 Degree+University; employment status optional at registration |
| 1.2 | **View Academic Profile**   | Any authorised user | Returns empNr, name, rank, access level, extension, degrees, employment status                                               |
| 1.3 | **Update Academic Name**    | HR administrator    | EmpName ≤15 chars; uniqueness not enforced                                                                                   |
| 1.4 | **Search / List Academics** | Any authorised user | Filter by name, rank, access level, employment status, degree, university                                                    |
| 1.5 | **Deregister Academic**     | HR off-boarding     | Releases Extension; removes tenured/contract status; retains historical degree records                                       |

---

## 2. Employment Status

An Academic is either **tenured**, **contracted until a Date**, or **neither** — never both.

| #   | Workflow                       | Trigger                     | Key Rules                                                                              |
| --- | ------------------------------ | --------------------------- | -------------------------------------------------------------------------------------- |
| 2.1 | **Grant Tenure**               | HR / Faculty Board decision | Clears any existing contract date; sets tenured flag                                   |
| 2.2 | **Assign Contract**            | HR administrator            | Sets contract end Date; clears tenured flag; date must be future                       |
| 2.3 | **Renew Contract**             | HR administrator            | Replaces existing contract end Date with new date; Academic must already be contracted |
| 2.4 | **Convert Contract to Tenure** | HR / Faculty Board          | Clears contract date; sets tenured flag                                                |
| 2.5 | **Remove Employment Status**   | HR administrator            | Clears both tenured flag and contract date (e.g., temporary appointment ends)          |
| 2.6 | **View Expiring Contracts**    | HR administrator            | Returns Academics contracted until Date ≤ threshold (e.g., within 90 days)             |
| 2.7 | **List Tenured Academics**     | HR / Management report      | Returns all Academics where tenured flag is set                                        |
| 2.8 | **List Contracted Academics**  | HR / Management report      | Returns all Academics with a contract end date, ordered by date ascending              |

---

## 3. Rank & Access Level

Rank is mandatory on every Academic. Rank determines AccessLevel (P→INT, SL→NAT, L→LOC).

| #   | Workflow                           | Trigger                | Key Rules                                                         |
| --- | ---------------------------------- | ---------------------- | ----------------------------------------------------------------- |
| 3.1 | **Assign Rank to Academic**        | HR administrator       | Value must be one of: P, SL, L; AccessLevel derived automatically |
| 3.2 | **Change Academic Rank**           | Promotion / demotion   | Old rank replaced; AccessLevel recalculated; audit event raised   |
| 3.3 | **View Academic Access Level**     | Any authorised user    | Read-only; derived from current Rank                              |
| 3.4 | **List Academics by Rank**         | Management / reporting | Filter by rank code; include derived AccessLevel                  |
| 3.5 | **List Academics by Access Level** | Management / reporting | Filter: INT, NAT, or LOC                                          |

---

## 4. Extension Management

Each Academic uses exactly one Extension; each Extension is used by at most one Academic (1:1).

| #   | Workflow                         | Trigger                 | Key Rules                                                                                |
| --- | -------------------------------- | ----------------------- | ---------------------------------------------------------------------------------------- |
| 4.1 | **Assign Extension to Academic** | IT / HR setup           | Extension must not be in use by another Academic                                         |
| 4.2 | **Reassign Extension**           | Office move / reshuffle | Source Academic's extension released; target Academic must not already hold an extension |
| 4.3 | **Release Extension**            | Academic deregistration | Extension returned to available pool                                                     |
| 4.4 | **View Academic Extension**      | Any authorised user     | Returns extNr for named Academic                                                         |
| 4.5 | **List Available Extensions**    | IT administrator        | Returns all provisioned Extensions not currently assigned                                |
| 4.6 | **Provision Extension**          | IT administrator        | Adds new extNr to the system; must be numeric decimal                                    |
| 4.7 | **Deprovision Extension**        | IT administrator        | Removes extNr; must not be assigned to an Academic                                       |

---

## 5. Degree & Qualification Management

An Academic must have obtained at least one Degree from some University.
For a given Academic+Degree pair, at most one University is recorded.

| #   | Workflow                         | Trigger                    | Key Rules                                                                                |
| --- | -------------------------------- | -------------------------- | ---------------------------------------------------------------------------------------- |
| 5.1 | **Record Degree Obtained**       | HR / Academic self-service | Requires Academic, Degree code, University code; duplicate Academic+Degree pair rejected |
| 5.2 | **Update Degree University**     | HR correction              | Changes the University for an existing Academic+Degree record                            |
| 5.3 | **Remove Degree Record**         | HR correction              | Academic must retain ≥1 Degree after removal                                             |
| 5.4 | **List Academic Qualifications** | Any authorised user        | Returns all Degree+University pairs for a given Academic                                 |
| 5.5 | **List Academics by Degree**     | Reporting                  | Filter by degree code (e.g., all PHD holders)                                            |
| 5.6 | **List Academics by University** | Reporting                  | Filter by university code (e.g., all MIT graduates)                                      |

---

## 6. Reference Data Management

Lookup tables for Rank, Degree, University, and AccessLevel.

| #   | Workflow               | Trigger              | Key Rules                                                             |
| --- | ---------------------- | -------------------- | --------------------------------------------------------------------- |
| 6.1 | **View Ranks**         | Any authorised user  | Read-only list: P, SL, L with descriptions                            |
| 6.2 | **Add Rank**           | System administrator | Must include AccessLevel mapping; value must be unique                |
| 6.3 | **View Degrees**       | Any authorised user  | Read-only list of degree codes                                        |
| 6.4 | **Add Degree**         | System administrator | Code must be unique                                                   |
| 6.5 | **View Universities**  | Any authorised user  | Read-only list of university codes                                    |
| 6.6 | **Add University**     | System administrator | Code must be unique                                                   |
| 6.7 | **View Access Levels** | Any authorised user  | Read-only: INT, NAT, LOC; derived from Rank — not directly assignable |

---

## 7. Reporting & Analytics

| #   | Workflow                                | Audience            | Description                                                          |
| --- | --------------------------------------- | ------------------- | -------------------------------------------------------------------- |
| 7.1 | **Academic Directory**                  | All staff           | Full listing: name, rank, access level, extension, employment status |
| 7.2 | **Academics by Rank Report**            | Management          | Count and list per rank; include access level grouping               |
| 7.3 | **Academics by Access Level Report**    | Management          | Count and list: INT / NAT / LOC                                      |
| 7.4 | **Tenured Academics Report**            | HR / Administration | List of tenured Academics with rank and qualifications               |
| 7.5 | **Contracted Academics Report**         | HR                  | List with contract end dates, sorted ascending                       |
| 7.6 | **Expiring Contracts Report**           | HR administrator    | Contracts expiring within configurable threshold (default 90 days)   |
| 7.7 | **Academics by Qualification Report**   | Department heads    | Grouped by degree; shows university of award                         |
| 7.8 | **Qualification Summary by University** | Management          | Count of degrees awarded per university across all Academics         |
| 7.9 | **Access Level Distribution Report**    | Management          | Count of Academics per AccessLevel for planning and compliance       |

---

## Workflow Dependency Summary

```
Register Academic
  ├── Assign Extension (4.1)          — required at registration
  ├── Record Degree Obtained (5.1)    — ≥1 required at registration
  ├── Assign Rank (3.1)               — required at registration
  └── Assign Contract (2.2)           — optional at registration
        └── Renew Contract (2.3)
              └── Convert to Tenure (2.4)
                    └── Grant Tenure (2.1)

Change Rank (3.2)
  └── Access Level recalculated       — no separate workflow needed

Deregister Academic (1.5)
  └── Release Extension (4.3)         — triggered automatically
```

---

## Slice Candidates

Each workflow above maps to one candidate vertical slice. Priority grouping for MVP delivery:

| Wave                                            | Slices                                                                                                        |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| **Wave 1 — Core CRUD**                          | Register Academic (1.1), View Profile (1.2), List/Search (1.4), Record Degree (5.1), Assign Rank (3.1)        |
| **Wave 2 — Employment Status**                  | Grant Tenure (2.1), Assign Contract (2.2), Renew Contract (2.3), Convert to Tenure (2.4), Remove Status (2.5) |
| **Wave 3 — Extensions**                         | Assign Extension (4.1), Reassign Extension (4.2), Release Extension (4.3), Provision/Deprovision (4.6, 4.7)   |
| **Wave 4 — Reporting**                          | Expiring Contracts (2.6/7.6), Tenured Report (7.4), Academic Directory (7.1), Access Level Distribution (7.9) |
| **Wave 5 — Reference Data & Remaining Reports** | All of Section 6 and remaining Section 7 workflows                                                            |
