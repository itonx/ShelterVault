﻿using System;

namespace ShelterVault.Services
{
    public interface IUIThreadService
    {
        void Execute(Action rutine);
    }

    public class UIThreadService : IUIThreadService
    {
        public void Execute(Action rutine)
        {
            AppDispatcher.UIThreadDispatcher?.TryEnqueue(() =>
            {
                rutine.Invoke();
            });
        }
    }
}
