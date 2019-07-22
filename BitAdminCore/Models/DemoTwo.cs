using System;
using System.Collections.Generic;

namespace BitAdminCore.Models
{
    public partial class DemoTwo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? CreateUserId { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
