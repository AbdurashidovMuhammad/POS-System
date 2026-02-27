namespace Application.Constants;

public static class PermissionConstants
{
    public static class Products
    {
        public const string Section = "Products";
        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string AddStock = "AddStock";
    }

    public static class Categories
    {
        public const string Section = "Categories";
        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
    }

    public static class Sales
    {
        public const string Section = "Sales";
        public const string Create = "Create";
        public const string ViewHistory = "ViewHistory";
    }

    // (Section, Action, DisplayName) — used for seeding
    public static readonly (string Section, string Action, string DisplayName)[] AllPermissions =
    [
        (Products.Section, Products.Read,     "Mahsulotlarni ko'rish"),
        (Products.Section, Products.Create,   "Mahsulot qo'shish"),
        (Products.Section, Products.Update,   "Mahsulotni tahrirlash"),
        (Products.Section, Products.Delete,   "Mahsulotni o'chirish"),
        (Products.Section, Products.AddStock, "Ombor qo'shish"),

        (Categories.Section, Categories.Read,   "Kategoriyalarni ko'rish"),
        (Categories.Section, Categories.Create, "Kategoriya qo'shish"),
        (Categories.Section, Categories.Update, "Kategoriyani tahrirlash"),
        (Categories.Section, Categories.Delete, "Kategoriyani o'chirish"),

        (Sales.Section, Sales.Create,      "Sotuv qilish"),
        (Sales.Section, Sales.ViewHistory, "Sotuvlar tarixini ko'rish"),
    ];
}
