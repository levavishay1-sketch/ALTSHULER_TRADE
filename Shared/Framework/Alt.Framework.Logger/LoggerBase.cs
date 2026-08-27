using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Interfaces;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;

namespace Alt.Framework.Logger
{
    public abstract class LoggerBase : ILog
    {
        protected EntryPointTypeCode entryPointType;
        protected MessageLevel level = MessageLevel.Information;
        protected string requestId;
        protected string className;
        protected MessageLevel levelToLog = MessageLevel.Information;
        protected StringBuilder logMessageBuilder = new StringBuilder();
        protected string primaryEntityName;
        protected string primaryEntityId;

        public LoggerBase(EntryPointTypeCode entryPointType, string className, string requestId = "", MessageLevel levelToLog = MessageLevel.Information, string primaryEntityName = null, string primaryEntityId = null)
        {
            this.entryPointType = entryPointType;
            this.className = className;
            this.requestId = requestId;
            this.levelToLog = levelToLog;
            this.primaryEntityName = primaryEntityName;
            this.primaryEntityId = primaryEntityId;
        }

        public virtual void Critical(Exception ex, string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(ExeptionToMessage(ex, message), MessageLevel.Critical, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Critical(Exception ex, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(ExeptionToMessage(ex), MessageLevel.Critical, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Critical(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(message, MessageLevel.Critical, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Error(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(ExeptionToMessage(null, message), MessageLevel.Error, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Error(Exception ex, string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(ExeptionToMessage(ex, message), MessageLevel.Error, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Error(Exception ex, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(ExeptionToMessage(ex), MessageLevel.Error, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Info(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(message, MessageLevel.Information, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Warning(string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(message, MessageLevel.Warning, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Warning(Exception ex, string message, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(ExeptionToMessage(ex, message), MessageLevel.Warning, sourceFilePath, memberName, sourceLineNumber);
        }

        public virtual void Warning(Exception ex, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.WriteMessage(ExeptionToMessage(ex), MessageLevel.Warning, sourceFilePath, memberName, sourceLineNumber);
        }

        protected virtual string ExeptionToMessage(Exception ex, string message = "")
        {
            StringBuilder exeptionMessage = new StringBuilder();
            exeptionMessage.Append(message);
            exeptionMessage.AppendLine("\n\n******************");
            if (ex != null)
            {
                exeptionMessage.AppendLine($"\nMessage:{ex.Message}");
                exeptionMessage.AppendLine($"\nExeption StackTrace:\n{ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    if (!String.IsNullOrWhiteSpace(ex.InnerException.Message))
                    {
                        exeptionMessage.AppendLine($"Inner Exception message:\n{ ex.InnerException.Message}");
                    }
                    if (!String.IsNullOrWhiteSpace(ex.InnerException.StackTrace))
                    {
                        exeptionMessage.AppendLine($"Inner Exception stack trace:\n{ ex.InnerException.StackTrace}");
                    }
                }

                if (ex is FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
                {
                    exeptionMessage.AppendLine($"FaultException TraceText:\n {(ex as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>).Detail.TraceText}");
                }
            }
            exeptionMessage.AppendLine("******************");
            return exeptionMessage.ToString();

        }

        protected virtual void WriteMessage(string message, MessageLevel level, string sourceFilePath, string memberName, int sourceLineNumber)
        {
            string messageLevelStr = "INFO";
            if (level > this.level)
            {
                this.level = level;
            }

            switch (level)
            {
                case MessageLevel.Information:
                    {
                        messageLevelStr = "INFO";
                        break;
                    }
                case MessageLevel.Warning:
                    {
                        messageLevelStr = "WARN";
                        break;
                    }
                case MessageLevel.Error:
                    {
                        messageLevelStr = "ERROR";
                        break;
                    }
                case MessageLevel.Critical:
                    {
                        messageLevelStr = "CRITICAL";
                        break;
                    }
            }

            logMessageBuilder.AppendLine($"[{DateTime.UtcNow.ToString("O")} UTC]  [{messageLevelStr}]  {Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName} ({sourceLineNumber}) :: {message}");
        }


        public abstract void Execute();

    }
}
