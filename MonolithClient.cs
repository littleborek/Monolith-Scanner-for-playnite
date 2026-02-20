using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monolith
{
    public class MonolithClient : LibraryClient
    {
        public override bool IsInstalled => true;

        public override void Open()
        {
            // No client to open for Monolith
        }
    }
}