using System;

namespace Protoinject
{
    public class ValueChangedEventArgs : EventArgs
    {
        public object OldValue { get; set; }

        public object NewValue { get; set; }
    }
}