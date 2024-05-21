using CommandLine;

namespace GraphProcessor.Arguments;

[Verb("groups", HelpText = "Download AAD Groups into a specified folder")]
public class CliDownloadGroupsOptions
{
    [Option("output", Required = false, HelpText = "Output folder's path; current directory is used as default")]
    public string? OutputPath { get; set; } = Environment.CurrentDirectory;
}
