namespace E_Commerce_API
{
    public static class ApiEndpoints
    {
        private const string ApiBase = "api";

        public static class Auth
        {
            private const string Base = $"{ApiBase}/auth";

            public const string Register = $"{Base}/register";
            public const string Login = $"{Base}/login";
        }

        public static class Products
        {
            private const string Base = $"{ApiBase}/products";

            public const string Create = Base;
            public const string Get = $"{Base}/{{id:guid}}";
            public const string GetAll = Base;
            public const string Update = $"{Base}/{{id:guid}}";
            public const string Delete = $"{Base}/{{id:guid}}";
        }

        public static class Cart
        {
            private const string Base = $"{ApiBase}/cart";

            public const string Get = Base;
            public const string Clear = Base;
            public const string AddItem = $"{Base}/items";
            public const string UpdateItem = $"{Base}/items/{{productId:guid}}";
            public const string RemoveItem = $"{Base}/items/{{productId:guid}}";
        }

        public static class Orders
        {
            private const string Base = $"{ApiBase}/orders";

            public const string Checkout = Base;
            public const string GetMine = Base;
            public const string Get = $"{Base}/{{id:guid}}";
            public const string Cancel = $"{Base}/{{id:guid}}/cancel";
        }

        public static class Admin
        {
            private const string Base = $"{ApiBase}/admin";

            public const string GetAllOrders = $"{Base}/orders";
            public const string UpdateOrderStatus = $"{Base}/orders/{{id:guid}}/status";
        }
    }
}
