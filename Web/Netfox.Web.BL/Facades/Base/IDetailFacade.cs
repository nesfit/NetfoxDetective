using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.Web.BL.Facades.Base
{
    public interface IDetailFacade<TDTO, TKey>
    {
        TDTO GetDetail(TKey id);
        TDTO InitializeNew();
        TDTO Save(TDTO item);
    }
}
