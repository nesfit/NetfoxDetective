/* 
thrift -gen csharp MessengerProtocol.thrift
*/

namespace csharp Netfox.SnooperMessenger.Protocol

struct Coordinates {
	1: string Latitude
	2: string Longitude
	3: string Accuracy
}

struct LocationAttachement {
	1: Coordinates Coordinates
	2: bool IsCurrentLocation
	3: i64 PlaceID
}

struct SendMessageRequest {
	1: string To
	2: string Body
	3: i64 OfflineThreadingID
	4: Coordinates Coordinates
	5: map<string,string> ClientTags
	6: string ObjectAttachement
	7: string CopyMessageID
	8: string CopyAttachementID
	9: list<string> MediaAttachementsID
	10: string FBTraceMeta
	11: i32 ImageType
	12: i64 SenderFBID
	13: map<string,string> BroadcastRecipients
	14: i64 AtributionAppID
	15: string iOSBundleID
	16: string AndroidKeyHash
	17: LocationAttachement LocationAttachement
}

struct ClientInfo {
	1: i64 UserId
    2: string UserAgent
    3: i64 ClientCapabilities
    4: i64 EndpointCapabilities
    5: i32 PublishFormat
    6: bool NoAutomaticForeground
    7: bool MakeUserAvailableInForeground
    8: string DeviceId
    9: bool IsInitiallyForeground
    10: i32 NetworkType
    11: i32 NetworkSubtype
    12: i64 ClientMqttSessionId
    13: string ClientIpAddress
    14: list<i32> SubscribeTopics
    15: string ClientType
    16: i64 AppId
    17: bool  OverrideNectarLogging
    18: binary ConnectTokenHash
    19: string RegionPreference
    20: string DeviceSecret
    21: byte ClientStack
}

struct ConnectMessage {
	1: string ClientIdentifier
    2: string WillTopic
    3: string WillMessage
    4: ClientInfo ClientInfo
    5: string Password
    6: list<string> GetDiffsRequests
}

struct MNMessagesSyncDeltaNoOp {
    1: i32 NumNoOps
}


struct MNMessagesSyncThreadKey {
    1: i64 OtherUserFbId
    2: i64 ThreadFbId
}

struct MNMessagesSyncMessageMetadata {
    1: MNMessagesSyncThreadKey ThreadKey
    2: string MessageId
    3: i64 OfflineThreadingId
    4: i64 ActorFbId
    5: i64 Timestamp
    6: bool ShouldBuzzDevice
    7: string AdminText
    9: list<string> Tags
}

struct MNMessagesSyncDeltaNewMessage {
    1: MNMessagesSyncMessageMetadata MessageMetadata
    2: string Body
    3: i64 StickerId
    4: list<string> Attachments
   	5: i32 Ttl
}


struct MNMessagesSyncParticipantInfo {
	1: i64 UserFbId
	2: string FirstName
	3: string FullName
	4: bool IsMessengerUser
	5: map<i32,string> ProfPicURIMap
}


struct MNMessagesSyncDeltaNewGroupThread
{
    1: MNMessagesSyncThreadKey ThreadKey
    2: list<MNMessagesSyncParticipantInfo> Participants
}

struct MNMessagesSyncDeltaMarkRead {
    1: list<MNMessagesSyncThreadKey> ThreadKeys
    2: list<i32> Folders
    3: i64 WatermarkTimestamp
    4: i64 ActionTimestamp
}

struct MNMessagesSyncDeltaMarkUnread {
    1: list<MNMessagesSyncThreadKey> ThreadKeys
    2: list<i32> Folders
    3: i64 WatermarkTimestamp
    4: i64 ActionTimestamp
}

struct MNMessagesSyncDeltaMessageDelete {
    1: MNMessagesSyncThreadKey ThreadKey
    2: list<string> MessageIds
}

struct MNMessagesSyncDeltaThreadDelete {
    1: list<MNMessagesSyncThreadKey> ThreadKeys
}

struct MNMessagesSyncDeltaParticipantsAddedToGroupThread {
    1: MNMessagesSyncMessageMetadata MessageMetadata
    2: list<MNMessagesSyncParticipantInfo> AddedParticipants
}



struct MNMessagesSyncDeltaParticipantLeftGroupThread {
    1: MNMessagesSyncMessageMetadata MessageMetadata
    2: i64 LeftParticipantFbId
}

struct MNMessagesSyncDeltaThreadName {
    1: MNMessagesSyncMessageMetadata MessageMetadata
    2: string Name
}


