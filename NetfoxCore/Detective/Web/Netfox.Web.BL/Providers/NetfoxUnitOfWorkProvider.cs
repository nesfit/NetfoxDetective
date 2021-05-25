using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using Castle.Windsor;
using Netfox.Persistence;
using UnitOfWork.EF6UnitOfWork;
using Netfox.Core.Database;
using Netfox.Web.App.Settings;

namespace Netfox.Web.BL.Providers
{
    public class NetfoxUnitOfWorkProvider : INetfoxProvider
    {
        private Func<IWindsorContainer, SqlConnectionStringBuilder, NetfoxDbContext> dbContextFactory { get; set; }

        private IWindsorContainer container { get; set; }

        private EF6UnitOfWork unitOfWork { get; set; }

        private readonly INetfoxWebSettings _settings;

        public NetfoxUnitOfWorkProvider(INetfoxWebSettings settings, Func<IWindsorContainer, SqlConnectionStringBuilder, NetfoxDbContext> dbContextFactory, IWindsorContainer container)
        {
            _settings = settings;
            this.dbContextFactory = dbContextFactory;
            this.container = container;
        }

        public EF6UnitOfWork Create(Guid id)
        {

            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_settings.ConnectionString)
            {
                InitialCatalog = _settings.InvestigationPrefix + id
            };

            
            var observableNetfoxDBContext = this.container.Resolve<IObservableNetfoxDBContext>();

            if(observableNetfoxDBContext.Database.Connection.State == ConnectionState.Open)
            {
                observableNetfoxDBContext.Database.Connection.Close();
            }
            var connection = new SqlConnectionStringBuilder(observableNetfoxDBContext.Database.Connection.ConnectionString);
            connection.InitialCatalog = _settings.InvestigationPrefix + id;
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
