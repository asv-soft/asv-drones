namespace Asv.Drones.Gui.Sdr;

public class SeekStream : Stream
{
    private Stream _stream;
    private long _position;
    private bool _disposed = false;
    private readonly Func<Stream> _factory;

    public SeekStream(Func<Stream> factory)
    {
        _factory = factory;
        _stream = _factory();
        _position = 0;
    }
    
    public override long Length => _stream.Length;

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => true;

    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        // must do some calcs if origin is not Begin
        var pos = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            _ => this.Length - offset
        };
            
        // if need go back, must reset stream
        if (_position > pos)
        {
            _stream.Dispose();
            _stream = _factory();
            _position = 0;
        }
        
        var size = pos - _position;
        _position = pos;
        return Read(new byte[size], 0, (int)size);
    }
    
    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream.Read(buffer, offset, count);
    }    

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
    }    
    
    ~SeekStream()
    {
        Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!_disposed)
        {
            _stream.Dispose();
            _disposed = true;
        }
    }
}