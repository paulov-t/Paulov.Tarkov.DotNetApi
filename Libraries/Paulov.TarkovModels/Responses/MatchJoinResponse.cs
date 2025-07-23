namespace Paulov.TarkovModels.Responses
{
    public sealed class MatchJoinResponse : ProfileStatusResponse
    {
        public string ProfileId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;

        public MatchJoinResponse() { }

        public MatchJoinResponse(
            string profileId
            , string ipAddress
            , string port
            , string locationId
            , bool maxPveCountExceeded
            , ProfileStatusModel pmcStatusModel
            , ProfileStatusModel scavStatusModel
            )
            : base(maxPveCountExceeded, pmcStatusModel, scavStatusModel)
        {
            pmcStatusModel.Location = locationId;
            pmcStatusModel.Ip = ipAddress;
            pmcStatusModel.Port = port;
            pmcStatusModel.ShortId = profileId;
            pmcStatusModel.Sid = profileId;
            scavStatusModel.Location = locationId;
            scavStatusModel.Ip = ipAddress;
            scavStatusModel.Port = port;
            scavStatusModel.ShortId = profileId;
            scavStatusModel.Sid = profileId;

            ProfileId = profileId;
            IpAddress = ipAddress;
            Port = port;
            LocationId = locationId;
        }
    }
}
