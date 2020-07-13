using System.Collections.Generic;

namespace LargeJson.Shared
{
	public class CosmosQuery<T>
    {
        public List<T> Documents { get; set; }
    }

    public class MyItem
    {
        public string id { get; set; }
        public string Name { get; set; }

        public string FullName { get; set; }

        public Ref PrefVendorRef { get; set; }
    }

    public class Ref
    {
        public string ListID { get; set; }
        public string FullName { get; set; }
    }

}
