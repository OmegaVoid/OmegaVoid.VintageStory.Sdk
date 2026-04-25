namespace OmegaVoid.VintageStory.Sdk.Tasks;

// Source - https://stackoverflow.com/a/78761891
// Posted by angularsen
// Retrieved 2026-04-25, License - CC BY-SA 4.0

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///     Write to multiple streams by forwarding writes, reads and seeks.<br />
///     <br />
///     Reads go to the primary stream.<br />
///     Writes and seeks go to all the streams.<br />
///     <see cref="Stream.CanWrite" /> must be true for all streams.<br />
///     <see cref="Stream.CanSeek" /> must be true for secondary streams, if the primary stream is seekable.<br />
/// </summary>
/// <remarks>
///     Legacy async methods are not implemented, such as <see cref="Stream.BeginWrite"/> and <see cref="Stream.EndWrite"/>.
/// </remarks>
public sealed class MultiWriteStream : Stream
{
    private readonly Stream _primaryStream;
    private readonly List<Stream> _secondaryStreams;
    private readonly List<Stream> _streams;

    public MultiWriteStream(Stream primaryStream, Stream[] secondaryStreams)
    {
        _primaryStream = primaryStream ?? throw new ArgumentNullException(nameof(primaryStream));
        if (secondaryStreams.Length == 0) throw new ArgumentException("At least one secondary stream is required");

        if (!primaryStream.CanWrite) throw new ArgumentException("Primary stream must be writable.", nameof(primaryStream));
        if (secondaryStreams.Any(ss => !ss.CanWrite)) throw new ArgumentException("Secondary streams must be writable.", nameof(secondaryStreams));

        if (primaryStream.CanSeek && secondaryStreams.Any(ss => !ss.CanSeek))
            throw new ArgumentException($"The primary was seekable, but one of the secondary streams was not.", nameof(secondaryStreams));

        _streams = [primaryStream, ..secondaryStreams];
        _secondaryStreams = secondaryStreams.ToList();

        CanSeek = primaryStream.CanSeek;
        CanRead = primaryStream.CanRead;
        CanWrite = primaryStream.CanWrite;
    }

    public override bool CanRead { get; }
    public override bool CanSeek { get; }
    public override bool CanWrite { get; }

    public override long Length => _primaryStream.Length;

    public override long Position
    {
        get => _primaryStream.Position;
        set { foreach (Stream stream in _streams) stream.Position = value; }
    }

    public override bool CanTimeout => _primaryStream.CanTimeout;
    public override int ReadTimeout => _primaryStream.ReadTimeout;
    public override int WriteTimeout => _primaryStream.WriteTimeout;

    public override void CopyTo(Stream destination, int bufferSize)
    {
        _primaryStream.CopyTo(destination, bufferSize);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return _primaryStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override void Close()
    {
        foreach (Stream stream in _streams)
            stream.Close();

        base.Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (Stream stream in _streams)
                stream.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        foreach (Stream stream in _streams)
            await stream.DisposeAsync().ConfigureAwait(true);

        await base.DisposeAsync().ConfigureAwait(false);
    }

    public override void Flush()
    {
        foreach (Stream stream in _streams) stream.Flush();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        foreach (Stream stream in _streams)
            await stream.FlushAsync(cancellationToken).ConfigureAwait(true);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _primaryStream.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        return _primaryStream.Read(buffer);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _primaryStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        return _primaryStream.ReadAsync(buffer, cancellationToken);
    }

    public override int ReadByte()
    {
        return _primaryStream.ReadByte();
    }


    public override long Seek(long offset, SeekOrigin origin)
    {
        foreach (Stream stream in _secondaryStreams)
            stream.Seek(offset, origin);

        return _primaryStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        foreach (Stream stream in _streams)
            stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        foreach (Stream stream in _streams)
            stream.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        foreach (Stream stream in _streams)
            stream.Write(buffer);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        foreach (Stream stream in _streams)
            await stream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(true);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new())
    {
        foreach (Stream stream in _streams)
            await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(true);
    }

    public override void WriteByte(byte value)
    {
        foreach (Stream stream in _streams)
            stream.WriteByte(value);
    }
}
