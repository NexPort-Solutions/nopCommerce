using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class NexportPluginObjectContext : DbContext, IDbContext
    {
        public NexportPluginObjectContext(DbContextOptions<NexportPluginObjectContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new NexportProductMappingMap());
            modelBuilder.ApplyConfiguration(new NexportProductGroupMembershipMappingMap());
            modelBuilder.ApplyConfiguration(new NexportProductStoreMappingMap());
            modelBuilder.ApplyConfiguration(new NexportOrderProcessingQueueMap());
            modelBuilder.ApplyConfiguration(new NexportUserMappingMap());
            modelBuilder.ApplyConfiguration(new NexportOrderInvoiceItemMap());
            modelBuilder.ApplyConfiguration(new NexportOrderInvoiceRedemptionQueueMap());

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Install object context
        /// </summary>
        public void Install()
        {
            // Save new changes to database
            SaveChanges();
        }

        /// <summary>
        /// Uninstall object context
        /// </summary>
        public void Uninstall()
        {
            // Drop the table
            this.DropPluginTable(nameof(NexportProductMapping));
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public string GenerateCreateScript()
        {
            return Database.GenerateCreateScript();
        }

        public IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        public int ExecuteSqlCommand(RawSqlString sql, bool doNotEnsureTransaction = false, int? timeout = null,
            params object[] parameters)
        {
            using (var transaction = Database.BeginTransaction())
            {
                var result = Database.ExecuteSqlCommand(sql, parameters);
                transaction.Commit();

                return result;
            }
        }

        public void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        public IQueryable<TQuery> QueryFromSql<TQuery>(string sql, params object[] parameters) where TQuery : class
        {
            throw new NotImplementedException();
        }
    }
}
