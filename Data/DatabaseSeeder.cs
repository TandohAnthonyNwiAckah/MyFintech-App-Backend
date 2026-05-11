using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TANA.Models;

namespace TANA.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDBContext db)
        {
            // ─── 1. Roles ────────────────────────────────────────────────────────────
            if (!await db.Roles.AnyAsync())
            {
                var roles = new List<Roles>
                {
                    new() { role_name = "Admin", date_created = DateTime.UtcNow },
                    new() { role_name = "User", date_created = DateTime.UtcNow },
                };
                await db.Roles.AddRangeAsync(roles);
                await db.SaveChangesAsync();
                Console.WriteLine("[Seeder] Roles seeded.");
            }

            // ─── 2. Permissions ──────────────────────────────────────────────────────
            if (!await db.Permissions.AnyAsync())
            {
                // role_id 1 = Admin, 2 = User

                // Define all modules and actions to ensure role 1 (Admin) has full access
                var modules = new[]
                {
                    "audits",
                    "bankaccount",
                    "beneficiaries",
                    "employment",
                    "investmentname",
                    "investments",
                    "permissions",
                    "productname",
                    "roles",
                    "users",
                    "customers",
                    "account",
                };

                var actions = new[]
                {
                    "view",
                    "add",
                    "edit",
                    "delete",
                    "importdata",
                    "exportlist",
                    "exportview",
                    "index",
                    "searchusername",
                };

                var allPermissions = new HashSet<string> { "home/index" };

                foreach (var module in modules)
                {
                    foreach (var action in actions)
                    {
                        allPermissions.Add($"{module}/{action}");
                    }
                }

                var permissionsToSeed = new List<Permissions>();

                // Role 1 (Admin) gets EVERYTHING
                foreach (var p in allPermissions)
                {
                    permissionsToSeed.Add(
                        new Permissions
                        {
                            permission = p,
                            role_id = 1,
                            date_created = DateTime.UtcNow,
                        }
                    );
                }

                // Role 2 (User) gets restricted access
                var userPermissions = new[]
                {
                    "customers/searchusername",
                    "investmentname/index",
                    "productname/index",
                    "investments/add",
                };

                foreach (var p in userPermissions)
                {
                    permissionsToSeed.Add(
                        new Permissions
                        {
                            permission = p,
                            role_id = 2,
                            date_created = DateTime.UtcNow,
                        }
                    );
                }

                await db.Permissions.AddRangeAsync(permissionsToSeed);
                await db.SaveChangesAsync();
                Console.WriteLine("[Seeder] Permissions seeded.");
            }

            // ─── 3. Investment Names ──────────────────────────────────────────────────
            if (!await db.InvestmentName.AnyAsync())
            {
                var investmentNames = new List<InvestmentName>
                {
                    new() { name = "Mutual Fund", date_created = DateTime.UtcNow },
                    new() { name = "Fixed Deposit", date_created = DateTime.UtcNow },
                    new() { name = "Pension Plan", date_created = DateTime.UtcNow },
     
                };
                await db.InvestmentName.AddRangeAsync(investmentNames);
                await db.SaveChangesAsync();
                Console.WriteLine("[Seeder] InvestmentNames seeded.");
            }

            // ─── 4. Product Names ─────────────────────────────────────────────────────
            if (!await db.ProductName.AnyAsync())
            {
                var productNames = new List<ProductName>
                {
                    new() { name = "MyFIntech MFund", date_created = DateTime.UtcNow },
                    new() { name = "MyFIntech CFund", date_created = DateTime.UtcNow },
                };
                await db.ProductName.AddRangeAsync(productNames);
                await db.SaveChangesAsync();
                Console.WriteLine("[Seeder] ProductNames seeded.");
            }

            // ─── 5. Users ─────────────────────────────────────────────────────────────
            if (!await db.Users.AnyAsync())
            {
                string hashedPassword = Hash.ComputeHash("@password123");

                var users = new List<Users>
                {
                    new()
                    {
                        title = "Mr",
                        firstname = "Tony",
                        middlename = "",
                        surname = "Tandoh",
                        phone = "0504305596",
                        email = "anthony.tandoh@tanacom.io",
                        password = hashedPassword,
                        dob = "1980-01-01",
                        gender = "Male",
                        digital_address = "GA-001-0001",
                        residential_status = "Accra",
                        country = "Ghana",
                        user_role_id = 1,
                        date_created = DateTime.UtcNow,
                    },
                };
                await db.Users.AddRangeAsync(users);
                await db.SaveChangesAsync();
                Console.WriteLine("[Seeder] Users seeded.");
            }

            Console.WriteLine("[Seeder] Database seeding complete.");
        }
    }
}
