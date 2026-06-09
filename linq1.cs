using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int AuthorId { get; set; }
    public double Price { get; set; }
    public int PublishYear { get; set; }
}

public class Program
{
    public static void Main()
    {
        var authors = new List<Author>
        {
            new Author { Id = 1, Name = "George Orwell", Country = "UK" },
            new Author { Id = 2, Name = "Haruki Murakami", Country = "Japan" },
            new Author { Id = 3, Name = "J.K. Rowling", Country = "UK" },
            new Author { Id = 4, Name = "Isaac Asimov", Country = "Russia" }
        };

        var books = new List<Book>
        {
            new Book { Id = 101, Title = "1984", AuthorId = 1, Price = 15.99, PublishYear = 1949 },
            new Book { Id = 102, Title = "Animal Farm", AuthorId = 1, Price = 9.99, PublishYear = 1945 },
            new Book { Id = 103, Title = "Norwegian Wood", AuthorId = 2, Price = 14.50, PublishYear = 1987 },
            new Book { Id = 104, Title = "Kafka on the Shore", AuthorId = 2, Price = 18.20, PublishYear = 2002 },
            new Book { Id = 105, Title = "Harry Potter", AuthorId = 3, Price = 25.00, PublishYear = 1997 },
            new Book { Id = 106, Title = "Foundation", AuthorId = 4, Price = 12.00, PublishYear = 1951 },
            new Book { Id = 107, Title = "I, Robot", AuthorId = 4, Price = 10.50, PublishYear = 1950 }
        };

        // Write your LINQ queries below!

        //Level 1: The Basics(Filtering and Selecting)

        //Find cheap books: Write a query to find all books that cost less than $15.Return the entire Book objects.

        var cheapBooks = books.Where(b => b.Price < 15).ToList();

        //Extract titles only: Write a query to get a list of just the Title(strings) of books published after 1990.
        var titles = books.Where(b=> b.PublishYear > 1990).Select(b=> b.Title).ToList();

        //Level 2: Shaping Data(Anonymous Types and Sorting)
        //3.Format the output: Write a query that finds all books written by AuthorId 1.Select a new anonymous object containing only the Title and Price, and order them by Price from highest to lowest(descending).
        var result = books.Where(b => b.AuthorId == 1).Select(b => new { b.Title, b.Price }).OrderByDescending(b => b.Price).ToList();

        //Level 3: Math and Single Elements
        //4.Find the oldest book: Use LINQ to figure out the PublishYear of the oldest book in the list.
        var oldestBook = books.OrderBy(b => b.PublishYear).FirstOrDefault();

        //5.Check for existence: Write a query that returns a bool(true / false) if there are any books in the list that cost more than $20.
        bool result1 = books.Any(b => b.Price > 20);

        //Level 4: The Boss Fights(Joining and Grouping)
        //6.Join lists: Write a query that joins the books list and the authors list. Return an anonymous object with the BookTitle and the AuthorName.
        var joinResult = books.Join(authors, b => b.AuthorId, a => a.Id, (b, a) => new { BookName = b.Title, Author = a.Name });
        var joinQuery = from b in books
                        join a in authors on b.AuthorId equals a.Id
                        select new
                        {
                            BookName = b.Title,
                            AuthorName = a.Name
                        };

        //7.Group by Author: Group the books list by their AuthorId.Select an anonymous object that contains the AuthorId and the count of how many books that author has written.
        var groupRes = books.GroupBy(b => b.AuthorId).Select(b => new { AuthorId = b.Key, CountOfBooks = b.Count() });


        
    }
}
