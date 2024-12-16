using System.ComponentModel.DataAnnotations;

namespace Gridify
{
    public class AnnotatedPaging<T>
    {
        private readonly Paging<T> _paging;

        public AnnotatedPaging(int count, IEnumerable<T> data)
        {
            _paging = new Paging<T>(count, data);
        }

        public AnnotatedPaging() : this(0, System.Linq.Enumerable.Empty<T>())
        {
        }

        [Range(0, int.MaxValue)]
        public int Count
        {
            get => _paging.Count;
            set => _paging.Count = value;
        }

        [MaxLength(10000)]
        public IEnumerable<T> Data
        {
            get => _paging.Data;
            set => _paging.Data = value;
        }

        public void Deconstruct(out int count, out IEnumerable<T> data) => _paging.Deconstruct(out count, out data);
    }
}
