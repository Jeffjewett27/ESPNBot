using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ESPNBot
{
    /// <summary>
    /// Waits for a period of time in between browser actions
    /// </summary>
    class BrowserWait
    {
        private Random random;
        private int value;
        private int variance;

        public BrowserWait(int value, int variance)
        {
            random = new Random();
            this.value = value;
            this.variance = variance;
        }

        public void Wait()
        {
            int wait = random.Next(value - variance / 2, value + variance / 2);
            Thread.Sleep(wait);
        }
    }
}
