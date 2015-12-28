using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientsState
{
    class TextWriterLogger : ILogger
    {
        TextWriter _writter;

        public TextWriterLogger(TextWriter writter)
        {
            if (writter == null) throw new ArgumentNullException(nameof(writter));
            _writter = writter;
        }

        public void Log(string message)
        {
            _writter.WriteLine(message);
        }

        public void Log(string message, params object[] args)
        {
            _writter.WriteLine(message, args);
        }
    }
}
