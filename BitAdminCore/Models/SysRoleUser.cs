using System;
using System.Collections.Generic;

namespace BitAdminCore.Models
{
    public partial class SysRoleUser
    {
        public Guid Id { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? UserId { get; set; }
    }
}
