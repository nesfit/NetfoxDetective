using AutoMapper;
using Netfox.Snoopers.SnooperFTP.Models;
using Netfox.Snoopers.SnooperFTP.WEB.DTO;
using Netfox.Web.BL.Mappings;

namespace Netfox.Snoopers.SnooperFTP.WEB.Mapping
{
    public class SnooperFTPMapping : IMapping
    {
        public void Configure(IMapperConfigurationExpression mapper) { mapper.CreateMap<SnooperExportedDataObjectFTP, SnooperFTPListDTO>(); }
    }
}
