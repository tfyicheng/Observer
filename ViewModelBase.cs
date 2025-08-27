using System;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Windows.Input;

namespace Observer
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion // ICommand Members
    }

    public class ViewModelBase : System.Dynamic.DynamicObject, INotifyPropertyChanged
    {
        protected object wrappedObject;

        public ViewModelBase(object wrappedObject)
        {
            this.wrappedObject = wrappedObject;
        }

        public object WrappedObject
        {
            get { return this.wrappedObject; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string propertyName = binder.Name;
            if (this.WrappedObject == null)
            {
                result = null;
                return false;
            }

            PropertyInfo property = this.WrappedObject.GetType().GetProperty(propertyName);
            if (property == null || property.CanRead == false)
            {
                result = null;
                return false;
            }

            result = property.GetValue(this.WrappedObject, null);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string propertyName = binder.Name;
            if (this.WrappedObject == null)
            {
                return false;
            }

            PropertyInfo property = this.WrappedObject.GetType().GetProperty(propertyName);

            if (property == null || property.CanWrite == false)
            {
                return false;
            }
            property.SetValue(this.WrappedObject, value, null);
            this.OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        //public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, String peopertyName)
        {
            if (object.Equals(storage, value)) { return false; }
            storage = value;
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(peopertyName));
            }
            return true;
        }

        #endregion
    }
}
