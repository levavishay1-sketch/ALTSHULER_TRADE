using System;
using System.Runtime.CompilerServices;

namespace Alt.DataModel.Crm.Core.Interfaces
{
    public interface ILog
    {
        void Info(string message,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Warning(string message,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Warning(Exception ex,
           string message,
           [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Warning(Exception ex,
            [CallerFilePath] string sourceFilePath = "",
           [CallerMemberName] string memberName = "",
           [CallerLineNumber] int sourceLineNumber = 0);

        void Error(string message,
            [CallerFilePath] string sourceFilePath = "",
           [CallerMemberName] string memberName = "",
           [CallerLineNumber] int sourceLineNumber = 0);

        void Error(Exception ex,
            string message,
            [CallerFilePath] string sourceFilePath = "",
             [CallerMemberName] string memberName = "",
             [CallerLineNumber] int sourceLineNumber = 0);

        void Error(Exception ex,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Critical(Exception ex,
            string message,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0);
        void Critical(Exception ex,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Critical(string message,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Execute();

    }
}
