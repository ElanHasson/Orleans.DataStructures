namespace Orleans.DataStructures
{
    public static class ArrayGrainExtensions
    {
        public static ArrayGrainProxy<T> ToProxy<T>(this IArrayGrain<T> arrayGrain)
        {
            return new ArrayGrainProxy<T>(arrayGrain);
        }
    }
}
