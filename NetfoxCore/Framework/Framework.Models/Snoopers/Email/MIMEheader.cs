namespace Netfox.Framework.Models.Snoopers.Email
{
    /// <summary> A mim eheader.</summary>
    public class MIMEheader
    {
        public MIMEheader()
        {
        } //EF

        /// <summary> Gets or sets the type.</summary>
        /// <value> The type.</value>
        public string Type { get; set; }

        /// <summary> Gets or sets the value.</summary>
        /// <value> The value.</value>
        public string Value { get; set; }
    }
}