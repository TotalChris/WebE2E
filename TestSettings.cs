using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebE2E
{
    public enum BrowserType
    {
        Chromium,
        Firefox,
        Webkit
    }

    public class TestSettings
    {
        public static List<string> ProductUrls         {
            get
            {
                return new List<string>
                {
                    "http://localhost:5173",
                    "https://example.hosting.app",
                    "https://example.com",
                };
            }
        }
    }
}
