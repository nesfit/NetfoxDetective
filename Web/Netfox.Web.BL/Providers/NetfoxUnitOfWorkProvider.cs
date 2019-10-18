using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Netfox.Persistence;
using Netfox.Web.DAL.Properties;
using UnitOfWork;
using UnitOfWork.EF6UnitOfWork;
using System.Configuration;
using Netfox.Core.Database;

namespace Netfox.Web.BL.Providers
{
    public class NetfoxUnitOfWorkProvider : INetfoxProvider
    {
        private Func<IWindsorContainer, SqlConnectionStringBuilder, NetfoxDbContext> dbContextFactory { get; set; }

        private IWindsorContainer container { get; set; }

        private EF6UnitOfWork unitOfWork { get; set; }


        public NetfoxUnitOfWorkProvider(Func<IWindsorContainer, SqlConnectionStringBuilder, NetfoxDbContext> dbContextFactory, IWindsorContainer container)
        {
            this.dbContextFactory = dbContextFactory;
            this.container = container;
        }

        public EF6UnitOfWork Create(Guid id)
        {

            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(NetfoxWebSettings.Default.ConnectionString)
            {
                InitialCatalog = NetfoxWebSettings.Default.InvestigationPrefix + id
            };

            
            var observableNetfoxDBContext = this.container.Resolve<IObservableNetfoxDBContext>();

            if(observableNetfoxDBContext.Database.Connection.State == ConnectionState.Open)
            {
                observableNetfoxDBContext.Database.Connection.Close();
            }
            var connection = new SqlConnectionStringBuilder(observableNetfoxDBContext.Database.Connection.ConnectionString);
            connection.InitialCatalog = NetfoxWebSettings.Default.InvestigationPrefix + id;
            connection.Password = sqlConnectionStringBuilder.Password;
            connection.UserID = sqlConnectionStringBuilder.UserID;
            observableNetfoxDBContext.Database.Connection.ConnectionString = connection.ConnectionString;
            observableNetfoxDBContext.Database.CreateIfNotExists();
            this.unitOfWork = new EF6UnitOfWork(dbContextFactory(this.container, sqlConnectionStringBuilder), IsolationLevel.Unspecified);
            return this.unitOfWork;
        }

        public DbContext TryGetContext() { return this.unitOfWork.DbContext; }
    }
    
}
