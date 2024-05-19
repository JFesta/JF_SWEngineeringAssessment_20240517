using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListGroups.Configs
{
    public class ListGroupsOptions
    {
        public required string OutputPath { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
