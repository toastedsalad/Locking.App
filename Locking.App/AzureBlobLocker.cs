using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locking.App
{
    public class AzureBlobLocker : ILocker
    {
        public bool GetLock(LockLocation lockLocation)
        {
            throw new NotImplementedException();
        }

        public bool ReleaseLock(LockLocation lockLocation)
        {
            throw new NotImplementedException();
        }
    }
}
