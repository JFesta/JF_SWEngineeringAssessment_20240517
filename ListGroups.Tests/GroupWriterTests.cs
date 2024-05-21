using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ListGroups.Services;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace ListGroups.Tests
{
    public class GroupWriterTests
    {
        private ILogger<GroupWriter> _logger;

        public GroupWriterTests()
        {
            _logger = new NullLogger<GroupWriter>();
        }

        [Fact]
        public void ResetThrowsIfEmptyPath()
        {
            var groupWriter = new GroupWriter(_logger);
            Assert.Throws<ArgumentNullException>(() => groupWriter.Reset(string.Empty));
        }

        [Fact]
        public void ResetClearsNotEmptyFolder()
        {
            //setup
            var basePath = Path.Combine(Environment.CurrentDirectory, "Tests001");
            SetupFolder(basePath);
            var testFilePath = Path.Combine(basePath, "test.txt");
            File.WriteAllText(testFilePath, "Test File");

            //test
            var groupWriter = new GroupWriter(_logger);
            groupWriter.Reset(basePath);

            //asserts
            Assert.True(Directory.Exists(basePath));
            Assert.False(File.Exists(testFilePath));
        }

        [Fact]
        public async Task WriteThrowsIfEmptyPath()
        {
            var groupWriter = new GroupWriter(_logger);
            await Assert.ThrowsAsync<ArgumentNullException>(() => groupWriter.WriteGroupAsync(string.Empty, CreateGroup()));
        }

        [Fact]
        public async Task WriteThrowsIfNullGroup()
        {
            var basePath = Path.Combine(Environment.CurrentDirectory, "Tests002");
            SetupFolder(basePath);

            var groupWriter = new GroupWriter(_logger);
            await Assert.ThrowsAsync<ArgumentNullException>(() => groupWriter.WriteGroupAsync(basePath, null));
        }

        [Fact]
        public async Task WriteDoesNothingIfMissingDisplayName()
        {
            var basePath = Path.Combine(Environment.CurrentDirectory, "Tests003");
            SetupFolder(basePath);

            var groupWriter = new GroupWriter(_logger);
            await groupWriter.WriteGroupAsync(basePath, new Group()
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = string.Empty,
            });

            Assert.False(Directory.EnumerateFiles(basePath).Any());
        }

        [Fact]
        public async Task WriteGeneratesACorrectFile()
        {
            //setup
            var basePath = Path.Combine(Environment.CurrentDirectory, "Tests004");
            SetupFolder(basePath);

            //test
            var groupWriter = new GroupWriter(_logger);
            var group = CreateGroup();
            await groupWriter.WriteGroupAsync(basePath, group);

            //assert: the output dir should contain only one file
            Assert.True(Directory.EnumerateFiles(basePath).Count() == 1);

            //assert: test expects to have one file named exactly after the group's DisplayName
            var filePath = GetGroupRelativePath(basePath, group);
            Assert.True(File.Exists(filePath));

            //assert: test expects a correct json with corresponding set properties (we won't check the values for the sake of keeping things simple)
            using var jsonObject = JsonDocument.Parse(File.ReadAllText(filePath));
            foreach (var p in jsonObject.RootElement.EnumerateObject())
            {
                Assert.NotNull(typeof(Group).GetProperty(p.Name));
            }
        }

        [Fact]
        public async Task WriteOverridesSameNameGroup()
        {
            //setup
            var basePath = Path.Combine(Environment.CurrentDirectory, "Tests005");
            SetupFolder(basePath);

            //test
            var groupWriter = new GroupWriter(_logger);
            var group1 = CreateGroup();
            var group2 = CreateGroup();
            await groupWriter.WriteGroupAsync(basePath, group1);
            await groupWriter.WriteGroupAsync(basePath, group2);

            //assert: the output dir should contain only one file
            Assert.True(Directory.EnumerateFiles(basePath).Count() == 1);

            //assert: test expects to have one file named exactly after the group's DisplayName
            var filePath = GetGroupRelativePath(basePath, group2);
            Assert.True(File.Exists(filePath));

            //assert: test expects a json containing an Id matching with group2 (since it's the last written)
            using var jsonObject = JsonDocument.Parse(File.ReadAllText(filePath));
            var parsedId = jsonObject.RootElement.GetProperty(nameof(Group.Id)).GetString();
            Assert.Equal(group2.Id, parsedId);
        }

        [Fact]
        public async Task WriteHandlesMultipleGroups()
        {
            //setup
            var basePath = Path.Combine(Environment.CurrentDirectory, "Tests006");
            SetupFolder(basePath);

            //test
            var groupWriter = new GroupWriter(_logger);
            var group1 = CreateGroup("Test01");
            var group2 = CreateGroup("Test02");
            var group3 = CreateGroup("Test03");
            await groupWriter.WriteGroupAsync(basePath, group1);
            await groupWriter.WriteGroupAsync(basePath, group2);
            await groupWriter.WriteGroupAsync(basePath, group3);

            //assert: the output dir should contain N files
            Assert.True(Directory.EnumerateFiles(basePath).Count() == 3);
        }

        private void SetupFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }

        private Group CreateGroup()
        {
            return new Group()
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = "Test",
                Theme = "Teal",
                CreatedDateTime = new DateTime(2020, 12, 12, 13, 5, 2, DateTimeKind.Utc)
            };
        }

        private Group CreateGroup(string displayName)
        {
            return new Group()
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = displayName,
                Theme = "Teal",
                CreatedDateTime = DateTimeOffset.UtcNow
            };
        }

        private string GetGroupRelativePath(string basePath, Group group)
        {
            return Path.Combine(basePath, $"{group.DisplayName}.json");
        }
    }
}
