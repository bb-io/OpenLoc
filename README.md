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
| Project name contains | Text contained in the project name, ignoring capitalization |
| Status | Draft, in progress, completed, or on hold |
| Created after | Only projects created after this date and time |
| Created before | Only projects created before this date and time |
| Due in (hours) | Positive whole number of hours from now; only projects due within that window |

All selected filters must match. Creation-date filters are applied within the app across all pages. Dates without a time zone are treated as UTC; projects created exactly at either boundary are excluded.

The due window is calculated once when the search starts and applied across all pages. It includes deadlines from that time through the specified number of hours ahead, including both boundaries. Overdue projects and projects without a deadline are excluded when this filter is set.

Returns **Projects**, including project IDs, names, descriptions, statuses, languages, dates, and optional creator and template IDs.

### Get project

Choose **Project ID** from the dropdown or enter a numeric project ID. Returns the project's details, including its languages and deadline.

### Create project

| Input | Required | Meaning |
| --- | --- | --- |
| Project name | Yes | Name shown on the project page |
| Source language | Yes | Select a language name; its language code is sent to the API |
| Target languages | Yes | Select one or more language names; their codes are sent as an array |
| Description | No | Notes shown on the project page |
| Deadline | No | Due date and time; dates without a time zone are treated as UTC |
| Customer label value ID | No | Select a customer by name; the selected value is its label value ID |
| Template ID | No | Search templates by name and select the setup to apply |
| Workflow ID | No | Search workflows by name; an explicitly selected workflow overrides the template's workflow |

Returns the created project's details and **Project ID** for subsequent actions. This creates a live project. Use **Upload file** to add documents afterward.

The language dropdown offers English (`en`), French (`fr`), and German (`de`), the codes documented in the supplied OpenAPI creation examples. The specification does not publish a complete supported-language catalog; the dropdown deliberately does not assume that every ISO language code is accepted.

The customer dropdown reads the built-in label whose key is `customer`, independently of its display name. If selected, the app validates the customer before creation, creates the project once, reads its current labels, and assigns the customer with a separate label update. Other labels, including those inherited from a template, are preserved. Customer IDs are not sent in the project creation body.

If the customer step fails after creation, the action reports partial success with the created project ID. Check and update that existing project; do not rerun Create project, which would create another project. Project creation and label writes are never retried automatically.

Template searches use the API's name search and cursor pagination. Workflow and customer searches filter the complete collections locally because those endpoints do not document search or pagination parameters. Project ID inputs reuse the existing searchable project dropdown.

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

### Save draft translation

Choose **Job ID** and **Segment ID**, optionally selecting a project first to populate the job dropdown. The segment dropdown is filtered by the selected job and supports text search and cursor pagination. Enter optional **Translation text** to save it without approval. Leaving text empty preserves the current translation. The resulting status is `needs_review`.

Optional **Expected updated at** accepts the segment's original `updated_at` timestamp, including its time zone and fractional seconds. The timestamp is sent unchanged. A stale timestamp produces a concurrency conflict; retrieve and review the latest segment before trying again. The app never fetches a newer timestamp and retries the write automatically.

Returns **Segment ID**, nullable **Translation text**, **Status**, and **Updated at**. The returned timestamp is exact text that can be mapped into the next write's **Expected updated at** field.

### Confirm translation

Uses the same job, segment, text, and timestamp inputs as **Save draft translation**. Leaving **Translation text** empty confirms the existing translation unchanged. Confirmation approves the segment and saves it to the project's writable translation memories.

A concurrency conflict stops the action. When QA blocks confirmation, the error includes the API error code/message and each finding's check type, severity, message, and ignored flag. Correct the findings before confirming again; the app does not bypass checks or retry the write.

Returns the same segment-state fields as **Save draft translation**. These segment writes require write access. Owner, admin, project manager, and Linguist roles are documented; Linguists are restricted to jobs assigned to them. Enter known Job and Segment IDs directly if your role cannot list projects.

### Add segment comment

Select **Job ID** and **Segment ID**, then enter a required **Comment** of 1 to 5000 characters. Returns **Comment ID**, **Comment**, nullable **Author ID**, nullable **Author name**, and **Created date**. The comment ID identifies the new comment; it is distinct from the selected segment ID. Owner, admin, project manager, and assigned Linguist access is enforced by the API. A write-enabled key is required.

### Upload file

Select **Project ID** from the existing project dropdown and map **File** from a previous Bird step. The file is downloaded through Blackbird's file-management service and uploaded as multipart form data. The API detects its format and creates one job per target language automatically. The app does not create any additional jobs or retry the upload automatically.

| Input | Required | Meaning |
| --- | --- | --- |
| Project ID | Yes | Project that receives the document |
| File | Yes | Native Blackbird file, up to the API's 50 MB limit |
| Analyze | No | Request word-count analysis after jobs are created; false is sent explicitly when selected |
| XLSX layout | No | Serialized XLSXLayout JSON text, for XLSX files without language codes in row 1 |

For XLSX, leave **XLSX layout** empty when row 1 contains language codes. The app checks row 1 for the project's source and target language codes when a layout is supplied; the API also validates workbook layout rules. Without a layout, every text cell of every visible sheet is translated. With a layout, only listed sheets are translated: listed columns of each, or all text cells when columns are omitted or empty. Translations are written back to their own cells. A sheet's optional `header_row` flag leaves its first row untranslated.

