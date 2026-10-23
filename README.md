# Blackbird.io OpenLoc

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Connect OpenLoc to Blackbird to find and create translation projects, retrieve and assign their jobs, read source text and translations from job segments, and start workflows when project statuses change.

## Before setting up

You need an OpenLoc account, the HTTPS hostname serving your OpenLoc customer API, and an access key from **AI settings > Access keys**.

The key acts as its owner in one organization. Project actions require an owner, admin, or project manager role. Reading segments also supports linguists for jobs assigned to them. Creating projects and updating job assignments require a key with write access. Updating assignments requires an owner, admin, or project manager role. Get job also supports linguists reading jobs assigned to them.

## Connecting

1. Navigate to Apps and search for **OpenLoc**.
2. Click **Add connection** and give the connection a name.
3. Enter **API hostname**: your own HTTPS API origin, without a path. This is required; the app adds `/api/v1` automatically.
4. Enter **Access key**
5. Click **Connect**.

Connection validation reads one project. A rejected key fails validation. A permissions or service failure produces a warning message without labelling the key invalid.

## Actions

Search actions fetch all pages automatically. You do not need to manage page sizes or cursors. No matches returns an empty collection. Project results are newest first; jobs are oldest first; segments follow document order.

### Search projects

Find projects by any combination of the following optional filters.

| Input | Meaning |
| --- | --- |
| Project name | Text to search for in project names |
| Status | Draft, in progress, completed, or on hold |
| Source language | A language code, for example en |
| Target language | A language code included in the project's targets, for example de |

Returns **Projects**, including project IDs, names, descriptions, statuses, languages, dates, and optional creator and template IDs.

### Get project

Choose **Project ID** from the dropdown or enter a numeric project ID. Returns the project's details, including its languages and deadline.

### Create project

| Input | Required | Meaning |
| --- | --- | --- |
| Project name | Yes | Name shown on the project page |
| Source language | Yes | Source language code, for example en |
| Target languages | Yes | One or more target language codes, for example de and fr |
| Description | No | Notes shown on the project page |
| Deadline | No | Due date and time; dates without a time zone are treated as UTC |
| Template ID | No | Numeric ID of a template whose setup should be applied |
| Workflow ID | No | Numeric ID of a workflow; overrides the template's workflow |

Returns the created project's details and **Project ID** for subsequent actions. This creates a live project. Uploading files is outside this initial version. Customer and domain labels are not creation inputs in the API.

### Search jobs

Choose or enter **Project ID**. Returns **Jobs**, one per file and target language, including job and file IDs, language codes, word and segment counts, processing status, segment totals by state, and the current workflow step.

Combine any of these optional filters. All specified criteria must match; text and language comparisons ignore capitalization.

| Input | Meaning |
| --- | --- |
| File name | Text contained in the source file name |
| Status | Created, in progress, needs attention, or completed |
| Processing status | Automatic processing state, for example queued, in_progress, completed, or failed |
| Source language | Source language code, for example en |
| Target language | Target language code, for example de |
| Assigned user ID | User assigned to the current workflow step |

Filters are applied within the app across all pages because the API's project-job list only defines pagination parameters.

### Get job

Choose **Job ID** or enter its numeric ID directly. **Project ID** is optional and populates the job dropdown. Returns the job's full details, including translation progress, segment totals, and its current workflow step.

### Update job

Choose or enter **Job ID** and select **Assigned user ID** from the active-member dropdown, or enter the user ID directly. **Project ID** is optional and populates the job dropdown.

Updates the assignee of the job's current workflow step and returns the updated job. The supplied API spec supports this edit through POST /jobs/{jobID}/assign. It does not define a general job update endpoint or allow directly editing job status, languages, or file name.

### Search job segments

| Input | Required | Meaning |
| --- | --- | --- |
| Job ID | Yes | Numeric job ID, entered directly or chosen from the dropdown |
| Project ID | No | Select a project to populate the job dropdown |
| Search text | No | Search source text and translations |
| Status | No | Needs review, automatically approved, or approved |

