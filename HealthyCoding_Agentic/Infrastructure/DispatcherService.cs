using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HealthyCoding_Agentic.Infrastructure {
    public class DispatcherService(Dispatcher dispatcher) : IDispatcherService {
        Dispatcher dispatcher = dispatcher;
        public void Invoke(Action callback) =>
            dispatcher.Invoke(callback);

    }

    public interface IDispatcherService {
        void Invoke(Action callback);
    }
}
