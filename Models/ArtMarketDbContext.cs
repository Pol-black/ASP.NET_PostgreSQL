using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RazorPagesMovie.Models;

public partial class ArtMarketDbContext : DbContext
{
    public ArtMarketDbContext()
    {
    }

    public ArtMarketDbContext(DbContextOptions<ArtMarketDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<CatalogForMaterial> CatalogForMaterials { get; set; }

    public virtual DbSet<ConnectProductMaterial> ConnectProductMaterials { get; set; }

    public virtual DbSet<ItemPurchase> ItemPurchases { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductionPurchase> ProductionPurchases { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ProductCostLog> ProductCostLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=localhost;Database=art_market_db;Username=postgres;Password=1234;Persist Security Info=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.IdAccount).HasName("account_pkey");

            entity.ToTable("account", "art_market_schema");

            entity.Property(e => e.IdAccount).HasColumnName("id_account");
            entity.Property(e => e.AccountName)
                .HasMaxLength(50)
                .HasColumnName("account_name");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Password)
                .HasMaxLength(20)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .HasColumnName("phone");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("account_id_role_fkey");
        });

        modelBuilder.Entity<CatalogForMaterial>(entity =>
        {
            entity.HasKey(e => e.IdMaterial).HasName("catalog_for_material_pkey");

            entity.ToTable("catalog_for_material", "art_market_schema");

            entity.Property(e => e.IdMaterial).HasColumnName("id_material");
            entity.Property(e => e.CostPerUnit)
                .HasPrecision(10, 2)
                .HasColumnName("cost_per_unit");
            entity.Property(e => e.MaterialName)
                .HasMaxLength(100)
                .HasColumnName("material_name");
        });

        modelBuilder.Entity<ConnectProductMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("connect_product_material_pkey");

            entity.ToTable("connect_product_material", "art_market_schema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdMaterial).HasColumnName("id_material");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 3)
                .HasColumnName("quantity");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");

            entity.HasOne(d => d.IdMaterialNavigation).WithMany(p => p.ConnectProductMaterials)
                .HasForeignKey(d => d.IdMaterial)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("connect_product_material_id_material_fkey");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.ConnectProductMaterials)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("connect_product_material_id_product_fkey");
        });

        modelBuilder.Entity<ItemPurchase>(entity =>
        {
            entity.HasKey(e => e.IdItemPurchase).HasName("item_purchase_pkey");

            entity.ToTable("item_purchase", "art_market_schema");

            entity.Property(e => e.IdItemPurchase).HasColumnName("id_item_purchase");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IdPurchase).HasColumnName("id_purchase");
            entity.Property(e => e.PriceInPurchase)
                .HasPrecision(12, 2)
                .HasColumnName("price_in_purchase");
            entity.Property(e => e.QuantityInPurchase).HasColumnName("quantity_in_purchase");
            entity.Property(e => e.StatusItem)
                .HasMaxLength(30)
                .HasColumnName("status_item");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.ItemPurchases)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("item_purchase_id_product_fkey");

            entity.HasOne(d => d.IdPurchaseNavigation).WithMany(p => p.ItemPurchases)
                .HasForeignKey(d => d.IdPurchase)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("item_purchase_id_purchase_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProduct).HasName("product_pkey");

            entity.ToTable("product", "art_market_schema");

            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IdIndivBuyer).HasColumnName("id_indiv_buyer");
            entity.Property(e => e.IdSeller).HasColumnName("id_seller");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.QuantityForSale).HasColumnName("quantity_for_sale");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.TypeArt)
                .HasMaxLength(100)
                .HasColumnName("type_art");

            entity.HasOne(d => d.IdIndivBuyerNavigation).WithMany(p => p.ProductIdIndivBuyerNavigations)
                .HasForeignKey(d => d.IdIndivBuyer)
                .HasConstraintName("product_id_indiv_buyer_fkey");

            entity.HasOne(d => d.IdSellerNavigation).WithMany(p => p.ProductIdSellerNavigations)
                .HasForeignKey(d => d.IdSeller)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_id_seller_fkey");
        });

        modelBuilder.Entity<ProductionPurchase>(entity =>
        {
            entity.HasKey(e => e.IdProductionPurchase).HasName("production_purchase_pkey");

            entity.ToTable("production_purchase", "art_market_schema");

            entity.Property(e => e.IdProductionPurchase).HasColumnName("id_production_purchase");
            entity.Property(e => e.DirectionFromSeller).HasColumnName("direction_from_seller");
            entity.Property(e => e.IdBuyer).HasColumnName("id_buyer");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IdSeller).HasColumnName("id_seller");
            entity.Property(e => e.TextAccounts).HasColumnName("text_accounts");

            entity.HasOne(d => d.IdBuyerNavigation).WithMany(p => p.ProductionPurchaseIdBuyerNavigations)
                .HasForeignKey(d => d.IdBuyer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("production_purchase_id_buyer_fkey");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.ProductionPurchases)
                .HasForeignKey(d => d.IdProduct)
                .HasConstraintName("production_purchase_id_product_fkey");

            entity.HasOne(d => d.IdSellerNavigation).WithMany(p => p.ProductionPurchaseIdSellerNavigations)
                .HasForeignKey(d => d.IdSeller)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("production_purchase_id_seller_fkey");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.IdPurchase).HasName("purchase_pkey");

            entity.ToTable("purchase", "art_market_schema");

            entity.Property(e => e.IdPurchase).HasColumnName("id_purchase");
            entity.Property(e => e.AddressDeparture).HasColumnName("address_departure");
            entity.Property(e => e.AddressReceiving).HasColumnName("address_receiving");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("created_at");
            entity.Property(e => e.IdBuyer).HasColumnName("id_buyer");
            entity.Property(e => e.IdSeller).HasColumnName("id_seller");
            entity.Property(e => e.MethodDelivery)
                .HasMaxLength(50)
                .HasColumnName("method_delivery");
            entity.Property(e => e.NumberPurchase)
                .HasMaxLength(20)
                .HasColumnName("number_purchase");

            entity.HasOne(d => d.IdBuyerNavigation).WithMany(p => p.PurchaseIdBuyerNavigations)
                .HasForeignKey(d => d.IdBuyer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchase_id_buyer_fkey");

            entity.HasOne(d => d.IdSellerNavigation).WithMany(p => p.PurchaseIdSellerNavigations)
                .HasForeignKey(d => d.IdSeller)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchase_id_seller_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("role_pkey");

            entity.ToTable("role", "art_market_schema");

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<ProductCostLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("product_cost_log_pkey");
            entity.ToTable("product_cost_log", "art_market_schema");
            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.TotalCost)
                .HasPrecision(12, 2)
                .HasColumnName("total_cost");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.HasOne(d => d.IdProductNavigation).WithMany()
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_cost_log_id_product_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
