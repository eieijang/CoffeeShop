using CoffeeShop.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Web.Data;

public class CoffeeShopDbContext : IdentityDbContext<IdentityUser>
{
    public CoffeeShopDbContext(DbContextOptions<CoffeeShopDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OptionGroup> OptionGroups => Set<OptionGroup>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<ProductOptionGroup> ProductOptionGroups => Set<ProductOptionGroup>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // ต้องเรียกก่อน เพื่อสร้างตาราง Identity

        // 1) คีย์คู่ (Composite key) ของตารางเชื่อม
        builder.Entity<ProductOptionGroup>()
            .HasKey(x => new { x.ProductId, x.OptionGroupId });
        builder.Entity<Recipe>()
            .HasKey(x => new { x.ProductId, x.IngredientId });

        // 2) ความละเอียดของตัวเลขทศนิยม
        builder.Entity<Product>().Property(p => p.BasePrice).HasPrecision(10, 2);
        builder.Entity<Option>().Property(o => o.ExtraPrice).HasPrecision(10, 2);
        builder.Entity<Ingredient>(e =>
        {
            e.Property(i => i.CostPerUnit).HasPrecision(10, 4);
            e.Property(i => i.StockQty).HasPrecision(12, 3);
            e.Property(i => i.ReorderLevel).HasPrecision(12, 3);
        });
        builder.Entity<Recipe>().Property(r => r.Quantity).HasPrecision(12, 3);

        // 3) ห้ามลบหมวดที่ยังมีเมนูอยู่
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // 4) พนักงาน 1 คน ต่อ 1 บัญชีผู้ใช้ และรหัสพนักงานห้ามซ้ำ
        builder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
