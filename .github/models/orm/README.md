# ORM Model Description: Academic Domain

## **Entity Types** (Object Types with independent existence):

1. **Academic** - Primary entity with reference mode `empNr` (6-character fixed text)
2. **Date** - Entity with reference mode `mdy` (temporal date type)
3. **Rank** - Entity with reference mode `.code` (enumerated values: 'P', 'SL', 'L')
4. **Degree** - Entity with reference mode `.code` (examples: 'PHD', 'MCS', 'BSc')
5. **University** - Entity with reference mode `.code` (examples: 'UCSD', 'MIT', 'USW', 'UQ')
6. **AccessLevel** - Entity with reference mode `.code` (enumerated values: 'INT', 'NAT', 'LOC')
7. **Extension** - Entity with reference mode `extNr` (numeric decimal type)

## **Value Types** (Object Types without independent existence):
1. **EmpName** - Variable length text (15 characters)

## **Fact Types** (Binary Relationships):

1. **Academic has EmpName** - Mandatory participation from Academic (1:1)
2. **Academic is tenured** - Unary predicate (optional)
3. **Academic is contracted until Date** - Optional participation from Academic (M:1)
4. **Academic has Rank** - Mandatory participation from Academic (M:1)
5. **Academic obtained Degree from University** - Ternary fact type with mandatory participation from Academic
6. **Academic uses Extension** - One-to-one relationship (1:1)
7. **Rank ensures AccessLevel** - Functional relationship (M:1)

## **Uniqueness Constraints**:
- Each Academic has exactly one EmpName, Rank, and Extension
- Each Extension is used by at most one Academic
- Each Academic is contracted until at most one Date
- For each Academic-Degree pair, at most one University

## **Mandatory Role Constraints**:
- Every Academic must have an EmpName
- Every Academic must have a Rank
- Every Academic must use an Extension
- Every Academic must have obtained some Degree from some University

## **Exclusion Constraint**:
- Each Academic is either tenured OR contracted until a date (mutually exclusive)

## **Reference Schemes** (Identification Patterns):
- Academic is identified by empNr
- Date is identified by mdy
- Rank, Degree, University, AccessLevel use `.code` reference modes
- Extension is identified by extNr

## **Value Constraints**:
- Rank values are restricted to {'P', 'SL', 'L'}
- AccessLevel values are restricted to {'INT', 'NAT', 'LOC'}

This ORM model represents a typical academic personnel system with comprehensive constraints ensuring data integrity and business rule enforcement through formal ORM constructs.
