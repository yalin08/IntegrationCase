using Integration.Service;
using System.Diagnostics;

namespace Integration;

public abstract class Program
{

    private static Stopwatch totalStopwatch = new Stopwatch();
    public static void Main(string[] args)
    {

        var service = new ItemIntegrationService();

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

        Thread.Sleep(500);

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));


        Thread.Sleep(500);

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

        Thread.Sleep(500);

 
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("d"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("e"));
        Thread.Sleep(500);


        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("d"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("f"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));

        Thread.Sleep(5000);


        Console.WriteLine("Everything recorded:");

        service.GetAllItems().ForEach(Console.WriteLine);




        Console.ReadLine();
    }
    
}

