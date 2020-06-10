namespace Erth.Shared.Models
{
     public class TbActionResult<T> 
    {
        public bool Success { get; set; }
        public string Desc { get; set; }
        public T Object { get; set; }
    }
}