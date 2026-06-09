using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string Membership { get; set; }
}

public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public bool IsDelivered { get; set; }
    public List<OrderItem> Items { get; set; }
}

public class Program
{
    public static void Main()
    {
        var customers = new List<Customer>
        {
            new Customer { CustomerId = 1, Name = "Alice", Membership = "Gold" },
            new Customer { CustomerId = 2, Name = "Bob", Membership = "Silver" },
            new Customer { CustomerId = 3, Name = "Charlie", Membership = "Bronze" }
        };

        var orders = new List<Order>
        {
            new Order
            {
                OrderId = 101, CustomerId = 1, OrderDate = new DateTime(2023, 1, 15), IsDelivered = true,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Laptop", Quantity = 1, UnitPrice = 1200.00m },
                    new OrderItem { ProductName = "Mouse", Quantity = 2, UnitPrice = 25.50m }
                }
            },
            new Order
            {
                OrderId = 102, CustomerId = 2, OrderDate = new DateTime(2023, 2, 20), IsDelivered = false,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Monitor", Quantity = 2, UnitPrice = 300.00m }
                }
            },
            new Order
            {
                OrderId = 103, CustomerId = 1, OrderDate = new DateTime(2023, 3, 5), IsDelivered = false,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Keyboard", Quantity = 1, UnitPrice = 75.00m },
                    new OrderItem { ProductName = "Mouse", Quantity = 1, UnitPrice = 25.50m }
                }
            },
            new Order
            {
                OrderId = 104, CustomerId = 3, OrderDate = new DateTime(2023, 4, 10), IsDelivered = true,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Desk", Quantity = 1, UnitPrice = 450.00m },
                    new OrderItem { ProductName = "Chair", Quantity = 4, UnitPrice = 150.00m }
                }
            }
        };

        //Find the total amount of money spent by "Alice" across all of her orders. (Hint: Money spent = Quantity * UnitPrice)
        var id = customers.FirstOrDefault(c => c.Name.Equals("Alice"))!.CustomerId;
        var amt = orders.Where(o => o.CustomerId == id).SelectMany(o => o.Items).Sum(i => i.Quantity * i.UnitPrice); //struggled here but autocomplete helped 
        Console.WriteLine(amt);

        //Get a list of all unique products(just the product names as strings) that have ever been ordered by anyone, sorted alphabetically.
        var unique = orders.SelectMany(o => o.Items).DistinctBy(i => i.ProductName).OrderBy(i => i.ProductName).Select(i=> i.ProductName);
        //orders.SelectMany(o => o.Items).Select(i => i.ProductName).Distinct().OrderBy(name => name);
        foreach (var u in unique) { Console.WriteLine(u); }

        //Find the OrderId of any order that contains at least one item costing more than $400, but only if the order has NOT been delivered yet.
        var resultId = orders.Where(o => o.Items.Any(i => i.UnitPrice > 400) && !o.IsDelivered).Select(o => o.OrderId);
        foreach (var id1 in resultId)
        {
            Console.WriteLine("ID: "+id1);
        }

        //Find the name of the customer who placed the single most expensive order(calculated by the total cost of all items in that specific order).
        var custId = orders.OrderByDescending(o => o.Items.Sum(i => i.Quantity * i.UnitPrice)).Select(o => o.CustomerId).FirstOrDefault();
        Console.WriteLine(customers.FirstOrDefault(c => c.CustomerId.Equals(custId)).Name);

        //Generate a report showing each Membership tier and the total number of items(summing the Quantity) purchased by all customers in that tier.
        //Return an anonymous object with MembershipLevel and TotalItemsBought.

        var result = orders.Join(customers, o => o.CustomerId, c => c.CustomerId, (o, c) => new
        {
            MembershipTier = c.Membership,
            ItemsInThisOrder = o.Items.Sum(i => i.Quantity) // Add up the physical items
        })
        .GroupBy(n => n.MembershipTier)
        .Select(n => new
        {
            MembershipLevel = n.Key,
            TotalItemsBought = n.Sum(x => x.ItemsInThisOrder) // Add up the totals from the orders
        });


    }
}
