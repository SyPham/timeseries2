using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Data.Dto
{
    /// <summary>
    ///     The user class to create or update a user.
    /// </summary>
    public class DtoCreateUpdateUser
    {
        /// <summary>
        ///     Gets or sets the user name.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the client identifier prefix.
        /// </summary>
        public string ClientidPrefix { get; set; }

        /// <summary>
        ///     Gets or sets the client identifier.
        /// </summary>
        public string Clientid { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the client id is validated or not.
        /// </summary>
        public bool ValidateClientid { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the user is throttled after a certain limit or not.
        /// </summary>
        public bool ThrottleUser { get; set; }

        /// <summary>
        ///     Gets or sets a user's monthly limit in byte.
        /// </summary>
        public long? MonthlyByteLimit { get; set; }

        /// <summary>
        ///     Gets or sets the created at timestamp.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the deleted at timestamp.
        /// </summary>
        public DateTimeOffset? DeletedAt { get; set; }

        /// <summary>
        ///     Gets or sets the updated at timestamp.
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; set; } = null;

        /// <summary>
        ///     Returns a <see cref="string"></see> representation of the <see cref="DtoCreateUpdateUser" /> class.
        /// </summary>
        /// <returns>A <see cref="string"></see> representation of the <see cref="DtoCreateUpdateUser" /> class.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
