﻿namespace Chiota.Services.BackgroundServices.Base
{
    public interface IBackgroundJobWorker
    {
        void Init(params object[] data);

        void Start();
        void Stop();

        void Add<T>(params object[] data) where T : BaseBackgroundJob;
        void Remove(int id);

        void Dispose();
    }
}