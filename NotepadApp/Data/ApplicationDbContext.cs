using Microsoft.EntityFrameworkCore;
using NotepadApp.Models;

namespace NotepadApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteConnection> NoteConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Note>()
                .Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Note>()
                .Property(n => n.Content)
                .IsRequired();

            modelBuilder.Entity<Note>()
                .Property(n => n.Color)
                .IsRequired()
                .HasMaxLength(7); // #RRGGBB format

            modelBuilder.Entity<Note>()
                .Property(n => n.PositionX)
                .IsRequired();

            modelBuilder.Entity<Note>()
                .Property(n => n.PositionY)
                .IsRequired();

            // NoteConnection configurations
            modelBuilder.Entity<NoteConnection>(entity =>
            {
                entity.HasKey(nc => nc.Id);

                entity.HasOne(nc => nc.SourceNote)
                    .WithMany() // If Note had a collection of connections originating from it, it would go here.
                    .HasForeignKey(nc => nc.SourceNoteId)
                    .OnDelete(DeleteBehavior.Cascade); // If a source note is deleted, delete the connection.

                entity.HasOne(nc => nc.TargetNote)
                    .WithMany() // If Note had a collection of connections pointing to it, it would go here.
                    .HasForeignKey(nc => nc.TargetNoteId)
                    .OnDelete(DeleteBehavior.Cascade); // If a target note is deleted, delete the connection.

                // Prevent creating a connection from a note to itself if desired (optional database constraint)
                // entity.HasCheckConstraint("CK_NoteConnection_SourceTargetDifferent", "\"SourceNoteId\" <> \"TargetNoteId\"");
            });
        }
    }
} 