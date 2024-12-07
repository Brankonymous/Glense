namespace Glense.Server
{
    public class Category
    {
        public int categoryID {  get; set; }
        public string name { get; set; }
        public ICollection<Video> Videos { get; set; }


    }
}
