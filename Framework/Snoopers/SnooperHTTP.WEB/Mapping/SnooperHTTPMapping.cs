using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Netfox.SnooperHTTP.Models;
using Netfox.SnooperHTTP.WEB.DTO;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Mappings;
using Newtonsoft.Json;

namespace Netfox.SnooperHTTP.WEB.Mapping
{
    public class SnooperHTTPMapping : IMapping
    {
        public void Configure(IMapperConfigurationExpression mapper)
        {
            mapper.CreateMap<SnooperExportedDataObjectHTTP, SnooperHTTPFileDTO>()
                .ForMember(i => i.SourceEndPoint, m => m.MapFrom(e => e.SourceEndpointString))
                .ForMember(i => i.DestinationEndPoint, m => m.MapFrom(e => e.DestinationEndpointString))
                .ForMember(i => i.FrameGuids, m => m.MapFrom(e => e.Message.FrameGuids.ToList()))
                .ForMember(i => i.StatusLine, m => m.MapFrom(e => e.Message.HTTPHeader.StatusLine))
                .ForMember(i => i.ContentType, m => m.MapFrom(e => (e.Message.HTTPHeader.Fields.ContainsKey("Content-Type") ? e.Message.HTTPHeader.Fields["Content-Type"].First() : "")))
                .ForMember(i => i.Size, m => m.MapFrom(e => e.Message.HTTPContent.Content.Length))
                .ForMember(i => i.Path, m => m.Ignore())
                .ForMember(i => i.Url, m => m.Ignore());

            mapper.CreateMap<SnooperExportedDataObjectHTTP, SnooperHTTPDetailDTO>()
                .ForMember(i => i.Info, m => m.MapFrom(e => Mapper.Map<SnooperHTTPListDTO>(e)) )
                .ForMember(i => i.Header, m => m.MapFrom(e => e.Message.HTTPHeader.ToString()))
                .ForMember(i => i.Content, m => m.MapFrom(e => e.Message.HTTPContent.ToString()));

            mapper.CreateMap<SnooperExportedDataObjectHTTP, SnooperHTTPListDTO>()
                .ForMember(i => i.SourceEndPoint, m => m.MapFrom(e => e.SourceEndpointString))
                .ForMember(i => i.DestinationEndPoint, m => m.MapFrom(e => e.DestinationEndpointString))
                .ForMember(i => i.MessageType, m => m.MapFrom(e => e.Message.MessageType))
                .ForMember(i => i.StatusLine, m => m.MapFrom(e => e.Message.HTTPHeader.StatusLine));
        }
    }
}
