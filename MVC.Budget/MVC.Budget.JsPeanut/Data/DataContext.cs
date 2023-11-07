using Microsoft.EntityFrameworkCore;
using MVC.Budget.JsPeanut.Models;

namespace MVC.Budget.JsPeanut.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
