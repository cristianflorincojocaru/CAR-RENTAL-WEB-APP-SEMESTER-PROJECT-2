using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Branches ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id        = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Name      = table.Column<string>(maxLength: 100, nullable: false),
                    City      = table.Column<string>(maxLength: 100, nullable: false),
                    Address   = table.Column<string>(maxLength: 300, nullable: false),
                    Phone     = table.Column<string>(maxLength: 20,  nullable: false),
                    ManagerId = table.Column<int>(nullable: true),
                    IsActive  = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateIndex("IX_Branches_Name", "Branches", "Name");
            migrationBuilder.CreateIndex("IX_Branches_City", "Branches", "City");

            // ── Users ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id                  = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Username            = table.Column<string>(maxLength: 50,  nullable: false),
                    PasswordHash        = table.Column<string>(maxLength: 500, nullable: false),
                    Email               = table.Column<string>(maxLength: 200, nullable: false),
                    FullName            = table.Column<string>(maxLength: 150, nullable: false),
                    Phone               = table.Column<string>(maxLength: 20,  nullable: true),
                    Role                = table.Column<int>(nullable: false),
                    BranchId            = table.Column<int>(nullable: true),
                    CreatedByUserId     = table.Column<int>(nullable: true),
                    IsActive            = table.Column<bool>(nullable: false, defaultValue: true),
                    IsLocked            = table.Column<bool>(nullable: false, defaultValue: false),
                    FailedLoginAttempts = table.Column<int>(nullable: false, defaultValue: 0),
                    LockedUntil         = table.Column<DateTime>(nullable: true),
                    LastActivityAt      = table.Column<DateTime>(nullable: true),
                    LastLoginAt         = table.Column<DateTime>(nullable: true),
                    CreatedAt           = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey("FK_Users_Branches_BranchId", x => x.BranchId,
                        "Branches", "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_Users_Users_CreatedByUserId", x => x.CreatedByUserId,
                        "Users", "Id", onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex("IX_Users_Username", "Users", "Username", unique: true);
            migrationBuilder.CreateIndex("IX_Users_Email",    "Users", "Email",    unique: true);
            migrationBuilder.CreateIndex("IX_Users_Role",     "Users", "Role");
            migrationBuilder.CreateIndex("IX_Users_BranchId", "Users", "BranchId");

            // Branches.ManagerId FK (after Users exists)
            migrationBuilder.AddForeignKey("FK_Branches_Users_ManagerId", "Branches", "ManagerId",
                "Users", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
            migrationBuilder.CreateIndex("IX_Branches_ManagerId", "Branches", "ManagerId");

            // ── Vehicles ──────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id                 = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationNumber = table.Column<string>(maxLength: 20,  nullable: false),
                    BranchId           = table.Column<int>(nullable: false),
                    AddedByUserId      = table.Column<int>(nullable: false),
                    Brand              = table.Column<string>(maxLength: 100, nullable: false),
                    Model              = table.Column<string>(maxLength: 100, nullable: false),
                    Year               = table.Column<int>(nullable: false),
                    Category           = table.Column<int>(nullable: false),
                    DailyRate          = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status             = table.Column<int>(nullable: false),
                    IsActive           = table.Column<bool>(nullable: false, defaultValue: true),
                    IsOffer            = table.Column<bool>(nullable: false, defaultValue: false),
                    DiscountPercent    = table.Column<int>(nullable: true),
                    ColorHex           = table.Column<string>(maxLength: 10, nullable: true),
                    FuelType           = table.Column<string>(maxLength: 30, nullable: true),
                    Transmission       = table.Column<string>(maxLength: 20, nullable: true),
                    Seats              = table.Column<int>(nullable: true),
                    Rating             = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    CreatedAt          = table.Column<DateTime>(nullable: false),
                    UpdatedAt          = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey("FK_Vehicles_Branches_BranchId",       x => x.BranchId,
                        "Branches", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Vehicles_Users_AddedByUserId", x => x.AddedByUserId,
                        "Users",    "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_Vehicles_RegistrationNumber", "Vehicles", "RegistrationNumber", unique: true);
            migrationBuilder.CreateIndex("IX_Vehicles_Category",            "Vehicles", "Category");
            migrationBuilder.CreateIndex("IX_Vehicles_BranchId",            "Vehicles", "BranchId");
            migrationBuilder.CreateIndex("IX_Vehicles_Status",              "Vehicles", "Status");

            // ── Clients ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id         = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    FullName   = table.Column<string>(maxLength: 150, nullable: false),
                    Email      = table.Column<string>(maxLength: 200, nullable: false),
                    Phone      = table.Column<string>(maxLength: 20,  nullable: false),
                    Address    = table.Column<string>(maxLength: 300, nullable: true),
                    IsFlagged  = table.Column<bool>(nullable: false, defaultValue: false),
                    FlagReason = table.Column<string>(maxLength: 500, nullable: true),
                    IsActive   = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt  = table.Column<DateTime>(nullable: false),
                    UpdatedAt  = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateIndex("IX_Clients_Email",    "Clients", "Email", unique: true);
            migrationBuilder.CreateIndex("IX_Clients_Phone",    "Clients", "Phone", unique: true);
            migrationBuilder.CreateIndex("IX_Clients_FullName", "Clients", "FullName");

            // ── Rentals ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Rentals",
                columns: table => new
                {
                    Id                 = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId          = table.Column<int>(nullable: false),
                    ClientId           = table.Column<int>(nullable: false),
                    BranchId           = table.Column<int>(nullable: false),
                    CreatedByUserId    = table.Column<int>(nullable: false),
                    CompletedByUserId  = table.Column<int>(nullable: true),
                    BookingReference   = table.Column<string>(maxLength: 20,   nullable: false),
                    StartDate          = table.Column<DateTime>(nullable: false),
                    EndDate            = table.Column<DateTime>(nullable: false),
                    PickupLocation     = table.Column<string>(maxLength: 200,  nullable: false),
                    ReturnLocation     = table.Column<string>(maxLength: 200,  nullable: false),
                    TotalCost          = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status             = table.Column<int>(nullable: false),
                    CancellationReason = table.Column<string>(maxLength: 500,  nullable: true),
                    ProtectionPlan     = table.Column<string>(maxLength: 50,   nullable: true),
                    Extras             = table.Column<string>(maxLength: 500,  nullable: true),
                    Notes              = table.Column<string>(maxLength: 1000, nullable: true),
                    PayNow             = table.Column<bool>(nullable: false, defaultValue: true),
                    RowVersion         = table.Column<byte[]>(rowVersion: true, nullable: false),
                    CreatedAt          = table.Column<DateTime>(nullable: false),
                    UpdatedAt          = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rentals", x => x.Id);
                    table.ForeignKey("FK_Rentals_Vehicles_VehicleId",       x => x.VehicleId,
                        "Vehicles", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Rentals_Clients_ClientId",         x => x.ClientId,
                        "Clients",  "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Rentals_Branches_BranchId",        x => x.BranchId,
                        "Branches", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Rentals_Users_CreatedByUserId",    x => x.CreatedByUserId,
                        "Users",    "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Rentals_Users_CompletedByUserId",  x => x.CompletedByUserId,
                        "Users",    "Id", onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex("IX_Rentals_BookingReference",   "Rentals", "BookingReference", unique: true);
            migrationBuilder.CreateIndex("IX_Rentals_VehicleId",          "Rentals", "VehicleId");
            migrationBuilder.CreateIndex("IX_Rentals_ClientId",           "Rentals", "ClientId");
            migrationBuilder.CreateIndex("IX_Rentals_BranchId",           "Rentals", "BranchId");
            migrationBuilder.CreateIndex("IX_Rentals_Status",             "Rentals", "Status");
            migrationBuilder.CreateIndex("IX_Rentals_StartDate_EndDate",  "Rentals", new[] { "StartDate", "EndDate" });

            // ── RefreshTokens ─────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id        = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId    = table.Column<int>(nullable: false),
                    Token     = table.Column<string>(maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(nullable: false),
                    IsRevoked = table.Column<bool>(nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey("FK_RefreshTokens_Users_UserId", x => x.UserId,
                        "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_RefreshTokens_Token",  "RefreshTokens", "Token", unique: true);
            migrationBuilder.CreateIndex("IX_RefreshTokens_UserId", "RefreshTokens", "UserId");

            // ── AuditLogs ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id         = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId     = table.Column<int>(nullable: true),
                    EntityType = table.Column<string>(maxLength: 100,  nullable: false),
                    EntityId   = table.Column<int>(nullable: false),
                    Action     = table.Column<string>(maxLength: 100,  nullable: false),
                    Details    = table.Column<string>(maxLength: 1000, nullable: false),
                    Timestamp  = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey("FK_AuditLogs_Users_UserId", x => x.UserId,
                        "Users", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex("IX_AuditLogs_EntityType_EntityId", "AuditLogs", new[] { "EntityType", "EntityId" });
            migrationBuilder.CreateIndex("IX_AuditLogs_Timestamp",           "AuditLogs", "Timestamp");
            migrationBuilder.CreateIndex("IX_AuditLogs_UserId",              "AuditLogs", "UserId");

            // ── SecurityAlerts ────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "SecurityAlerts",
                columns: table => new
                {
                    Id          = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId      = table.Column<int>(nullable: false),
                    AlertType   = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: false),
                    IsRead      = table.Column<bool>(nullable: false, defaultValue: false),
                    CreatedAt   = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAlerts", x => x.Id);
                    table.ForeignKey("FK_SecurityAlerts_Users_UserId", x => x.UserId,
                        "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_SecurityAlerts_UserId", "SecurityAlerts", "UserId");
            migrationBuilder.CreateIndex("IX_SecurityAlerts_IsRead", "SecurityAlerts", "IsRead");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("SecurityAlerts");
            migrationBuilder.DropTable("AuditLogs");
            migrationBuilder.DropTable("RefreshTokens");
            migrationBuilder.DropTable("Rentals");
            migrationBuilder.DropTable("Clients");
            migrationBuilder.DropTable("Vehicles");
            migrationBuilder.DropForeignKey("FK_Branches_Users_ManagerId", "Branches");
            migrationBuilder.DropTable("Users");
            migrationBuilder.DropTable("Branches");
        }
    }
}
