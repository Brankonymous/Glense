namespace Glense.Server
{
    public class Category
    {
        public int categoryID { get; set; }
        public string name { get; set; } = string.Empty;
        public ICollection<Video> Videos { get; set; } = new List<Video>();
    }
}
