using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApi.Modules.Dispatches.Models;

namespace MyApi.Modules.Dispatches.Data
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("dispatch_expenses");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasMaxLength(50);
            builder.Property(e => e.Type).HasMaxLength(100);
            builder.Property(e => e.Currency).HasMaxLength(10);
            builder.Property(e => e.Amount).HasColumnType("decimal(10,2)");
            builder.Property(e => e.Status).HasMaxLength(50);
            builder.Property(e => e.CreatedAt).IsRequired();
        }
    }
}
