namespace ConfigCenter.Repository
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AppSetting")]
    public partial class AppSetting
    {
        public int Id { get; set; }

        public int? AppId { get; set; }

        [StringLength(50)]
        public string ConfigKey { get; set; }

        [StringLength(500)]
        public string ConfigValue { get; set; }

        public int ConfigType { get; set; }
    }
}
