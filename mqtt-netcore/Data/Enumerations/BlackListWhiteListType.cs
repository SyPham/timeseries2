using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Data.Enumerations
{
    /// <summary>
    ///     An enumeration representing the available blacklist or whitelist types.
    /// </summary>
    public enum BlackListWhiteListType
    {
        /// <summary>
        ///     The subscribe blacklist or whitelist type.
        /// </summary>
        Subscribe,

        /// <summary>
        ///     The publish blacklist or whitelist type.
        /// </summary>
        Publish
    }
}
