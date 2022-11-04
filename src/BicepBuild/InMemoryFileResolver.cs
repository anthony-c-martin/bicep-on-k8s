// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.IO.Abstractions.TestingHelpers;
using Bicep.Core.FileSystem;

namespace BicepBuild;

public class InMemoryFileResolver : FileResolver
{
    public MockFileSystem MockFileSystem { get; }

    public InMemoryFileResolver(MockFileSystem fileSystem)
        : base(fileSystem)
    {
        MockFileSystem = fileSystem;
    }

    public InMemoryFileResolver(IReadOnlyDictionary<Uri, string> fileLookup)
        : this(new MockFileSystem(fileLookup.ToDictionary(x => x.Key.LocalPath, x => new MockFileData(x.Value))))
    {
    }

    public InMemoryFileResolver(IReadOnlyDictionary<Uri, MockFileData> fileLookup)
        : this(new MockFileSystem(fileLookup.ToDictionary(x => x.Key.LocalPath, x => x.Value)))
    {
    }
}