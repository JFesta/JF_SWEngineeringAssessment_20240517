using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System.Reflection;


var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

var configuration = configurationBuilder.Build();

var credentials = new ClientSecretCredential(configuration["AZURE_TENANT_ID"], configuration["AZURE_CLIENT_ID"], configuration["AZURE_CLIENT_SECRET"]);
var graphClient = new GraphServiceClient(credentials);

var response = await graphClient.Groups.GetAsync();
foreach (var group in response!.Value!.Take(10))
{
    Console.WriteLine("{0}\t{1}", group.Id, group.DisplayName);
}