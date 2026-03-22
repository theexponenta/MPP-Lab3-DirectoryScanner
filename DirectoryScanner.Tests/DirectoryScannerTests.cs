namespace DirectoryScanner.Tests;

using System.Text;
using DirectoryScanner.Core;

public class DirectoryScannerTests
{
    private void AssertScanInfoEqualsDirectoryInfo(DirectoryScanInfo scanInfo, FakeDirectoryInfo directoryInfo)
    {
        Assert.Equal(scanInfo.DirectoriesCount, directoryInfo.DirectoriesCount);
        Assert.Equal(scanInfo.FilesCount, directoryInfo.FilesCount);
        Assert.Equal(scanInfo.IsSymlink, directoryInfo.LinkTarget is not null);

        foreach (DirectoryScanInfo subdirScanInfo in scanInfo.EnumerateDirectories())
        {
            FakeDirectoryInfo? subdirInfo = directoryInfo.GetDirectory(subdirScanInfo.Name);
            Assert.NotNull(subdirInfo);
            AssertScanInfoEqualsDirectoryInfo(subdirScanInfo, subdirInfo);
        }

        foreach (FileScanInfo fileScanInfo in scanInfo.EnumerateFiles())
        {
            Assert.True(directoryInfo.HasFile(fileScanInfo.Name));
        }
    }

    [Theory]
    [InlineData(1, 5, 3, 4)]
    [InlineData(5, 10, 6, 5)]
    [InlineData(100, 10, 8, 5)]
    public void TestScanResultStructure(int maxThreads, int maxFiles, int maxDirectories, int maxDepth)
    {
        FakeDirectoryInfo directory = TestUtils.RandomDirectoryInfo(maxFiles, maxDirectories, maxDepth);

        FakeDirectoryProvider provider = new FakeDirectoryProvider(directory);
        DirectoryScanner scanner = new DirectoryScanner(provider);
        scanner.ScanSync("/", maxThreads);

        Assert.NotNull(scanner.Result);
        AssertScanInfoEqualsDirectoryInfo(scanner.Result, directory);
    }

    [Fact]
    public void TestDirectorySize()
    {
        FakeDirectoryInfo directory = new FakeDirectoryInfo(
            "Dir1",
            null,
            new List<FakeFileInfo> {
                new FakeFileInfo("file1", null, 100),
                new FakeFileInfo("file2", null, 200),
                new FakeFileInfo("file3", "sosi", 300),
            },
            new List<FakeDirectoryInfo> {
                new FakeDirectoryInfo(
                    "Dir2",
                    null,
                    new List<FakeFileInfo> {
                        new FakeFileInfo("file4", null, 400)
                    },
                    new List<FakeDirectoryInfo>()
                ),
                new FakeDirectoryInfo(
                    "Dir3",
                    null,
                    new List<FakeFileInfo> {
                        new FakeFileInfo("file5", null, 500)
                    },
                    new List<FakeDirectoryInfo> {
                        new FakeDirectoryInfo(
                            "Dir4",
                            "sosi2",
                            new List<FakeFileInfo> {
                                new FakeFileInfo("file6", null, 600),
                                new FakeFileInfo("file7", null, 700)
                            },
                            new List<FakeDirectoryInfo>()
                        )
                    }
                )
            }
        );

        FakeDirectoryProvider provider = new FakeDirectoryProvider(directory);
        DirectoryScanner scanner = new DirectoryScanner(provider);
        scanner.ScanSync("/", 2);

        Assert.NotNull(scanner.Result);
        Assert.Equal(1200, scanner.Result.Size);
    }

    [Fact]
    public void TestRecursiveSymlinks()
    {
        FakeDirectoryInfo dir1 = new FakeDirectoryInfo(
            "Dir1",
            null,
            new List<FakeFileInfo> {
                new FakeFileInfo("file1", null, 100),
                new FakeFileInfo("file2", null, 200),
            },
            new List<FakeDirectoryInfo> {
                new FakeDirectoryInfo(
                    "Dir2",
                    null,
                    new List<FakeFileInfo> {
                        new FakeFileInfo("file4", null, 400)
                    },
                    new List<FakeDirectoryInfo>()
                ),
                new FakeDirectoryInfo(
                    "Dir3",
                    null,
                    new List<FakeFileInfo> {
                        new FakeFileInfo("file5", null, 500)
                    },
                    new List<FakeDirectoryInfo>()
                )
            }
        );
        
        FakeDirectoryInfo dir2 = dir1.GetDirectory("Dir2")!;
        FakeDirectoryInfo dir3 = dir1.GetDirectory("Dir3")!;

        FakeDirectoryInfo symlink1 = new FakeDirectoryInfo("symlink1", "/", dir1);
        FakeDirectoryInfo symlink2 = new FakeDirectoryInfo("symlink2", "/", dir1);

        dir2.AddDirectory(symlink1);
        dir3.AddDirectory(symlink2);
        
        FakeDirectoryProvider provider = new FakeDirectoryProvider(dir1);
        DirectoryScanner scanner = new DirectoryScanner(provider);
        scanner.ScanSync("/", 3);
    }

    [Fact]
    public void TestCancellation()
    {
        FakeDirectoryInfo dir1 = new FakeDirectoryInfo(
            "Dir1",
            null,
            new List<FakeFileInfo>(),
            new List<FakeDirectoryInfo> {
                new FakeDirectoryInfo(
                    "Dir2",
                    null,
                    new List<FakeFileInfo> {
                        new FakeFileInfo("file1", null, 400),
                        new FakeFileInfo("file2", null, 100),
                        new FakeFileInfo("file3", null, 200)
                    },
                    new List<FakeDirectoryInfo>(),
                    25
                ),
                new FakeDirectoryInfo(
                    "Dir3",
                    null,
                    new List<FakeFileInfo> {
                        new FakeFileInfo("file4", null, 100),
                        new FakeFileInfo("file5", null, 00)
                    },
                    new List<FakeDirectoryInfo>()
                ),
                new FakeDirectoryInfo(
                    "Dir4",
                    null,
                    new List<FakeFileInfo> {
                        new FakeFileInfo("file6", null, 100),
                    },
                    new List<FakeDirectoryInfo>()
                ),
            },
            25
        );

        FakeDirectoryProvider provider = new FakeDirectoryProvider(dir1);
        DirectoryScanner scanner = new DirectoryScanner(provider);
        scanner.ScanSync("/", 2);

        DirectoryScanInfo result = scanner.Result!;
        Assert.Equal(3, result.DirectoriesCount);

        DirectoryScanInfo dir3 = result.GetDirectory("Dir3")!;
        DirectoryScanInfo dir4 = result.GetDirectory("Dir3")!;

        Assert.Equal(0, dir3.DirectoriesCount);
        Assert.Equal(0, dir4.DirectoriesCount);
    }
}
