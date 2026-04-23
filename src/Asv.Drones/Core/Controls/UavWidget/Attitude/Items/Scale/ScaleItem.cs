using System;
using Avalonia;

namespace Asv.Drones;

public partial class ScaleItem : AvaloniaObject
{
    public ScaleItem(
        double value,
        double valueRange,
        int index,
        int itemCount,
        double fullLength,
        double length,
        bool isInverse = false,
        bool showNegative = true,
        string? fixedTitle = null
    )
    {
        _valueRange = valueRange;
        _showNegative = showNegative;
        _isFixedTitle = fixedTitle != null;
        var step = fullLength / (itemCount % 2 != 0 ? itemCount - 1 : itemCount);
        _positionStep = step / valueRange;

        if (!isInverse)
        {
            _startPosition = ((length - fullLength) / 2.0) + (step * index);
        }
        else
        {
            _startPosition = ((length - fullLength) / 2.0) + (step * (itemCount - index));
            _positionStep *= -1;
        }

        var centerIndex = itemCount % 2 == 0 ? itemCount / 2 : (itemCount / 2) + 1;

        var indexOffset = index - centerIndex;
        _valueOffset = -1 * valueRange * indexOffset;

        if (_isFixedTitle)
        {
            Title = fixedTitle;
        }

        UpdateValue(value);
    }

    public void UpdateValue(double value)
    {
        Value = GetValue(value);
        Position = GetPosition(value);
        if (!_isFixedTitle)
        {
            Title = GetTitle(Value);
        }

        IsVisible = _showNegative || Value >= 0;
    }

    protected virtual string? GetTitle(double value)
    {
        return Math.Round(value).ToString("F0");
    }

    private double GetValue(double value)
    {
        return Math.Round(value) - (Math.Round(value) % _valueRange) + _valueOffset;
    }

    private double GetPosition(double value)
    {
        return _startPosition + (_positionStep * (Math.Round(value) % _valueRange));
    }
}
