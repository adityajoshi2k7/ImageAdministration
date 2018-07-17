using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MVCNetAdmin.Models
{
    public partial class NetAdminContext : DbContext
    {
        public virtual DbSet<AccessionCodes> AccessionCodes { get; set; }
        public virtual DbSet<AccLoc> AccLoc { get; set; }
        public virtual DbSet<Location> Location { get; set; }

       
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=5CG71213VB\ADITYA;;Database=NetAdmin;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessionCodes>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IsTouch)
                    .IsRequired()
                    .HasColumnType("char(10)");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<AccLoc>(entity =>
            {
                entity.HasKey(e => new { e.LocCode, e.AccCode });

                entity.Property(e => e.LocCode).HasColumnType("char(5)");

                entity.Property(e => e.AccCode).HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.AccCodeNavigation)
                    .WithMany(p => p.AccLoc)
                    .HasForeignKey(d => d.AccCode)
                    .HasConstraintName("FK_AccLoc_AccessionCodes");

                entity.HasOne(d => d.LocCodeNavigation)
                    .WithMany(p => p.AccLoc)
                    .HasForeignKey(d => d.LocCode)
                    .HasConstraintName("FK_AccLoc_Location");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Code)
                    .HasColumnType("char(5)")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Server)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });
           

        }
    }
}