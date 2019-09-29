﻿using System.Collections.Generic;

namespace PInvoke.Server.Model
{
    public class SourceInfo
    {
        public string Name { get; set; }
        public IEnumerable<string> Libraries { get; set; }
        
        public override string ToString() => Name;
    }
}
