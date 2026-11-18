# Login System and Roles

The site uses **ASP.NET Identity**. Users, hashed passwords and roles are stored in the database tables created in
issue #35 (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`). No new migration is needed for the login system.

## Roles

| Role | Who gets it | What it can do |
|---|---|---|
| `User` | Everyone who registers on the site | Sign in, and (later) comment on blog posts |
| `Admin` | Only the account created from the User Secrets below | Everything a User can do, plus manage blogs and games |

Registering always gives the `User` role. There is no way to become an Admin from the website.

## 1. Create the first Admin account (once per computer)

The Admin email and password are stored in **User Secrets**, the same place as the connection string, so they are never committed.

1. Right-click the **DCSProject** project > **Manage User Secrets**.
2. Add a `SeedAdmin` section next to the existing `ConnectionStrings` section:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DCSProject;Trusted_Connection=True;TrustServerCertificate=True"
     },
     "SeedAdmin": {
       "Email": "your-admin@example.com",
       "Password": "ChooseAStrongPassword1"
     }
   }
   ```

3. Run the site. On startup it creates that account (if it does not exist yet) and gives it the `Admin` role.
   The password must follow the same rules as everyone else: at least 8 characters, with an uppercase letter, a lowercase letter and a number.

For a real hosted site, use environment variables named `SeedAdmin__Email` and `SeedAdmin__Password`, then remove them after the first run.

## 2. Protecting a page

Put an attribute on a controller or an action:

```csharp
[Authorize]                      // any signed-in user
[Authorize(Roles = "Admin")]     // Admins only
```

- Visitors who are not signed in are sent to `/Account/Login` and returned to the page they wanted afterwards.
- Signed-in users without the right role are sent to `/Account/AccessDenied`.
- In a view, `User.IsInRole("Admin")` and `User.Identity?.IsAuthenticated` show or hide parts of a page.
  Hiding a button is only for looks. The `[Authorize]` attribute is what actually protects the page.

## 3. Pages

| URL | Purpose |
|---|---|
| `/Account/Register` | Create an account (display name, email, password) |
| `/Account/Login` | Sign in |
| `/Account/Logout` | Sign out (POST only, uses the Logout button in the navbar) |
| `/Account/AccessDenied` | Shown when a role is not allowed |

## 4. Security settings in use

- Passwords are hashed by Identity. The site never stores or logs the real password.
- 8+ character passwords with upper, lower and a number.
- Accounts lock for 15 minutes after 5 wrong passwords in a row.
- Emails must be unique.
- The login cookie is HttpOnly, https only, and expires after 8 hours of no use.
- Login, register and logout forms all use anti-forgery tokens.
- The "return to the page you wanted" link only works for pages on this site.
- The login error says "Invalid email or password" without saying which one was wrong.

Not included yet: email confirmation and password reset (they need an email service).

## 5. Testing checklist

1. Register a new account. You should be signed in and see "Hello, name" in the navbar.
2. Logout, then Login with the same account.
3. Try a wrong password 5 times. The account should lock.
4. Try to register with the same email again. You should get an error.
5. In SQL Server Object Explorer, open `AspNetUsers`. The `PasswordHash` column should be long unreadable text, and `AspNetUserRoles` should link your user to the `User` role.
6. Sign in with the Admin account and check `AspNetUserRoles` shows the `Admin` role.