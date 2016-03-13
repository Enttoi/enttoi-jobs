
namespace JobsCommon.Logger
{
    public interface ILogger
    {
        void Log(string message);

        void Error(string message);
    }
}
