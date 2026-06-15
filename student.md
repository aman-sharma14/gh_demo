**Task 1 — Search and Sort**

```csharp
// Controller
public async Task<IActionResult> Index(string? searchName, int minCourses = 0)
{
    var students = await _context.Students
        .Include(s => s.Enrollments)
        .ToListAsync();

    if (!string.IsNullOrEmpty(searchName))
        students = students.Where(s => s.Name.Contains(searchName, 
            StringComparison.OrdinalIgnoreCase)).ToList();

    if (minCourses > 0)
        students = students.Where(s => s.Enrollments.Count >= minCourses).ToList();

    students = students.OrderByDescending(s => s.EnrollmentDate).ToList();

    ViewData["searchName"] = searchName;
    ViewData["minCourses"] = minCourses;

    return View(students);
}
```

```html
<!-- View — search form -->
<form asp-action="Index" method="get">
    <input type="text" name="searchName" value="@ViewData["searchName"]" placeholder="Search by name" />
    <input type="number" name="minCourses" value="@ViewData["minCourses"]" placeholder="Min courses" min="0" />
    <button type="submit">Search</button>
</form>

@if (!Model.Any())
{
    <p>No students found.</p>
}
else
{
    <table>
        <!-- table rows -->
    </table>
}
```

---

**Task 2 — Enrollment Validation with Capacity**

```csharp
// Add to Course model
public int MaxCapacity { get; set; }
```

```csharp
// Enroll POST — returns feedback instead of just redirecting
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Enroll(int id, List<int> selectedCourseIds)
{
    selectedCourseIds ??= new List<int>();

    var student = await _context.Students
        .Include(s => s.Enrollments)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null) return NotFound();

    var rejectedCourses = new List<string>();

    // Remove existing enrollments
    var existing = _context.Enrollments.Where(e => e.StudentId == id);
    _context.Enrollments.RemoveRange(existing);
    await _context.SaveChangesAsync();

    foreach (var courseId in selectedCourseIds)
    {
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) continue;

        if (course.Enrollments.Count >= course.MaxCapacity)
        {
            rejectedCourses.Add(course.Title);
            continue;
        }

        _context.Enrollments.Add(new Enrollment
        {
            StudentId = id,
            CourseId = courseId,
            EnrolledOn = DateOnly.FromDateTime(DateTime.Now)
        });
    }

    await _context.SaveChangesAsync();

    if (rejectedCourses.Any())
    {
        TempData["CapacityWarning"] = "Following courses were full: " + 
            string.Join(", ", rejectedCourses);
        return RedirectToAction("Details", new { id });
    }

    return RedirectToAction("Details", new { id });
}
```

```html
<!-- In Details view -->
@if (TempData["CapacityWarning"] != null)
{
    <p style="color:red;">@TempData["CapacityWarning"]</p>
}
```

---

**Task 3 — Report Page**

```csharp
// ViewModel
public class ReportViewModel
{
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public string BusiestCourse { get; set; } = string.Empty;
    public string MostEnrolledStudent { get; set; } = string.Empty;
    public List<Student> StudentsWithNoEnrollments { get; set; } = new();
}
```

```csharp
// Controller action
public async Task<IActionResult> Report()
{
    var students = await _context.Students
        .Include(s => s.Enrollments)
        .ToListAsync();

    var courses = await _context.Courses
        .Include(c => c.Enrollments)
        .ToListAsync();

    var vm = new ReportViewModel
    {
        TotalStudents = students.Count,
        TotalCourses = courses.Count,
        TotalEnrollments = await _context.Enrollments.CountAsync(),

        BusiestCourse = courses
            .OrderByDescending(c => c.Enrollments.Count)
            .FirstOrDefault()?.Title ?? "None",

        MostEnrolledStudent = students
            .OrderByDescending(s => s.Enrollments.Count)
            .FirstOrDefault()?.Name ?? "None",

        StudentsWithNoEnrollments = students
            .Where(s => s.Enrollments.Count == 0)
            .ToList()
    };

    return View(vm);
}
```

