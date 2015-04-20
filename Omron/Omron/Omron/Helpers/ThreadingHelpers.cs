using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Omron.Helpers
{
    //
    //http://stackoverflow.com/questions/3312004/how-does-a-timer-elapsed-event-compete-with-high-priority-threads/3313267#3313267
    //

    public class ElapsedEventReceiver : ISynchronizeInvoke
    {
        private Thread m_Thread;
        private BlockingCollection<Message> m_Queue = new BlockingCollection<Message>();

        public ElapsedEventReceiver(ThreadPriority priority)
        {
            m_Thread = new Thread(run);
            m_Thread.Priority = priority;
            m_Thread.Start();
        }

        void run()
        {
            while (true)
            {
                Message message = m_Queue.Take();
                message.Return = message.Method.DynamicInvoke(message.Args);
                message.Finished.Set();
            }
        }

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            Message message = new Message();
            message.Method = method;
            message.Args = args;
            m_Queue.Add(message);
            return message;
        }

        public object EndInvoke(IAsyncResult result)
        {
            Message message = result as Message;
            if (message != null)
            {
                message.Finished.WaitOne();
                return message.Return;
            }
            throw new ArgumentException("result");
        }

        public object Invoke(Delegate method, object[] args)
        {
            Message message = new Message();
            message.Method = method;
            message.Args = args;
            m_Queue.Add(message);
            message.Finished.WaitOne();
            return message.Return;
        }

        public bool InvokeRequired
        {
            get { return Thread.CurrentThread != m_Thread; }
        }

        private class Message : IAsyncResult
        {
            public Delegate Method;
            public object[] Args;
            public object Return;
            public object State;
            public ManualResetEvent Finished = new ManualResetEvent(false);

            public object AsyncState
            {
                get { return State; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return Finished; }
            }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            public bool IsCompleted
            {
                get { return Finished.WaitOne(0); }
            }
        }
    }
}
