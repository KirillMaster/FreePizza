using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;

namespace FreePizza
{
    public class Response
    {
        public List<Comment> Items { get; set; }
        public List<Profile> Profiles { get; set; }
    }
}
