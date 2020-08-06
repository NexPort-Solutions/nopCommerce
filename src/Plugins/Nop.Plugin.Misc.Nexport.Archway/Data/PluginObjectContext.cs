using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Data;
using Nop.Data.Extensions;

namespace Nop.Plugin.Misc.Nexport.Archway.Data
{
    public class PluginObjectContext : DbContext, IDbContext
    {
        public PluginObjectContext(DbContextOptions<PluginObjectContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ArchwayStoreRecordInfoMap());
            modelBuilder.ApplyConfiguration(new ArchwayStoreEmployeePositionMap());
            modelBuilder.ApplyConfiguration(new ArchwayStudentRegistrationFieldKeyMappingMap());
            modelBuilder.ApplyConfiguration(new ArchwayStudentRegistrationFieldAnswerMap());

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
            this.DropPluginTable("ArchwayStore");
            this.DropPluginTable("ArchwayStoreEmployeePosition");
            this.DropPluginTable("ArchwayStudentRegistrationFieldKeyMapping");
            this.DropPluginTable("ArchwayStudentRegistrationFieldAnswer");
        }

        protected virtual string CreateSqlWithParameters(string sql, params object[] parameters)
        {
            //add parameters to sql
            for (var i = 0; i <= (parameters?.Length ?? 0) - 1; i++)
            {
                if (!(parameters[i] is DbParameter parameter))
                    continue;

                sql = $"{sql}{(i > 0 ? "," : string.Empty)} @{parameter.ParameterName}";

                //whether parameter is output
                if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
                    sql = $"{sql} output";
            }

            return sql;
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
            return Query<TQuery>().FromSql(CreateSqlWithParameters(sql, parameters), parameters);
        }
    }
}