struct MNMessagesSyncAppAttributionVisibility {
	1: bool HideAttribution
	2: bool HideInstallButton
	3: bool HideReplyButton
	4: bool DisableBroadcasting
	5: bool HideAppIcon
}

struct MNMessagesSyncAttachmentAppAttribution {
	1: i64 AttributionAppId
	2: string AttributionMetadata
	3: string AttributionAppName
	4: string AttributionAppIconURI
	5: string AndroidPackageName
	6: i64 IOSStoreId
	7: map<i64,i64> OtherUserAppScopedFbIds
	8: MNMessagesSyncAppAttributionVisibility Visibility
}

struct MNMessagesSyncImageMetadata {
	1: i32 Width
	2: i32 Height
	3: map<i32,string> ImageURIMap
	4: i32 ImageSource
	5: string RawImageURI
	6: string RawImageURIFormat
	7: map<i32,string> AnimatedImageURIMap
	8: string ImageURIMapFormat
	9: string AnimatedImageURIMapFormat
	10: bool RenderAsSticker
}

struct MNMessagesSyncVideoMetadata {
	1: i32 Width
	2: i32 Height
	3: i32 DurationMs
	4: string ThumbnailUri
	5: string VideoUri
	6: i32 Source
	7: i32 Rotation
}


struct MNMessagesSyncAttachment {
	1: string Id
	2: string Mimetype
	3: string Filename
	4: i64 Fbid
	5: i64 Filesize
	6: MNMessagesSyncAttachmentAppAttribution Attributioninfo
	7: string Xmagraphql
	8: MNMessagesSyncImageMetadata Imagemetadata
	9: MNMessagesSyncVideoMetadata Videometadata
}


struct MNMessagesSyncDeltaThreadImage {
    1: MNMessagesSyncMessageMetadata MessageMetadata
    2: MNMessagesSyncAttachment Image
}

struct MNMessagesSyncDeltaThreadMuteSettings {
	1: MNMessagesSyncThreadKey ThreadKey
	2: i64 ExpireTime
}

struct MNMessagesSyncDeltaThreadAction {
	1: MNMessagesSyncThreadKey ThreadKey
	2: i32 Action
}

struct MNMessagesSyncDeltaThreadFolder {
	1: MNMessagesSyncThreadKey ThreadKey
	2: i32 Folder
}

struct MNMessagesSyncDeltaRTCEventLog {
	1: MNMessagesSyncMessageMetadata MessageMetadata
	2: bool Answered
	3: i64 StartTime
	4: i64 Duration
	5: i32 EventType
}

struct MNMessagesSyncDeltaVideoCall {
	1: MNMessagesSyncMessageMetadata MessageMetadata
	2: bool Answered
	3: i64 StartTime
	4: i64 Duration
}

struct MNMessagesSyncDeltaAdminTextMessage {
	1: MNMessagesSyncMessageMetadata MessageMetadata
	2: string Type
	3: map<string,string> UntypedData
}

struct MNMessagesSyncDeltaForcedFetch {
	1: MNMessagesSyncThreadKey ThreadKey
	2: string MessageId
	3: bool IsLazy
}

struct MNMessagesSyncDeltaReadReceipt {
	1: MNMessagesSyncThreadKey ThreadKey
	2: i64 ActorFbId
	3: i64 ActionTimestampMs
	4: i64 WatermarkTimestampMs
}

struct MNMessagesSyncDeltaBroadcastMessage {
	1: list<MNMessagesSyncMessageMetadata> MessageMetadatas
	2: string Body
	3: i64 StickerId
	4: list<MNMessagesSyncAttachment> Attachments
}

struct MNMessagesSyncDeltaMarkFolderSeen {
	1: list<i32> Folders
	2: i64 Timestamp
}

struct MNMessagesSyncDeltaSentMessage {
	1: MNMessagesSyncMessageMetadata MessageMetadata
	2: list<MNMessagesSyncAttachment> Attachments
}

struct MNMessagesSyncDeltaPinnedGroups {
	1: list<MNMessagesSyncThreadKey> ThreadKeys
}

struct MNMessagesSyncDeltaPageAdminReply {
	1: MNMessagesSyncMessageMetadata MessageMetadata
	2: string ActivityToken
	3: i32 ReplyType
}

struct MNMessagesSyncDeltaDeliveryReceipt {
	1: MNMessagesSyncThreadKey ThreadKey
	2: i64 ActorFbId
	3: string DeviceId
	4: i64 AppId
	5: i64 TimestampMs
	6: list<string> MessageIds
	7: i64 DeliveredWatermarkTimestampMs
}

