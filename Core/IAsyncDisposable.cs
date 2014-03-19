using System;
using System.Threading.Tasks;

namespace SvnAutoCommitter.Core {
    public interface IAsyncDisposable {
        Task DisposeAsync();
    }
}
