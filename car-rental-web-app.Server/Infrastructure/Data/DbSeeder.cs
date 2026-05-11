using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Data;

public static class DbSeeder
{
    // ── Image path helper ─────────────────────────────────────────────────
    // Pattern: assets/cars/{category}/{subfolder}/updated/{filename}
    // We always use color variant 1, angle 1 as the main image
    private static string Img(string category, string subfolder, string filename)
        => $"assets/cars/{category}/{subfolder}/updated/{filename}";

    // Shortcuts per category suffix
    private static string Low(string sub, string file) => Img("lowbudget", sub, file);
    private static string Med(string sub, string file) => Img("mediumbudget", sub, file);
    private static string High(string sub, string file) => Img("highbudget", sub, file);
    private static string Elite(string sub, string file) => Img("elitebudget", sub, file);
    private static string Van(string sub, string file) => Img("vans", sub, file);

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> hasher)
    {
        await db.Database.MigrateAsync();

        // ── Branches ──────────────────────────────────────────────────────
        if (!await db.Branches.AnyAsync())
        {
            db.Branches.AddRange(
                Branch.Create("Bucharest — Central", "Bucharest", "Șos. Kiseleff 1, București 011341", "+40 21 201 4000"),
                Branch.Create("Bucharest — Otopeni", "Bucharest", "Calea București-Ploiești, Otopeni 075150", "+40 21 204 1000"),
                Branch.Create("Cluj — Airport", "Cluj-Napoca", "Str. Traian Vuia 149, Cluj-Napoca 400397", "+40 264 416 702"),
                Branch.Create("Timișoara — Airport", "Timișoara", "Str. Aeroport 2, Ghiroda 307200", "+40 256 386 000")
            );
            await db.SaveChangesAsync();
        }

        var buc = await db.Branches.FirstAsync(b => b.Name == "Bucharest — Central");
        var oto = await db.Branches.FirstAsync(b => b.Name == "Bucharest — Otopeni");
        var clj = await db.Branches.FirstAsync(b => b.Name == "Cluj — Airport");
        var tim = await db.Branches.FirstAsync(b => b.Name == "Timișoara — Airport");

        // ── Admin ─────────────────────────────────────────────────────────
        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Administrator))
        {
            var admin = User.Create("admin", "", "admin@wheeldeal.ro",
                "System Administrator", "+40 700 000 001", UserRole.Administrator);
            admin.SetPasswordHash(hasher.HashPassword(admin, "Admin@123!"));
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }

        // ── Managers ──────────────────────────────────────────────────────
        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Manager))
        {
            var adm = await db.Users.FirstAsync(u => u.Role == UserRole.Administrator);

            var m1 = User.Create("mgr_bucharest", "", "manager.buc@wheeldeal.ro", "Alexandru Marin", "+40 721 001 001", UserRole.Manager, buc.Id, adm.Id);
            m1.SetPasswordHash(hasher.HashPassword(m1, "Manager@123!"));
            var m2 = User.Create("mgr_cluj", "", "manager.clj@wheeldeal.ro", "Ioana Radu", "+40 721 001 002", UserRole.Manager, clj.Id, adm.Id);
            m2.SetPasswordHash(hasher.HashPassword(m2, "Manager@123!"));
            var m3 = User.Create("mgr_timisoara", "", "manager.tim@wheeldeal.ro", "Mihai Neagu", "+40 721 001 003", UserRole.Manager, tim.Id, adm.Id);
            m3.SetPasswordHash(hasher.HashPassword(m3, "Manager@123!"));
            var m4 = User.Create("mgr_otopeni", "", "manager.oto@wheeldeal.ro", "Elena Dumitrescu", "+40 721 001 004", UserRole.Manager, oto.Id, adm.Id);
            m4.SetPasswordHash(hasher.HashPassword(m4, "Manager@123!"));

            db.Users.AddRange(m1, m2, m3, m4);
            await db.SaveChangesAsync();

            buc.AssignManager(m1.Id); clj.AssignManager(m2.Id);
            tim.AssignManager(m3.Id); oto.AssignManager(m4.Id);
            await db.SaveChangesAsync();
        }

        // ── Operators ─────────────────────────────────────────────────────
        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Operator))
        {
            var adm = await db.Users.FirstAsync(u => u.Role == UserRole.Administrator);
            var op1 = User.Create("operator_buc1", "", "op1.buc@wheeldeal.ro", "Andrei Pop", "+40 722 001 001", UserRole.Operator, buc.Id, adm.Id);
            op1.SetPasswordHash(hasher.HashPassword(op1, "Operator@123!"));
            var op2 = User.Create("operator_oto1", "", "op1.oto@wheeldeal.ro", "Maria Ion", "+40 722 001 002", UserRole.Operator, oto.Id, adm.Id);
            op2.SetPasswordHash(hasher.HashPassword(op2, "Operator@123!"));
            db.Users.AddRange(op1, op2);
            await db.SaveChangesAsync();
        }

        // ── Vehicles ──────────────────────────────────────────────────────
        if (!await db.Vehicles.AnyAsync())
        {
            var m1 = await db.Users.FirstAsync(u => u.Username == "mgr_bucharest");
            var m2 = await db.Users.FirstAsync(u => u.Username == "mgr_cluj");
            var m3 = await db.Users.FirstAsync(u => u.Username == "mgr_timisoara");
            var m4 = await db.Users.FirstAsync(u => u.Username == "mgr_otopeni");

            var vehicles = new List<Vehicle>
            {
                // ══════════════════════════════════════════════════════════
                // BUCHAREST — CENTRAL  (40 vehicles)
                // ══════════════════════════════════════════════════════════

                // LOW BUDGET — Economy (10)
                Vehicle.Create("B-001-WHL", buc.Id, m1.Id, "Dacia",      "Sandero",    2024, VehicleCategory.Economy, 19m, "Petrol",   "Manual",    5, "#60A5FA", Low("dacia_sandero",      "dacia_sandero_2024_1_carlowbudget1.png")),
                Vehicle.Create("B-002-WHL", buc.Id, m1.Id, "Dacia",      "Logan",      2024, VehicleCategory.Economy, 18m, "Petrol",   "Manual",    5, "#93C5FD", Low("dacia_logan",        "dacia_logan_2024_1_carlowbudget1.png")),
                Vehicle.Create("B-003-WHL", buc.Id, m1.Id, "Renault",    "Clio",       2024, VehicleCategory.Economy, 22m, "Petrol",   "Manual",    5, "#BFDBFE", Low("renult_clio",        "renault_clio_2024_1_carlowbudget1.png")),
                Vehicle.Create("B-004-WHL", buc.Id, m1.Id, "Hyundai",    "i20",        2024, VehicleCategory.Economy, 21m, "Petrol",   "Manual",    5, "#7C3AED", Low("hyundai_i20",        "hyundai_i20_2024_1_carlowbudget1.png")),
                Vehicle.Create("B-005-WHL", buc.Id, m1.Id, "Skoda",      "Fabia",      2023, VehicleCategory.Economy, 20m, "Petrol",   "Manual",    5, "#A78BFA", Low("skoda_fabia",        "skoda_fabia_2023_1_carlowbudget1.png")),
                Vehicle.Create("B-006-WHL", buc.Id, m1.Id, "Kia",        "Picanto",    2024, VehicleCategory.Economy, 17m, "Petrol",   "Manual",    5, "#DDD6FE", Low("kia_picanto",        "kia_picanto_2024_1_carlowbudget1.png")),
                Vehicle.Create("B-007-WHL", buc.Id, m1.Id, "Suzuki",     "Swift",      2024, VehicleCategory.Economy, 20m, "Petrol",   "Manual",    5, "#EDE9FE", Low("suzuki_swift",       "suzuki_swift_2024_1_carlowbudget1.png")),
                Vehicle.Create("B-008-WHL", buc.Id, m1.Id, "Fiat",       "500",        2023, VehicleCategory.Economy, 19m, "Hybrid",   "Manual",    4, "#FEF3C7", Low("fiat500",            "fiat_500mildhybrid_2023_1_carlowbudget1.png")),
                Vehicle.Create("B-009-WHL", buc.Id, m1.Id, "Mitsubishi", "Space Star", 2023, VehicleCategory.Economy, 16m, "Petrol",   "Manual",    5, "#FDE68A", Low("mitsubishi_space",   "mitsubishi_spacestar_2023_1_carlowbudget1.png")),
                Vehicle.Create("B-010-WHL", buc.Id, m1.Id, "MG",         "3",          2024, VehicleCategory.Economy, 21m, "Petrol",   "Automatic", 5, "#FCD34D", Low("mg3",                "mg_3_2024_1_carlowbudget1.png")),

                // MEDIUM BUDGET — Compact/SUV (10)
                Vehicle.Create("B-011-WHL", buc.Id, m1.Id, "Toyota",     "Corolla",    2025, VehicleCategory.Compact, 35m, "Hybrid",   "Automatic", 5, "#6EE7B7", Med("toyota_corolla",     "toyota_corolla_2025_1_carmediumbudget1.png")),
                Vehicle.Create("B-012-WHL", buc.Id, m1.Id, "Volkswagen", "Golf",       2024, VehicleCategory.Compact, 38m, "Petrol",   "Automatic", 5, "#1A56DB", Med("volkswagen_golf",    "volkswagen_golf_2024_1_carmediumbudget1.png")),
                Vehicle.Create("B-013-WHL", buc.Id, m1.Id, "Skoda",      "Octavia",    2024, VehicleCategory.Compact, 33m, "Diesel",   "Automatic", 5, "#3B82F6", Med("skoda_octavia",      "skoda_octavia_2024_1_carmediumbudget1.png")),
                Vehicle.Create("B-014-WHL", buc.Id, m1.Id, "Honda",      "Civic",      2025, VehicleCategory.Compact, 39m, "Hybrid",   "Automatic", 5, "#1E40AF", Med("honda_civic",        "honda_civic_2025_1_carmediumbudget1.png")),
                Vehicle.Create("B-015-WHL", buc.Id, m1.Id, "Renault",    "Austral",    2025, VehicleCategory.Compact, 36m, "Hybrid",   "Automatic", 5, "#2563EB", Med("renault_austral",    "renault_austral_2025_1_carmediumbudget1.png")),
                Vehicle.Create("B-016-WHL", buc.Id, m1.Id, "Hyundai",    "Tucson",     2025, VehicleCategory.SUV,     55m, "Hybrid",   "Automatic", 5, "#059669", Med("hyundai_tucson",     "hyundai_tucson_2025_1_carmediumbudget1.png")),
                Vehicle.Create("B-017-WHL", buc.Id, m1.Id, "Kia",        "Sportage",   2025, VehicleCategory.SUV,     58m, "Hybrid",   "Automatic", 5, "#34D399", Med("kia_sportage",       "kia_sportage_2025_1_carmediumbudget1.png")),
                Vehicle.Create("B-018-WHL", buc.Id, m1.Id, "Mazda",      "CX-5",       2025, VehicleCategory.SUV,     52m, "Petrol",   "Automatic", 5, "#10B981", Med("mazda_cx5",          "mazda_cx5_2025_1_carmediumbudget1.png")),
                Vehicle.Create("B-019-WHL", buc.Id, m1.Id, "Tesla",      "Model 3",    2024, VehicleCategory.Compact, 65m, "Electric", "Automatic", 5, "#0F172A", Med("tesla_model3",       "tesla_model3_2024_1_carmediumbudget1.png")),
                Vehicle.Create("B-020-WHL", buc.Id, m1.Id, "Volvo",      "XC40",       2024, VehicleCategory.SUV,     60m, "Electric", "Automatic", 5, "#1E293B", Med("volvo_xc40",         "volvo_xc40_2024_1_carmediumbudget1.png")),

                // HIGH BUDGET — Premium (10)
                Vehicle.Create("B-021-WHL", buc.Id, m1.Id, "BMW",           "X5",           2025, VehicleCategory.Premium, 120m, "Hybrid",   "Automatic", 5, "#1340B0", High("bmw_x5",                "bmw_x5_2025_1_carhighbudget1.png")),
                Vehicle.Create("B-022-WHL", buc.Id, m1.Id, "Mercedes-Benz", "GLE",          2024, VehicleCategory.Premium, 135m, "Diesel",   "Automatic", 5, "#0F172A", High("mercedesbenz_gle",      "mercedesbenz_gle_2024_1_carhighbudget1.png")),
                Vehicle.Create("B-023-WHL", buc.Id, m1.Id, "Audi",          "Q7",           2025, VehicleCategory.Premium, 145m, "Diesel",   "Automatic", 7, "#1E293B", High("audi_q7",               "audi_q7_2025_1_carhighbudget1.png")),
                Vehicle.Create("B-024-WHL", buc.Id, m1.Id, "Porsche",       "Macan",        2024, VehicleCategory.Premium, 160m, "Electric", "Automatic", 5, "#7C3AED", High("porsche_macan",          "porche_macan_2024_1_carhighbudget1.png")),
                Vehicle.Create("B-025-WHL", buc.Id, m1.Id, "Lexus",         "RX",           2025, VehicleCategory.Premium, 130m, "Hybrid",   "Automatic", 5, "#6D28D9", High("lexus_rx",              "lexus_rx_2025_1_carhighbudget1.png")),
                Vehicle.Create("B-026-WHL", buc.Id, m1.Id, "Volvo",         "XC90",         2024, VehicleCategory.Premium, 125m, "Hybrid",   "Automatic", 7, "#4C1D95", High("volvo_xc90",            "volvo_xc90_2024_1_carhighbudget1.png")),
                Vehicle.Create("B-027-WHL", buc.Id, m1.Id, "Land Rover",    "Defender 110", 2025, VehicleCategory.Premium, 155m, "Petrol",   "Automatic", 5, "#374151", High("landrover_defender110", "landrover_defender110_2025_1_carhighbudget1.png")),
                Vehicle.Create("B-028-WHL", buc.Id, m1.Id, "Tesla",         "Model S",      2026, VehicleCategory.Premium, 175m, "Electric", "Automatic", 5, "#111827", High("tesla_models",          "tesla_models_2026_1_carhighbudget1.png")),
                Vehicle.Create("B-029-WHL", buc.Id, m1.Id, "Genesis",       "GV80",         2025, VehicleCategory.Premium, 140m, "Petrol",   "Automatic", 5, "#1F2937", High("genesis_gv80",          "genesis_gv80_2025_1_carhighbudget1.png")),
                Vehicle.Create("B-030-WHL", buc.Id, m1.Id, "Alfa Romeo",    "Stelvio",      2024, VehicleCategory.Premium, 118m, "Petrol",   "Automatic", 5, "#DC2626", High("alfaromeo_stelvi",      "alfaromeo_stelvio_2024_1_carhighbudget1.png")),

                // ELITE BUDGET — Premium (7)
                Vehicle.Create("B-031-WHL", buc.Id, m1.Id, "Mercedes-Benz", "G 63 AMG",     2025, VehicleCategory.Premium, 350m, "Petrol", "Automatic", 5, "#111827", Elite("mercedesbenz_g63amg",        "mercedesbenz_g63amg_2025_1_carelitebudget1.png")),
                Vehicle.Create("B-032-WHL", buc.Id, m1.Id, "Lamborghini",   "Urus",          2024, VehicleCategory.Premium, 420m, "Petrol", "Automatic", 5, "#92400E", Elite("lamborghini_urusperformante", "lamborghini_urusperformante_2024_1_carelitebudget1.png")),
                Vehicle.Create("B-033-WHL", buc.Id, m1.Id, "Rolls-Royce",   "Cullinan",      2025, VehicleCategory.Premium, 600m, "Petrol", "Automatic", 5, "#1C1917", Elite("rollsroyce_cullinan",         "rollsroyce_cullinan_2025_1_carelitebudget1.png")),
                Vehicle.Create("B-034-WHL", buc.Id, m1.Id, "Range Rover",   "SV",            2026, VehicleCategory.Premium, 380m, "Hybrid", "Automatic", 5, "#292524", Elite("rangerover_sv",              "rangerover_sv_2026_1_carelitebudget1.png")),
                Vehicle.Create("B-035-WHL", buc.Id, m1.Id, "BMW",           "XM",            2024, VehicleCategory.Premium, 320m, "Hybrid", "Automatic", 5, "#1C1917", Elite("bmw_xm",                     "bmw_xm_2024_1_carelitebudget1.png")),
                Vehicle.Create("B-036-WHL", buc.Id, m1.Id, "Audi",          "RS Q8",         2025, VehicleCategory.Premium, 290m, "Petrol", "Automatic", 5, "#1E3A5F", Elite("audi_rsq8",                  "audi_rsq8_2025_1_carelitebudget1.png")),
                Vehicle.Create("B-037-WHL", buc.Id, m1.Id, "Ferrari",       "Purosangue",    2025, VehicleCategory.Premium, 750m, "Petrol", "Automatic", 4, "#B91C1C", Elite("ferrari_purosang",            "ferrari_purosangue_2025_1_carelitebudget1.png")),

                // VANS (3)
                Vehicle.Create("B-038-WHL", buc.Id, m1.Id, "Mercedes-Benz", "V-Class",    2024, VehicleCategory.Van, 95m, "Diesel", "Automatic", 7, "#374151", Van("mercedesbenz_vclass",   "mercedesbenz_vclass_2024_1_carvan1.png")),
                Vehicle.Create("B-039-WHL", buc.Id, m1.Id, "Volkswagen",    "Multivan T7",2024, VehicleCategory.Van, 85m, "Diesel", "Automatic", 7, "#1E40AF", Van("volkswagen_multivant7",  "volkswagen_multivant7_1_carvan1.png")),
                Vehicle.Create("B-040-WHL", buc.Id, m1.Id, "Toyota",        "Alphard",    2024, VehicleCategory.Van, 90m, "Hybrid", "Automatic", 7, "#0F172A", Van("toyota_alphard",         "toyota_alphard_2024_1_carvan1.png")),

                // ══════════════════════════════════════════════════════════
                // BUCHAREST — OTOPENI  (35 vehicles)
                // ══════════════════════════════════════════════════════════

                // LOW BUDGET (8)
                Vehicle.Create("OT-001-WHL", oto.Id, m4.Id, "Dacia",   "Sandero",    2024, VehicleCategory.Economy, 19m, "Petrol",   "Manual",    5, "#60A5FA", Low("dacia_sandero",    "dacia_sandero_2024_2_carlowbudget1.png")),
                Vehicle.Create("OT-002-WHL", oto.Id, m4.Id, "Dacia",   "Logan",      2024, VehicleCategory.Economy, 18m, "Petrol",   "Manual",    5, "#93C5FD", Low("dacia_logan",      "dacia_logan_2024_2_carlowbudget1.png")),
                Vehicle.Create("OT-003-WHL", oto.Id, m4.Id, "Renault", "Clio",       2024, VehicleCategory.Economy, 22m, "Petrol",   "Manual",    5, "#BFDBFE", Low("renult_clio",      "renault_clio_2024_2_carlowbudget1.png")),
                Vehicle.Create("OT-004-WHL", oto.Id, m4.Id, "Hyundai", "i20",        2024, VehicleCategory.Economy, 21m, "Petrol",   "Manual",    5, "#7C3AED", Low("hyundai_i20",      "hyundai_i20_2024_2_carlowbudget1.png")),
                Vehicle.Create("OT-005-WHL", oto.Id, m4.Id, "Kia",     "Picanto",    2024, VehicleCategory.Economy, 17m, "Petrol",   "Manual",    5, "#DDD6FE", Low("kia_picanto",      "kia_picanto_2024_2_carlowbudget1.png")),
                Vehicle.Create("OT-006-WHL", oto.Id, m4.Id, "Suzuki",  "Swift",      2024, VehicleCategory.Economy, 20m, "Petrol",   "Manual",    5, "#EDE9FE", Low("suzuki_swift",     "suzuki_swift_2024_2_carlowbudget1.png")),
                Vehicle.Create("OT-007-WHL", oto.Id, m4.Id, "Fiat",    "500",        2023, VehicleCategory.Economy, 19m, "Hybrid",   "Manual",    4, "#FEF3C7", Low("fiat500",          "fiat_500mildhybrid_2023_2_carlowbudget1.png")),
                Vehicle.Create("OT-008-WHL", oto.Id, m4.Id, "MG",      "3",          2024, VehicleCategory.Economy, 21m, "Petrol",   "Automatic", 5, "#FCD34D", Low("mg3",              "mg_3_2024_2_carlowbudget1.png")),

                // MEDIUM BUDGET (10)
                Vehicle.Create("OT-009-WHL",  oto.Id, m4.Id, "Toyota",     "Corolla",  2025, VehicleCategory.Compact, 35m, "Hybrid",   "Automatic", 5, "#6EE7B7", Med("toyota_corolla",  "toyota_corolla_2025_2_carmediumbudget1.png")),
                Vehicle.Create("OT-010-WHL",  oto.Id, m4.Id, "Volkswagen", "Golf",     2024, VehicleCategory.Compact, 38m, "Petrol",   "Automatic", 5, "#1A56DB", Med("volkswagen_golf", "volkswagen_golf_2024_2_carmediumbudget1.png")),
                Vehicle.Create("OT-011-WHL",  oto.Id, m4.Id, "Skoda",      "Octavia",  2024, VehicleCategory.Compact, 33m, "Diesel",   "Automatic", 5, "#3B82F6", Med("skoda_octavia",   "skoda_octavia_2024_2_carmediumbudget1.png")),
                Vehicle.Create("OT-012-WHL",  oto.Id, m4.Id, "Honda",      "Civic",    2025, VehicleCategory.Compact, 39m, "Hybrid",   "Automatic", 5, "#1E40AF", Med("honda_civic",     "honda_civic_2025_2_carmediumbudget1.png")),
                Vehicle.Create("OT-013-WHL",  oto.Id, m4.Id, "Hyundai",    "Tucson",   2025, VehicleCategory.SUV,     55m, "Hybrid",   "Automatic", 5, "#059669", Med("hyundai_tucson",  "hyundai_tucson_2025_2_carmediumbudget1.png")),
                Vehicle.Create("OT-014-WHL",  oto.Id, m4.Id, "Kia",        "Sportage", 2025, VehicleCategory.SUV,     58m, "Hybrid",   "Automatic", 5, "#34D399", Med("kia_sportage",    "kia_sportage_2025_2_carmediumbudget1.png")),
                Vehicle.Create("OT-015-WHL",  oto.Id, m4.Id, "Mazda",      "CX-5",     2025, VehicleCategory.SUV,     52m, "Petrol",   "Automatic", 5, "#10B981", Med("mazda_cx5",       "mazda_cx5_2025_2_carmediumbudget1.png")),
                Vehicle.Create("OT-016-WHL",  oto.Id, m4.Id, "Renault",    "Austral",  2025, VehicleCategory.Compact, 36m, "Hybrid",   "Automatic", 5, "#2563EB", Med("renault_austral", "renault_austral_2025_2_carmediumbudget1.png")),
                Vehicle.Create("OT-017-WHL",  oto.Id, m4.Id, "Tesla",      "Model 3",  2024, VehicleCategory.Compact, 65m, "Electric", "Automatic", 5, "#0F172A", Med("tesla_model3",    "tesla_model3_2024_2_carmediumbudget1.png")),
                Vehicle.Create("OT-018-WHL",  oto.Id, m4.Id, "Volvo",      "XC40",     2024, VehicleCategory.SUV,     60m, "Electric", "Automatic", 5, "#1E293B", Med("volvo_xc40",      "volvo_xc40_2024_2_carmediumbudget1.png")),

                // HIGH BUDGET (10)
                Vehicle.Create("OT-019-WHL",  oto.Id, m4.Id, "BMW",           "X5",           2025, VehicleCategory.Premium, 120m, "Hybrid",   "Automatic", 5, "#1340B0", High("bmw_x5",                "bmw_x5_2025_2_carhighbudget1.png")),
                Vehicle.Create("OT-020-WHL",  oto.Id, m4.Id, "Mercedes-Benz", "GLE",          2024, VehicleCategory.Premium, 135m, "Diesel",   "Automatic", 5, "#0F172A", High("mercedesbenz_gle",      "mercedesbenz_gle_2024_2_carhighbudget1.png")),
                Vehicle.Create("OT-021-WHL",  oto.Id, m4.Id, "Audi",          "Q7",           2025, VehicleCategory.Premium, 145m, "Diesel",   "Automatic", 7, "#1E293B", High("audi_q7",               "audi_q7_2025_2_carhighbudget1.png")),
                Vehicle.Create("OT-022-WHL",  oto.Id, m4.Id, "Porsche",       "Macan",        2024, VehicleCategory.Premium, 160m, "Electric", "Automatic", 5, "#7C3AED", High("porsche_macan",          "porche_macan_2024_2_carhighbudget1.png")),
                Vehicle.Create("OT-023-WHL",  oto.Id, m4.Id, "Lexus",         "RX",           2025, VehicleCategory.Premium, 130m, "Hybrid",   "Automatic", 5, "#6D28D9", High("lexus_rx",              "lexus_rx_2025_2_carhighbudget1.png")),
                Vehicle.Create("OT-024-WHL",  oto.Id, m4.Id, "Volvo",         "XC90",         2024, VehicleCategory.Premium, 125m, "Hybrid",   "Automatic", 7, "#4C1D95", High("volvo_xc90",            "volvo_xc90_2024_2_carhighbudget1.png")),
                Vehicle.Create("OT-025-WHL",  oto.Id, m4.Id, "Land Rover",    "Defender 110", 2025, VehicleCategory.Premium, 155m, "Petrol",   "Automatic", 5, "#374151", High("landrover_defender110", "landrover_defender110_2025_2_carhighbudget1.png")),
                Vehicle.Create("OT-026-WHL",  oto.Id, m4.Id, "Tesla",         "Model S",      2026, VehicleCategory.Premium, 175m, "Electric", "Automatic", 5, "#111827", High("tesla_models",          "tesla_models_2026_2_carhighbudget1.png")),
                Vehicle.Create("OT-027-WHL",  oto.Id, m4.Id, "Genesis",       "GV80",         2025, VehicleCategory.Premium, 140m, "Petrol",   "Automatic", 5, "#1F2937", High("genesis_gv80",          "genesis_gv80_2025_2_carhighbudget1.png")),
                Vehicle.Create("OT-028-WHL",  oto.Id, m4.Id, "Alfa Romeo",    "Stelvio",      2024, VehicleCategory.Premium, 118m, "Petrol",   "Automatic", 5, "#DC2626", High("alfaromeo_stelvi",      "alfaromeo_stelvio_2024_2_carhighbudget1.png")),

                // ELITE + VANS (7)
                Vehicle.Create("OT-029-WHL",  oto.Id, m4.Id, "Mercedes-Benz", "G 63 AMG",      2025, VehicleCategory.Premium, 350m, "Petrol", "Automatic", 5, "#111827", Elite("mercedesbenz_g63amg",  "mercedesbenz_g63amg_2025_2_carelitebudget1.png")),
                Vehicle.Create("OT-030-WHL",  oto.Id, m4.Id, "Bentley",        "Continental GT",2024, VehicleCategory.Premium, 500m, "Petrol", "Automatic", 4, "#78350F", Elite("bentley_continentalgt","bentley_continentalgt_2024_1_carelitebudget1.png")),
                Vehicle.Create("OT-031-WHL",  oto.Id, m4.Id, "Porsche",        "911 Turbo S",   2025, VehicleCategory.Premium, 650m, "Petrol", "Automatic", 4, "#B45309", Elite("porsche_911turbos",    "porsche_911turbos_2025_1_carelitebudget1.png")),
                Vehicle.Create("OT-032-WHL",  oto.Id, m4.Id, "Aston Martin",   "DB12",          2024, VehicleCategory.Premium, 550m, "Petrol", "Automatic", 4, "#065F46", Elite("astonmartin_db1",      "astonmartin_db12_2024_1_carelitebudget1.png")),
                Vehicle.Create("OT-033-WHL",  oto.Id, m4.Id, "Mercedes-Benz",  "V-Class",       2024, VehicleCategory.Van,     95m, "Diesel", "Automatic", 7, "#374151", Van("mercedesbenz_vclass",    "mercedesbenz_vclass_2024_2_carvan1.png")),
                Vehicle.Create("OT-034-WHL",  oto.Id, m4.Id, "Hyundai",        "Staria",        2024, VehicleCategory.Van,     80m, "Petrol", "Automatic", 7, "#1E3A8A", Van("hyundai_staria",          "hyundai_staria_2024_1_carvan1.png")),
                Vehicle.Create("OT-035-WHL",  oto.Id, m4.Id, "Lexus",          "LM",            2024, VehicleCategory.Van,    120m, "Hybrid", "Automatic", 7, "#0F172A", Van("lexsus_lm",              "lexus_lm_2024_1_carvan1.png")),

                // ══════════════════════════════════════════════════════════
                // CLUJ — AIRPORT  (28 vehicles)
                // ══════════════════════════════════════════════════════════

                // LOW BUDGET (7)
                Vehicle.Create("CJ-001-WHL", clj.Id, m2.Id, "Dacia",      "Sandero",    2024, VehicleCategory.Economy, 19m, "Petrol", "Manual",    5, "#60A5FA", Low("dacia_sandero",    "dacia_sandero_2024_1_carlowbudget2.png")),
                Vehicle.Create("CJ-002-WHL", clj.Id, m2.Id, "Renault",    "Clio",       2024, VehicleCategory.Economy, 22m, "Petrol", "Manual",    5, "#BFDBFE", Low("renult_clio",      "renault_clio_2024_1_carlowbudget2.png")),
                Vehicle.Create("CJ-003-WHL", clj.Id, m2.Id, "Hyundai",    "i20",        2024, VehicleCategory.Economy, 21m, "Petrol", "Manual",    5, "#7C3AED", Low("hyundai_i20",      "hyundai_i20_2024_1_carlowbudget2.png")),
                Vehicle.Create("CJ-004-WHL", clj.Id, m2.Id, "Kia",        "Picanto",    2024, VehicleCategory.Economy, 17m, "Petrol", "Manual",    5, "#DDD6FE", Low("kia_picanto",      "kia_picanto_2024_1_carlowbudget2.png")),
                Vehicle.Create("CJ-005-WHL", clj.Id, m2.Id, "Suzuki",     "Swift",      2024, VehicleCategory.Economy, 20m, "Petrol", "Manual",    5, "#EDE9FE", Low("suzuki_swift",     "suzuki_swift_2024_1_carlowbudget2.png")),
                Vehicle.Create("CJ-006-WHL", clj.Id, m2.Id, "Fiat",       "500",        2023, VehicleCategory.Economy, 19m, "Hybrid", "Manual",    4, "#FEF3C7", Low("fiat500",          "fiat_500mildhybrid_2023_1_carlowbudget2.png")),
                Vehicle.Create("CJ-007-WHL", clj.Id, m2.Id, "MG",         "3",          2024, VehicleCategory.Economy, 21m, "Petrol", "Automatic", 5, "#FCD34D", Low("mg3",              "mg_3_2024_1_carlowbudget2.png")),

                // MEDIUM BUDGET (10)
                Vehicle.Create("CJ-008-WHL",  clj.Id, m2.Id, "Toyota",     "Corolla",  2025, VehicleCategory.Compact, 35m, "Hybrid",   "Automatic", 5, "#6EE7B7", Med("toyota_corolla",  "toyota_corolla_2025_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-009-WHL",  clj.Id, m2.Id, "Volkswagen", "Golf",     2024, VehicleCategory.Compact, 38m, "Petrol",   "Automatic", 5, "#1A56DB", Med("volkswagen_golf", "volkswagen_golf_2024_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-010-WHL",  clj.Id, m2.Id, "Skoda",      "Octavia",  2024, VehicleCategory.Compact, 33m, "Diesel",   "Automatic", 5, "#3B82F6", Med("skoda_octavia",   "skoda_octavia_2024_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-011-WHL",  clj.Id, m2.Id, "Hyundai",    "Tucson",   2025, VehicleCategory.SUV,     55m, "Hybrid",   "Automatic", 5, "#059669", Med("hyundai_tucson",  "hyundai_tucson_2025_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-012-WHL",  clj.Id, m2.Id, "Kia",        "Sportage", 2025, VehicleCategory.SUV,     58m, "Hybrid",   "Automatic", 5, "#34D399", Med("kia_sportage",    "kia_sportage_2025_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-013-WHL",  clj.Id, m2.Id, "Mazda",      "CX-5",     2025, VehicleCategory.SUV,     52m, "Petrol",   "Automatic", 5, "#10B981", Med("mazda_cx5",       "mazda_cx5_2025_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-014-WHL",  clj.Id, m2.Id, "Tesla",      "Model 3",  2024, VehicleCategory.Compact, 65m, "Electric", "Automatic", 5, "#0F172A", Med("tesla_model3",    "tesla_model3_2024_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-015-WHL",  clj.Id, m2.Id, "Volvo",      "XC40",     2024, VehicleCategory.SUV,     60m, "Electric", "Automatic", 5, "#1E293B", Med("volvo_xc40",      "volvo_xc40_2024_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-016-WHL",  clj.Id, m2.Id, "Honda",      "Civic",    2025, VehicleCategory.Compact, 39m, "Hybrid",   "Automatic", 5, "#1E40AF", Med("honda_civic",     "honda_civic_2025_1_carmediumbudget2.png")),
                Vehicle.Create("CJ-017-WHL",  clj.Id, m2.Id, "Renault",    "Austral",  2025, VehicleCategory.Compact, 36m, "Hybrid",   "Automatic", 5, "#2563EB", Med("renault_austral", "renault_austral_2025_1_carmediumbudget2.png")),

                // HIGH BUDGET (7)
                Vehicle.Create("CJ-018-WHL",  clj.Id, m2.Id, "BMW",           "X5",      2025, VehicleCategory.Premium, 120m, "Hybrid", "Automatic", 5, "#1340B0", High("bmw_x5",           "bmw_x5_2025_1_carhighbudget2.png")),
                Vehicle.Create("CJ-019-WHL",  clj.Id, m2.Id, "Mercedes-Benz", "GLE",     2024, VehicleCategory.Premium, 135m, "Diesel", "Automatic", 5, "#0F172A", High("mercedesbenz_gle", "mercedesbenz_gle_2024_1_carhighbudget2.png")),
                Vehicle.Create("CJ-020-WHL",  clj.Id, m2.Id, "Audi",          "Q7",      2025, VehicleCategory.Premium, 145m, "Diesel", "Automatic", 7, "#1E293B", High("audi_q7",          "audi_q7_2025_1_carhighbudget2.png")),
                Vehicle.Create("CJ-021-WHL",  clj.Id, m2.Id, "Lexus",         "RX",      2025, VehicleCategory.Premium, 130m, "Hybrid", "Automatic", 5, "#6D28D9", High("lexus_rx",         "lexus_rx_2025_1_carhighbudget2.png")),
                Vehicle.Create("CJ-022-WHL",  clj.Id, m2.Id, "Volvo",         "XC90",    2024, VehicleCategory.Premium, 125m, "Hybrid", "Automatic", 7, "#4C1D95", High("volvo_xc90",       "volvo_xc90_2024_1_carhighbudget2.png")),
                Vehicle.Create("CJ-023-WHL",  clj.Id, m2.Id, "Genesis",       "GV80",    2025, VehicleCategory.Premium, 140m, "Petrol", "Automatic", 5, "#1F2937", High("genesis_gv80",     "genesis_gv80_2025_1_carhighbudget2.png")),
                Vehicle.Create("CJ-024-WHL",  clj.Id, m2.Id, "Alfa Romeo",    "Stelvio", 2024, VehicleCategory.Premium, 118m, "Petrol", "Automatic", 5, "#DC2626", High("alfaromeo_stelvi", "alfaromeo_stelvio_2024_1_carhighbudget2.png")),

                // VANS (4)
                Vehicle.Create("CJ-025-WHL",  clj.Id, m2.Id, "Mercedes-Benz", "V-Class",    2024, VehicleCategory.Van, 95m,  "Diesel",  "Automatic", 7, "#374151", Van("mercedesbenz_vclass",  "mercedesbenz_vclass_2024_1_carvan2.png")),
                Vehicle.Create("CJ-026-WHL",  clj.Id, m2.Id, "Volkswagen",    "Multivan T7",2024, VehicleCategory.Van, 85m,  "Diesel",  "Automatic", 7, "#1E40AF", Van("volkswagen_multivant7", "volkswagen_multivant7_2_carvan1.png")),
                Vehicle.Create("CJ-027-WHL",  clj.Id, m2.Id, "Volvo",         "EM90",       2025, VehicleCategory.Van, 88m,  "Electric","Automatic", 6, "#0F4C75", Van("volvo_em90",            "volvo_em90_2025_1_carvan1.png")),
                Vehicle.Create("CJ-028-WHL",  clj.Id, m2.Id, "Hyundai",       "Staria",     2024, VehicleCategory.Van, 80m,  "Petrol",  "Automatic", 7, "#1E3A8A", Van("hyundai_staria",        "hyundai_staria_2024_2_carvan1.png")),

                // ══════════════════════════════════════════════════════════
                // TIMIȘOARA — AIRPORT  (17 vehicles)
                // ══════════════════════════════════════════════════════════

                // LOW BUDGET (5)
                Vehicle.Create("TM-001-WHL", tim.Id, m3.Id, "Dacia",   "Sandero",  2024, VehicleCategory.Economy, 19m, "Petrol", "Manual", 5, "#60A5FA", Low("dacia_sandero",  "dacia_sandero_2024_2_carlowbudget2.png")),
                Vehicle.Create("TM-002-WHL", tim.Id, m3.Id, "Dacia",   "Logan",    2024, VehicleCategory.Economy, 18m, "Petrol", "Manual", 5, "#93C5FD", Low("dacia_logan",    "dacia_logan_2024_2_carlowbudget2.png")),
                Vehicle.Create("TM-003-WHL", tim.Id, m3.Id, "Renault", "Clio",     2024, VehicleCategory.Economy, 22m, "Petrol", "Manual", 5, "#BFDBFE", Low("renult_clio",    "renault_clio_2024_2_carlowbudget2.png")),
                Vehicle.Create("TM-004-WHL", tim.Id, m3.Id, "Kia",     "Picanto",  2024, VehicleCategory.Economy, 17m, "Petrol", "Manual", 5, "#DDD6FE", Low("kia_picanto",    "kia_picanto_2024_2_carlowbudget2.png")),
                Vehicle.Create("TM-005-WHL", tim.Id, m3.Id, "Suzuki",  "Swift",    2024, VehicleCategory.Economy, 20m, "Petrol", "Manual", 5, "#EDE9FE", Low("suzuki_swift",   "suzuki_swift_2024_2_carlowbudget2.png")),

                // MEDIUM BUDGET (6)
                Vehicle.Create("TM-006-WHL",  tim.Id, m3.Id, "Volkswagen", "Golf",     2024, VehicleCategory.Compact, 38m, "Petrol",   "Automatic", 5, "#1A56DB", Med("volkswagen_golf", "volkswagen_golf_2024_2_carmediumbudget2.png")),
                Vehicle.Create("TM-007-WHL",  tim.Id, m3.Id, "Skoda",      "Octavia",  2024, VehicleCategory.Compact, 33m, "Diesel",   "Automatic", 5, "#3B82F6", Med("skoda_octavia",   "skoda_octavia_2024_2_carmediumbudget2.png")),
                Vehicle.Create("TM-008-WHL",  tim.Id, m3.Id, "Hyundai",    "Tucson",   2025, VehicleCategory.SUV,     55m, "Hybrid",   "Automatic", 5, "#059669", Med("hyundai_tucson",  "hyundai_tucson_2025_2_carmediumbudget2.png")),
                Vehicle.Create("TM-009-WHL",  tim.Id, m3.Id, "Kia",        "Sportage", 2025, VehicleCategory.SUV,     58m, "Hybrid",   "Automatic", 5, "#34D399", Med("kia_sportage",    "kia_sportage_2025_2_carmediumbudget2.png")),
                Vehicle.Create("TM-010-WHL",  tim.Id, m3.Id, "Tesla",      "Model 3",  2024, VehicleCategory.Compact, 65m, "Electric", "Automatic", 5, "#0F172A", Med("tesla_model3",    "tesla_model3_2024_2_carmediumbudget2.png")),
                Vehicle.Create("TM-011-WHL",  tim.Id, m3.Id, "Renault",    "Austral",  2025, VehicleCategory.Compact, 36m, "Hybrid",   "Automatic", 5, "#2563EB", Med("renault_austral", "renault_austral_2025_2_carmediumbudget2.png")),

                // HIGH BUDGET (4)
                Vehicle.Create("TM-012-WHL",  tim.Id, m3.Id, "BMW",           "X5",    2025, VehicleCategory.Premium, 120m, "Hybrid",   "Automatic", 5, "#1340B0", High("bmw_x5",          "bmw_x5_2025_2_carhighbudget2.png")),
                Vehicle.Create("TM-013-WHL",  tim.Id, m3.Id, "Mercedes-Benz", "GLE",   2024, VehicleCategory.Premium, 135m, "Diesel",   "Automatic", 5, "#0F172A", High("mercedesbenz_gle", "mercedesbenz_gle_2024_2_carhighbudget2.png")),
                Vehicle.Create("TM-014-WHL",  tim.Id, m3.Id, "Audi",          "Q7",    2025, VehicleCategory.Premium, 145m, "Diesel",   "Automatic", 7, "#1E293B", High("audi_q7",          "audi_q7_2025_2_carhighbudget2.png")),
                Vehicle.Create("TM-015-WHL",  tim.Id, m3.Id, "Porsche",       "Macan", 2024, VehicleCategory.Premium, 160m, "Electric", "Automatic", 5, "#7C3AED", High("porsche_macan",    "porche_macan_2024_2_carhighbudget2.png")),

                // VANS (2)
                Vehicle.Create("TM-016-WHL",  tim.Id, m3.Id, "Volkswagen", "Multivan T7", 2024, VehicleCategory.Van, 85m, "Diesel", "Automatic", 7, "#1E40AF", Van("volkswagen_multivant7", "volkswagen_multivant7_2_carvan2.png")),
                Vehicle.Create("TM-017-WHL",  tim.Id, m3.Id, "Toyota",     "Alphard",     2024, VehicleCategory.Van, 90m, "Hybrid", "Automatic", 7, "#0F172A", Van("toyota_alphard",         "toyota_alphard_2024_2_carvan1.png")),
            };

            // Set ratings (deterministic random)
            var rng = new Random(42);
            foreach (var v in vehicles)
                v.SetRating(Math.Round((decimal)(rng.Next(45, 51)) / 10m, 1));

            // Offers
            foreach (var v in vehicles.Where(v => v.Brand == "Dacia"))
                v.SetOffer(true, 15);
            foreach (var v in vehicles.Where(v => v.Brand == "Renault" && v.Model == "Clio"))
                v.SetOffer(true, 12);
            foreach (var v in vehicles.Where(v => v.Brand == "Skoda" && v.Model == "Octavia"))
                v.SetOffer(true, 10);
            foreach (var v in vehicles.Where(v => v.Brand == "Kia" && v.Model == "Sportage"))
                v.SetOffer(true, 8);
            foreach (var v in vehicles.Where(v => v.Brand == "Tesla"))
                v.SetOffer(true, 5);

            db.Vehicles.AddRange(vehicles);
            await db.SaveChangesAsync();
        }

        // ── Sample clients ────────────────────────────────────────────────
        if (!await db.Clients.AnyAsync())
        {
            db.Clients.AddRange(
                Client.Create("Andrew Peterson", "andrew.p@email.com", "+40 740 001 001", "Str. Florilor 12, București"),
                Client.Create("Maria Ionescu", "maria.i@email.com", "+40 740 001 002", "Bd. Eroilor 5, Cluj-Napoca"),
                Client.Create("Chris Dumitrescu", "chris.d@email.com", "+40 740 001 003", "Str. Libertății 8, Timișoara"),
                Client.Create("Elena Popa", "elena.p@email.com", "+40 740 001 004", "Calea Victoriei 22, București"),
                Client.Create("Radu Constantin", "radu.c@email.com", "+40 740 001 005", "Bd. Mihai Viteazu 3, Cluj-Napoca")
            );
            await db.SaveChangesAsync();
        }

        // ── Promo codes ───────────────────────────────────────────────────
        if (!await db.PromoCodes.AnyAsync())
        {
            db.PromoCodes.AddRange(
                PromoCode.Create("SUMMER30", PromoType.Percentage, 30,
                    new DateTime(2026, 8, 31), "30% off all weekend rentals", weekendOnly: true),
                PromoCode.Create("FLEET10", PromoType.Percentage, 10,
                    new DateTime(2026, 6, 30), "10% off all Premium models", applicableCategory: "Premium"),
                PromoCode.Create("DRIVE25", PromoType.Percentage, 25,
                    new DateTime(2026, 7, 15), "25% off Dacia Logan")
            );
            await db.SaveChangesAsync();
        }
    }
}