# JobMailer

JobMailer sends job-application emails from the user's own Gmail account. Users keep multiple
professional profiles (contact details + CV), pick a pre-built email template, preview the rendered
message, and send it with the CV attached through the Gmail API.

## Technology stack

| Layer | Technology |
| --- | --- |
| Presentation (web) | React 18 + TypeScript, Vite, Material-UI, Axios, React Context API |
| Presentation (API) | ASP.NET Core Web API controllers, JWT bearer auth |
| Business logic | `JobMailer.Business`: Google OAuth, token encryption/refresh, JWT issuance, template rendering, Gmail sending |
| Data access | `JobMailer.Data`: EF Core 8, SQL Server, entities, repositories, migrations |
| Database | SQL Server |
| Auth | Google OAuth 2.0 (`openid email profile gmail.send`) |
| Email | Gmail API |

## Repository layout

```text
JobMailer.sln
api/
  JobMailer.Api/            # Presentation: Program.cs, controllers, contracts, middleware
  JobMailer.Business/       # Business logic: services, options, abstractions
  JobMailer.Data/           # Data access: DbContext, entities, repositories, migrations
web/
  src/api/                  # Axios instance and session storage
  src/context/              # AuthContext + FeedbackContext (Context API)
  src/pages/                # Login, Compose, Profiles, History
  src/theme.ts              # Material-UI theme
```

## Prerequisites

- .NET 8 SDK or newer (the projects target `net8.0`).
- Node.js 18+ and npm.
- SQL Server reachable with the configured connection string (Express, Developer, or LocalDB).
- A Google Cloud project with the Gmail API enabled.

## 1. Google Cloud setup

