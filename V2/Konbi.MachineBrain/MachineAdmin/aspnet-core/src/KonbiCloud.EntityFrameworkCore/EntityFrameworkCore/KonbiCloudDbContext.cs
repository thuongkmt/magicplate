using KonbiCloud.Plate;
using Abp.IdentityServer4;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KonbiCloud.Authorization.Roles;
using KonbiCloud.Authorization.Users;
using KonbiCloud.Chat;
using KonbiCloud.Editions;
using KonbiCloud.Friendships;
using KonbiCloud.MultiTenancy;
using KonbiCloud.MultiTenancy.Accounting;
using KonbiCloud.MultiTenancy.Payments;
using KonbiCloud.Products;
using KonbiCloud.Storage;
using KonbiCloud.Prices;
using KonbiCloud.Sessions;
using KonbiCloud.SignalR;
using KonbiCloud.Transactions;
using KonbiCloud.Services;

namespace KonbiCloud.EntityFrameworkCore
{
    public class KonbiCloudDbContext : AbpZeroDbContext<Tenant, Role, User, KonbiCloudDbContext>, IAbpPersistedGrantDbContext
    {

        public virtual DbSet<Disc> Discs { get; set; }

        public virtual DbSet<Plate.Plate> Plates { get; set; }

        public virtual DbSet<PlateCategory> PlateCategories { get; set; }

        /* Define an IDbSet for each entity of the application */

        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

        public virtual DbSet<Friendship> Friendships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }
        public virtual DbSet<GeneralMessage> GeneralMessages { get; set; }

        public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        //public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<ProductMenu.ProductMenu> ProductMenus { get; set; }
        public DbSet<PriceStrategyCode> PriceStrategyCodes { get; set; }
        public DbSet<PriceStrategy> PriceStrategies { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public DbSet<DetailTransaction> Transactions { get; set; }
        public DbSet<ProductTransaction> ProductTransactions { get; set; }
        public DbSet<CashlessDetail> CashlessDetails { get; set; }
        public DbSet<CashDetail> CashDetails { get; set; }
        public DbSet<Service> ServiceStatuses { get; set; }

        public KonbiCloudDbContext(DbContextOptions<KonbiCloudDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductMenu.ProductMenu>(e =>
            {
                e.HasIndex(f => new { f.SelectedDate });
            });
            modelBuilder.Entity<Product>(e =>
            {
                e.HasIndex(f => new { f.SKU });
            });

            modelBuilder.Entity<Disc>(D =>
            {
                D.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<Plate.Plate>(P =>
                       {
                           P.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PlateCategory>(P =>
                       {
                           P.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BinaryObject>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
            });

            modelBuilder.Entity<Friendship>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId });
                b.HasIndex(e => new { e.TenantId, e.FriendUserId });
                b.HasIndex(e => new { e.FriendTenantId, e.UserId });
                b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
            });

            modelBuilder.Entity<Tenant>(b =>
            {
                b.HasIndex(e => new { e.SubscriptionEndDateUtc });
                b.HasIndex(e => new { e.CreationTime });
            });

            modelBuilder.Entity<SubscriptionPayment>(b =>
            {
                b.HasIndex(e => new { e.Status, e.CreationTime });
                b.HasIndex(e => new { e.PaymentId, e.Gateway });
            });

            //modelBuilder.Entity<Service>().HasData(
            //    new Service { Id = 1, Name = "MagicPlate Server", Type = "MagicPlateServer.Service", IsArchived = false ,TenantId = 1},
            //    new Service { Id = 2, Name = "RabbitMq Server", Type = "RabbitMqServer.Service", IsArchived = false, TenantId = 1 },
            //    new Service { Id = 3, Name = "Tag reader", Type = "TagReader.Service", IsArchived = false, TenantId = 1 },
            //    new Service { Id = 4, Name = "LightAlarm", Type = "LightAlarm.Service", IsArchived = false, TenantId = 1 },
            //    new Service { Id = 5, Name = "Camera", Type = "Camera.Service", IsArchived = false, TenantId = 1 },
            //    new Service { Id = 6, Name = " Payment Controller", Type = "PaymentController.Service", IsArchived = false, TenantId = 1 }
            //);

            modelBuilder.ConfigurePersistedGrantEntity();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
