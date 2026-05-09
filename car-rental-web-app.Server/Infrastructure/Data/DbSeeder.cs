using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> hasher)
    {
        await db.Database.MigrateAsync();

        // ── Branches ─────────────────────────────────────────────
        if (!await db.Branches.AnyAsync())
        {
            var branches = new[]
            {
                Branch.Create("Bucharest — Central",    "Bucharest",  "Șos. Kiseleff 1, București 011341",          "+40 21 201 4000"),
                Branch.Create("Bucharest — Otopeni",    "Bucharest",  "Calea București-Ploiești, Otopeni 075150",    "+40 21 204 1000"),
                Branch.Create("Cluj — Airport",          "Cluj-Napoca","Str. Traian Vuia 149, Cluj-Napoca 400397",   "+40 264 416 702"),
                Branch.Create("Timișoara — Airport",    "Timișoara",  "Str. Aeroport 2, Ghiroda 307200",             "+40 256 386 000"),
            };
            db.Branches.AddRange(branches);
            await db.SaveChangesAsync();
        }

        var buc = await db.Branches.FirstAsync(b => b.Name == "Bucharest — Central");
        var oto = await db.Branches.FirstAsync(b => b.Name == "Bucharest — Otopeni");
        var clj = await db.Branches.FirstAsync(b => b.Name == "Cluj — Airport");
        var tim = await db.Branches.FirstAsync(b => b.Name == "Timișoara — Airport");

        // ── Admin user ────────────────────────────────────────────
        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Administrator))
        {
            var admin = User.Create("admin", "", "admin@wheeldeal.ro",
                "System Administrator", "+40 700 000 001", UserRole.Administrator);
            var hash = hasher.HashPassword(admin, "Admin@123!");
            admin.SetPasswordHash(hash);
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }

        // ── Manager users ─────────────────────────────────────────
        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Manager))
        {
            var admin = await db.Users.FirstAsync(u => u.Role == UserRole.Administrator);

            var mgr1 = User.Create("mgr_bucharest", "", "manager.buc@wheeldeal.ro",
                "Alexandru Marin", "+40 721 001 001", UserRole.Manager, buc.Id, admin.Id);
            mgr1.SetPasswordHash(hasher.HashPassword(mgr1, "Manager@123!"));

            var mgr2 = User.Create("mgr_cluj", "", "manager.clj@wheeldeal.ro",
                "Ioana Radu", "+40 721 001 002", UserRole.Manager, clj.Id, admin.Id);
            mgr2.SetPasswordHash(hasher.HashPassword(mgr2, "Manager@123!"));

            var mgr3 = User.Create("mgr_timisoara", "", "manager.tim@wheeldeal.ro",
                "Mihai Neagu", "+40 721 001 003", UserRole.Manager, tim.Id, admin.Id);
            mgr3.SetPasswordHash(hasher.HashPassword(mgr3, "Manager@123!"));

            db.Users.AddRange(mgr1, mgr2, mgr3);
            await db.SaveChangesAsync();

            // Assign managers to branches
            buc.AssignManager(mgr1.Id);
            clj.AssignManager(mgr2.Id);
            tim.AssignManager(mgr3.Id);
            await db.SaveChangesAsync();
        }

        // ── Operator users ────────────────────────────────────────
        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Operator))
        {
            var admin = await db.Users.FirstAsync(u => u.Role == UserRole.Administrator);

            var op1 = User.Create("operator_buc1", "", "op1.buc@wheeldeal.ro",
                "Andrei Pop", "+40 722 001 001", UserRole.Operator, buc.Id, admin.Id);
            op1.SetPasswordHash(hasher.HashPassword(op1, "Operator@123!"));

            var op2 = User.Create("operator_oto1", "", "op1.oto@wheeldeal.ro",
                "Maria Ion", "+40 722 001 002", UserRole.Operator, oto.Id, admin.Id);
            op2.SetPasswordHash(hasher.HashPassword(op2, "Operator@123!"));

            db.Users.AddRange(op1, op2);
            await db.SaveChangesAsync();
        }

        // ── Vehicles ──────────────────────────────────────────────
        if (!await db.Vehicles.AnyAsync())
        {
            var mgr1 = await db.Users.FirstAsync(u => u.Username == "mgr_bucharest");
            var mgr2 = await db.Users.FirstAsync(u => u.Username == "mgr_cluj");
            var mgr3 = await db.Users.FirstAsync(u => u.Username == "mgr_timisoara");

            var vehicles = new[]
            {
                Vehicle.Create("B-100-WHL", buc.Id, mgr1.Id, "Dacia", "Logan",    2023, VehicleCategory.Economy,  19m, "Petrol",   "Manual",    5, "#60A5FA"),
                Vehicle.Create("B-200-WHL", buc.Id, mgr1.Id, "VW",    "Golf 8",   2023, VehicleCategory.Compact,  32m, "Diesel",   "Automatic", 5, "#1A56DB"),
                Vehicle.Create("B-300-WHL", oto.Id, mgr1.Id, "BMW",   "X3",       2024, VehicleCategory.SUV,      65m, "Hybrid",   "Automatic", 5, "#1340B0"),
                Vehicle.Create("B-400-WHL", oto.Id, mgr1.Id, "Mercedes", "C220",  2024, VehicleCategory.Premium,  85m, "Diesel",   "Automatic", 5, "#0F172A"),
                Vehicle.Create("B-500-WHL", buc.Id, mgr1.Id, "Skoda", "Octavia",  2023, VehicleCategory.Compact,  28m, "Diesel",   "Manual",    5, "#3B82F6"),
                Vehicle.Create("B-600-WHL", oto.Id, mgr1.Id, "Dacia", "Duster",   2024, VehicleCategory.SUV,      39m, "Petrol",   "Manual",    5, "#1D4ED8"),
                Vehicle.Create("CJ-100-WHL", clj.Id, mgr2.Id, "Toyota", "Yaris",  2023, VehicleCategory.Economy,  22m, "Petrol",   "Manual",    5, "#60A5FA"),
                Vehicle.Create("CJ-200-WHL", clj.Id, mgr2.Id, "Audi",  "A4",      2024, VehicleCategory.Premium,  78m, "Diesel",   "Automatic", 5, "#1E293B"),
                Vehicle.Create("CJ-300-WHL", clj.Id, mgr2.Id, "Ford",  "Kuga",    2023, VehicleCategory.SUV,      48m, "Hybrid",   "Automatic", 5, "#2563EB"),
                Vehicle.Create("TM-100-WHL", tim.Id, mgr3.Id, "Renault", "Clio",  2023, VehicleCategory.Economy,  20m, "Petrol",   "Manual",    5, "#7C3AED"),
                Vehicle.Create("TM-200-WHL", tim.Id, mgr3.Id, "Hyundai", "Tucson",2024, VehicleCategory.SUV,      52m, "Hybrid",   "Automatic", 5, "#059669"),
                Vehicle.Create("TM-300-WHL", tim.Id, mgr3.Id, "Peugeot", "3008",  2023, VehicleCategory.SUV,      45m, "Diesel",   "Automatic", 5, "#DC2626"),
            };

            // Set ratings
            var ratings = new[] { 4.8m, 4.9m, 4.9m, 5.0m, 4.7m, 4.8m, 4.6m, 4.9m, 4.7m, 4.5m, 4.8m, 4.7m };
            for (int i = 0; i < vehicles.Length; i++)
                vehicles[i].SetRating(ratings[i]);

            // Set some offers
            vehicles[0].SetOffer(true, 15);  // Dacia Logan -15%
            vehicles[4].SetOffer(true, 10);  // Skoda Octavia -10%
            vehicles[8].SetOffer(true, 20);  // Ford Kuga -20%
            vehicles[9].SetOffer(true, 12);  // Renault Clio -12%

            db.Vehicles.AddRange(vehicles);
            await db.SaveChangesAsync();
        }

        // ── Sample clients ────────────────────────────────────────
        if (!await db.Clients.AnyAsync())
        {
            var clients = new[]
            {
                Client.Create("Andrew Peterson",  "andrew.p@email.com",    "+40 740 001 001", "Str. Florilor 12, București"),
                Client.Create("Maria Ionescu",    "maria.i@email.com",     "+40 740 001 002", "Bd. Eroilor 5, Cluj-Napoca"),
                Client.Create("Chris Dumitrescu", "chris.d@email.com",     "+40 740 001 003", "Str. Libertății 8, Timișoara"),
                Client.Create("Elena Popa",       "elena.p@email.com",     "+40 740 001 004", "Calea Victoriei 22, București"),
                Client.Create("Radu Constantin",  "radu.c@email.com",      "+40 740 001 005", "Bd. Mihai Viteazu 3, Cluj-Napoca"),
            };
            db.Clients.AddRange(clients);
            await db.SaveChangesAsync();
        }

        // ── Promo codes ───────────────────────────────────────────
        if (!await db.PromoCodes.AnyAsync())
        {
            // Dacia Duster e vehiculul 6 (B-600-WHL)
            var duster = await db.Vehicles.FirstOrDefaultAsync(v => v.RegistrationNumber == "B-600-WHL");

            var promos = new[]
            {
                PromoCode.Create("SUMMER30", PromoType.Percentage, 30,
                    new DateTime(2026, 8, 31), "30% off all weekend rentals", weekendOnly: true),
                PromoCode.Create("FLEET10", PromoType.Percentage, 10,
                    new DateTime(2026, 6, 30), "10% off all Premium models", applicableCategory: "Premium"),
                PromoCode.Create("DRIVE25", PromoType.Percentage, 25,
                    new DateTime(2026, 7, 15), "25% off Dacia Duster 4x4",
                    applicableVehicleId: duster?.Id),
            };
            db.PromoCodes.AddRange(promos);
            await db.SaveChangesAsync();
        }
    }
}
