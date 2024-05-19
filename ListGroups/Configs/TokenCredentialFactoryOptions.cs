using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListGroups.Configs;

public class TokenCredentialFactoryOptions
{
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? Secret { get; set; }
}
