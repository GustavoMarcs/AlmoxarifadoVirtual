using Domain.Entities;
using Domain.Entities.Products;
using Domain.Entities.Suppliers;
using Domain.Entities.Tracker;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess;

public sealed class AlmoxarifadoVirtualContext(DbContextOptions<AlmoxarifadoVirtualContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductPriceHistory> ProductPriceHistories { get; set; }
    public DbSet<SupplierProduct> SupplierProducts { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierCategory> SupplierCategories { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<DepartmentLocation> DepartmentLocalizations { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Movement> Movements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==================== Supplier ====================
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasOne(s => s.SupplierCategory)
                .WithMany(sc => sc.Suppliers)
                .HasForeignKey(s => s.SupplierCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(s => s.Email)
                .IsUnique();

            entity.Property(s => s.Cnpj)
                .HasMaxLength(18)
                .IsRequired();

            entity.Property(s => s.CorporateName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(s => s.TradeName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(s => s.Email)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.Phone)
                .HasMaxLength(20)
                .IsRequired();

            entity.OwnsOne(s => s.Address, address =>
            {
                address.WithOwner();

                address.Property(a => a.StreetAdress)
                    .HasColumnName("Street")
                    .HasMaxLength(150)
                    .IsRequired();

                address.Property(a => a.CountryId)
                    .HasColumnName("CountryId")
                    .IsRequired();

                address.Property(a => a.City)
                    .HasColumnName("City")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.State)
                    .HasColumnName("State")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(a => a.ZipCode)
                    .HasColumnName("ZipCode")
                    .HasMaxLength(12)
                    .IsRequired();

                address.HasOne(a => a.Country)
                    .WithMany()
                    .HasForeignKey(a => a.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Owned type
            entity.OwnsOne(s => s.Address, address =>
            {
                address.WithOwner();

                address.Property(a => a.StreetAdress).HasColumnName("Street");
                address.Property(a => a.CountryId).HasColumnName("CountryId");
                address.Property(a => a.City).HasColumnName("City");
                address.Property(a => a.State).HasColumnName("State");
                address.Property(a => a.ZipCode).HasColumnName("ZipCode");
                address.HasOne(a => a.Country)
                    .WithMany()
                    .HasForeignKey(a => a.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        });

        // ==================== SupplierProduct ====================
        modelBuilder.Entity<SupplierProduct>(entity =>
        {
            entity.Property(sp => sp.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(sp => sp.Description)
                .HasMaxLength(1000);

            entity.Property(sp => sp.Barcode)   // Agora opcional
                .HasMaxLength(50);

            entity.Property(sp => sp.Sku)       // Agora opcional
                .HasMaxLength(50);

            entity.Property(sp => sp.Price)
                .HasPrecision(18, 2);

            entity.HasOne(sp => sp.Supplier)
                .WithMany(s => s.SupplierProducts)
                .HasForeignKey(sp => sp.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sp => sp.ProductCategory)
                .WithMany(pc => pc.SupplierProducts)
                .HasForeignKey(sp => sp.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // �ndices �nicos s� se desejar que n�o existam duplicados mesmo sendo opcionais
            entity.HasIndex(sp => sp.Barcode)
                .IsUnique()
                .HasFilter("[Barcode] IS NOT NULL"); // necess�rio para permitir NULL

            entity.HasIndex(sp => sp.Sku)
                .IsUnique()
                .HasFilter("[Sku] IS NOT NULL");     // necess�rio para permitir NULL
        });

        // ==================== SupplierCategory ====================
        modelBuilder.Entity<SupplierCategory>(entity =>
        {
            entity.Property(sc => sc.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(sc => sc.Description)
                .HasMaxLength(200);
        });

        // ==================== Product ====================

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(p => p.SellingPrice)
                    .HasPrecision(18, 2);

                entity.HasOne(p => p.ProductLocation)
                    .WithMany(l => l.Products)
                    .HasForeignKey(p => p.ProductLocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== ProductCategory ====================
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.Property(pc => pc.Name)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            // ==================== ProductPriceHistory ====================
            modelBuilder.Entity<ProductPriceHistory>(entity =>
            {
                entity.Property(pph => pph.OldPrice)
                    .HasPrecision(18, 2);
                
                entity.Property(pph => pph.NewPrice)
                    .HasPrecision(18, 2);

                entity.HasOne(pph => pph.Product)
                    .WithMany(p => p.ProductPriceHistories)
                    .HasForeignKey(pph => pph.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(pph => pph.UpdatedPriceAt)
                    .IsRequired();
            });

            // ==================== DepartmentLocalization ====================
            modelBuilder.Entity<DepartmentLocation>(entity =>
            {
                entity.Property(d => d.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(d => d.Description)
                    .HasMaxLength(200);
            });

            // ==================== User ====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(u => u.Email)
                    .IsUnique();
            });

            // ==================== Country ====================
            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // ==================== Movement ====================
            modelBuilder.Entity<Movement>(entity =>
            {
                entity.Property(m => m.Quantity)
                    .IsRequired();

                entity.Property(m => m.Type)
                    .IsRequired()
                    .HasConversion<int>();

                entity.HasOne(m => m.Product)
                    .WithMany(p => p.Movements)
                    .HasForeignKey(m => m.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

    }
}