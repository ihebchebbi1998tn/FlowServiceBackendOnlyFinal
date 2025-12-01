using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Dispatches.Models;

namespace MyApi.Modules.Dispatches.Data
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.ToTable("dispatch_attachments");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasMaxLength(50);
            builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
            builder.Property(a => a.FileType).HasMaxLength(100);
            builder.Property(a => a.FileSizeMb).HasColumnType("decimal(10,2)");
            builder.Property(a => a.Category).HasMaxLength(100);
            builder.Property(a => a.UploadedAt).IsRequired();
        }
    }
}
