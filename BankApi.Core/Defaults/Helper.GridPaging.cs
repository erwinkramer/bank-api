using System.ComponentModel.DataAnnotations;

public class Paging<T> : Gridify.Paging<T>
{
    public Paging(int count, IEnumerable<T> data) : base(count, data)
    {
        Data = data;
        Count = count;
    }

    public Paging() : base(0, Enumerable.Empty<T>())
    {
        Data = Enumerable.Empty<T>(); // Default initialization
        Count = 0;
    }

    [Range(0, Int32.MaxValue)]
    public new int Count { get; set; }

    [MaxLength(Int32.MaxValue)]
    public new IEnumerable<T> Data { get; set; }
}
