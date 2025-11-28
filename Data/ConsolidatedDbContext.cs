using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ConsolidatedApi.Models;

namespace ConsolidatedApi.Data
{
    public class ConsolidatedDbContext : IdentityDbContext<ApplicationUser, Role, string,
        IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ConsolidatedDbContext(DbContextOptions<ConsolidatedDbContext> options)
            : base(options) { }

        // User API entities
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ClientOrganization> ClientOrganizations { get; set; }
        public DbSet<UiPage> UiPages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserConnectedDevice> UserConnectedDevices { get; set; }

        // Chat API entities
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<UserToUserChat> UserToUserChats { get; set; }
        public DbSet<UserToUserMessage> UserToUserMessages { get; set; }

        // Notification API entities
        public DbSet<Notification> Notifications { get; set; }

        // Service API entities
        public DbSet<Device> Devices { get; set; }
        public DbSet<CustomerDevice> CustomerDevices { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<InstallationRequest> InstallationRequests { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ServiceRequestImage> ServiceRequestImages { get; set; }
        public DbSet<SparePart> SpareParts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");

            // User API relationships
            builder.Entity<Address>()
                .HasOne(a => a.User)
                .WithOne(u => u.Address)
                .HasForeignKey<Address>(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<UserConnectedDevice>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.ConnectedDevices)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.Device)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ClientOrganization>()
                .HasMany(co => co.Users)
                .WithOne(u => u.ClientOrganization)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClientOrganization>()
                .HasMany(co => co.Roles)
                .WithOne(r => r.ClientOrganization)
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClientOrganization>()
                .HasMany(co => co.UiPages)
                .WithOne(uip => uip.ClientOrganization)
                .HasForeignKey(uip => uip.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure value conversions for PostgreSQL with comparers
            builder.Entity<Role>()
                .Property(r => r.Permissions)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            builder.Entity<UiPage>()
                .Property(u => u.FieldsToNotDisplay)
                .HasConversion(
                    f => string.Join(',', f),
                    f => f.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Chat API relationships
            builder.Entity<ChatMessage>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserToUserChat>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(utuc => utuc.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserToUserChat>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(utuc => utuc.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserToUserMessage>()
                .HasOne(utm => utm.Chat)
                .WithMany(utuc => utuc.Messages)
                .HasForeignKey(utm => utm.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserToUserMessage>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(utm => utm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification API relationships
            builder.Entity<Notification>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Service API relationships
            builder.Entity<CustomerDevice>()
                .HasOne(cd => cd.Device)
                .WithMany(d => d.CustomerDevices)
                .HasForeignKey(cd => cd.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CustomerDevice>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(cd => cd.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ServiceRequest>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Device)
                .WithMany()
                .HasForeignKey(sr => sr.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InstallationRequest>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(ir => ir.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Invoice>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Feedback>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Feedback>()
                .HasOne(f => f.ServiceRequest)
                .WithOne(sr => sr.Feedback)
                .HasForeignKey<Feedback>(f => f.ServiceRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ServiceRequestImage>()
                .HasOne(sri => sri.ServiceRequest)
                .WithMany(sr => sr.Images)
                .HasForeignKey(sri => sri.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InstallationRequest>()
                .HasOne(ir => ir.Device)
                .WithMany(d => d.Requests)
                .HasForeignKey(ir => ir.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.SparePart)
                .WithMany(sp => sp.OrderItems)
                .HasForeignKey(oi => oi.SparePartId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JSON conversion for Notification.Data with comparer
            builder.Entity<Notification>()
                .Property(n => n.Data)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Dictionary<string, object>?>(
                    (c1, c2) => c1 == null && c2 == null || (c1 != null && c2 != null && c1.Count == c2.Count && c1.All(kvp => c2.ContainsKey(kvp.Key) && Equals(kvp.Value, c2[kvp.Key]))),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value == null ? 0 : v.Value.GetHashCode())),
                    c => c == null ? null : new Dictionary<string, object>(c)));
        }
    }
}