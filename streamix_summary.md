# 🚀 Interview Revision Sheet: Streamix & ASP.NET Core

This document summarizes all the key talking points, technical concepts, and architectural decisions we discussed. Review this before your interview!

---

## 📖 1. The Interview Storyline
**Your Pitch:** *"I architected a polyglot microservices platform (Streamix) in Java to master distributed systems. To transition my skills to the .NET stack, I built a Course Enrollment system using ASP.NET Core MVC. This allowed me to map my advanced understanding of Domain Modeling, ORMs, and Controllers directly into C# and Entity Framework Core."*

**Separation of Concerns (SoC):**
*   **MVC Monolith (Course Enrollment):** SoC happens within *folders* (Models, Views, Controllers, Data) in the same memory space. If one part crashes, the whole app crashes.
*   **Microservices (Streamix):** SoC happens across *network boundaries*. The "View" is a standalone React app. The "Controllers/Models" are split into independent backend services. If the Catalog service crashes, the Identity service stays up.

---

## 🗄️ 2. Polyglot Database Design
*"The Right Tool for the Job"*

*   **PostgreSQL (Identity Service):** Used for User Credentials. Requires strict **ACID compliance**, rigid relational structure, and uncompromising data integrity.
*   **MongoDB (Catalog & Interaction Services):** Used for Movie Metadata and Watch History. TMDB API data is heavily nested JSON. MongoDB (NoSQL) stores this as single, flexible documents, avoiding massive SQL `JOIN` operations. Watch History generates high-velocity writes, which NoSQL handles effortlessly.
*   **Redis (API Gateway):** Used for Rate Limiting, Session Concurrency, and Refresh Tokens. It is an **in-memory (RAM)** data store, offering sub-millisecond lookups (bypassing slow Disk I/O). It has native **TTL (Time-To-Live)** support, automatically deleting rate-limit counters or expired sessions without needing background cron jobs.

---

## 🛡️ 3. Advanced Security Concepts

### JWT & Authentication Pattern
*   **What is a JWT?** A stateless token containing a Header, Payload (claims like email/ID), and a cryptographically secure **Signature**.
*   **Why Stateless?** The server verifies the signature using a Secret Key. It does not need to look up a database to know who the user is, saving massive CPU and Database bottlenecking.
*   **The Dual-Token Pattern:** 
    *   *Access Token (15-min):* Stateless. If stolen, damage is limited to 15 minutes.
    *   *Refresh Token (Long-lived):* Stateful (stored in Redis). Used to silently fetch new Access Tokens (via Axios Interceptors). Can be deleted by an admin to force a global logout.
*   **BCrypt Password Hashing:** A one-way hash. Uses a **Salt** (random string) to defeat Rainbow Tables, and is intentionally slow (Work Factor) to defeat brute-force guessing attacks.

### API Gateway Defenses
*   **Single Entry Point:** The React frontend only talks to the Gateway. Microservices are hidden on a private internal network.
*   **Centralized Verification:** The Gateway verifies the JWT signature *once*, extracts the email, and passes it downstream via an `X-User-Email` header. Downstream services do no JWT math.
*   **Internal Protection (Defense in Depth):** If a hacker discovers a microservice IP, they are blocked. The Gateway injects an `X-Internal-Secret` header into traffic. Microservices use an `InternalApiFilter` to reject any request missing this secret.

---

## 🌐 4. Network Fundamentals (The Protocol Stack)

*   **TCP (Transport Layer):** The "Mail Truck." Establishes a connection (3-way handshake) and guarantees ordered, lossless delivery of data packets.
*   **HTTP (Application Layer):** The "Letter." Formats the GET/POST request. By default, it is insecure plain text.
*   **SSL/TLS (Cryptographic Layer):** The "Unbreakable Safe" that creates HTTPS. 

### The TLS Handshake (How Keys Work)
1.  **Asymmetric Cryptography (Heavy Math):** The server shares its **Public Key** via an SSL Certificate. The browser generates a random **Session Key**, encrypts it with the server's Public Key, and sends it. Only the server's highly guarded **Private Key** can decrypt and read the Session Key.
2.  **Symmetric Cryptography (Hardware Accelerated):** Asymmetric math is too slow for streaming video. Once the handshake safely exchanges the Session Key, both sides switch to **Symmetric encryption (AES)** for all HTTP traffic. This is blazing fast because modern CPUs have dedicated hardware circuits (AES-NI) to process it instantly.

### Security Headers (Your `SecurityHeadersFilter`)
*   **HSTS:** Forces the browser to strictly use HTTPS, preventing downgrade attacks.
*   **CSP (Content-Security-Policy):** Blocks malicious scripts from executing (prevents XSS).
*   **X-Frame-Options (DENY):** Prevents Clickjacking by stopping other sites from embedding your app in an `<iframe>`.

---

## 💻 5. C# & .NET Core specific focal points

*   **Dependency Injection (Loose Coupling):** Injecting interfaces (e.g., `IStudentService`) into Controller constructors instead of using `new StudentService()`. Lifetimes: *Transient* (every time), *Scoped* (per HTTP request), *Singleton* (once per app).
*   **Single Responsibility Principle (SRP):** Controllers handle HTTP routing. `AppDbContext` handles SQL queries. `Services` handle business logic.
*   **LINQ:** Master methods like `.Where()`, `.SelectMany()` (flattening lists), `.GroupBy()`, and knowing that `.GroupBy` returns an object where the grouped value is stored in the `.Key` property.
*   **EF Core (Entity Framework):** C#'s ORM. Replaces Spring Data JPA. Translates C# Objects into SQL tables. Uses "Deferred Execution" (queries aren't executed until `.ToList()` is called).
