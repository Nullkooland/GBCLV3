using System;
using System.Collections.Generic;
using System.Text;

namespace GBCLV3.Models.Auxiliary
{
    public class ShaderPack
    {
        public string Name => System.IO.Path.GetFileName(Path);

        public string Path { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsExtracted { get; set; }
    }
}
