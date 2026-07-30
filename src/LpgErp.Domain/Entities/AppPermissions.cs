namespace LpgErp.Domain.Entities;

public static class AppPermissions
{
    public static class Customers
    {
        public const string View = "customers.view";
        public const string Create = "customers.create";
        public const string Edit = "customers.edit";
        public const string Delete = "customers.delete";
    }

    public static class SalesOrders
    {
        public const string View = "salesorders.view";
        public const string Create = "salesorders.create";
        public const string Edit = "salesorders.edit";
        public const string Delete = "salesorders.delete";

        /// <summary>Marking a sale delivered draws down stock and updates the customer's cylinder
        /// ledger — a workflow transition, kept distinct from Edit for the same reason as Receive.</summary>
        public const string Deliver = "salesorders.deliver";
    }

    public static class PurchaseOrders
    {
        public const string View = "purchaseorders.view";
        public const string Create = "purchaseorders.create";
        public const string Edit = "purchaseorders.edit";
        public const string Delete = "purchaseorders.delete";

        /// <summary>Receiving goods is a workflow transition, not a general edit — Warehouse holds
        /// this without holding Edit, the same way Settlements.Approve differs from Edit.</summary>
        public const string Receive = "purchaseorders.receive";
    }

    public static class Payments
    {
        public const string View = "payments.view";
        public const string Create = "payments.create";
        public const string Edit = "payments.edit";
        public const string Delete = "payments.delete";
    }

    public static class Inventory
    {
        public const string View = "inventory.view";
        public const string Transfer = "inventory.transfer";
    }

    public static class Reports
    {
        public const string View = "reports.view";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Edit = "users.edit";
        public const string Delete = "users.delete";
        public const string ManageRoles = "users.manageRoles";
    }

    public static class Settings
    {
        public const string View = "settings.view";
        public const string Edit = "settings.edit";
    }

    public static class VehicleLoading
    {
        public const string View = "vehicleloading.view";
        public const string Create = "vehicleloading.create";
        public const string Edit = "vehicleloading.edit";
        public const string Delete = "vehicleloading.delete";

        /// <summary>Closing out a loading is the driver's own end-of-day workflow step, not a
        /// general edit — Drivers hold this without holding Edit.</summary>
        public const string Close = "vehicleloading.close";
    }

    public static class Settlements
    {
        public const string View = "settlements.view";
        public const string Create = "settlements.create";
        public const string Approve = "settlements.approve";
    }

    public static class Commission
    {
        public const string View = "commission.view";
        public const string Manage = "commission.manage";
    }

    public static class Notifications
    {
        public const string View = "notifications.view";
        public const string Send = "notifications.send";
        public const string Manage = "notifications.manage";
    }

    public static IEnumerable<string> GetAll()
    {
        return
            typeof(AppPermissions)
                .GetNestedTypes()
                .SelectMany(t => t.GetFields()
                    .Where(f => f.FieldType == typeof(string))
                    .Select(f => (string)f.GetValue(null)!));
    }

    public static IEnumerable<string> GetGroups()
    {
        return typeof(AppPermissions).GetNestedTypes().Select(t => t.Name);
    }
}
