using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RaceMonitor
{
    /// <summary>
    /// A producer/consumer queue. 
    /// Example use: 
    /// <code>
    /// using (ProducerConsumerQueue<string> q = new ProducerConsumerQueue<string>(TaskReader))
    /// {
    ///    q.EnqueueTask ("Hello");
    ///    for (int i = 0; i < 10; i++) q.EnqueueTask ("Say " + i);
    ///    q.EnqueueTask ("Goodbye!");
    /// }
    /// </code>
    /// Exiting the using statement calls q's Dispose method, which
    /// enqueues a null task and waits until the consumer finishes.
    /// </summary>
    class ProducerConsumerQueue<T> : IDisposable
    {
        #region data
        /// <summary>
        /// handle to signal queue events
        /// </summary>
        private readonly EventWaitHandle handle = new AutoResetEvent(false);
        /// <summary>
        /// worker thread to perform tasks
        /// </summary>
        private readonly Thread worker;
        /// <summary>
        /// locker to protect access to the task queue
        /// </summary>
        readonly object locker = new object();
        /// <summary>
        /// queue of tasks
        /// </summary>
        private readonly Queue<T> tasks = new Queue<T>();

        public IPerformTask<T> Perform { get; }
        #endregion

        /// <summary>
        /// Create a thread for reading items from the queue
        /// </summary>
        /// <param name="perform">object that can process queued tasks</param>
        /// <param name="runWorkerThread">false to stop the creation of a worker thread, the Work() function 
        /// must be called to process tasks from the queue</param>
        public ProducerConsumerQueue(IPerformTask<T> perform, bool runWorkerThread = true)
        {
            if (runWorkerThread)
            {
                worker = new Thread(Work);
                worker.Start();
            }
            Perform = perform;
        }

        /// <summary>
        /// add an item to the queue for processing
        /// </summary>
        /// <param name="task"></param>
        public void EnqueueTask(T task)
        {
            lock (locker) tasks.Enqueue(task);
            handle.Set();
        }

        /// <summary>
        /// tidy up
        /// </summary>
        public void Dispose()
        {
            EnqueueTask(default);    // Signal the consumer to exit.
            worker.Join();              // Wait for the consumer's thread to finish.
            handle.Close();             // Release any OS resources.
        }

        /// <summary>
        /// process entries from the queue
        /// </summary>
        public void Work()
        {
            while (true)
            {
                T task = default;
                lock (locker)
                {
                    if (tasks.Count > 0)
                    {
                        task = tasks.Dequeue();
                        if (task == null)
                        {
                            return;
                        }
                    }
                }
                if (task != null)
                {
                    Perform.PerformTask(task);
                }
                else
                {
                    handle.WaitOne();         // No more tasks - wait for a signal
                }
            }
        }
    }
}
