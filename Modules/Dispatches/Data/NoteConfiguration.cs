using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Dispatches.Models;

namespace MyApi.Modules.Dispatches.Data
{
    public class NoteConfiguration : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> builder)
        {
            builder.ToTable("dispatch_notes");
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Id).HasMaxLength(50);
            builder.Property(n => n.Content).IsRequired().HasMaxLength(2000);
            builder.Property(n => n.Category).HasMaxLength(100);
            builder.Property(n => n.Priority).HasMaxLength(50);
            builder.Property(n => n.CreatedAt).IsRequired();
        }
    }
}
