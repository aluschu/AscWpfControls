using System.Collections.ObjectModel;

namespace AscWpfControls.Controls;

/// <summary>
/// The actions inside a field. Its own type rather than a bare collection so that XAML can fill it
/// without a type argument in sight.
/// </summary>
public sealed class FieldActionCollection : ObservableCollection<FieldAction>
{
}
