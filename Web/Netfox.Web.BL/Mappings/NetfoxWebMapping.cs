using System;
using System.Collections.Generic;
using AutoMapper;
using Castle.Core.Internal;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Mappings;
using Netfox.Web.DAL.Entities;
using Newtonsoft.Json;

namespace Netfox.Web.Mappings
{
    public class NetfoxWebMapping : IMapping
    {
        public void Configure(IMapperConfigurationExpression mapper)
        {
            mapper.CreateMap<Investigation, InvestigationDTO>()
                .ForMember(i => i.CanEditRemove, m => m.Ignore())
                .ForMember(i => i.LastAccess, m => m.Ignore())
                .ForMember(i => i.Jobs, m => m.Ignore())
                .ForMember(i => i.Owner, m => m.Ignore());
            mapper.CreateMap<InvestigationDTO, Investigation>()
                .ForMember(i => i.Stats, m => m.Ignore());

            mapper.CreateMap<UserInvestigation, InvestigationDTO>()
                .ForMember(i => i.Id, m => m.MapFrom(ui => ui.Investigation.Id))
                .ForMember(i => i.Created, m => m.MapFrom(ui => ui.Investigation.Created))
                .ForMember(i => i.OwnerID, m => m.MapFrom(ui => ui.Investigation.OwnerID))
                .ForMember(i => i.Jobs, m => m.Ignore())
                .ForMember(i => i.JobsSerialized, m => m.MapFrom(ui => ui.Investigation.JobsSerialized))
                .ForMember(i => i.CanEditRemove, m => m.Ignore())
                .ForMember(i => i.LastAccess, m => m.MapFrom(ui => ui.LastAccess))
                .ForMember(i => i.Name, m => m.MapFrom(ui => ui.Investigation.Name))
                .ForMember(i => i.Description, m => m.MapFrom(ui => ui.Investigation.Description))
                .ForMember(i => i.Owner, m => m.Ignore())
                .ForMember(i => i.UserInvestigations, m => m.Ignore())
                .ForMember(i => i.Stats, m => m.Ignore())
                .ForMember(i => i.ExportStats, m => m.Ignore());

            mapper.CreateMap<UserInvestigation, UserInvestigationDTO>();
            mapper.CreateMap<UserInvestigationDTO, UserInvestigation>()
                .ForMember(u => u.Investigation, m => m.Ignore());

            mapper.CreateMap<User, UserDTO>();
            mapper.CreateMap<UserDTO, User>()
                .ForMember(u => u.Password, m => m.Ignore())
                .ForMember(u => u.UserInvestigations, m => m.Ignore())
                .ForMember(u => u.Role, m => m.Ignore());

            mapper.CreateMap<CaptureStats, ConvesationStatisticsDTO>()
                .ForMember(c => c.AppProtocolsSummaryJSON, m => m.MapFrom( s => s.AppProtocolsSummary))
                .ForMember(c => c.TransportProtocolsSummaryJSON, m => m.MapFrom( s => s.TransportProtocolsSummary))
                .ForMember(c => c.ExportedProtocolsJSON, m => m.MapFrom( s => s.ExportedProtocols))
                .ForMember(c => c.AppProtocolsDistribution, m => m.Ignore())
                .ForMember(c => c.AppProtocolsSummary, m => m.Ignore())
                .ForMember(c => c.ExportedProtocols, m => m.Ignore())
                .ForMember(c => c.TransportProtocolsSummary, m => m.Ignore())
                .ForMember(c => c.TransportProtocolsDistribution, m => m.Ignore())
                .ForMember(c => c.LinkProtocolsDistribution, m => m.Ignore()).ForMember(c => c.LinkProtocolsSummary, m => m.Ignore())
                .ForMember(c => c.TrafficErrors, m => m.Ignore()).ForMember(c => c.TrafficHistory, m => m.Ignore());
            mapper.CreateMap<ConvesationStatisticsDTO, CaptureStats>()
                .ForMember(c => c.Id, m => m.Ignore())
                .ForMember(c => c.Name, m => m.Ignore())
                .ForMember(c => c.Investigation, m => m.Ignore())
                
                .ForMember(c => c.AppProtocolsDistribution, m => m.MapFrom(s => JsonConvert.SerializeObject(s.AppProtocolsDistribution)))
                .ForMember(c => c.AppProtocolsSummary, m => m.MapFrom(s => JsonConvert.SerializeObject(s.AppProtocolsSummary)))
                .ForMember(c => c.ExportedProtocols, m => m.MapFrom(s => JsonConvert.SerializeObject(s.ExportedProtocols)))
                .ForMember(c => c.TransportProtocolsDistribution, m => m.MapFrom(s => JsonConvert.SerializeObject(s.TransportProtocolsDistribution)))
                .ForMember(c => c.TransportProtocolsSummary, m => m.MapFrom(s => JsonConvert.SerializeObject(s.TransportProtocolsSummary)))
                .ForMember(c => c.LinkProtocolsDistribution, m => m.MapFrom(s => JsonConvert.SerializeObject(s.LinkProtocolsDistribution)))
                .ForMember(c => c.LinkProtocolsSummary, m => m.MapFrom(s => JsonConvert.SerializeObject(s.LinkProtocolsSummary)))
                .ForMember(c => c.TrafficErrors, m => m.MapFrom(s => JsonConvert.SerializeObject(s.TrafficErrors)))
                .ForMember(c => c.TrafficHistory, m => m.MapFrom(s => JsonConvert.SerializeObject(s.TrafficHistory)));

            mapper.CreateMap<ExportStatisticsDTO, ExportStats>().ForMember(c => c.Id, m => m.Ignore());
        }
    }
}