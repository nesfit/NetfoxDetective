using AutoMapper;
using Netfox.Snoopers.SnooperDNS.Models;
using Netfox.Snoopers.SnooperDNS.WEB.DTO;
using Netfox.Web.BL.Mappings;

namespace Netfox.Snoopers.SnooperDNS.WEB.Mapping
{
    public class SnooperDNSMapping : IMapping
    {
        public void Configure(IMapperConfigurationExpression mapper)
        {

            mapper.CreateMap<SnooperExportedDataObjectDNS, SnooperDNSDetailDTO>()
                .ForMember(i => i.Info, m => m.MapFrom(e => Mapper.Map<SnooperDNSListDTO>(e)))
                .ForMember(i => i.Answer, m => m.MapFrom(e => e.Answer))
                .ForMember(i => i.Queries, m => m.MapFrom(e => e.Queries))
                .ForMember(i => i.Authority, m => m.MapFrom(e => e.Authority))
                .ForMember(i => i.Additional, m => m.MapFrom(e => e.Additional));

            mapper.CreateMap<SnooperExportedDataObjectDNS, SnooperDNSListDTO>()
                .ForMember(i => i.SourceEndPoint, m => m.MapFrom(e => e.SourceEndpointString))
                .ForMember(i => i.DestinationEndPoint, m => m.MapFrom(e => e.DestinationEndpointString))
                .ForMember(i => i.QueryType, m => m.MapFrom(e => e.TypeQuery.ToString()))
                .ForMember(i => i.MessageId, m => m.MapFrom(e => e.__MessageId))
                .ForMember(i => i.Flags, m => m.MapFrom(e => e.__Flags));
        }
    }
}
