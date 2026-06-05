using System.Buffers.Binary;
using System.Text;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using R3;

namespace Asv.Drones.Api;

public class MavParamTextBoxPropertyViewModel
    : PropertyTextBoxViewModel,
        IMavParamPropertyViewModel,
        ISupportRefresh
{
    private readonly IMavParamContext _context;

    public const string TypeId = nameof(MavParamWidgetType.TextBox);

    public MavParamTextBoxPropertyViewModel(IMavParamContext context)
        : base(context.Info.Id)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        MavValue = Info.Metadata.DefaultValue;
        this.ApplyMavParamMetadata(Info);
        Units = Info.Metadata.Units;

        Text.EnableValidation(ValidateText).DisposeItWith(Disposable);
        ApplyTextFromMavValue(MavValue, false);

        Client
            .Filter(Info.Metadata.Name)
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(
                ApplyRemoteValue,
                error =>
                {
                    if (error.Exception is not null)
                    {
                        ApplyErrorFromModel(error.Exception);
                    }
                }
            )
            .DisposeItWith(Disposable);

        MavParamPropertyEditorMetadata.ScheduleInitialRead(Refresh).DisposeItWith(Disposable);
    }

    public MavParamInfo Info => _context.Info;

    protected IParamsClientEx Client => _context.Client;

    public MavParamValue MavValue
    {
        get;
        private set => SetField(ref field, value);
    }

    public async ValueTask Refresh(CancellationToken cancel = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            ApplyRemoteValue(await Client.GetFromCacheOrReadOnce(Info.Metadata.Name, cancel));
        }
        catch (Exception e)
        {
            ApplyErrorFromModel(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual string ValueToText(ValueType remoteValue)
    {
        return Info.Print(remoteValue) ?? string.Empty;
    }

    protected virtual Exception? TextToValue(string? valueAsString, out ValueType value)
    {
        return Info.ValidateString(valueAsString ?? string.Empty, out value);
    }

    protected override async ValueTask ApplyFromUser(CancellationToken cancel)
    {
        var error = TextToValue(Text.Value, out var value);
        if (error is not null)
        {
            throw error;
        }

        var mavValue = Info.Convert(value);
        await Client.WriteOnce(Info.Metadata.Name, mavValue, cancel);
        MavValue = mavValue;
        var text = ValueToText(value);
        ApplyTextFromModel(text, text, true, true);
    }

    private Exception? ValidateText(string? value)
    {
        try
        {
            return TextToValue(value, out _);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private void ApplyRemoteValue(MavParamValue value)
    {
        if (IsInEditMode)
        {
            return;
        }

        ApplyTextFromMavValue(value, true);
    }

    private void ApplyTextFromMavValue(MavParamValue value, bool markUpdated)
    {
        MavValue = value;
        var text = ValueToText(Info.Convert(value));
        ApplyTextFromModel(text, text, true, markUpdated);
    }
}

public sealed class MavParamAsciiCharPropertyViewModel : MavParamTextBoxPropertyViewModel
{
    public new const string TypeId = nameof(MavParamWidgetType.AsciiChars);

    public MavParamAsciiCharPropertyViewModel(IMavParamContext context)
        : base(context) { }

    protected override string ValueToText(ValueType remoteValue)
    {
        Span<byte> raw = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(raw, Convert.ToInt32(remoteValue));

        var builder = new StringBuilder(4);
        foreach (var rawByte in raw)
        {
            var character = (char)rawByte;
            if (
                !char.IsControl(character)
                && !char.IsWhiteSpace(character)
                && char.IsLetterOrDigit(character)
            )
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    protected override Exception? TextToValue(string? valueAsString, out ValueType value)
    {
        if (string.IsNullOrWhiteSpace(valueAsString))
        {
            value = 0;
            return null;
        }

        var filtered = string.Concat(
            valueAsString.Where(c =>
                !char.IsControl(c) && !char.IsWhiteSpace(c) && char.IsLetterOrDigit(c)
            )
        );

        if (filtered.Length == 0)
        {
            value = 0;
            return null;
        }

        if (filtered.Length > 4)
        {
            filtered = filtered[..4];
        }

        Span<byte> buffer = stackalloc byte[4];
        Encoding.ASCII.GetBytes(filtered, buffer);
        value = BinaryPrimitives.ReadInt32BigEndian(buffer);
        return null;
    }
}