1. Create/select a project in [Google Cloud Console](https://console.cloud.google.com/).
2. Enable **Gmail API**.
3. Configure the **OAuth consent screen** (External is fine for testing):
   - Add your Google account as a test user.
   - Add scopes: `openid`, `email`, `profile`,
     `https://www.googleapis.com/auth/gmail.send`.
4. Create **Credentials → OAuth client ID → Web application**.
5. Add an authorized redirect URI: `http://localhost:5080/api/auth/google/callback`. It must exactly
   match `Google:RedirectUri` in the API configuration.
6. Copy the client ID and client secret. No JavaScript origin or client ID is needed in the web app;
   the API owns the OAuth exchange.

## 2. Configure the API

Edit `api/JobMailer.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=JobMailer;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Google": {
    "ClientId": "YOUR_CLIENT_ID.apps.googleusercontent.com",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "RedirectUri": "http://localhost:5080/api/auth/google/callback"
  },
  "Jwt": {
    "Key": "CHANGE-ME-to-a-random-string-of-32+-chars",
    "Issuer": "JobMailer",
    "Audience": "JobMailer.Web",
    "ExpiryHours": 8
  },
  "Web": {
    "Url": "http://localhost:5173"
  },
  "Cors": "http://localhost:5173"
}
```

Notes:

- `Jwt:Key` must be at least 32 characters. Use a random secret, not the placeholder.
- `Cors` accepts multiple origins separated by `;`.
- `Web:Url` is where the API redirects after Google sign-in. Change it for a non-local deployment.
- `Google:RedirectUri` must match an authorized redirect URI in Google Cloud Console.

## 3. Configure the web app

```powershell
Copy-Item web/.env.example web/.env
```

No change is needed for local development: the web app defaults to `/api`, and Vite proxies `/api`
to `http://localhost:5080`. Set `VITE_API_URL` only when the API runs on a different origin:

```dotenv
# Same origin as the web app (recommended for production)
VITE_API_URL=/api

# Separate API host
# VITE_API_URL=https://your-api-domain.com/api
```

## 4. Database

EF Core migrations are applied automatically when the API starts. The connection string user needs
permission to create/alter the database.

> **Upgrading from the Stage-1 prototype:** the old app used `EnsureCreated` and has no migrations
> history. If a `JobMailer` database already exists, drop it once before starting this version (or
> point `Database=` at a new name):
>
> ```sql
> USE master;
> ALTER DATABASE JobMailer SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
> DROP DATABASE JobMailer;
> ```

To manage migrations manually:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add <Name> --project api/JobMailer.Data --startup-project api/JobMailer.Api --output-dir Migrations
dotnet tool run dotnet-ef database update --project api/JobMailer.Data --startup-project api/JobMailer.Api
```

## 5. Run

Terminal 1 — API:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project api/JobMailer.Api --urls http://localhost:5080
```

Swagger UI: <http://localhost:5080/swagger> (Development only). Health check:
<http://localhost:5080/health>.

Terminal 2 — web:

```powershell
npm run install:web
npm run dev
```

Open <http://localhost:5173>. The root `package.json` delegates `dev`, `build`, and `preview` to
`web`, so these commands work from the repository root.

## 6. Deploy to MonsterASP

The API serves the built SPA from `api/JobMailer.Api/wwwroot`, and `npm run build` writes directly
there. Publish the `api/JobMailer.Api` project after building the frontend.

For a same-origin deployment, the API derives the Google callback URI and the post-login SPA URL
from the incoming request. You only need to:

1. Add `https://your-domain.com/api/auth/google/callback` as an authorized redirect URI in Google
   Cloud Console.
2. Set the required secrets on MonsterASP:

```text
ConnectionStrings__Default=Server=...;Database=JobMailer;...
Google__ClientId=YOUR_CLIENT_ID.apps.googleusercontent.com
Google__ClientSecret=YOUR_CLIENT_SECRET
Jwt__Key=A_RANDOM_SECRET_OF_AT_LEAST_32_CHARACTERS
```

`Web__Url`, `Cors`, and `Google__RedirectUri` are optional for a same-origin deployment, but setting
them explicitly is recommended:

```text
Google__RedirectUri=https://your-domain.com/api/auth/google/callback
Web__Url=https://your-domain.com
Cors=https://your-domain.com
```

If the API is hosted on a different domain, set `VITE_API_URL` to its absolute URL before building
the web app, and set the three values above to match that deployment:

```powershell
$env:VITE_API_URL = "https://your-api-domain.com/api"
npm run build
```

Rebuild the SPA after changing `VITE_API_URL`, because Vite embeds it at build time. Then register
the exact API callback URL (`https://your-api-domain.com/api/auth/google/callback`) in Google Cloud
Console.

Keep the Data Protection key ring (`api/JobMailer.Api/keys`) on persistent storage so encrypted
Google tokens remain readable after an app restart.

## Authentication flow

1. The React app sends the browser to `GET /api/auth/google/start`. The API redirects to Google's
   consent screen with `access_type=offline`, `prompt=consent`, and the `gmail.send` scope.
2. Google redirects back to `GET /api/auth/google/callback`. The API validates the signed `state`,
   exchanges the code server-side, validates the Google ID token, and upserts the `Users` row.
3. Google access and refresh tokens are encrypted with ASP.NET Core Data Protection before being
   stored. The API redirects to the SPA with a short-lived JWT in the URL fragment; the SPA reads it,
   clears the fragment, and sends the JWT as a Bearer token.
4. `POST /api/email/send` uses the stored access token, refreshing it first when it expires. The
   refreshed token and expiry are written back to the database.
5. `POST /api/auth/logout` revokes the Google grant and clears the stored tokens.

Because consent runs on the server, the refresh token is available for background access-token
renewal. `prompt=consent` guarantees a new refresh token on every sign-in.

## API endpoints

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/auth/google/start` | Redirect the browser to Google's consent screen |
| GET | `/api/auth/google/callback` | Exchange Google's code and redirect back to the SPA |
| GET | `/api/auth/me` | Current user |
| POST | `/api/auth/logout` | Revoke Google tokens and clear the session |
| GET | `/api/profiles` | List the user's profiles |
| POST | `/api/profiles` | Create a profile (CV as base64) |
| PUT | `/api/profiles/{id}` | Update a profile |
| DELETE | `/api/profiles/{id}` | Delete a profile |
| GET | `/api/templates` | List email templates |
| POST | `/api/email/render` | Render subject/body placeholders |
| POST | `/api/email/send` | Send via Gmail with the profile CV attached |
| GET | `/api/applications` | Sent-application history |

## Data model

`Users`

- `UserId` (PK, int)
- `GoogleId` (`nvarchar(255)`, unique)
- `Email`, `FullName` (`nvarchar(255)`)
- `AccessToken`, `RefreshToken` (`nvarchar(max)`, **encrypted**)
- `TokenExpiry`, `CreatedAt`, `LastLogin` (`datetime2`)

`Profiles` — `Id`, `UserId` (FK), `Title`, `FullName`, `Phone`, `Email`, `CvFileName`, `CvBase64`,
`CreatedAt`, `UpdatedAt`.

`Templates` — `Id`, `Key`, `Subject`, `Body` (seeded with Full Stack and GIS templates).

`Applications` — `Id`, `UserId` (FK), `ProfileId`, `ProfileTitle`, `Company`, `JobTitle`, `ToEmail`,
`Subject`, `SentAt`.

## Security notes

- OAuth tokens are encrypted at rest with Data Protection. The key ring lives in
  `api/JobMailer.Api/keys` and is DPAPI-protected on Windows. Deleting it invalidates stored tokens
  and forces users to sign in again.
- JWTs are validated for issuer, audience, lifetime, and signing key.
- Logout revokes the Google grant rather than only clearing the browser session.
- CV uploads are limited to PDF/DOC/DOCX and 5 MB (validated on both client and server).
- CORS is restricted to the configured web origin.

## Build verification

```powershell
dotnet build JobMailer.sln
npm run build
```

`npm run build` also replaces the React files in `api/JobMailer.Api/wwwroot`, so the API project
always serves the latest frontend build.
