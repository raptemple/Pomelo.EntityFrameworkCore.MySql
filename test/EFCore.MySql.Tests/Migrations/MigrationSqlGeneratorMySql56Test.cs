using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Xunit;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using MySql.Data.MySqlClient;

namespace Pomelo.EntityFrameworkCore.MySql.Tests.Migrations
{
    public class MigrationSqlGeneratorMySql56Test : MigrationSqlGeneratorTestBase
    {
        protected override IMigrationsSqlGenerator SqlGenerator
        {
            get
            {
                // type mapper
                var typeMapper = new MySqlTypeMapper(new RelationalTypeMapperDependencies());

                // migrationsSqlGeneratorDependencies
                var commandBuilderFactory = new RelationalCommandBuilderFactory(
                    new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                    typeMapper);
                var migrationsSqlGeneratorDependencies = new MigrationsSqlGeneratorDependencies(
                    commandBuilderFactory,
                    new MySqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                    typeMapper);

                var mySqlOptions = new Mock<IMySqlOptions>();
                mySqlOptions.SetupGet(opts => opts.ConnectionSettings).Returns(
                    new MySqlConnectionSettings(new MySqlConnectionStringBuilder(), new ServerVersion("5.6.2")));
                
                return new MySqlMigrationsSqlGenerator(
                    migrationsSqlGeneratorDependencies,
                    mySqlOptions.Object);
            }
        }

        private static FakeRelationalConnection CreateConnection(IDbContextOptions options = null)
            => new FakeRelationalConnection(options ?? CreateOptions());

        private static IDbContextOptions CreateOptions(RelationalOptionsExtension optionsExtension = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(optionsExtension
                                      ?? new FakeRelationalOptionsExtension().WithConnectionString("test"));

            return optionsBuilder.Options;
        }


        public override void RenameIndexOperation_works()
        {
            base.RenameIndexOperation_works();
            
            Assert.Equal("ALTER TABLE `People` DROP INDEX `IX_People_Name`;" + EOL 
                         + "ALTER TABLE `People` CREATE INDEX `IX_People_Better_Name`;" + EOL,
                Sql);
        }
    }
}
