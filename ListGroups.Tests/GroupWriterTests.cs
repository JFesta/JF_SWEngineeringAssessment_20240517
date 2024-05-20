using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ListGroups.Services;
using Microsoft.Graph.Models;

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
        public void ResetDeletesNotEmptyFolder()
        {
            var basePath = Path.Combine(Environment.CurrentDirectory, "Tests");
            Directory.CreateDirectory(basePath); //TODO requires reset
            var testFilePath = Path.Combine(basePath, "test.txt");
            File.WriteAllText(testFilePath, "Test File");

            var groupWriter = new GroupWriter(_logger);
            groupWriter.Reset(basePath);

            Assert.True(Directory.Exists(basePath));
            Assert.False(File.Exists(testFilePath));
        }
    }
}
