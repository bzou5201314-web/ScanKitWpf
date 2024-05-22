// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

Console.WriteLine("Hello, World!");
Process[] processes = Process.GetProcessesByName("BarTend");
foreach (Process process in processes)
{
    try
    {
        process.Kill();
        process.WaitForExit(); // 等待进程退出
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error killing process: {ex.Message}");
    }
}
