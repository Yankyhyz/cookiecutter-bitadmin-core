using System;
using System.Collections.Generic;

namespace BitAdminCore.Models
{
    public partial class FlowMain
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
