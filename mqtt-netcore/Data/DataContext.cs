using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {
        }
        public DataContext(DbContextOptionsBuilder<DataContext> options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<BlackList> BlackList { get; set; }
        public DbSet<WhiteList> WhiteList { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
      
    }
}
