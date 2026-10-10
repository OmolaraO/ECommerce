namespace E_Commerce_API.Auth
{
    public static class IdentityExtensions
    {
        public static Guid? GetUserId(this HttpContext context)
        {
            var claim = context.User.FindFirst("sub");
            if(Guid.TryParse(claim?.Value, out var userId)) 
                return userId;
            return null;
        }

        public static bool IsAdmin(this HttpContext context)
        {
            return context.User.HasClaim("role", "Admin");
        }
    }
}
