using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace MixSounder
{
    class MutexHandle
    {
        public static bool hasHandle(string name)
        {
            bool hasHandle = false;

            Mutex mutex = new Mutex(false, name);

            try
            {
                try
                {
                    hasHandle = mutex.WaitOne(0, false);
                } catch (AbandonedMutexException)
                {
                    hasHandle = true;
                }
            } finally
            {
                if (hasHandle)
                {
                    mutex.ReleaseMutex();
                }

                mutex.Close();
            }

            return hasHandle;
        }

        public static void Handle(string name, Form form)
        {
            Mutex mutex = new Mutex(false, name);
            bool hasHandle = false;

            try
            {
                try
                {
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (AbandonedMutexException)
                {
                    hasHandle = true;
                }

                if (hasHandle)
                {
                    Application.Run(form);
                }
            }
            finally
            {
                if (hasHandle)
                {
                    mutex.ReleaseMutex();
                }

                mutex.Close();
            }
        }
    }
}
