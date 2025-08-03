namespace CalendarAssistant.Services
{
    public interface IPythonExecutorService
    {
       // dynamic CallPythonMethod(string fileName, string className, string methodName, params object[] parameters);
        int CallPython(string fileName);

    }
}
