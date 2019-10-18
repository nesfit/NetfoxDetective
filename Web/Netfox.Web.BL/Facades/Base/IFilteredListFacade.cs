using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace Netfox.Web.BL.Facades.Base
{
    public interface IFilteredListFacade<TDTO, TKey, TFilterDTO>
    {
        void FillDataSet(GridViewDataSet<TDTO> items, TFilterDTO filter);

        void Delete(TKey id);
    }
}
