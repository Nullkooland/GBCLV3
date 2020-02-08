using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace GBCLV3.Utils
{
    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore() => new BindingProxy();

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