struct MNMessagesSyncDeltaP2PPaymentMessage {
	1: MNMessagesSyncMessageMetadata MessageMetadata
	2: i64 TransferId
	3: i32 MessageType
}

struct MNMessagesSyncTagCount {
	1: i32 Count
	2: bool HasMore
}

struct MNMessagesSyncDeltaFolderCount {
	1: i32 ThreadFolder
	2: i32 Count
	3: bool HasMore
	4: map<i32,MNMessagesSyncTagCount> Counts
}

struct MNMessagesSyncDeltaPagesManagerEvent {
	1: MNMessagesSyncThreadKey ThreadKey
	2: string JsonBlob
}

struct MNMessagesSyncNotificationDoNotDisturbRange {
	1: byte Days
	2: i16 StartMinutes
	3: i16 DurationMinutes
}

struct MNMessagesSyncDeltaNotificationSettings {
	1: MNMessagesSyncThreadKey ThreadKey
	2: list<MNMessagesSyncNotificationDoNotDisturbRange> DoNotDisturbRanges
}

struct MNMessagesSyncDeltaReplaceMessage {
	1: MNMessagesSyncDeltaNewMessage NewMessage
	2: string ReplacedMessageId
}

struct MNMessagesSyncDeltaZeroRating {
	1: i32 NumFreeMessagesRemaining
}

struct MNMessagesSyncDeltaWrapper {
    1: MNMessagesSyncDeltaNoOp DeltaNoOp
    2: MNMessagesSyncDeltaNewMessage DeltaNewMessage
    3: MNMessagesSyncDeltaNewGroupThread DeltaNewGroupThread
   	4: MNMessagesSyncDeltaMarkRead DeltaMarkRead
    5: MNMessagesSyncDeltaMarkUnread DeltaMarkUnread
    6: MNMessagesSyncDeltaMessageDelete DeltaMessageDelete
    7: MNMessagesSyncDeltaThreadDelete DeltaThreadDelete
    8: MNMessagesSyncDeltaParticipantsAddedToGroupThread DeltaParticipantsAddedToGroupThread
    9: MNMessagesSyncDeltaParticipantLeftGroupThread DeltaParticipantLeftGroupThread
    10: MNMessagesSyncDeltaThreadName DeltaThreadName
    11: MNMessagesSyncDeltaThreadImage DeltaThreadImage
    12: MNMessagesSyncDeltaThreadMuteSettings DeltaThreadMuteSettings
    13: MNMessagesSyncDeltaThreadAction DeltaThreadAction
    14: MNMessagesSyncDeltaThreadFolder DeltaThreadFolder
    15: MNMessagesSyncDeltaRTCEventLog DeltaRTCEventLog
    16: MNMessagesSyncDeltaVideoCall DeltaVideoCall
    17: MNMessagesSyncDeltaAdminTextMessage DeltaAdminTextMessage
    18: MNMessagesSyncDeltaForcedFetch DeltaForcedFetch
    19: MNMessagesSyncDeltaReadReceipt DeltaReadReceipt
    20: MNMessagesSyncDeltaBroadcastMessage DeltaBroadcastMessage
    21: MNMessagesSyncDeltaMarkFolderSeen DeltaMarkFolderSeen
    22: MNMessagesSyncDeltaSentMessage DeltaSentMessage
    23: MNMessagesSyncDeltaPinnedGroups DeltaPinnedGroups
    24: MNMessagesSyncDeltaPageAdminReply DeltaPageAdminReply
    25: MNMessagesSyncDeltaDeliveryReceipt DeltaDeliveryReceipt
    26: MNMessagesSyncDeltaP2PPaymentMessage DeltaP2PPaymentMessage
    27: MNMessagesSyncDeltaFolderCount DeltaFolderCount
    28: MNMessagesSyncDeltaPagesManagerEvent DeltaPagesManagerEvent
    29: MNMessagesSyncDeltaNotificationSettings DeltaNotificationSettings
    30: MNMessagesSyncDeltaReplaceMessage DeltaReplaceMessage
    31: MNMessagesSyncDeltaZeroRating DeltaZeroRating
}

struct MNMessagesSyncFailedSend {
	1: i64 OfflineThreadingId
	2: string ErrorMessage
	3: bool IsRetryable
	4: i32 ErrorCode
}

struct MNMessagesSyncClientPayload {
	1: list<MNMessagesSyncDeltaWrapper> Deltas
	2: i64 FirstDeltaSeqId
	3: i64 LastIssuedSeqId
	4: i64 QueueEntityId
	5: MNMessagesSyncFailedSend FailedSend
	6: string SyncToken
	7: string ErrorCode
}