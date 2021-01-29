using DSharpPlus.Entities;
using System;

namespace Catamagne.Configuration
{
    [Serializable]
    class ConfigValues
    {
        [NonSerialized] public static ConfigValues configValues = new ConfigValues();
        public ulong[] RoleIDs;
        public ulong[] AdminRoleIDs;
        public ulong[] CommandChannels;
        public ulong AlertChannel;
        public ulong UpdatesChannel;
        public DiscordActivity DiscordActivity;
        public string[] Prefixes;
        public TimeSpan ShortInterval;
        public TimeSpan MediumInterval;
        public TimeSpan LongInterval;
        public string Folderpath;
        public string Filepath;
        public string SpreadsheetID;
        public string BungieAPIKey;
        public string DiscordToken;
        public ulong DevID;
        public ConfigValues()
        {
        }
    }
}
