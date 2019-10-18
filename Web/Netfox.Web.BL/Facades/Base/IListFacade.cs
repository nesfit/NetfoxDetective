using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace Netfox.Web.BL.Facades.Base
{
    public interface IListFacade<TDTO, TKey>
    {
        void FillDataSet(GridViewDataSet<TDTO> items);

        void Delete(TKey id);
    }
}
