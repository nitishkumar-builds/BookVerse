using BookVerse.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }  // needed by ApplicationUserRepository
        public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "History", DisplayOrder = 2 },
                new Category { Id = 3, Name = "SciFi", DisplayOrder = 3 }
            );

            // Seed Companies
            modelBuilder.Entity<Company>().HasData(
                new Company { Id = 1, Name = "Penguin Random House", City = "New York", State = "NY", PostalCode = "10019", Phone = "212-555-0100" },
                new Company { Id = 2, Name = "HarperCollins", City = "New York", State = "NY", PostalCode = "10007", Phone = "212-555-0200" },
                new Company { Id = 3, Name = "Macmillan Publishers", City = "New York", State = "NY", PostalCode = "10010", Phone = "212-555-0300" }
            );

            // Seed Products (8 books across Action, History, SciFi)
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Title = "The Last Mission", Author = "Jack Carrow", Description = "A high-octane thriller following an elite squad's final, impossible mission behind enemy lines.", ISBN = "978-1000000001", ListPrice = 799, Price = 699, Price50 = 649, Price100 = 599, CategoryId = 1, CompanyId = 1, ImageUrl = "/images/products/book1.jpg" },
                new Product { Id = 2, Title = "Shadow Protocol", Author = "Elena Marsh", Description = "A rogue agent uncovers a conspiracy that threatens to unravel the fragile peace between nations.", ISBN = "978-1000000002", ListPrice = 749, Price = 649, Price50 = 599, Price100 = 549, CategoryId = 1, CompanyId = 2, ImageUrl = "/images/products/book2.jpg" },
                new Product { Id = 3, Title = "Rise of the Empires", Author = "Dr. Samuel Whitfield", Description = "A sweeping account of how ancient civilizations rose to power and shaped the modern world.", ISBN = "978-1000000003", ListPrice = 699, Price = 599, Price50 = 549, Price100 = 499, CategoryId = 2, CompanyId = 3, ImageUrl = "/images/products/book3.jpg" },
                new Product { Id = 4, Title = "The Great Divide", Author = "Margaret Holloway", Description = "An in-depth look at the events and decisions that split a nation and redefined its future.", ISBN = "978-1000000004", ListPrice = 599, Price = 499, Price50 = 449, Price100 = 419, CategoryId = 2, CompanyId = 1, ImageUrl = "/images/products/book4.jpg" },
                new Product { Id = 5, Title = "Echoes of War", Author = "Thomas Reyes", Description = "A historian's deep dive into the personal stories behind the world's most pivotal conflicts.", ISBN = "978-1000000005", ListPrice = 649, Price = 549, Price50 = 499, Price100 = 449, CategoryId = 2, CompanyId = 2, ImageUrl = "/images/products/book5.jpg" },
                new Product { Id = 6, Title = "Beyond the Stars", Author = "Dr. Nina Volkov", Description = "Humanity's first interstellar voyage uncovers a galaxy stranger and more dangerous than imagined.", ISBN = "978-1000000006", ListPrice = 799, Price = 699, Price50 = 649, Price100 = 599, CategoryId = 3, CompanyId = 3, ImageUrl = "/images/products/book6.jpg" },
                new Product { Id = 7, Title = "The Last Colony", Author = "Marcus Lee", Description = "On a dying Earth, a desperate mission to colonize a distant planet tests the limits of survival.", ISBN = "978-1000000007", ListPrice = 749, Price = 649, Price50 = 599, Price100 = 549, CategoryId = 3, CompanyId = 1, ImageUrl = "/images/products/book7.jpg" },
                new Product { Id = 8, Title = "Quantum Horizon", Author = "Dr. Priya Anand", Description = "A brilliant physicist's discovery opens a door to parallel worlds — and the entities that inhabit them.", ISBN = "978-1000000008", ListPrice = 799, Price = 749, Price50 = 699, Price100 = 649, CategoryId = 3, CompanyId = 2, ImageUrl = "/images/products/book8.jpg" }
            );
        }
    }
}


