using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using R3;

namespace Asv.Drones;

public class RenameDialogViewModel : DialogViewModelBase
{
    private const string DialogId = "rename.dialog";
    private const int MaxNameLenght = 255;
    private const string AllowedCharacters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789._-()[]{} ";
    private static readonly HashSet<char> AllowedCharSet = AllowedCharacters.ToHashSet();

    public RenameDialogViewModel()
        : base(DialogId, DesignTime.LoggerFactory)
    {
        NewName = new BindableReactiveProperty<string>(string.Empty);
        NewName
            .EnableValidationRoutable(NameValidator, this, isForceValidation: true)
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string> NewName { get; }

    private static ValidationResult NameValidator(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            return ValidationResult.FailAsNullOrWhiteSpace;
        }

        if (
            arg.Any(Path.GetInvalidFileNameChars().Contains)
            || arg.Any(c => !AllowedCharSet.Contains(c))
        )
        {
            return new ValidationResult
            {
                IsSuccess = false,
                ValidationException = new ValidationException("Name contains invalid characters"),
            };
        }

        if (arg.Length > MaxNameLenght)
        {
            return ValidationResult.FailAsOutOfRange("1", MaxNameLenght.ToString());
        }

        return ValidationResult.Success;
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        _sub1?.Dispose();

        dialog.DefaultButton = ContentDialogButton.Primary;
        _sub1 = IsValid.Subscribe(b => dialog.IsPrimaryButtonEnabled = b);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    private IDisposable? _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1?.Dispose();
            NewName.Dispose();
        }

        base.Dispose(disposing);
    }
}
