using Microsoft.AspNetCore.Http;

namespace BitAdminCore.UEditor.Handlers
{
    public class ConfigHandler : Handler
    {
        public ConfigHandler(HttpContext context) : base(context) { }

        public override void Process()
        {
            var config = Config.Items;
            var isLocal = Config.GetValue<bool>("isLocal");
            if (!isLocal)
            {
                config["imageUrlPrefix"] = "";
                config["scrawlUrlPrefix"] = "";
                config["snapscreenUrlPrefix"] = "";
                config["catcherUrlPrefix"] = "";
                config["videoUrlPrefix"] = "";
                config["fileUrlPrefix"] = "";
            }
            WriteJson(config);
        }
    }
}