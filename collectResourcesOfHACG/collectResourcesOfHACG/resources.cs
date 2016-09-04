using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace collectResourcesOfHACG
{
    public struct baidupan
    {
        public string link;
        public bool havePassword;
        public string password;
    }

    class resources
    {
        public string errorMessage = "";
        public int index = 0;
        public string url = "";
        public string title = "";
        public string author = "";
        public DateTime datetime;
        public int numberOfMagnets = 0;
        public string[] magnets = new string[100];
        public int numberOfBaidupanLinks = 0;
        public baidupan[] baidupanLinks = new baidupan[20];
    }
}