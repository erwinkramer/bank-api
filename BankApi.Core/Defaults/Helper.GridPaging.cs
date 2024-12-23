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

    [GenericRange]
    public new int Count { get; set; }

    [GenericMaxLength]
    public new IEnumerable<T> Data { get; set; }
}
