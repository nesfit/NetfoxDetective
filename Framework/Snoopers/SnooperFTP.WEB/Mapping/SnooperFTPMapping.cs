using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Netfox.SnooperFTP.Models;
using Netfox.SnooperFTP.WEB.DTO;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Mappings;

namespace Netfox.SnooperFTP.WEB.Mapping
{
    public class SnooperFTPMapping : IMapping
    {
        public void Configure(IMapperConfigurationExpression mapper) { mapper.CreateMap<SnooperExportedDataObjectFTP, SnooperFTPListDTO>(); }
    }
}
