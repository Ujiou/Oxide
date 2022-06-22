using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Destroy", "Author", "1.0.0")]
    [Description("This plugin will enable players with permissions to destroy an object with a hammer.")]
    public class Destroy : RustPlugin
    {
        #region Configuration

        public Configuration cfg;

        public class Configuration
        {
            [JsonProperty(PropertyName = "Permission")]
            public string permission { get; set; } = "destroy.use";
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                cfg = Config.ReadObject<Configuration>();
                if (cfg == null) throw new Exception();
                SaveConfig();
            }
            catch (Exception ex)
            {
                PrintError("Your configuration file contains an error. Using default configuration values.");
                LoadDefaultConfig();
                Debug.LogException(ex);
            }
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(cfg);
        }

        protected override void LoadDefaultConfig()
        {
            cfg = new Configuration();
        }


        #endregion

        #region Commands

        private List<ulong> playerToggle = new List<ulong>();

        [ChatCommand("destroy")]
        private void DestroyToggle(BasePlayer player)
        {
            if (playerToggle.Contains(player.userID))
            {
                playerToggle.Remove(player.userID);
                Notify(player, "Disabled");
                return;
            }

            playerToggle.Add(player.userID);
            Notify(player, "Enabled");
        }

        #endregion

        #region Hooks

        private object OnHammerHit(BasePlayer player, HitInfo info)
        {
            if (player == null) return null;

            if (!player.IPlayer.HasPermission(cfg.permission))
                return null;

            var entity = info.HitEntity;
            if (info == null && entity == null)
            {
                Notify(player, "EntityIsNull");
                return null;
            }

            entity.Kill();
            return null;
        }

        private void Init()
        {
            LoadDefaultMessages();
            if(cfg != null)
                permission.RegisterPermission(cfg.permission, this);
        }

        #endregion

        #region Lang

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Enabled"] = "Destroy >> <color=green>Enabled</color>",
                ["Disabled"] = "Destroy >> <color=red>Disabled</color>",
                ["EntityIsNull"] = "Destroy >> The entity appears to be null."
            }, this);
        }

        private void Notify(BasePlayer player, string key) => player?.ChatMessage(lang.GetMessage(key, this));
        

        #endregion
    }
}
