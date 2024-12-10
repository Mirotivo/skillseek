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

        LessonCategorySeeder.Seed(context);
        ListingSeeder.Seed(context);
        LessonSeeder.Seed(context);
        ReviewSeeder.Seed(context);
        ChatSeeder.Seed(context);
        MessageSeeder.Seed(context);
    }
}