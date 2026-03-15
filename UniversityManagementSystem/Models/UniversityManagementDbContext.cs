using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace UniversityManagementSystem.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public partial class UniversityManagementDbContext : DbContext
{
    public UniversityManagementDbContext()
    {
    }

    public UniversityManagementDbContext(DbContextOptions<UniversityManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SchoolClass> SchoolClasses { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<SystemUser> SystemUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=DESKTOP-N16L2CC;Database=UniversityManagementDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SchoolClass>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927A07655A853");

            entity.ToTable("Classes"); // Đảm bảo nó vẫn map vào bảng Classes trong SQL

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(50);
            entity.Property(e => e.FacultyId).HasColumnName("FacultyID");

            // SỬA QUAN TRỌNG: p.SchoolClasses (Bạn phải đảm bảo bên file Faculty.cs có biến này)
            entity.HasOne(d => d.Faculty).WithMany(p => p.SchoolClasses)
                .HasForeignKey(d => d.FacultyId)
                .HasConstraintName("FK__Classes__Faculty__398D8EEE");
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.FacultyId).HasName("PK__Facultie__306F636E43D456BE");

            entity.Property(e => e.FacultyId).HasColumnName("FacultyID");
            entity.Property(e => e.FacultyName).HasMaxLength(100);
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Grades__54F87A3785EB9823");

            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            
            entity.Property(e => e.Semester).HasMaxLength(20);
            entity.Property(e => e.StudentId)
                .HasMaxLength(20)
                .HasColumnName("StudentID");
            entity.Property(e => e.SubjectId)
                .HasMaxLength(20)
                .HasColumnName("SubjectID");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Grades__StudentI__4222D4EF");

            entity.HasOne(d => d.Subject).WithMany(p => p.Grades)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__Grades__SubjectI__4316F928");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52A792971218D");

            entity.Property(e => e.StudentId)
                .HasMaxLength(20)
                .HasColumnName("StudentID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Studying");

            // SỬA QUAN TRỌNG: d.SchoolClass (Đảm bảo bên file Student.cs tên là SchoolClass)
            entity.HasOne(d => d.SchoolClass).WithMany(p => p.Students)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Students__ClassI__3C69FB99");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Subjects__AC1BA388952ADB97");

            entity.Property(e => e.SubjectId)
                .HasMaxLength(20)
                .HasColumnName("SubjectID");
            entity.Property(e => e.SubjectName).HasMaxLength(100);
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__SystemLo__5E5499A8E3E4AE1D");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.PerformedBy).HasMaxLength(50);
            entity.Property(e => e.PerformedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SystemUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__SystemUs__1788CCAC20A2F5A4");

            entity.HasIndex(e => e.Username, "UQ__SystemUs__536C85E4F24E8620").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

