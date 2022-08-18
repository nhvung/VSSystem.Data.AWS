using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VSSystem.Data.AWS.Models
{
    class MemoryStream64 : System.IO.Stream
    {

        const int BUFFER_SIZE = 1024 * 1024 * 1024;
        public const int MAX_LENGTH_PER_PARTITION_STREAM = int.MaxValue - 56;
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;
        public override long Length { get { return _subStreams?.Sum(ite => ite.Length) ?? 0; } }

        long _Position;
        public override long Position { get { return _Position; } set { _Position = value; } }
        List<Stream> _subStreams;
        int _currentStreamIndex;
        long _subStreamPostition;
        int _partitionLength;

        public MemoryStream64(int partitionMaximumLength = MAX_LENGTH_PER_PARTITION_STREAM)
        {
            _subStreams = new List<Stream>();
            _Position = 0;
            _currentStreamIndex = 0;
            _partitionLength = partitionMaximumLength;
        }
        public MemoryStream64(Stream s, int partitionMaximumLength = MAX_LENGTH_PER_PARTITION_STREAM)
        {
            _subStreams = new List<Stream>();
            _Position = 0;
            _currentStreamIndex = 0;
            _partitionLength = partitionMaximumLength;
            _Init(s);
        }

        void _Init(Stream s)
        {
            AppendStream(s);
            _currentStreamIndex = 0;
            _subStreamPostition = 0;
            _Position = 0;
        }
        public override void Flush()
        {

        }

        public long AppendStream(Stream stream)
        {
            long position = -1;
            try
            {
                if (stream != null)
                {
                    Seek(0, SeekOrigin.End);
                    position = _Position;
                    int read = -1;
                    byte[] buffer = new byte[BUFFER_SIZE];
                    while ((read = stream.Read(buffer, 0, BUFFER_SIZE)) > 0)
                    {
                        Write(buffer, 0, read);
                    }
                }
            }
            catch { }
            return position;
        }
        async public Task CopyToAsync(Stream destination, long offset, long length, int bufferSize = BUFFER_SIZE, CancellationToken cancellationToken = default)
        {
            try
            {
                if (destination != null && offset > 0 && length > 0)
                {
                    if (offset > 0)
                    {
                        Seek(offset, SeekOrigin.Begin);
                    }
                    if (length > 0)
                    {
                        long tLength = length;
                        int read = 0;
                        byte[] buffer = new byte[bufferSize];
                        do
                        {
                            int readCount = bufferSize < tLength ? bufferSize : (int)tLength;
                            read = await ReadAsync(buffer, 0, readCount, cancellationToken);
                            if (read > 0)
                            {
                                await destination.WriteAsync(buffer, 0, read, cancellationToken);
                                tLength -= read;
                            }
                        } while (read > 0 && tLength > 0);
                    }
                }
            }
            catch { }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).Result;
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int result = 0;
            try
            {
                Stream stream = null;
                if (_currentStreamIndex < _subStreams.Count)
                {
                    stream = _subStreams[_currentStreamIndex];
                    if (stream != null)
                    {
                        stream.Seek(_subStreamPostition, SeekOrigin.Begin);
                        long remainLength = stream.Length - _subStreamPostition;
                        if (count <= remainLength)
                        {
                            result = await stream.ReadAsync(buffer, offset, count, cancellationToken);
                            _subStreamPostition = stream.Position;
                        }
                        else
                        {
                            int tCount = count;
                            int newOffset = offset;
                            int newCount = (int)remainLength;
                            int tLength = await stream.ReadAsync(buffer, newOffset, newCount, cancellationToken);
                            newOffset += tLength;
                            tCount -= tLength;
                            _currentStreamIndex++;
                            _subStreamPostition = 0;
                            result = tLength + await ReadAsync(buffer, newOffset, tCount, cancellationToken);
                        }
                    }
                }
            }
            catch { }
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long position = -1;
            try
            {
                long tOffset = offset;
                _currentStreamIndex = 0;
                if (origin == SeekOrigin.Begin)
                {
                    _Position = 0;
                    for (int i = 0; i < _subStreams.Count; i++)
                    {
                        _currentStreamIndex = i;
                        if (tOffset >= _subStreams[i].Length)
                        {
                            tOffset -= _subStreams[i].Length;
                            position += _subStreams[i].Length;
                            _Position += _subStreams[i].Length;
                        }
                        else
                        {
                            if (tOffset > 0)
                            {
                                position += tOffset;
                                _Position += tOffset;
                            }
                            _subStreamPostition = tOffset;
                            break;
                        }
                    }
                }
                else if (origin == SeekOrigin.Current)
                {
                    for (int i = 0; i < _subStreams.Count; i++)
                    {
                        _currentStreamIndex = i;
                        if (tOffset >= _subStreams[i].Length)
                        {
                            tOffset -= _subStreams[i].Length;
                            position += _subStreams[i].Length;
                            _Position += _subStreams[i].Length;
                        }
                        else
                        {
                            if (tOffset > 0)
                            {
                                position += tOffset;
                                _Position += tOffset;
                            }
                            _subStreamPostition = tOffset;
                            break;
                        }
                    }
                }
                else if (origin == SeekOrigin.End)
                {
                    _Position = Length;
                    position = Length;
                    for (int i = _subStreams.Count - 1; i >= 0; i--)
                    {
                        _currentStreamIndex = i;
                        if (tOffset >= _subStreams[i].Length)
                        {
                            tOffset -= _subStreams[i].Length;
                            position -= _subStreams[i].Length;
                            _Position -= _subStreams[i].Length;
                        }
                        else
                        {
                            if (tOffset > 0)
                            {
                                position -= tOffset;
                                _Position -= tOffset;
                            }
                            _subStreamPostition = _subStreams[i].Length - tOffset;
                            break;
                        }
                    }
                }
            }
            catch { }
            return position;
        }

        public override void SetLength(long value)
        {
            try
            {
                _subStreams = new List<Stream>();
                long tLength = value;
                do
                {
                    int subLength = tLength > _partitionLength ? _partitionLength : (int)tLength;
                    var subStream = new MemoryStream(subLength);
                    subStream.SetLength(subLength);
                    _subStreams.Add(subStream);
                    tLength -= subLength;
                } while (tLength > 0);
            }
            catch
            {
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count).Wait();
        }
        async public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                if (_subStreams == null)
                {
                    _subStreams = new List<Stream>();
                    _currentStreamIndex = 0;
                    _subStreamPostition = 0;
                }
                if (_subStreams.Count == 0)
                {
                    _subStreams.Add(new MemoryStream(_partitionLength));
                }

                Stream currentStream = null;

                if (_currentStreamIndex < _subStreams.Count)
                {
                RetryGetCurrentStream:
                    currentStream = _subStreams[_currentStreamIndex];
                    if (currentStream == null)
                    {
                        _subStreams[_currentStreamIndex] = new MemoryStream(_partitionLength);
                        goto RetryGetCurrentStream;
                    }

                    long remainLength = _partitionLength - _subStreamPostition;
                    if (remainLength > 0)
                    {

                        currentStream.Seek(_subStreamPostition, SeekOrigin.Begin);
                        int remainBufferOffset = 0, remainBufferCount = 0;
                        if ((int)remainLength < count)
                        {
                            remainBufferCount = count - (int)remainLength;
                            remainBufferOffset = offset + (int)remainLength;
                            await currentStream.WriteAsync(buffer, offset, (int)remainLength, cancellationToken);
                        }
                        else
                        {
                            await currentStream.WriteAsync(buffer, offset, count, cancellationToken);
                        }

                        if (remainBufferCount > 0 && remainBufferOffset > 0)
                        {
                            _subStreams.Add(new MemoryStream(_partitionLength));
                            _currentStreamIndex++;
                            currentStream = _subStreams[_currentStreamIndex];
                            await currentStream.WriteAsync(buffer, remainBufferOffset, remainBufferCount, cancellationToken);
                        }

                        _subStreamPostition = currentStream.Position;
                    }
                    else
                    {
                        _subStreams.Add(new MemoryStream(_partitionLength));
                        _currentStreamIndex++;
                        goto RetryGetCurrentStream;
                    }
                }

                _Position = 0;
                for (int i = 0; i < _currentStreamIndex; i++)
                {
                    _Position += _subStreams[i].Length;
                }
                _Position += _subStreamPostition;
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_subStreams?.Count > 0)
                    {
                        for (int i = 0; i < _subStreams.Count; i++)
                        {
                            _subStreams[i].Close();
                            _subStreams[i].Dispose();
                        }
                        _subStreams = null;
                    }
                }
            }
            catch { }
        }

    }


}