Returns **Segments**, including segment IDs, positions, source text, translations, review statuses, translation origins, translation memory match percentages, update times, and the previous workflow step's result when available. Untranslated target text and unavailable optional details have no value.

## Events

### On project status changed

Periodically checks project statuses and returns **Projects** whose statuses changed. Each result includes the full project details, its new **Status**, and its **Previous status**.

| Option | Meaning |
| --- | --- |
| Project ID | Optional project dropdown; leave empty to monitor all accessible projects |
| Status | Optional destination-status dropdown; leave empty for any status change |

The first check records existing statuses without triggering. Later checks trigger only for a change from a previously observed status. New projects establish their own starting status and do not trigger immediately. Changes to the selected project or status reset the starting state.

Statuses are tracked even when they do not match the destination filter, so a project leaving and later returning to the selected status can trigger again. The event detects statuses observed at each check; transitions that begin and end between checks cannot be detected. Monitoring all projects reads every page on each check. A failed API request does not save a partial snapshot.

## Troubleshooting

- **Rejected access key:** replace it with an active key in the connection.
- **Permission denied:** ask an administrator to check the key owner's role, organization, and write access.
- **Item not found:** check the project or job ID and its organization.
- **Invalid input or conflict:** follow the error details and correct the field or refresh the item's state.
- **Request limit reached:** read requests retry up to three times when the required wait is at most 60 seconds. Longer waits and exhausted retries return a message including the wait time when supplied by the API.
- **Service unavailable:** retry later. Project creation is never retried automatically; if a request times out, check whether the project was created before submitting it again.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out through the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->

## Development

The solution contains a .NET 8 class library and an MSTest project. The file openloc-openapi.json is the source of truth for API routes and models. The Blackbird display name is OpenLoc, assembly name is Apps.OpenLoc, and category is CAT and TMS.

    dotnet build OpenLoc.sln
    dotnet test OpenLoc.sln --no-build --filter "TestCategory!=Integration"

### Integration tests

Copy Tests.OpenLoc/appsettings.json.example to Tests.OpenLoc/appsettings.json and fill in ConnectionDefinition.hostname and ConnectionDefinition.access_key. The real configuration is ignored by Git; never commit credentials.

Set TestData.project_id to a project containing a processed job and set TestData.job_id to that job's ID. It must contain segments. Use the canonical numeric IDs with no leading zeroes. Configure valid source and target language codes for creation.

The suite contains a real-API happy-path test for each of the seven actions, valid/invalid key checks, dynamic dropdown smoke tests, filtered job search checks, and an initial polling check. Local tests cover combined job filters and polling status transitions without making API calls. Tests without connection settings or required fixture IDs are marked inconclusive (reported as skipped).

The creation test additionally requires TestData.enable_create_project to be true. It creates a uniquely named project and prints its ID. It does not delete data; remove the disposable project manually when finished. The tests call the real API without mocking it.

Run only local validation and serialization checks:

    dotnet test OpenLoc.sln --filter "TestCategory!=Integration"

The update test additionally requires TestData.enable_update_job to be true and TestData.assignee_user_id to identify an active member. Use a disposable job with an active workflow step. This changes its assignment and leaves that assignment in place.

Run real API tests excluding creation and assignment changes:

    dotnet test OpenLoc.sln --filter "TestCategory=Integration&TestCategory!=CreatesData&TestCategory!=UpdatesData"

### Build and repository workflow

Restore dependencies and build with the normal NuGet sources:

    dotnet restore OpenLoc.sln
    dotnet build OpenLoc.sln --no-restore
    dotnet test OpenLoc.sln --no-build --filter "TestCategory!=Integration"

The repository preserves the template's central Blackbird workflow dispatcher. Pushes to develop dispatch the development workflow; merged pull requests targeting main dispatch the production workflow using the existing organization variables and secrets. The dispatcher does not run local tests itself.

The API specification is kept in openloc-openapi.json. Project listing uses GET /projects under /api/v1. Single-object responses unwrap data; search actions follow cursor and has_more automatically. The members dropdown uses GET /members without pagination parameters, as specified by that endpoint.

The embedded Apps.OpenLoc/image/icon.png is a placeholder; replace it with the official product icon before release.
