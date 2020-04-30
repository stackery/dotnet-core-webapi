using Microsoft.EntityFrameworkCore;
using System;

namespace webapi.Data
{
    public class webapiContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"Data Source={Environment.GetEnvironmentVariable("DB_ADDRESS")},{Environment.GetEnvironmentVariable("DB_PORT")};Initial Catalog=books;User ID=root;Password=stacksonstacks");
        }

        public DbSet<webapi.Models.Author> Authors { get; set; }

        public DbSet<webapi.Models.Book> Books { get; set; }
    }
}
