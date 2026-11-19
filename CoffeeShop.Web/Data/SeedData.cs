using CoffeeShop.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Web.Data;

public static class SeedData
{
    public static readonly string[] Roles = { "Admin", "Manager", "Cashier", "Barista" };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<CoffeeShopDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // 1) บทบาท 4 บทบาท
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2) ผู้ใช้ตัวอย่าง + ข้อมูลพนักงาน
        await CreateUserAsync(db, userManager, "admin@coffee.local", "Admin@1234",
            "Admin", "EMP001", "เจ้าของร้าน", "เจ้าของร้าน");
        await CreateUserAsync(db, userManager, "cashier@coffee.local", "Cashier@1234",
            "Cashier", "EMP002", "พนักงานแคชเชียร์", "แคชเชียร์");

        // 3) เมนูและวัตถุดิบ (ใส่ครั้งเดียว)
        if (await db.Categories.AnyAsync()) return;

        var coffee = new Category { Name = "กาแฟ", SortOrder = 1 };
        var tea = new Category { Name = "ชา", SortOrder = 2 };
        var bakery = new Category { Name = "เบเกอรี่", SortOrder = 3 };

        var size = new OptionGroup
        {
            Name = "ขนาด", IsRequired = true, MaxSelect = 1,
            Options = { new Option { Name = "ปกติ 16 oz", ExtraPrice = 0 },
                        new Option { Name = "ใหญ่ 22 oz", ExtraPrice = 10 } }
        };
        var sweet = new OptionGroup
        {
            Name = "ระดับความหวาน", IsRequired = true, MaxSelect = 1,
            Options = { new Option { Name = "หวานน้อย" },
                        new Option { Name = "หวานปกติ" },
                        new Option { Name = "หวานมาก" } }
        };
        var topping = new OptionGroup
        {
            Name = "ท็อปปิง", IsRequired = false, MaxSelect = 3,
            Options = { new Option { Name = "เพิ่มช็อต", ExtraPrice = 15 },
                        new Option { Name = "วิปครีม", ExtraPrice = 10 } }
        };

        var bean = new Ingredient { Name = "เมล็ดกาแฟคั่ว", Unit = "g", CostPerUnit = 0.80m, StockQty = 5000, ReorderLevel = 1000 };
        var milk = new Ingredient { Name = "นมสด", Unit = "ml", CostPerUnit = 0.05m, StockQty = 10000, ReorderLevel = 2000 };
        var syrup = new Ingredient { Name = "น้ำเชื่อม", Unit = "ml", CostPerUnit = 0.03m, StockQty = 3000, ReorderLevel = 500 };
        var teaLeaf = new Ingredient { Name = "ผงชาไทย", Unit = "g", CostPerUnit = 0.60m, StockQty = 2000, ReorderLevel = 500 };
        var cup = new Ingredient { Name = "แก้ว 16 oz", Unit = "ใบ", CostPerUnit = 2.50m, StockQty = 500, ReorderLevel = 100 };

        var espresso = new Product { Name = "เอสเปรสโซ่เย็น", Category = coffee, BasePrice = 50 };
        var latte = new Product { Name = "ลาเต้เย็น", Category = coffee, BasePrice = 55 };
        var thaiTea = new Product { Name = "ชาไทยเย็น", Category = tea, BasePrice = 45 };
        var croissant = new Product { Name = "ครัวซองต์", Category = bakery, BasePrice = 45 };

        // เครื่องดื่มทุกเมนูใช้ได้ 3 กลุ่มตัวเลือก
        foreach (var drink in new[] { espresso, latte, thaiTea })
        {
            drink.ProductOptionGroups.Add(new ProductOptionGroup { OptionGroup = size });
            drink.ProductOptionGroups.Add(new ProductOptionGroup { OptionGroup = sweet });
            drink.ProductOptionGroups.Add(new ProductOptionGroup { OptionGroup = topping });
        }

        // สูตรต่อ 1 แก้ว
        espresso.Recipes.Add(new Recipe { Ingredient = bean, Quantity = 18 });
        espresso.Recipes.Add(new Recipe { Ingredient = syrup, Quantity = 20 });
        espresso.Recipes.Add(new Recipe { Ingredient = cup, Quantity = 1 });
        latte.Recipes.Add(new Recipe { Ingredient = bean, Quantity = 18 });
        latte.Recipes.Add(new Recipe { Ingredient = milk, Quantity = 150 });
        latte.Recipes.Add(new Recipe { Ingredient = cup, Quantity = 1 });
        thaiTea.Recipes.Add(new Recipe { Ingredient = teaLeaf, Quantity = 15 });
        thaiTea.Recipes.Add(new Recipe { Ingredient = milk, Quantity = 100 });
        thaiTea.Recipes.Add(new Recipe { Ingredient = syrup, Quantity = 30 });
        thaiTea.Recipes.Add(new Recipe { Ingredient = cup, Quantity = 1 });

        db.AddRange(espresso, latte, thaiTea, croissant); // EF เพิ่มหมวด ตัวเลือก วัตถุดิบที่เชื่อมอยู่ให้อัตโนมัติ
        await db.SaveChangesAsync();
    }

    private static async Task CreateUserAsync(CoffeeShopDbContext db,
        UserManager<IdentityUser> userManager,
        string email, string password, string role, string code, string fullName, string position)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            await userManager.AddToRoleAsync(user, role);
        }

        if (!await db.Employees.AnyAsync(e => e.UserId == user.Id))
        {
            db.Employees.Add(new Employee
            {
                UserId = user.Id, EmployeeCode = code, FullName = fullName,
                Position = position, HireDate = DateTime.Today
            });
            await db.SaveChangesAsync();
        }
    }
}
