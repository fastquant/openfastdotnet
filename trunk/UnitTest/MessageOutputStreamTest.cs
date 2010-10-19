/*

The contents of this file are subject to the Mozilla Public License
Version 1.1 (the "License"); you may not use this file except in
compliance with the License. You may obtain a copy of the License at
http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS"
basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
License for the specific language governing rights and limitations
under the License.

The Original Code is OpenFAST.

The Initial Developer of the Original Code is The LaSalle Technology
Group, LLC.  Portions created by Shariq Muhammad
are Copyright (C) Shariq Muhammad. All Rights Reserved.

Contributor(s): Shariq Muhammad <shariq.muhammad@gmail.com>
                Yuri Astrakhan <FirstName><LastName>@gmail.com
*/
using System.IO;
using NUnit.Framework;
using OpenFAST;
using OpenFAST.Error;
using UnitTest.Test;

namespace UnitTest
{
    public class IoExceptionThrowingStream : Stream
    {
        public override bool CanRead
        {
            get { throw new IOException("The method or operation is not implemented."); }
        }

        public override bool CanSeek
        {
            get { throw new IOException("The method or operation is not implemented."); }
        }

        public override bool CanWrite
        {
            get { throw new IOException("The method or operation is not implemented."); }
        }

        public override long Length
        {
            get { throw new IOException("The method or operation is not implemented."); }
        }

        public override long Position
        {
            get { throw new IOException("The method or operation is not implemented."); }
            set { throw new IOException("The method or operation is not implemented."); }
        }

        public override void Flush()
        {
            throw new IOException("The method or operation is not implemented.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new IOException("The method or operation is not implemented.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new IOException("The method or operation is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new IOException("The method or operation is not implemented.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new IOException("The method or operation is not implemented.");
        }
    }

    [TestFixture]
    public class MessageOutputStreamTest
    {
        [Test]
        public void TestIoErrorOnClose()
        {
            var output = new MessageOutputStream(new IoExceptionOnCloseStream());
            output.RegisterTemplate(ObjectMother.AllocInstrctnTemplateId, ObjectMother.AllocationInstruction());
            Message message = ObjectMother.BasicAllocationInstruction();
            try
            {
                output.WriteMessage(message);
                output.Close();
                Assert.Fail();
            }
            catch (FastException e)
            {
                Assert.AreEqual(FastConstants.IO_ERROR, e.Code);
            }
        }

        [Test]
        public void TestIoErrorOnWrite()
        {
            var output = new MessageOutputStream(new IoExceptionThrowingStream());
            output.RegisterTemplate(ObjectMother.AllocInstrctnTemplateId, ObjectMother.AllocationInstruction());
            Message message = ObjectMother.BasicAllocationInstruction();
            try
            {
                output.WriteMessage(message);
                Assert.Fail();
            }
            catch (FastException e)
            {
                Assert.AreEqual(FastConstants.IO_ERROR, e.Code);
            }
        }

        [Test]
        public void TestWriteMessageMessage()
        {
            var byteOut = new MemoryStream();
            var output = new MessageOutputStream(byteOut);
            try
            {
                output.WriteMessage(new Message(ObjectMother.AllocationInstruction()));
                Assert.Fail();
            }
            catch (FastException e)
            {
                Assert.AreEqual(FastConstants.D9_TEMPLATE_NOT_REGISTERED, e.Code);
            }
        }
    }

    public class IoExceptionOnCloseStream : Stream
    {
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get { return 0; }
            set { }
        }

        public override void Flush()
        {
            throw new IOException("The method or operation is not implemented.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new IOException("The method or operation is not implemented.");
        }
    }
}