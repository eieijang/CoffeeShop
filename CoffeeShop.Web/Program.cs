using CoffeeShop.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) ฐานข้อมูล
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ไม่พบ Connection string 'DefaultConnection'");
builder.Services.AddDbContext<CoffeeShopDbContext>(options =>
    options.UseSqlServer(connStr));

// 2) Identity + Role
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // ไม่ต้องยืนยันอีเมลในห้องปฏิบัติการ
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<CoffeeShopDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // หน้า Login/Register ของ Identity UI เป็น Razor Pages

var app = builder.Build();

// ใส่ข้อมูลตั้งต้นทุกครั้งที่เริ่มโปรแกรม (ตรวจซ้ำใน SeedData แล้ว)
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // ต้องอยู่ก่อน UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
