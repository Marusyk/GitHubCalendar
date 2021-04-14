using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Web.Services
{
    public class MemoryStorage : IStorage
    {
        private readonly Dictionary<string, string> _links;

        public MemoryStorage()
        {
            _links = new Dictionary<string, string>();
        }

        public string Add(string url)
        {
            if (!_links.ContainsValue(url))
            {
                string shortName = Convert.ToBase64String(Encoding.UTF8.GetBytes(url)).Trim('=').Substring(0, 7).ToLower();
                //string shortName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
                _links.TryAdd(shortName, url);
                return shortName;
            }

            return _links.FirstOrDefault(x => x.Value.Equals(url)).Key;
        }

        public string Get(string name)
        {
            _links.TryGetValue(name, out var url);
            return url;
        }

        public void Clear()
        {
            _links.Clear();
        }
    }
}
