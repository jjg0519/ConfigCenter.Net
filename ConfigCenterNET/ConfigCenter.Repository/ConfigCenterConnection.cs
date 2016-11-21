namespace ConfigCenter.Repository
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ConfigCenterConnection : DbContext
    {
        public ConfigCenterConnection()
            : base("name=ConfigCenterConnection")
        {
        }

        public virtual DbSet<App> App { get; set; }
        public virtual DbSet<AppSetting> AppSetting { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
