using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Newtonsoft.Json;

namespace ListGroups.Services;

/// <summary>
/// This class is responsible for writing Groups into the File System.
/// </summary>
public class GroupWriter(ILogger<GroupWriter> logger)
{
    //default serializer: might be overwritten by an injected instance in a future release
    private static JsonSerializerSettings _defaultSerializerSettings = new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore, //ignore loops, if any, since we are dealing with a complex structure
        NullValueHandling = NullValueHandling.Ignore, //ignore nulls in order to keep the jsons light
        Formatting = Formatting.Indented //helps readibility, although it makes files heavier
    };

    /// <summary>
    /// To be called before a batch processing. This method clears completely the output folder, in order to avoid having
    /// files generated in a previous session and related to since-then deleted groups.
    /// </summary>
    /// <param name="outputBasePath">Folder to clear</param>
    /// <exception cref="ArgumentNullException">If <c>outputBasePath</c> is empty.</exception>
    public void Reset(string outputBasePath)
    {
        if (string.IsNullOrWhiteSpace(outputBasePath)) throw new ArgumentNullException(nameof(outputBasePath));

        if (Directory.Exists(outputBasePath))
            Directory.Delete(outputBasePath, true);
        Directory.CreateDirectory(outputBasePath);
    }

    /// <summary>
    /// Writes the serialized Group into a Json file named after Group's <c>DisplayName</c>
    /// </summary>
    /// <param name="outputBasePath">Target folder</param>
    /// <param name="group">Group to write to File System</param>
    /// <exception cref="ArgumentNullException">If <c>outputBasePath</c> is empty or <c>group</c> is null.</exception>
    public async Task WriteGroupAsync(string outputBasePath, Group group)
    {
        if (string.IsNullOrWhiteSpace(outputBasePath)) throw new ArgumentNullException(nameof(outputBasePath));
        if (group == null) throw new ArgumentNullException(nameof(group));

        logger.LogDebug("Processing Group {Id}: {DisplayName}", group.Id, group.DisplayName);

        if (string.IsNullOrWhiteSpace(group.DisplayName))
        {
            logger.LogWarning("Can't process group {Id}: Empty DisplayName", group.Id);
            return;
        }

        var body = JsonConvert.SerializeObject(group, _defaultSerializerSettings);
        await File.WriteAllTextAsync(Path.Combine(outputBasePath, $"{group.DisplayName}.json"), body);
    }
}
