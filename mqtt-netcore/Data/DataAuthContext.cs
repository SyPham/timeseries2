using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Data
{
    public class DataAuthContext : DbContext
    {
        public string _ConnectionString { get; set; }

        public DataAuthContext(string ConnectionString = "Server=.;Database=mqtt;MultipleActiveResultSets=true;User Id=sa;Password=shc@1234")
        {
            _ConnectionString = ConnectionString;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<BlackList> BlackList { get; set; }
        public DbSet<WhiteList> WhiteList { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this._ConnectionString);
        }
    }
}
