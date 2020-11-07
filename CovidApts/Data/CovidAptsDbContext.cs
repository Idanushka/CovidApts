using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CovidApts.Models;

namespace CovidApts.Data
{
    public class CovidAptsDbContext : IdentityDbContext
    {
        public DbSet<Company> Company { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Apartment> Apartment { get; set; }


        public CovidAptsDbContext(DbContextOptions<CovidAptsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Company>()
                .HasOne(mal => mal.CurrentApartment)
                .WithMany(apt => apt.Companies)
                .IsRequired();
        }
    }
}
