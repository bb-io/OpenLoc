using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Tests.OpenLoc.Base;

public class InMemoryFileManager(byte[]? input = null) : IFileManagementClient
{
    public byte[]? StoredBytes { get; private set; }
    public FileReference? StoredFile { get; private set; }

    public Task<Stream> DownloadAsync(FileReference reference) =>
        Task.FromResult<Stream>(new ForwardOnlyStream(input ?? []));

    public async Task<FileReference> UploadAsync(Stream stream, string contentType, string fileName)
    {
        StoredBytes = await stream.GetByteData();
        StoredFile = new FileReference { Name = fileName, ContentType = contentType };
        return StoredFile;
    }

    private sealed class ForwardOnlyStream(byte[] bytes) : Stream
    {
        private readonly MemoryStream inner = new(bytes, writable: false);
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => inner.ReadAsync(buffer, cancellationToken);
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
    }
}
