public static class DbInitializer
{
    public static void Initialize(skillseekDbContext context)
    {
        context.Database.EnsureCreated();

        StationGroupSeeder.Seed(context);
        CategorySeeder.Seed(context);
        ProductSeeder.Seed(context);
        UserSeeder.Seed(context);
        FriendshipSeeder.Seed(context);
    }
}