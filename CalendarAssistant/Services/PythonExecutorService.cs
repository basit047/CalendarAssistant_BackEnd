using Python.Runtime;
using System.Diagnostics;

namespace CalendarAssistant.Services
{
    public class PythonExecutorService : IPythonExecutorService
    {
        //public dynamic CallPythonMethod(string fileName, string className, string methodName, params object[] parameters)
        //{
        //    ScriptEngine engine = Python.CreateEngine();
        //    ScriptScope scope = engine.CreateScope();

        //    var fileLocation = Path.Combine(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")), "PythonScripts", fileName);

        //    engine.ExecuteFile(fileLocation, scope);

        //    dynamic pyClass = scope.GetVariable(className);
        //    dynamic pyInstance = pyClass();

        //    return engine.Operations.InvokeMember(pyInstance, methodName, parameters);
        //}

        public int CallPython(string fileName)
        {
            string condaEnvPath = @"C:\Users\harri\anaconda3\envs\new_environment";

            //PythonEngine.PythonHome = condaEnvPath;
            PythonEngine.PythonPath = $"{condaEnvPath}\\Lib;{condaEnvPath}\\Lib\\site-packages";
            Runtime.PythonDLL = @"C:\Users\harri\AppData\Local\Programs\Python\Python311\python311.dll";
            PythonEngine.Initialize();


            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    Console.WriteLine($"Python Executable: {sys.executable}");
                    Console.WriteLine("sys.path:");
                    foreach (var path in sys.path)
                    {
                        Console.WriteLine(path.ToString());
                    }

                    dynamic transformers = Py.Import("transformers");
                    Console.WriteLine("✅ Transformers imported successfully!");
                }
                catch (PythonException ex)
                {
                    Console.WriteLine("❌ Python exception:");
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ General exception:");
                    Console.WriteLine(ex.Message);
                }
            }

            PythonEngine.Shutdown();

            return 1;
        }
    }
}
