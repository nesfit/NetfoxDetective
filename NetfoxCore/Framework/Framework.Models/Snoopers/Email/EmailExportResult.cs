using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Netfox.Framework.Models.Snoopers.Email
{
    /// <summary> Encapsulates the result of an email export.</summary>
    public class EmailExportResult : SnooperExportBase
    {
        private Type _dataType;

        /// <summary> Gets or sets the type of the data.</summary>
        /// <value> The type of the data.</value>
        [NotMapped]
        public Type DataType
        {
            get
            {
                try
                {
                    return this._dataType ?? Type.GetType(this.DataTypeDb);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                this._dataType = value;
                this.DataTypeDb = value.FullName;
            }
        }

        public string DataTypeDb { get; set; }

        /// <summary> Gets or sets the full pathname of the data file.</summary>
        /// <value> The full pathname of the data file.</value>
        public string DataPath { get; set; }

        /// <summary> Gets or sets the description.</summary>
        /// <value> The description.</value>
        public string Description { get; set; }
    }
}