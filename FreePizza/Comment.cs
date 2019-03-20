using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreePizza
{
    public class Comment
    {
        public long Id { get; set; }
        //
        // Summary:
        //     Идентификатор автора комментария.
        public long from_id { get; set; }
        //
        // Summary:
        //     Дата и время создания комментария.
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? Date { get; set; }
        //
        // Summary:
        //     Текст комментария.
        public string Text { get; set; }

    }
}
