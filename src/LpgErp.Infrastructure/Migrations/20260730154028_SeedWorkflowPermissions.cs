using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <summary>
    /// Backfills the 3 new workflow-transition permissions (purchaseorders.receive,
    /// salesorders.deliver, vehicleloading.close) into an existing database. DbSeeder only ever
    /// seeds Permissions/Roles once, on an empty database, so a permission added to AppPermissions
    /// after that point never reaches a database that already has data — this migration is the
    /// backfill for that gap, for these three specifically.
    /// </summary>
    public partial class SeedWorkflowPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guarded by NOT EXISTS throughout so this is safe to run against a database that was
            // never seeded at all (nothing to backfill) or already has these rows (re-running the
            // migration, or a fresh DbSeeder run that already included them).
            migrationBuilder.Sql(@"
                INSERT INTO Permissions (Id, Name, Description, [Group], CreatedAt, IsDeleted)
                SELECT NEWID(), 'purchaseorders.receive', 'purchaseorders receive', 'purchaseorders', GETUTCDATE(), 0
                WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'purchaseorders.receive');

                INSERT INTO Permissions (Id, Name, Description, [Group], CreatedAt, IsDeleted)
                SELECT NEWID(), 'salesorders.deliver', 'salesorders deliver', 'salesorders', GETUTCDATE(), 0
                WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'salesorders.deliver');

                INSERT INTO Permissions (Id, Name, Description, [Group], CreatedAt, IsDeleted)
                SELECT NEWID(), 'vehicleloading.close', 'vehicleloading close', 'vehicleloading', GETUTCDATE(), 0
                WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'vehicleloading.close');
            ");

            // Admin holds every permission, by definition.
            migrationBuilder.Sql(@"
                INSERT INTO RolePermissions (RoleId, PermissionId)
                SELECT r.Id, p.Id
                FROM Roles r CROSS JOIN Permissions p
                WHERE r.Name = 'Admin' AND r.IsDeleted = 0
                  AND p.Name IN ('purchaseorders.receive', 'salesorders.deliver', 'vehicleloading.close')
                  AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
            ");

            // Warehouse only ever held PurchaseOrders.View — enforcing permissions without this
            // would have locked Warehouse staff out of receiving goods, the core of their job.
            migrationBuilder.Sql(@"
                INSERT INTO RolePermissions (RoleId, PermissionId)
                SELECT r.Id, p.Id
                FROM Roles r CROSS JOIN Permissions p
                WHERE r.Name = 'Warehouse' AND r.IsDeleted = 0 AND p.Name = 'purchaseorders.receive'
                  AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
            ");

            // Driver only ever held VehicleLoading.View — same reasoning, for closing their own loading.
            migrationBuilder.Sql(@"
                INSERT INTO RolePermissions (RoleId, PermissionId)
                SELECT r.Id, p.Id
                FROM Roles r CROSS JOIN Permissions p
                WHERE r.Name = 'Driver' AND r.IsDeleted = 0 AND p.Name = 'vehicleloading.close'
                  AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE rp FROM RolePermissions rp
                INNER JOIN Permissions p ON p.Id = rp.PermissionId
                WHERE p.Name IN ('purchaseorders.receive', 'salesorders.deliver', 'vehicleloading.close');

                DELETE FROM Permissions
                WHERE Name IN ('purchaseorders.receive', 'salesorders.deliver', 'vehicleloading.close');
            ");
        }
    }
}
