using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers.Email;
using Netfox.Web.BL.DTO;

namespace Netfox.Web.BL.Mappings
{
    public class NetfoxEntityMapping : IMapping
    {
        public void Configure(IMapperConfigurationExpression mapper)
        {

            mapper.CreateMap<PmFrameBase, PmFrameBaseDTO>()
                .ForMember(c => c.SourceEndPoint, m => m.MapFrom(s => s.SourceEndPoint.ToString()))
                .ForMember(c => c.DestinationEndPoint, m => m.MapFrom(s => s.DestinationEndPoint.ToString()))
                .ForMember(c => c.IpProtocol, m => m.MapFrom(s => s.IpProtocol.ToString()));
            mapper.CreateMap<PmFrameBase, PmFrameBaseDetailDTO>()
                .ForMember(c => c.SourceEndPoint, m => m.MapFrom(s => s.SourceEndPoint.ToString()))
                .ForMember(c => c.DestinationEndPoint, m => m.MapFrom(s => s.DestinationEndPoint.ToString()))
                .ForMember(c => c.IpProtocol, m => m.MapFrom(s => s.IpProtocol.ToString()))
                .ForMember(c => c.L3Conversation, m => m.MapFrom(s => Mapper.Map<L3ConversationDTO>(s.L3Conversation)))
                .ForMember(c => c.L4Conversation, m => m.MapFrom(s => Mapper.Map<L4ConversationDTO>(s.L4Conversation)))
                .ForMember(c => c.L7Conversation, m => m.MapFrom(s => Mapper.Map<L7ConversationDTO>(s.L7Conversation)))
                .ForMember(c => c.Data, m => m.MapFrom(s => s.L2Data()));
            mapper.CreateMap<L3Conversation, L3ConversationDTO>()
                .ForMember(c => c.IPAddress1, m => m.MapFrom(s => s.IPAddress1.ToString()))
                .ForMember(c => c.IPAddress2, m => m.MapFrom(s => s.IPAddress2.ToString())).ForMember(c => c.Transport, m => m.MapFrom(s => s.ProtocolType.ToString()))
                .ForMember(c => c.UpFlowFramesCount, m => m.MapFrom(s => s.ConversationFlowStatistics.FirstOrDefault(sq => sq.FlowDirection == DaRFlowDirection.up).Frames))
                .ForMember(c => c.DownFlowFramesCount, m => m.MapFrom(s => (long) s.ConversationFlowStatistics.FirstOrDefault(sq => sq.FlowDirection == DaRFlowDirection.down).Frames))
                .ForMember(c => c.UpFlowBytes, m => m.MapFrom(s => s.ConversationFlowStatistics.FirstOrDefault(sq => sq.FlowDirection == DaRFlowDirection.up).Bytes))
                .ForMember(c => c.DownFlowBytes, m => m.MapFrom(s => s.ConversationFlowStatistics.FirstOrDefault(sq => sq.FlowDirection == DaRFlowDirection.down).Bytes))
                .ForMember(c => c.MalformedFrames, m => m.MapFrom(s => s.ConversationFlowStatistics.Sum(sq => sq.MalformedFrames)));
                //.ForMember(c => c.UpFlowFramesCount, m => m.MapFrom(s => s.UpConversationStatistic.Frames))
                //.ForMember(c => c.DownFlowFramesCount, m => m.MapFrom(s => s.DownConversationStatistic.Frames))
                //.ForMember(c => c.UpFlowBytes, m => m.MapFrom(s => s.UpConversationStatistic.Bytes))
                //.ForMember(c => c.DownFlowBytes, m => m.MapFrom(s => s.DownConversationStatistic.Bytes))
                //.ForMember(c => c.MalformedFrames, m => m.MapFrom(s => s.ConversationStats.MalformedFrames));
            mapper.CreateMap<L4Conversation, L4ConversationDTO>().ForMember(c => c.SourceEndPoint, m => m.MapFrom(s => s.SourceEndPoint.ToString()))
                .ForMember(c => c.DestinationEndPoint, m => m.MapFrom(s => s.DestinationEndPoint.ToString()))
                .ForMember(c => c.Transport, m => m.MapFrom(s => s.ProtocolType.ToString()))
                .ForMember(c => c.UpFlowFramesCount, m => m.MapFrom(s => s.UpConversationStatistic.Frames))
                .ForMember(c => c.DownFlowFramesCount, m => m.MapFrom(s => s.DownConversationStatistic.Frames))
                .ForMember(c => c.UpFlowBytes, m => m.MapFrom(s => s.UpConversationStatistic.Bytes))
                .ForMember(c => c.DownFlowBytes, m => m.MapFrom(s => s.DownConversationStatistic.Bytes))
                .ForMember(c => c.MalformedFrames, m => m.MapFrom(s => s.ConversationStats.MalformedFrames));
            mapper.CreateMap<L4Conversation, L4ConversationDetailDTO>().ForMember(c => c.SourceEndPoint,
                    m => m.MapFrom(s => s.SourceEndPoint.ToString()))
                .ForMember(c => c.DestinationEndPoint, m => m.MapFrom(s => s.DestinationEndPoint.ToString()))
                .ForMember(c => c.Transport, m => m.MapFrom(s => s.ProtocolType.ToString()))
                .ForMember(c => c.UpFlowFramesCount, m => m.MapFrom(s => s.UpConversationStatistic.Frames))
                .ForMember(c => c.DownFlowFramesCount, m => m.MapFrom(s => s.DownConversationStatistic.Frames))
                .ForMember(c => c.UpFlowBytes, m => m.MapFrom(s => s.UpConversationStatistic.Bytes))
                .ForMember(c => c.DownFlowBytes, m => m.MapFrom(s => s.DownConversationStatistic.Bytes))
                .ForMember(c => c.MalformedFrames, m => m.MapFrom(s => s.ConversationStats.MalformedFrames))
                .ForMember(c => c.L3Conversation, m => m.MapFrom(s => Mapper.Map<L3ConversationDTO>(s.L3Conversation)));
            mapper.CreateMap<L7Conversation, L7ConversationDTO>().ForMember(c => c.SourceEndPoint, m => m.MapFrom(s => s.SourceEndPoint.ToString()))
                .ForMember(c => c.DestinationEndPoint, m => m.MapFrom(s => s.DestinationEndPoint.ToString()))
                .ForMember(c => c.Transport, m => m.Ignore()).ForMember(c => c.Application, m => m.Ignore())
                .ForMember(c => c.Transport, m => m.MapFrom(s => s.ProtocolType.ToString()))
                .ForMember(c => c.Application, m => m.MapFrom(s => Regex.Replace(s.ApplicationTags.SerializedValue, "[@\\[\\]\\.\";'\\\\]", string.Empty)))
                .ForMember(c => c.UpFlowFramesCount, m => m.MapFrom(s => s.UpConversationStatistic.Frames))
                .ForMember(c => c.DownFlowFramesCount, m => m.MapFrom(s => s.DownConversationStatistic.Frames))
                .ForMember(c => c.UpFlowBytes, m => m.MapFrom(s => s.UpConversationStatistic.Bytes))
                .ForMember(c => c.DownFlowBytes, m => m.MapFrom(s => s.DownConversationStatistic.Bytes))
                .ForMember(c => c.MalformedFrames, m => m.MapFrom(s => s.ConversationStats.MalformedFrames))
                .ForMember(c => c.ExtractedBytes, m => m.MapFrom(s => (s.ConversationStats as L7ConversationStatistics) == null? 0 : ((L7ConversationStatistics) s.ConversationStats).ExtractedBytes))
                .ForMember(c => c.MissingBytes, m => m.MapFrom(s => (s.ConversationStats as L7ConversationStatistics) == null? 0 : ((L7ConversationStatistics) s.ConversationStats).MissingBytes))
                .ForMember(c => c.MissingFrames, m => m.MapFrom(s => (s.ConversationStats as L7ConversationStatistics) == null? 0 : ((L7ConversationStatistics) s.ConversationStats).MissingFrames));
            mapper.CreateMap<L7Conversation, L7ConversationDetailDTO>().ForMember(c => c.SourceEndPoint,
                    m => m.MapFrom(s => s.SourceEndPoint.ToString()))
                .ForMember(c => c.DestinationEndPoint, m => m.MapFrom(s => s.DestinationEndPoint.ToString()))
                .ForMember(c => c.Transport, m => m.Ignore()).ForMember(c => c.Application, m => m.Ignore())
                .ForMember(c => c.Transport, m => m.MapFrom(s => s.ProtocolType.ToString()))
                .ForMember(c => c.Application,
                    m => m.MapFrom(s =>
                        Regex.Replace(s.ApplicationTags.SerializedValue, "[@\\[\\]\\.\";'\\\\]", string.Empty)))
                .ForMember(c => c.UpFlowFramesCount, m => m.MapFrom(s => s.UpConversationStatistic.Frames))
                .ForMember(c => c.DownFlowFramesCount, m => m.MapFrom(s => s.DownConversationStatistic.Frames))
                .ForMember(c => c.UpFlowBytes, m => m.MapFrom(s => s.UpConversationStatistic.Bytes))
                .ForMember(c => c.DownFlowBytes, m => m.MapFrom(s => s.DownConversationStatistic.Bytes))
                .ForMember(c => c.MalformedFrames, m => m.MapFrom(s => s.ConversationStats.MalformedFrames))
                .ForMember(c => c.ExtractedBytes,
                    m => m.MapFrom(s =>
                        (s.ConversationStats as L7ConversationStatistics) == null
                            ? 0
                            : ((L7ConversationStatistics) s.ConversationStats).ExtractedBytes))
                .ForMember(c => c.MissingBytes,
                    m => m.MapFrom(s =>
                        (s.ConversationStats as L7ConversationStatistics) == null
                            ? 0
                            : ((L7ConversationStatistics) s.ConversationStats).MissingBytes))
                .ForMember(c => c.MissingFrames,
                    m => m.MapFrom(s =>
                        (s.ConversationStats as L7ConversationStatistics) == null
                            ? 0
                            : ((L7ConversationStatistics) s.ConversationStats).MissingFrames))
                .ForMember(c => c.L3Conversation, m => m.MapFrom(s => Mapper.Map<L3ConversationDTO>(s.L3Conversation)))
                .ForMember(c => c.L4Conversation, m => m.MapFrom(s => Mapper.Map<L4ConversationDTO>(s.L4Conversation)));
            mapper.CreateMap<MIMEpart, ExportEmailDTO>()
                .ForMember(c => c.Timestamp, m => m.Ignore())
                .ForMember(c => c.Source, m => m.Ignore())
                .ForMember(c => c.Destination, m => m.Ignore());
            mapper.CreateMap<MIMEpart, EmailAttachmentDTO>();
            mapper.CreateMap<IChatMessage, ExportChatMessageDTO>();
            mapper.CreateMap<ICall, ExportCallDTO>()
                .ForMember(i => i.DurationText, i => i.MapFrom(j => j.Duration.HasValue ? j.Duration.Value.ToString("g") : null));
            mapper.CreateMap<ICallStream, ExportCallStreamDTO>()
                .ForMember(i => i.Duration, m => m.Ignore())
                .ForMember(i => i.To, m => m.Ignore())
                .ForMember(i => i.End, m => m.Ignore())
                .ForMember(i => i.From, m => m.Ignore())
                .ForMember(i => i.Id, m => m.Ignore())
                .ForMember(i => i.Start, m => m.Ignore())
                .ForMember(i => i.FirstSeen, m => m.Ignore())
                .ForMember(i => i.DurationText, m => m.Ignore());
        }
    }
}