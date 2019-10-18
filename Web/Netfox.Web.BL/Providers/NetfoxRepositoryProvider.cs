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
using UnitOfWork;
using UnitOfWork.BaseDataEntity;
using UnitOfWork.EF6UnitOfWork;
using UnitOfWork.Repository;
using UnitOfWork.EF6Repository;


namespace Netfox.Web.BL.Providers
{
    public class NetfoxRepositoryProvider : INetfoxProvider
    {
        private NetfoxUnitOfWorkProvider UnitOfWorkProvider { get; set; }

        private IUnitOfWork UniteOfWork { get; set; }

        public NetfoxRepositoryProvider(NetfoxUnitOfWorkProvider uowProvider)
        {
            this.UnitOfWorkProvider = uowProvider;
            this.UniteOfWork = this.UniteOfWork;
        }

        public BaseRepository<T> Create<T>(Guid id) where T : class, IDataEntity
        {
            if(this.UniteOfWork == null) { this.UniteOfWork = this.UnitOfWorkProvider.Create(id); }
            return new BaseRepository<T>(this.UniteOfWork);
        }

        public BaseRepository<T> Create<T>(Guid id, IUnitOfWork unitOfWork) where T : class, IDataEntity
        {
            if (this.UniteOfWork == null)
            {
                return new BaseRepository<T>(unitOfWork);
            }

            return null;
        }
    }
    
}
