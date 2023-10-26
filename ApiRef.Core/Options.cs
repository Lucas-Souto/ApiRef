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
        public bool FilterPublic = true;
        public string LibraryPath = @"C:\Users\Lucas\Desktop\Claw Engine\Claw\Claw.dll",
            OutputDirectory = @"C:\Users\Lucas\Desktop\ClawDocs\API",
            RootPath = "/API";
    }
}