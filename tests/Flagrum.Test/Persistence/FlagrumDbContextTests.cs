using Flagrum.Abstractions;
using Flagrum.Application.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Flagrum.Test.Persistence;

public class FlagrumDbContextTests
{
    [Fact]
    public void DoesTableExistTreatsTableNameAsValue()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            using var context = new FlagrumDbContext(new TestProfileService(databasePath));
            context.Database.OpenConnection();

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "CREATE TABLE StatePairs (Key INTEGER PRIMARY KEY, Value TEXT)";
                command.ExecuteNonQuery();
            }

            Assert.True(context.DoesTableExist("StatePairs"));
            Assert.False(context.DoesTableExist("StatePairs' OR 1=1 --"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    private sealed class TestProfileService(string databasePath) : IProfileService
    {
        public List<IProfileViewModel> Profiles => throw new NotSupportedException();
        public string ClientId => throw new NotSupportedException();
        public IProfile Current => throw new NotSupportedException();
        public string LastVersionNotes { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public Version LastVersion => throw new NotSupportedException();
        public bool DidMigrateThisSession => throw new NotSupportedException();
        public bool IsReady => throw new NotSupportedException();
        public string FlagrumDirectory => throw new NotSupportedException();
        public string DatabasePath { get; } = databasePath;
        public string FileIndexPath => throw new NotSupportedException();
        public string ImagesDirectory => throw new NotSupportedException();
        public string ModThumbnailWebDirectory => throw new NotSupportedException();
        public string TemporaryDirectory => throw new NotSupportedException();
        public string CacheDirectory => throw new NotSupportedException();
        public string ModStagingDirectory => throw new NotSupportedException();
        public string PatchDirectory => throw new NotSupportedException();
        public string ModFilesDirectory => throw new NotSupportedException();
        public string EarcModThumbnailDirectory => throw new NotSupportedException();
        public string EarcModBackupsDirectory => throw new NotSupportedException();
        public string SteamExePath => throw new NotSupportedException();
        public string BinmodDirectory => throw new NotSupportedException();
        public string WorkshopDirectory => throw new NotSupportedException();
        public string GameDataDirectory => throw new NotSupportedException();
        public string GameDirectory => throw new NotSupportedException();
        public string ModStatePath => throw new NotSupportedException();

        public void Dispose() { }
        public bool IsGamePathAvailable(string path) => throw new NotSupportedException();
        public void SetPremiumAccountToken(string? token, string? refreshToken, DateTime expiry) => throw new NotSupportedException();
        public void SetGiftToken(string token) => throw new NotSupportedException();
        public void SetCurrentProfile(string id) => throw new NotSupportedException();
        public void Add(IProfileViewModel profile) => throw new NotSupportedException();
        public void Update(IProfileViewModel profile) => throw new NotSupportedException();
        public void Delete(IProfileViewModel profile) => throw new NotSupportedException();
        public bool IsGameRunning() => throw new NotSupportedException();
    }
}