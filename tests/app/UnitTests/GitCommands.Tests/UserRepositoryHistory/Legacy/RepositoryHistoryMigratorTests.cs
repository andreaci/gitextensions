using System.Reflection;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.UserRepositoryHistory.Legacy;
using NSubstitute;
using Current = GitCommands.UserRepositoryHistory;

namespace GitCommandsTests.UserRepositoryHistory.Legacy
{
    [TestFixture]
    public class RepositoryHistoryMigratorTests
    {
        private IRepositoryStorage _repositoryStorage;
        private RepositoryHistoryMigrator _historyMigrator;

        [SetUp]
        public void Setup()
        {
            _repositoryStorage = Substitute.For<IRepositoryStorage>();
            _historyMigrator = new RepositoryHistoryMigrator(_repositoryStorage);
        }

        [Test]
        public void MigrateAsync_should_throw_if_currentHistory_null()
        {
            Func<Task> f = async () => { await _historyMigrator.MigrateAsync(null); };
            f.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task MigrateSettings_should_migrate_old_categorised_settings()
        {
            string xml = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.CategorisedRepositories02.xml");
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new FileFormatException("Unexpected data");
            }

            _repositoryStorage.Load().Returns(x => new RepositoryCategoryXmlSerialiser().Deserialize(xml));

            (IList<Current.Repository> currentHistory, bool migrated) = await _historyMigrator.MigrateAsync(new List<Current.Repository>());

            currentHistory.Should().HaveCount(8);
            currentHistory.Count(r => r.Category == "Git Extensions").Should().Be(1);
            currentHistory.Count(r => r.Category == "3rd Party").Should().Be(2);
            currentHistory.Count(r => r.Category == "Tests").Should().Be(5);

            migrated.Should().BeTrue();
        }
    }
}
