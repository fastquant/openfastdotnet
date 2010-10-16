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

*/
using System.IO;

namespace OpenFAST
{
    public struct MessageBlockReader_Fields
    {
        public static readonly IMessageBlockReader NULL;

        static MessageBlockReader_Fields()
        {
            NULL = new NullMessageBlockReader();
        }
    }

    public sealed class NullMessageBlockReader : IMessageBlockReader
    {
        #region MessageBlockReader Members

        public bool ReadBlock(Stream inStream)
        {
            return true;
        }

        public void MessageRead(Stream inStream, Message message)
        {
        }

        #endregion
    }

    public interface IMessageBlockReader
    {
        bool ReadBlock(Stream inStream);
        void MessageRead(Stream inStream, Message message);
    }
}