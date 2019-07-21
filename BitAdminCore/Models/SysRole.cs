using System;
using System.Collections.Generic;

namespace BitAdminCore.Models
{
    public partial class SysRole
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }
        public int? OrderNo { get; set; }
        public Guid? CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
