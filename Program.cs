using BookVerse.DataAccess.Repository;
using BookVerse.DataAccess.Repository.IRepository;
using BookVerse.Mappings;
using BookVerse.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookVerse.DataAccess.Data;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 4;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Override Identity's default redirect paths to use our AccountController
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(AutoMapperProfile)));
builder.Services.AddControllersWithViews();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

var app = builder.Build();

// â”€â”€ Auto-migrate and seed on startup â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply any pending EF migrations automatically
        Console.WriteLine("STARTUP: Applying migrations...");
        db.Database.Migrate();
        Console.WriteLine("STARTUP: Migrations applied successfully.");

        // â”€â”€ Seed Roles â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        string[] roles = { "Admin", "Employee", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    Console.WriteLine($"ROLE SEED ERROR ({role}):");
                    foreach (var error in roleResult.Errors)
                        Console.WriteLine($"  - {error.Code}: {error.Description}");
                }
                else
                {
                    Console.WriteLine($"STARTUP: Created role '{role}'.");
                }
            }
        }

        // â”€â”€ Seed Demo Users â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        await SeedUser(userManager, "demo-admin@bookverse.com", "Demo Admin", "Admin123*", "Admin");
        await SeedUser(userManager, "demo-employee@bookverse.com", "Demo Employee", "Admin123*", "Employee");
        await SeedUser(userManager, "demo-customer@bookverse.com", "Demo Customer", "Admin123*", "Customer");


        var productImageUrls = new Dictionary<int, string>
        {
            [1] = "/images/products/book1.jpg",
            [2] = "/images/products/book2.jpg",
            [3] = "/images/products/book3.jpg",
            [4] = "/images/products/book4.jpg",
            [5] = "/images/products/book5.jpg",
            [6] = "/images/products/book6.jpg",
            [7] = "/images/products/book7.jpg",
            [8] = "/images/products/book8.jpg"
        };

        foreach (var product in db.Products.Where(p => productImageUrls.Keys.Contains(p.Id)))
        {
            var imageUrl = productImageUrls[product.Id];
            if (product.ImageUrl != imageUrl)
            {
                product.ImageUrl = imageUrl;
            }
        }

        await db.SaveChangesAsync();
        Console.WriteLine("STARTUP: Product image URLs repaired.");
        Console.WriteLine("STARTUP: Seeding complete.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during migration/seeding.");
        Console.WriteLine("=================================================");
        Console.WriteLine("STARTUP FAILURE during migration/seeding:");
        Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
        Console.WriteLine(ex.StackTrace);

        if (ex.InnerException != null)
        {
            Console.WriteLine("--- Inner Exception ---");
            Console.WriteLine(ex.InnerException.GetType().Name + ": " + ex.InnerException.Message);
        }
        Console.WriteLine("=================================================");
    }
}


using (var imageRepairScope = app.Services.CreateScope())
{
    try
    {
        var db = imageRepairScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var productImageUrls = new Dictionary<int, string>
        {
            [1] = "/images/products/book1.jpg",
            [2] = "/images/products/book2.jpg",
            [3] = "/images/products/book3.jpg",
            [4] = "/images/products/book4.jpg",
            [5] = "/images/products/book5.jpg",
            [6] = "/images/products/book6.jpg",
            [7] = "/images/products/book7.jpg",
            [8] = "/images/products/book8.jpg"
        };

        var products = await db.Products
            .Where(p => productImageUrls.Keys.Contains(p.Id))
            .ToListAsync();

        foreach (var product in products)
        {
            product.ImageUrl = productImageUrls[product.Id];
            switch (product.Id)
            {
                case 1:
                    product.ListPrice = 799; product.Price = 699; product.Price50 = 649; product.Price100 = 599;
                    break;
                case 2:
                    product.ListPrice = 749; product.Price = 649; product.Price50 = 599; product.Price100 = 549;
                    break;
                case 3:
                    product.ListPrice = 699; product.Price = 599; product.Price50 = 549; product.Price100 = 499;
                    break;
                case 4:
                    product.ListPrice = 599; product.Price = 499; product.Price50 = 449; product.Price100 = 419;
                    break;
                case 5:
                    product.ListPrice = 649; product.Price = 549; product.Price50 = 499; product.Price100 = 449;
                    break;
                case 6:
                    product.ListPrice = 799; product.Price = 699; product.Price50 = 649; product.Price100 = 599;
                    break;
                case 7:
                    product.ListPrice = 749; product.Price = 649; product.Price50 = 599; product.Price100 = 549;
                    break;
                case 8:
                    product.ListPrice = 799; product.Price = 749; product.Price50 = 699; product.Price100 = 649;
                    break;
            }
        }

        var changedRows = await db.SaveChangesAsync();
        Console.WriteLine($"STARTUP: Independent product image repair saved {changedRows} row(s).");
    }
    catch (Exception ex)
    {
        Console.WriteLine("STARTUP: Independent product image repair failed: " + ex.Message);
    }
}
// â”€â”€ Middleware pipeline â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// â”€â”€ Helper â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
static async Task SeedUser(
    UserManager<ApplicationUser> userManager,
    string email, string name, string password, string role)
{
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Name = name,
            EmailConfirmed = true,
            StreetAddress = "123 Demo Street",
            City = "Demo City",
            State = "Demo State",
            PostalCode = "00000"
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            Console.WriteLine($"STARTUP: Seeded user '{email}' with role '{role}'.");
        }
        else
        {
            Console.WriteLine($"SEED ERROR for {email}:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Code}: {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine($"STARTUP: User '{email}' already exists, skipping.");
    }
}




