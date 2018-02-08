using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace OpenRasta.Configuration.MetaModel
{
    public class UriModel
    {
        public CultureInfo Language { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public ICollection<OperationModel> Operations { get; set; }
    }
}