Example layout:

```json
{"sheets":[{"name":"Sheet1","columns":["B","D"],"header_row":true}]}
```

Returns **File ID**, **Project ID**, **File name**, **File format**, **Segment count**, **Created date**, and **Warnings**. Warnings can describe analysis or other work that could not start after the file was saved; check them before retrying so you do not upload the document twice. Upload requires an owner, admin, or project manager and a key with write access.

### Download translated file

Select or enter **Job ID**. Optionally select **Project ID** first to populate the existing searchable job dropdown; only the Job ID is used for export. **Export format** offers the original document format (`source`, the default) or bilingual XLIFF 1.2 (`xliff`).

Returns a native Blackbird **File** that can be passed directly to subsequent actions. Binary content is stored through Blackbird's file-management service. The response filename is preserved, including its extension; if unavailable, the job's file name is used, with `.xlf` for an XLIFF export. Empty files and API failures produce errors. Owner, admin, and project manager roles can export; linguists can export jobs they are allowed to access.

### Download original file

Choose **File ID**, optionally selecting **Project ID** first to populate the searchable file dropdown. Files are fetched from the selected project using cursor pagination. The download itself uses only File ID and returns the document exactly as uploaded as a native Blackbird **File**.

The response filename is preserved. Without a filename header, available project/file metadata supplies the original upload name, falling back to the current name. If metadata is unavailable, the output uses `original-file-<fileID>.bin`; binary content remains unchanged. Empty responses and API failures stop the action.

Owner, admin, project manager, and Linguist roles can download, but a Linguist may access only files belonging to assigned jobs. Project file listing and metadata access require owner, admin, or project manager access, so Linguists can enter a known File ID directly. A failed optional metadata lookup does not block an otherwise authorized download.

### Testing the additional actions in a sandbox Bird

1. On a disposable job, select a segment from **Search job segments** or the segment dropdown. Run **Save draft translation** with test text and verify the returned status is `needs_review`. Map its exact **Updated at** text into the next write's **Expected updated at**.
2. Run **Confirm translation** with text omitted to confirm that draft. Check for `approved` and the project's writable translation memory update. On a QA-invalid test translation, verify that confirmation fails with finding details. For concurrency, change the segment in the editor and submit an older timestamp; the action must stop with a conflict.
3. Run **Add segment comment** on the same job/segment and verify its returned comment ID and content in the editor. An empty comment or more than 5000 characters must fail before a request is sent.
4. Run **Download original file** using the file ID from **Upload file** or the project file dropdown. Map its native file output into a storage action and compare it with the original upload. Test an assigned Linguist connection with a known File ID and no project helper.

### Testing file actions in a sandbox Bird

1. Use a test connection with your own API hostname and access key. Choose a disposable project with at least one target language.
2. Add an upstream step that produces a native file. Map its file output into **Upload file**, select the project, and run the Bird. Check the returned file ID and warnings.
3. Use **Search jobs** for that project. Verify that the uploaded file has one job per target language. Do not create extra jobs manually.
4. Complete or prepare the translation in the test job. Add **Download translated file**, select that job, and map its file output into a file-consuming action such as storage upload. Verify that `source` keeps the original extension and `xliff` opens as bilingual XLIFF 1.2.
5. Test XLSX separately: upload a workbook without language headers using the example layout, then a workbook with project language codes in row 1 with layout left empty. Supplying a layout for the latter must fail clearly.

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

The suite contains real-API action tests, valid/invalid key checks, dynamic dropdown smoke tests, filtered job search checks, and an initial polling check. Local tests cover combined job filters and polling status transitions without making API calls. Tests without connection settings or required fixture IDs are marked inconclusive (reported as skipped).

The creation test additionally requires TestData.enable_create_project to be true. It creates a uniquely named project and prints its ID. It does not delete data; remove the disposable project manually when finished. The tests call the real API without mocking it.

Optionally configure TestData.customer_label_value_id, TestData.workflow_id, and TestData.template_id to exercise the Create project dropdown selections. The creation test verifies the assigned customer when configured. Customer, workflow, and template dropdown integration tests need corresponding entries in the test account. Local tests cover numeric label IDs and preservation of non-customer labels.

Run only local validation and serialization checks:

The additional-action tests cover draft/confirm bodies, timestamp precision, concurrency conflicts, QA-blocked confirmations, comments, dependent dropdown pagination, and original binary downloads with mocked HTTP. Real-API tests use TestData.segment_id and TestData.file_id. Segment writes and comments are disabled unless their individual flags are enabled: enable_save_segment_draft, enable_confirm_segment, and enable_add_segment_comment. Set translation_text and use disposable segments; confirmation also writes to translation memories. These tests do not restore prior translations or remove comments.

    dotnet test OpenLoc.sln --filter "TestCategory!=Integration"

The update test additionally requires TestData.enable_update_job to be true and TestData.assignee_user_id to identify an active member. Use a disposable job with an active workflow step. This changes its assignment and leaves that assignment in place.

The file tests use mocked HTTP responses for multipart fields, binary storage, filenames, error responses, empty responses, and XLSX layout validation. Real-API file tests require configured credentials and a job that can be exported. Set TestData.enable_upload_file to true only for a disposable project; this test uploads an HTML file and creates real translation jobs. It does not clean them up automatically. No real API credentials are needed for the mocked tests.

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
