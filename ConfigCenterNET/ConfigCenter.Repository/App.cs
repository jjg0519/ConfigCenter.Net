namespace ConfigCenter.Repository
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("App")]
    public partial class App
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string AppId { get; set; }

        [StringLength(50)]
        public string Version { get; set; }
    }
}
