using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SpellBound.Core {
    public interface IRangeCheck {
        Task RangeCheckLoop(Transform requester, CancellationToken token);
    }
}