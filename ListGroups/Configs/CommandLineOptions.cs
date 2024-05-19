using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListGroups.Configs;

public class CommandLineOptions
{
    [Option("output", Required = false, HelpText = "Output folder's path; current directory is used as default")]
    public string? OutputPath { get; set; }

    [Option("tenantId", Required = false, HelpText = "Azure TenantId")]
    public string? TenantId { get; set; }

    [Option("clientId", Required = false, HelpText = "ClientId to use for AD Client+Secret client auth")]
    public string? ClientId { get; set; }

    [Option("clientSecret", Required = false, HelpText = "Secret to use for AD Client+Secret client auth")]
    public string? Secret { get; set; }
}
