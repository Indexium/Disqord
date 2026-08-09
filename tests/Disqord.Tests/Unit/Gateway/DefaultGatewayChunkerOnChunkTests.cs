using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Gateway.Api.Models;
using Disqord.Gateway.Default;
using Disqord.Models;
using Disqord.Utilities.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Disqord.Tests.Unit.Gateway;

public class DefaultGatewayChunkerOnChunkTests
{
    private static readonly Type ChunkOperationType = typeof(DefaultGatewayChunker)
        .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
        .Single(type => type.Name == "ChunkOperation");

    // Simulates the outcome of a Dispose()/OnChunk() race by disposing the timeout
    // Cts's inner CancellationTokenSource directly, bypassing Cts's own disposed flag.
    [Test]
    public void OnChunk_OperationTimeoutCtsDisposedConcurrently_SurfacesTheExceptionToTheWaiterInsteadOfSwallowingIt()
    {
        // Arrange
        var chunker = new DefaultGatewayChunker(
            Options.Create(new DefaultGatewayChunkerConfiguration()),
            NullLogger<DefaultGatewayChunker>.Instance);

        var constructor = ChunkOperationType.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(TimeSpan), typeof(bool), typeof(CancellationToken) },
            null)!;
        var operation = constructor.Invoke(new object[] { TimeSpan.FromSeconds(30), true, CancellationToken.None })!;

        var timeoutCts = (Cts) ChunkOperationType
            .GetField("_timeoutCts", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(operation)!;
        var innerCts = (CancellationTokenSource) typeof(Cts)
            .GetField("_cts", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(timeoutCts)!;
        innerCts.Dispose();

        var waitTask = (Task<IReadOnlyDictionary<Snowflake, IMember>?>) ChunkOperationType
            .GetMethod("WaitAsync")!
            .Invoke(operation, null)!;

        var nonce = (string) ChunkOperationType.GetProperty("Nonce")!.GetValue(operation)!;

        var operations = typeof(DefaultGatewayChunker)
            .GetField("_operations", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(chunker)!;
        operations.GetType()
            .GetMethod("Add", new[] { typeof(string), ChunkOperationType })!
            .Invoke(operations, new[] { (object) nonce, operation });

        var model = new GuildMembersChunkJsonModel
        {
            GuildId = default,
            Members = Array.Empty<MemberJsonModel>(),
            ChunkIndex = 0,
            ChunkCount = 2,
            Nonce = nonce
        };

        // Act
        chunker.OnChunk(model);

        // Assert
        Assert.That(waitTask.IsFaulted, Is.True);
        Assert.That(waitTask.Exception!.InnerException, Is.InstanceOf<ObjectDisposedException>());
    }
}
