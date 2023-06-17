using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DiabloDungeonTimer.Core.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected ViewModelBase()
    {
#if DEBUG
        ThrowOnInvalidPropertyName = true;
        PropertyChanging += (_, args) =>
        {
            if (args.PropertyName != null)
                VerifyPropertyName(args.PropertyName);
        };
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != null)
                VerifyPropertyName(args.PropertyName);
        };
#endif
    }

    private bool ThrowOnInvalidPropertyName { get; }

    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    private void VerifyPropertyName(string propertyName)
    {
        if (TypeDescriptor.GetProperties(this)[propertyName] != null)
            return;
        string message = "Invalid property name: " + propertyName;
        if (ThrowOnInvalidPropertyName)
            throw new Exception(message);
        Debug.Fail(message);
    }
}