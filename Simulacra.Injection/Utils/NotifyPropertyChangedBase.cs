using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Simulacra.Injection.Utils
{
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(value, field))
                return false;

            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}