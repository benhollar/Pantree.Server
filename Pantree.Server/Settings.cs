using System;

namespace Pantree.Server
{
    /// <summary>
    /// A collection of settings for connecting to Pantree.Server
    /// </summary>
    public class ConnectionSettings
    {
        /// <summary>
        /// A list of client origin URLs that may connect to Pantree.Server
        /// </summary>
        public string[] AllowedCorsOrigins { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// A collection of settings for configuring the `AutoMapper` library
    /// </summary>
    public class AutoMapperSettings
    {
        /// <summary>
        /// The license key, obtained from <see href="https://automapper.io/"/> 
        /// </summary>
        public string? LicenseKey { get; set; } = null;
    }
}
