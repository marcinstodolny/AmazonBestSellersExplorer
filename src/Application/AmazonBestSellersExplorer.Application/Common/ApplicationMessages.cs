namespace AmazonBestSellersExplorer.Application.Common;

public static class ApplicationMessages
{
    public static class Auth
    {
        public const string InvalidCredentials = "Invalid username or password.";
        public const string UsernameAlreadyTaken = "Username is already taken.";
        public const string UsernameRequired = "Username is required.";
        public const string PasswordRequired = "Password is required.";
        public const string PasswordUppercaseRequired = "Password must contain at least one uppercase letter.";
        public const string PasswordLowercaseRequired = "Password must contain at least one lowercase letter.";
        public const string PasswordDigitRequired = "Password must contain at least one digit.";

        public static string PasswordMinimumLength(int minimumLength) =>
            $"Password must be at least {minimumLength} characters long.";
    }

    public static class Favorites
    {
        public const string UserNotAuthenticated = "User is not authenticated.";
        public const string FavoriteProductAlreadyExists = "Favorite product already exists.";
        public const string FavoriteProductDoesNotExist = "Favorite product does not exist.";
        public const string AmazonProductIdRequired = "Amazon product id is required.";
        public const string TitleRequired = "Title is required.";
        public const string ProductUrlRequired = "Product URL is required.";
    }

    public static class Bestsellers
    {
        public const string FailedToRetrieveSoftwareBestSellers = "Failed to retrieve software best sellers.";
    }
}
