using System;
using System.Collections.Generic;
using System.Text;

namespace ApiRef.Core
{
    /// <summary>
    /// Opções de geração para <see cref="ApiReference"/>.
    /// </summary>
    public class Options
    {
        public bool FilterPublic;
        public string LibraryPath, OutputDirectory, RootPath;
    }
}