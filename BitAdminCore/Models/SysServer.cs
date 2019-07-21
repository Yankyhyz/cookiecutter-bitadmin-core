using System;
using System.Collections.Generic;

namespace BitAdminCore.Models
{
    public partial class SysServer
    {
        public string ServerIp { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
