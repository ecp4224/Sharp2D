using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sharp2D.Core
{
    public class ParallelStringWriter : StringWriter
    {
        private TextWriter copy;
        public ParallelStringWriter(TextWriter copy) : base()
        {
            this.copy = copy;
        }

        public TextWriter CopyStream
        {
            get
            {
                return copy;
            }
            set
            {
                copy = value;
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void Write(char value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(bool value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(char[] buffer)
        {
            copy.Write(buffer);

            base.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            copy.Write(buffer, index, count);

            base.Write(buffer, index, count);
        }

        public override void Write(decimal value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(double value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(float value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(int value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(long value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(object value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            copy.Write(format, arg0);

            base.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            copy.Write(format, arg0, arg1);

            base.Write(format, arg0, arg1);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            copy.Write(format, arg0, arg1, arg2);

            base.Write(format, arg0, arg1, arg2);
        }

        public override void Write(string format, params object[] arg)
        {
            copy.Write(format, arg);

            base.Write(format, arg);
        }

        public override void WriteLine(ulong value)
        {
            copy.Write(value);

            base.WriteLine(value);
        }

        public override void Write(string value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(uint value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override void Write(ulong value)
        {
            copy.Write(value);

            base.Write(value);
        }

        public override Task WriteAsync(char value)
        {
            copy.WriteAsync(value);

            return base.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            copy.WriteAsync(buffer, index, count);

            return base.WriteAsync(buffer, index, count);
        }

        public override Task WriteAsync(string value)
        {
            copy.WriteAsync(value);

            return base.WriteAsync(value);
        }

        public override void WriteLine()
        {
            copy.WriteLine();

            base.WriteLine();
        }

        public override void WriteLine(bool value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(char value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(char[] buffer)
        {
            base.WriteLine(buffer);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            base.WriteLine(buffer, index, count);
        }

        public override void WriteLine(decimal value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(double value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(float value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(int value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(long value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            copy.WriteLine(format, arg0);

            base.WriteLine(format, arg0);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            copy.WriteLine(format, arg0, arg1);

            base.WriteLine(format, arg0, arg1);
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            copy.WriteLine(format, arg0, arg1, arg2);

            base.WriteLine(format, arg0, arg1, arg2);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            copy.WriteLine(format, arg);

            base.WriteLine(format, arg);
        }

        public override void WriteLine(string value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override void WriteLine(uint value)
        {
            copy.WriteLine(value);

            base.WriteLine(value);
        }

        public override Task WriteLineAsync()
        {
            copy.WriteLineAsync();

            return base.WriteLineAsync();
        }

        public override Task WriteLineAsync(string value)
        {
            copy.WriteLineAsync(value);

            return base.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            copy.WriteLineAsync(buffer, index, count);

            return base.WriteLineAsync(buffer, index, count);
        }

        public override Task WriteLineAsync(char value)
        {
            copy.WriteLineAsync(value);

            return base.WriteLineAsync(value);
        }
    }
}
