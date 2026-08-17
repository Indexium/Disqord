using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Gateway.Default;

namespace Disqord.Tests.Unit.Gateway;

public class DefaultGatewayChunkerChunkOperationTests
{
    private static readonly Type ChunkOperationType = typeof(DefaultGatewayChunker)
        .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
        .Single(type => type.Name == "ChunkOperation");

    [Test]
    public void OnChunk_CalledImmediatelyAfterConstruction_DoesNotThrow()
    {
        // Arrange
        using var operation = CreateOperation(TimeSpan.FromSeconds(30));
        var onChunk = ChunkOperationType.GetMethod("OnChunk")!;

        // Act & Assert
        Assert.That(() => onChunk.Invoke(operation.Instance, null), Throws.Nothing);
    }

    [Test]
    public void Throw_CalledDuringChunkProcessing_SurfacesTheExceptionToTheWaiter()
    {
        // Arrange
        using var operation = CreateOperation(TimeSpan.FromSeconds(30));
        var task = operation.WaitAsync();
        var exception = new InvalidOperationException("Simulated processing failure.");

        // Act
        operation.Throw(exception);

        // Assert
        Assert.That(task.IsFaulted, Is.True);
        Assert.That(task.Exception!.InnerException, Is.SameAs(exception));
    }

    private static OperationHandle CreateOperation(TimeSpan timeout)
    {
        var constructor = ChunkOperationType.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(TimeSpan), typeof(bool), typeof(CancellationToken) },
            null)!;
        var instance = constructor.Invoke(new object[] { timeout, false, CancellationToken.None });
        return new OperationHandle(instance);
    }

    private sealed class OperationHandle(object instance) : IDisposable
    {
        public object Instance { get; } = instance;

        public Task WaitAsync()
        {
            var method = ChunkOperationType.GetMethod("WaitAsync")!;
            return (Task) method.Invoke(Instance, null)!;
        }

        public void Throw(Exception exception)
        {
            var method = ChunkOperationType.GetMethod("Throw")!;
            method.Invoke(Instance, new object[] { exception });
        }

        public void Dispose()
        {
            var method = ChunkOperationType.GetMethod("Dispose")!;
            method.Invoke(Instance, null);
        }
    }
}