```html
@model ReportViewModel

<h2>Clinic Report</h2>
<p>Total Students: @Model.TotalStudents</p>
<p>Total Courses: @Model.TotalCourses</p>
<p>Total Enrollments: @Model.TotalEnrollments</p>
<p>Busiest Course: @Model.BusiestCourse</p>
<p>Most Enrolled Student: @Model.MostEnrolledStudent</p>

<h3>Students with no enrollments</h3>
@if (!Model.StudentsWithNoEnrollments.Any())
{
    <p>All students are enrolled in at least one course.</p>
}
else
{
    <ul>
        @foreach (var s in Model.StudentsWithNoEnrollments)
        {
            <li>@s.Name</li>
        }
    </ul>
}
```

---

**Task 4 — Bulk Unenroll**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ClearEnrollments(int id)
{
    var enrollments = _context.Enrollments.Where(e => e.CourseId == id);

    if (!enrollments.Any())
    {
        TempData["Message"] = "No enrollments to remove.";
        return RedirectToAction("Details", new { id });
    }

    _context.Enrollments.RemoveRange(enrollments);
    await _context.SaveChangesAsync();

    TempData["Message"] = "All enrollments removed.";
    return RedirectToAction("Details", new { id });
}
```

```html
<!-- In Course Details view -->
@if (TempData["Message"] != null)
{
    <p>@TempData["Message"]</p>
}

<form asp-action="ClearEnrollments" asp-route-id="@Model.Id" method="post">
    @Html.AntiForgeryToken()
    <button type="submit">Remove All Enrollments</button>
</form>
```

---

**Task 5 — Prevent Duplicate Titles**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Enroll(int id, List<int> selectedCourseIds)
{
    selectedCourseIds ??= new List<int>();

    var student = await _context.Students
        .Include(s => s.Enrollments)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null) return NotFound();

    // Get all selected courses
    var selectedCourses = await _context.Courses
        .Where(c => selectedCourseIds.Contains(c.Id))
        .ToListAsync();

    // Get titles already enrolled (before clearing)
    var currentlyEnrolledTitles = student.Enrollments
        .Select(e => e.Course?.Title?.ToLower())
        .Where(t => t != null)
        .ToHashSet();

    var skipped = new List<string>();
    var seenTitles = new HashSet<string>(currentlyEnrolledTitles!);

    // Remove existing
    _context.Enrollments.RemoveRange(
        _context.Enrollments.Where(e => e.StudentId == id));
    await _context.SaveChangesAsync();

    seenTitles.Clear(); // reset — re-add from selected only

    foreach (var course in selectedCourses)
    {
        var titleLower = course.Title.ToLower();

        if (seenTitles.Contains(titleLower))
        {
            skipped.Add(course.Title);
            continue;
        }

        seenTitles.Add(titleLower);
        _context.Enrollments.Add(new Enrollment
        {
            StudentId = id,
            CourseId = course.Id,
            EnrolledOn = DateOnly.FromDateTime(DateTime.Now)
        });
    }

    await _context.SaveChangesAsync();

    if (skipped.Any())
        TempData["Skipped"] = "Skipped duplicate titles: " + string.Join(", ", skipped);

    return RedirectToAction("Details", new { id });
}
```

---

**Key concepts each task tested:**

Task 1 — chained LINQ filters, method params with defaults, ViewData roundtrip.

Task 2 — TempData for cross-redirect messages, capacity validation before insert, partial success.

Task 3 — ViewModel aggregating data from multiple tables, LINQ reporting with no DB columns.

Task 4 — TempData, conditional DB hit, RemoveRange.

Task 5 — HashSet for O(1) duplicate detection, case-insensitive comparison, partial enrollment with feedback.

Skim the patterns, not just the code. That's what you need tomorrow.
