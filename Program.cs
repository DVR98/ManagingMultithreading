using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManagingMultithreading
{
    class Program
    {                     
        static void Main(string[] args)
        {
            //Guide user
            Console.WriteLine("Please enter 1 to run Lock method");
            Console.WriteLine("Please enter 2 to run Interlocked class method");
            Console.WriteLine("Please enter 3 to run Task cancellation method");

            try {
                switch(Console.ReadLine()){
                    //1 pressed
                    case "1": {
                        //Run deadlocks method
                        Locks();
                        break;
                    }
                    //2 pressed
                    case "2": {
                        //Run Interlockec class method
                        InterlockedClass();
                        break;
                    }
                    //2 pressed
                    case "3": {
                        //Run Interlockec class method
                        CancelTask();
                        break;
                    }
                }
            }
            catch{

            }
        }

        //You can cancel task by creating a token
        public static void CancelTask() {
            //Token source
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            //cancellation token
            CancellationToken token = cancellationTokenSource.Token;

            Task task = Task.Run(() => {
                //Run task while cancellation isn't requested
                while (!token.IsCancellationRequested){
                    Console.Write("*");
                    Thread.Sleep(1000);
                }

                token.ThrowIfCancellationRequested();
            }, token);

            try {
                Console.WriteLine("Press enter to stop the task");
                Console.ReadLine();

                //Broadcast cancellation
                cancellationTokenSource.Cancel();
                Console.WriteLine("Task is stopping....");

                //Wait for finishing task operation
                task.Wait();
            }
            catch(AggregateException e){
                //Will broadcast that Task was canceled
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press enter to end the application");
            Console.ReadLine();
        }

        //A way so that both threads can manage the same variable at the same time without a deadlock occuring
        //Interlocked class helps making operations atomic
        public static void InterlockedClass(){
            int n = 0;

            var up = Task.Run(() => {
                for (int i = 0; i < 1000000; i++)
                {
                    //deadlock opportunity prevented
                    Interlocked.Increment(ref n);
                }
            });

            for (int i = 0; i < 100000; i++)
            {
                //deadlock opportunity prevented
                    Interlocked.Decrement(ref n);
            }

            up.Wait();

            Console.WriteLine("Result: {0}", n);
            Console.WriteLine("Press enter to end the application");
            Console.ReadLine();
        }

        public static void Locks()
        {
            int n = 0;

            //Lock operator
            object _lock = new object();

            var up = Task.Run(() => {
                for (int i = 0; i < 1000000; i++)
                {
                    //deadlock opportunity
                    lock(_lock)
                        n++;
                }
            });

            for (int i = 0; i < 1000000; i++)
            {
                //deadlock opportunity
                lock(_lock)
                    n--;
            }
            
            //Synchronizing deadlock opportunity
            up.Wait();

            //n will always be zero, because it has been synchronized.
            Console.WriteLine("Result: {0}", n);
            Console.WriteLine("Press enter to end the application");
            Console.ReadLine();
            
            //
            //
            //
            //How deadlocks can be handled: Request locks in same order
            object gate = new object();
            bool __lockTaken = false;

            try {
                //Enter gate
                Monitor.Enter(gate, ref __lockTaken);
            }
            finally {
                //if gate was entered, exit
                if(__lockTaken){
                    Monitor.Exit(gate);
                }
            }
        }
    }
}
