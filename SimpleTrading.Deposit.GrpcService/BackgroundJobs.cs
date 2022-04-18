using System;
using System.Diagnostics;
using DotNetCoreDecorators;

namespace SimpleTrading.Deposit.GrpcService
{
    public static class BackgroundJobs
    {
        private static readonly TaskTimer LogTaskTimer = new TaskTimer(TimeSpan.Parse("00:30:00"));
        
        public static void Init()
        {
            LogTaskTimer.Register("logDepositsEntitiesPusher", async () =>
            {
                var items = ServiceLocator.DepositEntitiesLoggerQueue.Dequeue();

                using (var activity = new Activity("logDepositsEntitiesPusher").Start())
                {
                    foreach (var item in items)
                    {
                        try
                        {
                            await ServiceLocator.InvoiceRepository.SaveAsync(item);
                        }
                        catch (Exception e)
                        {
                            ServiceLocator.Logger.Error("logDepositsEntitiesPusher error", e, e.Message);
                            ServiceLocator.DepositEntitiesLoggerQueue.Enqueue(item);
                        }
                    }
                }
                    
            });

            LogTaskTimer.Register("logCallbackEntitiesPusher", async () =>
            {
                var items = ServiceLocator.DepositCallbackEntitiesLogQueue.Dequeue();

                using (var activity = new Activity("logCallbackEntitiesPusher").Start())
                {
                    foreach (var item in items)
                    {
                        try
                        {
                            await ServiceLocator.CallbackRepository.SaveAsync(item);
                        }
                        catch (Exception e)
                        {
                            ServiceLocator.Logger.Error("logCallbackEntitiesPusher error", e, e.Message);
                            ServiceLocator.DepositCallbackEntitiesLogQueue.Enqueue(item);
                        }
                    }
                }
            });
        }
        
        public static void Start()
        {
            LogTaskTimer.Start();
        }
    }
}