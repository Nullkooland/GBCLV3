using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GBCLV3.Models.Auxiliary
{
    public class ShaderPack : INotifyPropertyChanged
    {
        [DoNotNotify]
        public string Id => System.IO.Path.GetFileName(Path);

        [DoNotNotify]
        public string Path { get; set; }

        public bool IsEnabled { get; set; }

        [DoNotNotify]
        public bool IsExtracted { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
