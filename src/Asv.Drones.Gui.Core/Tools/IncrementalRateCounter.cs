using System.Reactive.Linq;
using Asv.Common;

namespace Asv.Drones.Gui.Core
{
    public class IncrementalRateCounter
    {
        private readonly CircularBuffer2<double> _valueBuffer;
        private long _lastValue = long.MaxValue;
        private DateTime _lastUpdated = DateTime.MaxValue;

        public IncrementalRateCounter(int movingAverageSize = 5)
        {
            _valueBuffer = new CircularBuffer2<double>(movingAverageSize);
        }

        public double Calculate(long sum)
        {
            var now = DateTime.Now;
            var deltaSeconds = (now - _lastUpdated).TotalSeconds;
            if (deltaSeconds <= 0) deltaSeconds = 1;
            _lastUpdated = now;
            var value = (sum - _lastValue) / deltaSeconds;
            if (value >= 0)
            {
                _valueBuffer.PushFront(value);
            }
            _lastValue = sum;
            return _valueBuffer.IsFull ? _valueBuffer.Average() : double.NaN;
        }
        
    }
}