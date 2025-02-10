using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Identity;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultSuperAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a default SuperAdmin user
            var userId = "8e445865-a24d-4543-a6c6-9443d048cdb9"; // Fixed GUID for reproducibility
            var roleId = "867f39a9-9ee4-4e30-8e2d-49b809815227"; // This is the SuperAdmin role ID from previous migration
            var hasher = new PasswordHasher<IdentityUser>();
            var passwordHash = hasher.HashPassword(null, "SuperAdmin@123"); // Default password

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] {
                    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
                    "FirstName", "LastName", "CreatedAt", "CreatedBy", "IsDeleted", "IsActive"
                },
                values: new object[] {
                    userId,
                    "superadmin@splitter.com",
                    "SUPERADMIN@SPLITTER.COM",
                    "superadmin@splitter.com",
                    "SUPERADMIN@SPLITTER.COM",
                    true, // EmailConfirmed
                    passwordHash,
                    Guid.NewGuid().ToString(), // SecurityStamp
                    Guid.NewGuid().ToString(), // ConcurrencyStamp
                    false, // PhoneNumberConfirmed
                    false, // TwoFactorEnabled
                    true,  // LockoutEnabled
                    0,     // AccessFailedCount
                    "Super", // FirstName
                    "Admin", // LastName
                    DateTime.UtcNow, // CreatedAt
                    "SYSTEM", // CreatedBy
                    false,   // IsDeleted
                    true    // IsActive
                }
            );

            // Assign SuperAdmin role to the user
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { userId, roleId }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var userId = "8e445865-a24d-4543-a6c6-9443d048cdb9";

            // Remove the user role assignment first
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { userId, "867f39a9-9ee4-4e30-8e2d-49b809815227" }
            );

            // Then remove the user
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: userId
            );
        }
    }
}
