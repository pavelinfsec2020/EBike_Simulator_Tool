using EBike_Simulator.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EBike_Simulator.Data.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Translation> Translations { get; set; }

        public string DbPath { get; }

        public AppDbContext()
        {
            DbPath = "ebike.db";
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            DbPath = "ebike.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={DbPath}");

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Translation>(entity =>
            {
                entity.HasIndex(e => e.Key)
                      .IsUnique()
                      .HasDatabaseName("IX_Translations_Key_Unique");

                entity.ToTable("Translations");
            });
        }
    }
}
