using System;
using System.Collections.Generic;
using System.Text;

namespace DevNet.Online
{
    public class ApplicationContext
    {
        public Application Application { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        
        public Tenant Tenant { get; set; }

    }
}
