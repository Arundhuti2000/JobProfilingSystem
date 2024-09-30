using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace JobProfilingSystem
{
    public enum JobPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }
    public abstract class SortingJobSystem
    {
        protected List<int> Data { get; set; }
        public string Name { get;  set; }
        public long BigOScore { get;  set; }
        public double LastExecutionTime { get;  set; }
        public double AverageExecutionTime { get;  set; }
        public int ExecutionCount { get;  set; }

        protected SortingJobSystem(List<int> data, string name, Action<string> logMessage)
        {
            Data = data;
            Name = name;
            BigOScore = 0;
            LastExecutionTime = 0;
            AverageExecutionTime = 0;
            ExecutionCount = 0;
            LogMessage = logMessage;
        }
        public readonly Action<string> LogMessage;
        public abstract void Execute();


        protected void UpdateProfilingInfo(double executionTime)
        {
            LastExecutionTime = executionTime;
            ExecutionCount++;
            AverageExecutionTime = ((AverageExecutionTime * (ExecutionCount - 1)) + executionTime) / ExecutionCount;
        }

        protected void LogData(string prefix)
        {
            string dataString = string.Join(", ", Data);
            LogMessage?.Invoke($"{prefix} Data: {dataString}");
        }
    }

    public class BubbleSortJob : SortingJobSystem
    {
        public BubbleSortJob(List<int> data, Action<string> logMessage) : base(data, "Bubble Sort", logMessage)
        {
            BigOScore = (long)Math.Pow(data.Count, 2); // O(n^2)
        }

        public override void Execute()
        {
            LogData("Bubble Sort Before sorting:");
            var stopwatch = Stopwatch.StartNew();
            int n = Data.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (Data[j] > Data[j + 1])
                    {
                        int temp = Data[j];
                        Data[j] = Data[j + 1];
                        Data[j + 1] = temp;
                    }
                }
            }
            stopwatch.Stop();
            LogData("Bubble Sort After sorting:");
            UpdateProfilingInfo(stopwatch.Elapsed.TotalSeconds);
        }

    }

    public class QuickSortJob : SortingJobSystem
    {
        public QuickSortJob(List<int> data, Action<string> logMessage) : base(data, "Quick Sort", logMessage)
        {
            BigOScore = (long)(data.Count * Math.Log(data.Count)); // O(n log n)
        }

        public override void Execute()
        {
            LogData("Quick Sort Before sorting:");
            var stopwatch = Stopwatch.StartNew();
            QuickSort(0, Data.Count - 1);
            stopwatch.Stop();
            LogData("Quick Sort After sorting:");
            UpdateProfilingInfo(stopwatch.Elapsed.TotalSeconds);
        }

        private void QuickSort(int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(low, high);
                QuickSort(low, pi - 1);
                QuickSort(pi + 1, high);
            }
        }

        private int Partition(int low, int high)
        {
            int pivot = Data[high];
            int i = (low - 1);
            for (int j = low; j < high; j++)
            {
                if (Data[j] < pivot)
                {
                    i++;
                    int temp = Data[i];
                    Data[i] = Data[j];
                    Data[j] = temp;
                }
            }
            int temp1 = Data[i + 1];
            Data[i + 1] = Data[high];
            Data[high] = temp1;
            return i + 1;
        }
    }

    public class JobScheduler
    {
        private readonly int _numThreads;
        private SortedDictionary<int, Queue<SortingJobSystem>> _jobQueue;
        private readonly List<Task> _tasks;
        private readonly CancellationTokenSource _cts;
        private int _jobCounter;
        public readonly Action<string> LogMessage;

        public event EventHandler<JobEventArgs> JobStarted;
        public event EventHandler<JobEventArgs> JobCompleted;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll")]
        static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

        public JobScheduler(Action<string> logMessage, int? requestedThreads = null)
        {
            LogMessage = logMessage;
            _numThreads = Math.Min(requestedThreads ?? Environment.ProcessorCount, Environment.ProcessorCount);
            _jobQueue = new SortedDictionary<int, Queue<SortingJobSystem>>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
            _tasks = new List<Task>();
            _cts = new CancellationTokenSource();
            _jobCounter = 0;

            LogMessage?.Invoke($"JobScheduler initialized with {_numThreads} threads (Available cores: {Environment.ProcessorCount})");
        }

        public void AddJob(SortingJobSystem job, JobPriority priority)
        {
            lock (_jobQueue)
            {
                if (!_jobQueue.ContainsKey((int)priority))
                {
                    _jobQueue[(int)priority] = new Queue<SortingJobSystem>();
                }
                _jobQueue[(int)priority].Enqueue(job);
                _jobCounter++;
            }
        }

        public void Start()
        {
            for (int i = 0; i < _numThreads; i++)
            {
                int threadId = i;
                _tasks.Add(Task.Factory.StartNew(() => Worker(threadId), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default));
            }
        }

        private void Worker(int threadId)
        {
            // Set thread affinity
            IntPtr threadHandle = GetCurrentThread();
            IntPtr affinityMask = (IntPtr)(1L << threadId);
            SetThreadAffinityMask(threadHandle, affinityMask);

            LogMessage?.Invoke($"Thread {threadId} set to run on CPU core {threadId}");

            while (!_cts.Token.IsCancellationRequested)
            {
                SortingJobSystem job = null;
                lock (_jobQueue)
                {
                    foreach (var priorityQueue in _jobQueue)
                    {
                        if (priorityQueue.Value.Count > 0)
                        {
                            job = priorityQueue.Value.Dequeue();
                            _jobCounter--;
                            break;
                        }
                    }
                }

                if (job != null)
                {
                    JobStarted?.Invoke(this, new JobEventArgs { Job = job });
                    LogMessage?.Invoke($"Thread {threadId} executing {job.Name}");
                    var startTime = DateTime.Now;
                    job.Execute();
                    job.LastExecutionTime = (DateTime.Now - startTime).TotalSeconds;
                    LogMessage?.Invoke($"Thread {threadId} finished {job.Name}. Execution time: {job.LastExecutionTime:F4}s");
                    JobCompleted?.Invoke(this, new JobEventArgs { Job = job });
                }
                else
                {
                    Thread.Sleep(100); // Sleep if no jobs are available
                }
            }
        }
        public bool CancelJob(SortingJobSystem job)
        {
            lock (_jobQueue)
            {
                foreach (var priorityQueue in _jobQueue)
                {
                    if (priorityQueue.Value.Contains(job))
                    {
                        priorityQueue.Value.Dequeue();
                        _jobCounter--;
                        return true;
                    }
                }
            }
            return false;
        }

        public void Stop()
        {
            _cts.Cancel();
            Task.WaitAll(_tasks.ToArray());
        }

        public int JobCount
        {
            get
            {
                lock (_jobQueue)
                {
                    return _jobCounter;
                }
            }
        }
    }
}
