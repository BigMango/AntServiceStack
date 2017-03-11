namespace CHystrix
{
    using System.Threading.Tasks;

    public interface IThreadIsolation<T>
    {
        Task<T> RunAsync();
    }
}

