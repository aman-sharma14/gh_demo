using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EmployeeSolutions
{
    class Program
    {
        static void Main(string[] args)
        {
            // Make sure to update this path to where your XML file is located
            XDocument doc = XDocument.Load("C:\\Path\\To\\Your\\Employees.xml");
            var employees = doc.Root.Elements("Employee");

            // --- 1. Display the full details of all employees who work in the "IT" department. ---
            Console.WriteLine("1. Employees in the IT department:");
            var itEmployees = employees.Where(e => e.Element("Department").Value == "IT");
            foreach (var emp in itEmployees)
            {
                Console.WriteLine(emp);
            }
            Console.WriteLine("------------------------------------");

            // --- 2. Display only the full name and the salary of every employee. ---
            Console.WriteLine("\n2. Employee names and salaries:");
            var namesAndSalaries = employees.Select(e => new
            {
                FullName = e.Element("FirstName").Value + " " + e.Element("LastName").Value,
                Salary = (int)e.Element("Salary")
            });
            foreach (var emp in namesAndSalaries)
            {
                Console.WriteLine($"{emp.FullName} - {emp.Salary:C}");
            }
            Console.WriteLine("------------------------------------");

            // --- 3. Find all employees who earn more than $60,000, sorted. ---
            Console.WriteLine("\n3. Employees earning > $60,000 (sorted high to low):");
            var highEarners = employees
                .Where(e => (int)e.Element("Salary") > 60000)
                .OrderByDescending(e => (int)e.Element("Salary"))
                .Select(e => new
                {
                    FullName = e.Element("FirstName").Value + " " + e.Element("LastName").Value,
                    Salary = (int)e.Element("Salary")
                });
            foreach (var emp in highEarners)
            {
                Console.WriteLine($"{emp.FullName} - {emp.Salary:C}");
            }
            Console.WriteLine("------------------------------------");

            // --- 4. Find names of all employees who joined before the year 2022. ---
            Console.WriteLine("\n4. Employees who joined before 2022:");
            var earlyJoiners = employees
                .Where(e => DateTime.Parse(e.Element("JoinDate").Value).Year < 2022)
                .Select(e => e.Element("FirstName").Value + " " + e.Element("LastName").Value);
            foreach (var name in earlyJoiners)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine("------------------------------------");

            // --- 5. Group employees by Department and display the count. ---
            Console.WriteLine("\n5. Employee count by department:");
            var deptGroups = employees.GroupBy(e => e.Element("Department").Value);
            foreach (var group in deptGroups)
            {
                Console.WriteLine($"Department: {group.Key}, Count: {group.Count()}");
            }
            Console.WriteLine("------------------------------------");

            // --- 6. Calculate total salary paid to employees in the "Sales" department. ---
            Console.WriteLine("\n6. Total salary for the Sales department:");
            var totalSalesSalary = employees
                .Where(e => e.Element("Department").Value == "Sales")
                .Sum(e => (int)e.Element("Salary"));
            Console.WriteLine($"Total: {totalSalesSalary:C}");
            Console.WriteLine("------------------------------------");
            
            // --- 7. Find the most recently hired employee(s). ---
            Console.WriteLine("\n7. Most recently hired employee(s):");
            var latestDate = employees.Max(e => DateTime.Parse(e.Element("JoinDate").Value));
            var recentHires = employees.Where(e => DateTime.Parse(e.Element("JoinDate").Value) == latestDate);
            foreach(var emp in recentHires)
            {
                Console.WriteLine($"{emp.Element("FirstName").Value} {emp.Element("LastName").Value} on {latestDate:d}");
            }
            Console.WriteLine("------------------------------------");
            
            // --- 8. Find the maximum salary for each department. ---
            Console.WriteLine("\n8. Maximum salary by department:");
            var maxSalaryByDept = employees
                .GroupBy(e => e.Element("Department").Value)
                .Select(g => new
                {
                    Department = g.Key,
                    MaxSalary = g.Max(e => (int)e.Element("Salary"))
                });
            foreach (var dept in maxSalaryByDept)
            {
                Console.WriteLine($"Department: {dept.Department}, Max Salary: {dept.MaxSalary:C}");
            }
            Console.WriteLine("------------------------------------");

            // --- 9. Calculate the average salary for each department. ---
            Console.WriteLine("\n9. Average salary by department:");
            var avgSalaryByDept = employees
                .GroupBy(e => e.Element("Department").Value)
                .Select(g => new
                {
                    Department = g.Key,
                    AverageSalary = g.Average(e => (double)e.Element("Salary"))
                });
            foreach (var dept in avgSalaryByDept)
            {
                Console.WriteLine($"Department: {dept.Department}, Average Salary: {dept.AverageSalary:C}");
            }
            Console.WriteLine("------------------------------------");

            // --- 10. Find employee whose EmployeeID attribute is "107". ---
            Console.WriteLine("\n10. Employee with ID 107:");
            var employee107 = employees.FirstOrDefault(e => e.Attribute("EmployeeID").Value == "107");
            if (employee107 != null)
            {
                Console.WriteLine($"{employee107.Element("FirstName").Value} {employee107.Element("LastName").Value}");
            }
            Console.WriteLine("------------------------------------");

            // --- 11. Count IT employees who joined after the end of 2021. ---
            Console.WriteLine("\n11. IT employees who joined after 2021:");
            var itCountAfter2021 = employees
                .Count(e => e.Element("Department").Value == "IT" && DateTime.Parse(e.Element("JoinDate").Value).Year > 2021);
            Console.WriteLine($"Count: {itCountAfter2021}");
            Console.WriteLine("------------------------------------");
            
            // --- 12. Display employees sorted by Department, then by LastName. ---
            Console.WriteLine("\n12. Employees sorted by Department, then LastName:");
            var sortedEmployees = employees
                .OrderBy(e => e.Element("Department").Value)
                .ThenBy(e => e.Element("LastName").Value)
                .Select(e => new {
                    Dept = e.Element("Department").Value,
                    Name = e.Element("FirstName").Value + " " + e.Element("LastName").Value
                });
            foreach (var emp in sortedEmployees)
            {
                Console.WriteLine($"Dept: {emp.Dept}, Name: {emp.Name}");
            }
            Console.WriteLine("------------------------------------");
            
            // --- 13. Find the employee with the lowest salary in the "Sales" department. ---
            Console.WriteLine("\n13. Lowest salary employee in Sales:");
            var lowestSalesEmployee = employees
                .Where(e => e.Element("Department").Value == "Sales")
                .OrderBy(e => (int)e.Element("Salary"))
                .FirstOrDefault();
            if (lowestSalesEmployee != null)
            {
                Console.WriteLine($"{lowestSalesEmployee.Element("FirstName").Value} {lowestSalesEmployee.Element("LastName").Value}");
            }
            Console.WriteLine("------------------------------------");

            // --- 14. Display name and years of service (as of Sep 28, 2025). ---
            Console.WriteLine("\n14. Employee years of service (as of Sep 28, 2025):");
            DateTime currentDate = new DateTime(2025, 9, 28);
            var yearsOfService = employees.Select(e => {
                DateTime joinDate = DateTime.Parse(e.Element("JoinDate").Value);
                double years = (currentDate - joinDate).TotalDays / 365.25; // Account for leap years
                return new {
                    FullName = e.Element("FirstName").Value + " " + e.Element("LastName").Value,
                    Service = years
                };
            });
            foreach (var emp in yearsOfService)
            {
                Console.WriteLine($"{emp.FullName} - Years of Service: {emp.Service:F1}");
            }
            Console.WriteLine("------------------------------------");
        }
    }
}
