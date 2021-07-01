using System;
using System.Net;
using System.Runtime.InteropServices;

namespace BYOPython
{
    class Program
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PyRun_SimpleString(string command);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Py_Initialize();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Py_FinalizeEx();

        static void Main(string[] args)
        {
            var pythonPath = @"C:\Program Files (x86)\Dropbox\Client\123.4.4832";

            Console.WriteLine($"[+] Setting enviroment variables for python");
            Environment.SetEnvironmentVariable("PYTHONPATH", $@"{pythonPath}\python-packages.zip");

            Console.WriteLine($"    -> PYTHONPATH = {Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process)}");

            string pythondll = $@"{pythonPath}\python38.dll";

            Console.WriteLine($"[+] Loading pythonNN.dll from Dropbox directory.");
            var pyDll = LoadLibrary(pythondll);

            Console.WriteLine($"[+] Getting Functions Addresses.");
            var pyrunAddr = GetProcAddress(pyDll, "PyRun_SimpleString");
            Console.WriteLine($"    -> PyRun_SimpleString   => {"0x" + pyrunAddr.ToString("X")}");
            var pyInitAddr = GetProcAddress(pyDll, "Py_Initialize");
            Console.WriteLine($"    -> Py_Initialize        => {"0x" + pyInitAddr.ToString("X")}");
            var pyFinAddr = GetProcAddress(pyDll, "Py_FinalizeEx");
            Console.WriteLine($"    -> Py_FinalizeEx        => {"0x" + pyFinAddr.ToString("X")}");
            var pyNoSiteFlagAddr = GetProcAddress(pyDll, "Py_NoSiteFlag");
            Console.WriteLine($"    -> Py_NoSiteFlag        => {"0x" + pyNoSiteFlagAddr.ToString("X")}");

            Console.WriteLine($"[+] Setting Py_NoSiteFlag value to 1.");

            int[] variable = new int[1];
            Marshal.Copy(pyNoSiteFlagAddr, variable, 0, 1);

            Console.WriteLine($"    -> Current Py_NoSiteFlag Value => {variable[0].ToString()}");

            variable[0] = 1; // 0 for False, 1 for True
            Marshal.Copy(variable, 0, pyNoSiteFlagAddr, 1);

            Console.WriteLine($"    -> Modifyed Py_NoSiteFlag Value => {variable[0].ToString()}");

            Py_Initialize Py_Initialize = (Py_Initialize)Marshal.GetDelegateForFunctionPointer(pyInitAddr, typeof(Py_Initialize));

            Console.WriteLine($"[+] Initializing Python.");
            Py_Initialize();

            Console.WriteLine($"[+] Setting Python Payload.");
            PyRun_SimpleString PyRun_SimpleString = (PyRun_SimpleString)Marshal.GetDelegateForFunctionPointer(pyrunAddr, typeof(PyRun_SimpleString));

            string pythonCode = @"print('Hello from Python')";

            Console.WriteLine($"[+] Executing Python Payload.");

            int result = PyRun_SimpleString(pythonCode);

            Py_FinalizeEx Py_FinalizeEx = (Py_FinalizeEx)Marshal.GetDelegateForFunctionPointer(pyFinAddr, typeof(Py_FinalizeEx));

            Console.WriteLine($"[+] Finalizing Python.");
            Py_FinalizeEx();
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string name);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }
}
