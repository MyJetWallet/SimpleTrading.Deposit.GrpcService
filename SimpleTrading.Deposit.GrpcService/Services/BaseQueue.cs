using System;
using System.Collections.Generic;

namespace SimpleTrading.Deposit.GrpcService.Services
{
    public class BaseQueue<T>
    {
        private readonly object _lockObject = new object();

        private readonly string _queueName;

        private List<T> _entitiesToDequeue = new List<T>();

        public BaseQueue(string queueName)
        {
            _queueName = queueName;
        }
        
        public void Enqueue(T entity)
        {
            lock (_lockObject)
            {
                _entitiesToDequeue.Add(entity);
                ServiceLocator.Logger.Information("Entities in {queueName} queue: {count}",
                    _queueName, _entitiesToDequeue.Count);
            }
        }
        
        public void Enqueue(IEnumerable<T> entity)
        {
            lock (_lockObject)
            {
                _entitiesToDequeue.AddRange(entity);
                ServiceLocator.Logger.Information("Entities in {queueName} queue: {count}",
                    _queueName, _entitiesToDequeue.Count);
            }
        }

        public IReadOnlyList<T> Dequeue()
        {
            lock (_lockObject)
            {
                if (_entitiesToDequeue.Count == 0)
                    return Array.Empty<T>();

                var result = _entitiesToDequeue;
                _entitiesToDequeue = new List<T>();
                return result;
            }
        }
    }
}