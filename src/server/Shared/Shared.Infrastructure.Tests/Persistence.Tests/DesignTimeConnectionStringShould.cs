using System;
using System.IO;
using FluentPOS.Shared.Infrastructure.Persistence;
using Xunit;

namespace FluentPOS.Shared.Infrastructure.Tests.Persistence.Tests;

public class DesignTimeConnectionStringShould : IDisposable
{
    private readonly string _tempRoot;

    public DesignTimeConnectionStringShould()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

    [Fact]
    public void Returns_connection_string_when_API_folder_is_in_start_directory()
    {
        // Arrange
        WriteAppSettings(_tempRoot, "Host=testhost;Database=testdb;Username=testuser;Password=testpass");

        // Act
        var result = DesignTimeConnectionString.Read(_tempRoot);

        // Assert
        Assert.Equal("Host=testhost;Database=testdb;Username=testuser;Password=testpass", result);
    }

    [Fact]
    public void Returns_connection_string_when_API_folder_is_one_level_up()
    {
        // Arrange
        WriteAppSettings(_tempRoot, "Host=parenthost;Database=parentdb;Username=u;Password=p");
        var childDir = Directory.CreateDirectory(Path.Combine(_tempRoot, "child")).FullName;

        // Act
        var result = DesignTimeConnectionString.Read(childDir);

        // Assert
        Assert.Equal("Host=parenthost;Database=parentdb;Username=u;Password=p", result);
    }

    [Fact]
    public void Returns_connection_string_when_API_folder_is_several_levels_up()
    {
        // Arrange — mirrors the real layout: Modules/Catalog/Modules.Catalog.Infrastructure/
        WriteAppSettings(_tempRoot, "Host=deephost;Database=deepdb;Username=u;Password=p");
        var deepDir = Directory.CreateDirectory(
            Path.Combine(_tempRoot, "Modules", "Catalog", "Modules.Catalog.Infrastructure")).FullName;

        // Act
        var result = DesignTimeConnectionString.Read(deepDir);

        // Assert
        Assert.Equal("Host=deephost;Database=deepdb;Username=u;Password=p", result);
    }

    [Fact]
    public void Returns_most_specific_connection_string_when_multiple_API_folders_exist_in_hierarchy()
    {
        // Arrange — inner API wins because the walk stops at the first match
        WriteAppSettings(_tempRoot, "Host=outerhost;Database=outerdb;Username=u;Password=p");
        var childDir = Directory.CreateDirectory(Path.Combine(_tempRoot, "child")).FullName;
        WriteAppSettings(childDir, "Host=innerhost;Database=innerdb;Username=u;Password=p");

        // Act
        var result = DesignTimeConnectionString.Read(childDir);

        // Assert
        Assert.Equal("Host=innerhost;Database=innerdb;Username=u;Password=p", result);
    }

    [Fact]
    public void Throws_when_API_folder_exists_but_appsettings_json_is_missing()
    {
        // Arrange — API directory present but empty
        Directory.CreateDirectory(Path.Combine(_tempRoot, "API"));

        // Act
        var act = () => DesignTimeConnectionString.Read(_tempRoot);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Throws_with_helpful_message_when_appsettings_not_found()
    {
        // Arrange
        var isolated = Directory.CreateDirectory(Path.Combine(_tempRoot, "isolated")).FullName;

        // Act
        var ex = Assert.Throws<InvalidOperationException>(
            () => DesignTimeConnectionString.Read(isolated));

        // Assert
        Assert.Contains("appsettings.json", ex.Message);
        Assert.Contains("dotnet ef", ex.Message);
    }

    private static void WriteAppSettings(string dir, string postgresConnectionString)
    {
        var apiDir = Directory.CreateDirectory(Path.Combine(dir, "API")).FullName;
        File.WriteAllText(
            Path.Combine(apiDir, "appsettings.json"),
            $$"""
            {
              "PersistenceSettings": {
                "connectionStrings": {
                  "postgres": "{{postgresConnectionString}}"
                }
              }
            }
            """);
    }
}
