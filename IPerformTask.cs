namespace RaceMonitor
{
    /// <summary>
    /// Interface for performing a task
    /// </summary>
    /// <typeparam name="T">the type of task object to be processed </typeparam>
    public interface IPerformTask<T>
    {
        abstract void PerformTask(T task);
    }
}