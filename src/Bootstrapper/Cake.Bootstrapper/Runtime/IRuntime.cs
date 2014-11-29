namespace Cake.Bootstrapper.Runtime
{
    public interface IRuntime
    {
        void ReportProgress(string title, string description, int percentage);
    }
}
