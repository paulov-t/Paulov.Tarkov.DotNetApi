﻿using Newtonsoft.Json;

namespace Paulov.TarkovModels
{
    public class AccountProfileModes
    {
        [JsonProperty("regular")]
        public AccountProfileMode Regular { get; set; } = new AccountProfileMode();

        [JsonProperty("pve")]
        public AccountProfileMode PVE { get; set; } = new AccountProfileMode();

        [JsonProperty("arena")]
        public AccountProfileMode Arena { get; set; } = new AccountProfileMode();
    }
}
