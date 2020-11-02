using System;
using System.Collections.Generic;
using System.Text;

namespace DevNet.Online
{
    public class Application
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string PrivateKey { get; set; }

        public Uri RedirectUri { get; set; }


    }
}
