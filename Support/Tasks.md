# Open Tasks

Open work only. Each item names its type and the result to deliver. The [architecture](Architecture.md) and [database workflow](sqlFirstWorkflow.md) record the decisions behind these tasks.

## Database and data access

- [ ] **Refactor** — Extract the data-access DLL. Decide ownership of shared models and interfaces first, then move EF contexts, mappings, and repository implementations behind them.
- [ ] **Bug** — Make missing records explicit in repository contracts that can return null. Update callers to handle absence.
- [ ] **Design** — Define job field and search filter rules before imposing constraints. Cover salary, priority, status, timestamps, and the meaning of an all-source filter. Assess the dormant sample seeder against that contract.

## Dashboard

- [ ] **Feature** - Size the listing viewport to show whole rows at the bottom when resizing the window or dragging the splitter.

- [ ] **Feature** — Filter the job collection using explicit lead criteria and show why listings match. Add pagination when list size warrants it.
- [ ] **Feature** - Hide the Location column when the dashboard grid is narrow. Keep the location accessible in the selected posting details.
- [ ] **Feature** — Save application status, notes, and archive state through library operations. Provide a way to view archived jobs.
- [ ] **Feature** — Add **Refresh Db** once the acquisition bugs below are addressed. Await the library refresh, reload the list, and report partial failures in the status line.
- [ ] **Feature** — Offer optional UTM removal when opening a posting. Preserve query parameters needed to identify or access the job.

## Email import

- [ ] **Feature** — Add text fallback for LinkedIn exports with no usable HTML. Keep a paired export as one message.
- [ ] **Feature** — Add provider parsers in small batches. Evaluate Remogu, DTCC, and DirectlyApply first, then handle the distinct Dice, Indeed, and other digest or recruiter formats.
- [ ] **Feature** — Add email provenance with a reviewed SQL upgrade. Store source identity, fingerprints, parser versions, and outcomes. Link each message to its jobs and record when a posting date came from the email.
- [ ] **Feature** — Add bounded retention for email content and stale postings. Base expiry on successful source checks and preserve application history and notes.
- [ ] **Feature** — Enrich leads from full postings as needed. Preserve known values during partial updates and replace estimated dates when a reliable posting date is available.

## Acquisition and credentials

- [ ] **Design** - Move scrapers to opt-in plugins. Define the provider contract, discovery, version compatibility, and failure handling. Keep validation and persistence in JobsDb, and provide a small example for third-party providers.
- [ ] **Bug** — Remove random source IDs from TokyoDev acquisition. Use a stable posting ID or canonical URL and report listings whose identity cannot be established.
- [ ] **Bug** — Preserve known details during partial scraper refreshes. Report per-item failures and partial success accurately before exposing refresh in the dashboard.
- [ ] **Refactor** — Consolidate TokyoDev parsing where the HTTP and Selenium paths can share tested logic. Preserve the existing console diagnostics.
- [ ] **Refactor · Deferred** — Switch LinkedIn to unauthenticated public listings. Verify coverage and stable identifiers against representative guest-page fixtures.
- [ ] **Security · Deferred** — Replace the hard-coded LinkedIn encryption password if credentials remain necessary. Remove the dependency if guest access makes it unnecessary. Account for existing encrypted records and review plaintext configuration, encryption, and cookie storage if retained.
- [ ] **Security** — Review the API key formerly documented in `Support/JobsDbNotes.txt` and revoke or rotate it if it is still usable.

## Diagnostics and tests

- [ ] **Refactor** — Move remaining diagnostic console writes to LoggingFoundation in focused passes. Keep CLI output intended for callers and exclude private payloads from logs.
- [ ] **Refactor** — Address remaining build warnings in small batches, starting with nullability and resource lifetime. Review submodule warnings separately.
- [ ] **Test** — Isolate cookie tests from the application profile and verify the database tests use fresh mutable fixtures.
