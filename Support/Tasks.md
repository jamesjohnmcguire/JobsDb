# Open Tasks

Open work only. Each item names its type and the result to deliver. The [architecture](Architecture.md) and [database workflow](sqlFirstWorkflow.md) record the decisions behind these tasks.

## Database and data access

- [ ] **Refactor** — Extract the data-access DLL. Decide ownership of shared models and interfaces first, then move EF contexts, mappings, and repository implementations behind them.
- [ ] **Bug** — Make missing records explicit in repository contracts that can return null. Update callers to handle absence.
- [ ] **Design** — Define job field and search filter rules before imposing constraints. Cover salary, priority, status, timestamps, and the meaning of an all-source filter. Assess the dormant sample seeder against that contract.

## Dashboard

- [ ] **Feature** - Add an X button on each posting to hide it from the dashboard. Make hidden postings accessible for restoration.
- [ ] **Feature** - Add a top-level **Purge** command with a confirmation dialog and an optional **From date** filter. Show what will be removed before confirming.
- [ ] **Feature** - Add an **Import Emails** button to choose an export directory, run a manual import through the library, and report the outcome.
- [ ] **Feature** - Size the listing viewport to show whole rows at the bottom when resizing the window or dragging the splitter.

- [ ] **Feature** — Filter the job collection using explicit lead criteria and show why listings match. Add pagination when list size warrants it.
- [ ] **Feature** - Hide the Location column when the dashboard grid is narrow. Keep the location accessible in the selected posting details.
- [ ] **Bug** - Align dashboard column headers with their cell text. Add horizontal padding to Source, Posted, and Status.
- [ ] **Feature** - Rename the dashboard Status column to Applied. Show a green check mark for applied postings and a red X for postings without an application. Let users click the indicator to mark a posting as applied or undo an accidental mark. Save the change through the library.
- [ ] **Feature** — Save application status, notes, and archive state through library operations. Provide a way to view archived jobs.
- [ ] **Feature** - Add a **Pull Full Job Description** button in the details pane. Fetch and save the full description for the selected posting through the library, then refresh the pane.
- [ ] **Feature** — Add **Refresh Db** once the acquisition bugs below are addressed. Await the library refresh, reload the list, and report partial failures in the status line.
- [ ] **Feature** — Offer optional UTM removal when opening a posting. Preserve query parameters needed to identify or access the job.
- [ ] **Feature** - Add a macOS desktop client. Prototype the existing dashboard in Avalonia using the shared JobsDb library. Verify SQLite initialization, posting review, browser links, and packaging on macOS before deciding whether to share the desktop UI with Windows.

## Email import

- [ ] **Feature** — Add plain-text fallback for LinkedIn .eml messages with no usable HTML body.
- [ ] **Feature** — Add provider parsers in small batches. Evaluate Remogu, DTCC, and DirectlyApply first, then handle the distinct Dice, Indeed, and other digest or recruiter formats.
- [ ] **Feature** — Add email provenance with a reviewed SQL upgrade. Store source identity, fingerprints, parser versions, and outcomes. Link each message to its jobs and record when a posting date came from the email.
- [ ] **Feature** — Add bounded retention for email content and stale postings. Base expiry on successful source checks and preserve application history and notes.
- [ ] **Feature** — Enrich leads from full postings as needed. Preserve known values during partial updates and replace estimated dates when a reliable posting date is available.

## Acquisition and credentials

- [ ] **Feature** - Record acquisition transport, such as email import or web scrape, separately from the posting provider. Preserve provider job identity when a lead arrives through multiple transports.
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

Replace Console.WriteLines with Logging
Test and Merge TokyoDevScraperSelenium
Review OnModelCreating
Remove: Support/JobsDbNotes.txt (Support/JobsDbNotes.txt:11) contains a literal API key in an HTTP header example.
Ignored 00000000A45886A902B9E044977F9E53108E433C04D82900: added 0, updated 0, unchanged 0  Unsupported LinkedIn sender.
Review and Improve Importing
Move Scrapers to PlugIns
