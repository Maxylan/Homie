// (c) 2024 @Maxylan
// Scaffolded, then altered to suit my needs. @see scaffold.txt
namespace Homie.Database;

using System;
using System.Collections.Generic;
using Homie.Database.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

/// <summary>
/// The 'HomieDB' context, reflects the database.
/// </summary>
public partial class HomieDB : DbContext
{
    public HomieDB()
    { }

    public HomieDB(DbContextOptions<HomieDB> options)
        : base(options) { }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AccessLog> AccessLogs { get; set; }

    public virtual DbSet<Export> Exports { get; set; }

    public virtual DbSet<Platform> Platforms { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductIndex> ProductIndexes { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<List> Lists { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeRating> RecipeRatings { get; set; }

    public virtual DbSet<RecipeTag> RecipeTags { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<ReminderTag> ReminderTags { get; set; }

    public virtual DbSet<RemindersArchive> RemindersArchives { get; set; }

    public virtual DbSet<Row> Rows { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAvatar> UserAvatars { get; set; }

    public virtual DbSet<Visibility> Visibilities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        // For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        optionsBuilder.UseMySql(Backoffice.App.Configuration.GetConnectionString("HomieDB"), ServerVersion.Parse("8.3.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");
        
        base.OnModelCreating(modelBuilder);
        
        // This is quite satisfying to see. DbContexts no longer need to be 2000+ lines.
        modelBuilder
            .Entity(Attachment.Configuration())
            .Entity(AccessLog.Configuration())
            .Entity(Export.Configuration())
            .Entity(Platform.Configuration())
            .Entity(Product.Configuration())
            .Entity(ProductIndex.Configuration())
            .Entity(Group.Configuration())
            .Entity(Item.Configuration())
            .Entity(List.Configuration())
            .Entity(Note.Configuration())
            .Entity(Option.Configuration())
            .Entity(Recipe.Configuration())
            .Entity(RecipeRating.Configuration())
            .Entity(RecipeTag.Configuration())
            .Entity(Reminder.Configuration())
            .Entity(ReminderTag.Configuration())
            .Entity(RemindersArchive.Configuration())
            .Entity(Row.Configuration())
            .Entity(User.Configuration())
            .Entity(UserAvatar.Configuration())
            .Entity(Visibility.Configuration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
