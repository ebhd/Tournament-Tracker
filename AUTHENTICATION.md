# Authentication port

The branch starts at `opleveringP5` (`f68da36417537b7423938bba1d562324ad45eafa`).
`Sprint-8` (`3503a590108c795d695eb953c12a872cd0ad0afe`) supplies only the identity and authorization behavior.

## Added behavior

- Identity registration, login, logout, default account management, cookies, and `user`/`admin` roles.
- New registrations receive only the `user` role.
- Team creation, championship creation, and adding a team to a group require login.
- Team creation takes the maintainer ID from the authenticated principal, never from submitted form data.
- `PUT /api/teams/{teamId}/teamvalue` permits only that team's maintainer or an admin. Negative values are rejected; `null` clears the value.
- API authentication failures return 401/403 without redirecting to HTML login pages.
- Cookie-authenticated writes require an antiforgery token. Forms supply it automatically; JSON clients must send the page's `__RequestVerificationToken` value in the `RequestVerificationToken` header, together with the same session cookies. `/Group/Details/1` supplies a token after login.

## Baseline preserved

The existing controllers, DTOs, `NewTeamViewModel`, validation, exception handling, repository methods, console UI, seed data, relationship rules, XML formatters, default Home route, HTTPS handling, static assets, and monitoring remain based on P5. Existing successful API writes still return their original DTOs, 201 status, and resource Location headers.

`Program.cs` adds Identity registration, API cookie callbacks, identity seeding, authentication middleware, and Razor Page registration/mapping. Its existing startup flow is retained. Identity seed code lives in a separate authentication-only file.

The domain stores only `Team.MaintainerId`; it does not reference ASP.NET Identity. The DAL maps that ID to the Identity user with restricted deletion. Ownership checks use stable IDs rather than mutable usernames. The existing EF Core 8.0.22 dependency is retained, with the matching Identity EF package.

The optional maintainer argument preserves P5's console and direct manager creation calls. These teams have no web owner and can only be edited through the new API by an admin. Web-created teams always have an owner. All six P5 seed teams receive the same owner sequence as Sprint-8 without replacing any P5 seed data.

## Run and verify

With a .NET 9 SDK:

```powershell
dotnet build TournamentTracker.sln
dotnet test TournamentTracker.Tests/TournamentTracker.Tests.csproj
dotnet run --project TournamentTracker.UI-MVC --launch-profile http
```

Install the .NET 9 SDK using the standard installer for your operating system.
On Windows x64, the default installation directory is `C:\Program Files\dotnet`.

The application is available at `http://localhost:5199` with that launch profile.

Sprint-8's evaluator accounts are retained. All use `Password1!`:

| Accounts | Role |
| --- | --- |
| lars@kdg.be, ebrahim@kdg.be, ayoub@kdg.be | user |
| ali@kdg.be, chris@kdg.be | admin |

P5's `CreateDatabase(dropDatabase: true)` behavior remains: starting the application recreates the database, including registered accounts. These are evaluator credentials, and the existing startup remains a demonstration setup rather than a persistent production deployment.

Tests use an isolated in-memory SQLite database and real Identity cookies. They cover public P5 routes, anonymous write rejection, registration and role assignment, owner spoofing, owner/non-owner/admin permissions, antiforgery protection, logout, redirect safety, API response contracts, and baseline validation and seed relationships.
