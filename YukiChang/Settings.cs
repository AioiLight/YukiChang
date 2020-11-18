using System.Collections.Generic;

namespace YukiChang
{
    public class Settings
    {
        public Settings()
        {
            Servers = new List<Server>();
        }


        public List<Server> Servers { get; set; }
    }
}
