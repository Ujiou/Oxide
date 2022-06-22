
### Last Updated: 6/22/22

# Starting Up...
To begin your journey as a Rust plugin developer you'll need a few tools.

* [**Rust Server**](https://www.rustafied.com/how-to-host-your-own-rust-server)
    - Once installed, proceed to install [**Oxide**](https://umod.org/).
        * Afterwards, go to your server and find RustDedicated_Data, inside of this folder should be a nested folder dubbed Managed. Now head to [**Starting the Journey**](#starting-the-journey). 
* [**Visual Studio**](https://visualstudio.microsoft.com/vs/)     
    1. Make a **C# Class Library**. 
    * ***Note: You can call this whatever you want.***
        - Inside this project, find **'Dependencies'** in the Solution Explorer.
        * Remember the **RustDedicated_Data > Managed** folder you were at before? Now it's time to use it. 
        * Right click **'Dependencies'** and every 30 DLLs or so import them as directed below. 
            ![](https://media4.giphy.com/media/Y8Oi1fg8N8ZlVrr7a7/giphy.gif?cid=790b7611d1a8e25136c15bb15fe5510d4ed83f6b56dc82da&rid=giphy.gif&ct=g)
        ***If you get an error just ignore it, the files will more than likely be imported.***
* [**DN Spy**](https://dnspy.dev)        
    - This is a utility to peek into DLLs to see code and how it is implemented/used.

# Starting the Journey...
 * **Setting up Visual Studio**
    - Make a new CS file and call it **Destroy.cs** and open it to where code is visually shown.
        * Now the following is mandatory and must be done for each plugin. 
            
            Format your **Destroy.cs** to look like this.
        ```cs
            namespace Oxide.Plugins // This is what allows us to make a plugin under already defined pretext.
            {
                [Info("Destroy", "Author", "1.0.0")] // Plugin Name, Author, Version
                [Description("This plugin will enable players with permissions to destroy an object with a hammer.")]
                public class Destroy : RustPlugin // This is extending to the class RustPlugin.
                {

                }
            }
        ```

### Configuration
* Having a **configurable** plugin is always important, this allows you or a customer to edit values without touching code.
* First we'll build our configuration class:
```cs
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



```

### Commands
* These deter from **RustPlugin** and **CovalencePlugin**
```cs

private List<ulong> playerToggle = new List<ulong>();

// RustPlugin
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

// CovalencePlugin
[Command("destroy")]
private void DestroyToggle(IPlayer iPlayer) 
{
    BasePlayer player = iPlayer.Object as BasePlayer;
    if (player == null) return;

        if (playerToggle.Contains(player.userID))
    {
        playerToggle.Remove(player.userID);
        Notify(player, "Disabled");
        return;
    }

    playerToggle.Add(player.userID);
    Notify(player, "Enabled");
}
```

### Hooks
* This is how we'll implement the aspect of hitting an object with a hammer to destroy said object.
    * You can find hooks and more documentation on [**uMod**](https://umod.org/documentation/games/rust). 

    ```cs
        private object OnHammerHit(BasePlayer player, HitInfo info)
        {
            if (player == null) return null;

            // Check to see if the player has permission to continue.
            if (!player.IPlayer.HasPermission(cfg.permission))
                return null;

            if (!playerToggled.Contains(player.userID))
                return null;

            // Getting the entity from HitInfo.
            var entity = info.HitEntity;

            // Simple null check to see if the player 
            if (info == null && entity == null)
            {
                Notify(player, "EntityIsNull"); 
                return null;
            }

            entity.Kill(); // Destroy / Kill the entity.
            return null;
        }

        private void Init()
        {
            LoadDefaultMessages();
            if(cfg != null)
                permission.RegisterPermission(cfg.permission, this);
        }
    ```

### Localization
* Localization allows server owners to edit the output to players through a "lang" file.
* More doc: [**uMod - Localization**](https://umod.org/documentation/api/localization).
```cs
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
```
