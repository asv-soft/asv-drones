using System.Linq.Expressions;
using System.Reactive.Linq;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Asv.Drones.Gui.Api;

public static class LocalizationHelper
{
    public static IDisposable BindMeasureUnit<TObject, TProperty>(this TObject source,
        Expression<Func<TObject, TProperty>> valuePropertyAccessor, Action<TProperty> setter,
        Expression<Func<TObject, string?>> stringPropertyAccessor, Action<string> stringSetter,
        IMeasureUnitItem<TProperty> measureUnit)
        where TObject : IReactiveObject, IValidatableViewModel
    {
        return new MeasureUnitBind<TObject, TProperty>(source, valuePropertyAccessor, setter, stringPropertyAccessor,
            stringSetter, measureUnit);
    }

    class MeasureUnitBind<TObject, TProperty> : IDisposable
        where TObject : IReactiveObject, IValidatableViewModel
    {
        private readonly ValidationHelper _sub1;
        private readonly IDisposable _sub2;
        private readonly IDisposable _sub3;

        public MeasureUnitBind(TObject source,
            Expression<Func<TObject, TProperty>> valueProperty, Action<TProperty> setter,
            Expression<Func<TObject, string?>> stringProperty, Action<string> stringSetter,
            IMeasureUnitItem<TProperty> measureUnit)
        {
            _sub1 = source.ValidationRule(stringProperty, measureUnit.IsValid,
                x => measureUnit.GetErrorMessage(x) ?? string.Empty);
            _sub2 = source.WhenValueChanged(valueProperty).Select(measureUnit.FromSiToString!).Subscribe(stringSetter);
            _sub3 = source.WhenValueChanged(stringProperty).Select(measureUnit.ConvertToSi).Subscribe(setter);
        }

        public void Dispose()
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
        }
    }
}