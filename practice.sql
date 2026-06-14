
-- Yes, practice them. He specifically said relational DB and table design — queries will come up.

-- Here are 10. Write the SQL by hand, don't look anything up:

-- ---

-- **Easy — warmup:**

-- 1. Get all students who enrolled after January 1, 2024

select * from students where enrolledOn > 2024-01-01

SELECT * FROM Students WHERE EnrollmentDate > '2024-01-01';


-- 2. Get the total number of enrollments per student, show student name and count, order by count descending

select students.name, Count(*) as Count from students inner join enrollments on student.id = enrollment.student_id group by student.name order by Count desc

SELECT s.Name, COUNT(*) AS Count
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
GROUP BY s.Id, s.Name
ORDER BY Count DESC;

-- 3. Get all courses that have more than 2 students enrolled
select course.title from courses inner join enrollments on course.id = enrollment.course_id group by course.title having count(*) > 2

SELECT c.Title
FROM Courses c
INNER JOIN Enrollments e ON c.Id = e.CourseId
GROUP BY c.Id, c.Title
HAVING COUNT(*) > 2;

-- ---

-- **Medium — joins:**

-- 4. Get names of all students along with the titles of courses they're enrolled in. Students with no enrollments should still appear.
select student.name, course.title from students left join enrollments on students.id = enrollments.student_id inner join courses on enrollment.course_id on course.id

-- Yours: inner join courses on enrollment.course_id on course.id — invalid syntax

-- Correct
SELECT s.Name, c.Title
FROM Students s
LEFT JOIN Enrollments e ON s.Id = e.StudentId
LEFT JOIN Courses c ON e.CourseId = c.Id;
-- Second join must also be LEFT JOIN
-- If student has no enrollment, e is null, so c would also be null
-- Using INNER JOIN for courses would exclude unenrolled students

----------

-- 5. Get all courses that have NO students enrolled
select courses.name from courses left join enrollments on course.id = enrollemnts.course_id where enrollments.course_id is null

SELECT c.Title
FROM Courses c
LEFT JOIN Enrollments e ON c.Id = e.CourseId
WHERE e.CourseId IS NULL;

---------------

-- 6. Get the name of the student enrolled in the most courses
-- Basic version (yours is correct)
SELECT s.Name
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
GROUP BY s.Id, s.Name
ORDER BY COUNT(*) DESC
LIMIT 1;

-- If multiple students tied for most — subquery approach:
SELECT s.Name
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
GROUP BY s.Id, s.Name
HAVING COUNT(*) = (
    SELECT MAX(cnt) FROM (
        SELECT COUNT(*) AS cnt
        FROM Enrollments
        GROUP BY StudentId
    ) counts
);

-- **Medium — aggregates:**

-- 7. Get the average number of enrollments per course
select course.name, avg(*) from courses inner join enrollments on course.id= enrollment.course_id group by course.name

AVG is for averaging a columns values, not counting. What you want is: count enrollments per course, then average those counts.
-- Correct
SELECT AVG(EnrollmentCount) AS AvgEnrollments
FROM (
    SELECT COUNT(*) AS EnrollmentCount
    FROM Enrollments
    GROUP BY CourseId
) counts;

-- 8. Get all students who are enrolled in ALL three of these courses: Mathematics, Physics, Computer Science
select student.name from students inner join enrollments on students.id = enrollments.student_id inner join courses on enrollment.course_id on course.id where course.name = 'Mathematics' and course.name = 'Physics' and course.name = 'CS' (ig this is wrong)

SELECT s.Name
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
INNER JOIN Courses c ON e.CourseId = c.Id
WHERE c.Title IN ('Mathematics', 'Physics', 'Computer Science')
GROUP BY s.Id, s.Name
HAVING COUNT(DISTINCT c.Title) = 3;
-- COUNT(DISTINCT c.Title) = 3 means student has all three
-- ---

-- **Hard:**

-- 9. For each course, get the most recently enrolled student's name and enrollment date
SELECT c.Title, s.Name, e.EnrolledOn
FROM Enrollments e
INNER JOIN Students s ON e.StudentId = s.Id
INNER JOIN Courses c ON e.CourseId = c.Id
WHERE e.EnrolledOn = (
    SELECT MAX(e2.EnrolledOn)
    FROM Enrollments e2
    WHERE e2.CourseId = e.CourseId  -- correlated subquery — max per course
);

-- 10. Get pairs of students who are enrolled in at least one common course (no duplicate pairs like Alice-Bob and Bob-Alice)

SELECT s1.Name, s2.Name
FROM Enrollments e1
INNER JOIN Enrollments e2 ON e1.CourseId = e2.CourseId
    AND e1.StudentId < e2.StudentId  -- prevents duplicates and self-pairs
INNER JOIN Students s1 ON e1.StudentId = s1.Id
INNER JOIN Students s2 ON e2.StudentId = s2.Id
GROUP BY s1.Id, s1.Name, s2.Id, s2.Name;

