# ASP.NET Core MVC & EF Core Interview Cheat Sheet

## 1. Entity Framework Core Concepts

### DbContext vs. DbSet
*   **`DbContext`:** The bridge to the database. It manages the connection, holds the **Change Tracker** (which tracks object modifications in memory), and executes `SaveChanges()`. Registered as **Scoped** in `Program.cs`.
*   **`DbSet<T>`:** The "gateway" to a specific table. It translates LINQ queries to SQL. It is **not** a list that stores data; it remains empty while the Change Tracker holds the actual objects.

### The "Internal Map" & `OnModelCreating`
*   **The Model Blueprint:** EF Core builds an invisible metadata map of your database relationships.
*   **`OnModelCreating`:** A `protected` method used to explicitly define complex rules EF Core can't guess (like Composite Primary Keys or specific join behaviors) using the Fluent API. 
*   **Execution:** It runs **exactly once** per application lifetime (on the very first query) and caches the result for speed.

### Keeping Objects in Sync (Navigation Fix-up)
If you add an `Enrollment` object with a specific `StudentId`, EF Core uses its blueprint and Foreign Keys to automatically insert that enrollment into the in-memory `Student.Enrollments` list. This is called **Navigation Fix-up**.

### Database Concurrency (Race Conditions)
Because DbContext is Scoped, multiple users don't corrupt each other's server memory. However, if two users edit the exact same database row simultaneously, EF Core defaults to **"Last In Wins"**. To prevent data loss, you implement **Optimistic Concurrency Control** (e.g., using a `[Timestamp]` attribute).

### ORM (Object-Relational Mapper)
An abstraction layer that maps C# classes to SQL tables, translates C# LINQ into native SQL queries, and **Materializes** (hydrates) the returned flat database rows into tracked, interconnected C# objects.

---

## 2. Querying Data

### `Find` vs `FirstOrDefault` vs `First`
*   **`Find(id)`:** Searches *only* by Primary Key. **Fastest** because it checks the Change Tracker memory before hitting the database. **Limitation:** Cannot chain `.Include()`.
*   **`FirstOrDefault(condition)`:** Searches by any condition. Hits the database every time. **Best feature:** Supports `.Include()` for loading related data. Returns `null` if nothing is found. Used 90% of the time.
*   **`First(condition)`:** Same as above, but **crashes** (throws `InvalidOperationException`) if nothing is found. Use only when you are 100% mathematically certain the record exists.

*Note: `FirstOrDefault` is a universal LINQ method that works on any C# collection. `Find(id)` is EF Core specific.*

---

## 3. ASP.NET Core MVC Architecture

### Model Binding
The engine that automatically looks at the incoming HTTP request (Form Data, Route URL, Query String), extracts the text, converts it to C# data types (like integers and Dates), and populates your Controller method parameters or objects.

### The `[Bind]` Attribute (Security)
Used to prevent **Overposting / Mass Assignment attacks**. It provides a strict whitelist of properties the Model Binder is allowed to populate. If omitted, a hacker could use a tool like Postman to submit hidden fields (e.g., `IsAdmin=true`) and force the server to save them.

### Data Annotations
Attributes placed above model properties (e.g., `[Required]`, `[StringLength]`). They serve three purposes:
1.  **Server-Side Validation:** Populates `ModelState.IsValid` in the controller.
2.  **UI & Client-Side:** Tells Razor to generate jQuery validation and specific HTML input types.
3.  **EF Core Schema:** Can define database rules like `[Key]` or `[NotMapped]`.

---

## 4. Razor UI Concepts

### ASP.NET Core MVC vs. Razor Pages
*   **MVC:** Highly separated. Controllers intercept URLs, fetch data, and pass it to a `.cshtml` Razor View.
*   **Razor Pages:** Page-centric (no controllers). A `.cshtml` view is paired directly with a `.cshtml.cs` code-behind file. Uses **Page Handlers** (like `OnGet()` and `OnPostAsync()`) instead of Action Methods.

### Layouts & Partial Views
*   **Layout (`_Layout.cshtml`):** The "Master Template" (navbars, footers). It wraps around specific page content using the `@RenderBody()` method.
*   **Partial View:** A reusable chunk of Razor HTML (like a Course Card component) that you can inject into multiple different views to avoid duplicating code.

### Blazor
Microsoft's framework for building rich, interactive Single Page Applications (SPAs) entirely in **C# and HTML** instead of JavaScript. It runs natively in the browser using WebAssembly, or over WebSockets via Blazor Server.

---

## 5. C# OOP & Modern Language Features

### Access Modifiers (Public vs Private)
*   **Data Bags (DTOs / EF Models):** Use `public { get; set; }`. They exist purely to carry data for the framework to read/write.
*   **True OOP Logic Classes:** Use `private` fields for actual data storage, and `public { get; private set; }` properties. This enforces **Encapsulation**, forcing other developers to use your methods to change the data.
*   **Inheritance:** Use `protected` to hide data from the outside world, but share it between a Parent and Child class.

### Nullable Reference Types (Blue Squiggly Lines)
In modern .NET, the compiler warns you if properties that default to `null` are uninitialized. To fix them in EF Core models:
*   **Strings:** `public string Name { get; set; } = string.Empty;`
*   **Collections:** `public List<Enrollment> Enrollments { get; set; } = new();`
*   **Required Navigation Objects:** `public Student Student { get; set; } = null!;` (The null-forgiving operator tells the compiler EF Core will fill it in later).
