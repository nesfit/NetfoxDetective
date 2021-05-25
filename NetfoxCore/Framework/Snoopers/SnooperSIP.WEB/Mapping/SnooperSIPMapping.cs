using System.Linq;
using AutoMapper;
using Netfox.Snoopers.SnooperSIP.Models;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Mappings;

namespace Netfox.Snoopers.SnooperSIP.WEB.Mapping
{
    public class SnooperSIPMapping : IMapping
    {
        public void Configure(IMapperConfigurationExpression mapper)
        {
            mapper.CreateMap<SIPCall, ExportCallDetailDTO>()
                .ForMember(i => i.Id, m => m.MapFrom(e => e.Id))
                .ForMember(i => i.CallId, m => m.MapFrom(e => e.CallId))
                .ForMember(i => i.Start, m => m.MapFrom(e => e.Start))
                .ForMember(i => i.End, m => m.MapFrom(e => e.End))
                .ForMember(i => i.RTPAddress, m => m.MapFrom(e => e.RTPAddressesString.ToList()))
                .ForMember(i => i.Duration, m => m.MapFrom(e => e.Duration))
                .ForMember(i => i.DurationText, m => m.Ignore())
                .ForMember(i => i.From, m => m.MapFrom(e => e.From))
                .ForMember(i => i.To, m => m.MapFrom(e => e.To));
        }
    }
}
