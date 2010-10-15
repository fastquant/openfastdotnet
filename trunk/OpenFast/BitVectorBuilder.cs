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
using System;

namespace OpenFAST
{
    public sealed class BitVectorBuilder
    {
        private readonly BitVector vector;
        private int index;

        public BitVectorBuilder(int size)
        {
            vector = new BitVector(size);
        }

        public BitVector BitVector
        {
            get { return vector; }
        }

        public Object OnValueSkipOnNull
        {
            set
            {
                if (value == null)
                    Skip();
                else
                    Set();
            }
        }

        public int Index
        {
            get { return index; }
        }

        public void Set()
        {
            vector.Set(index);
            index++;
        }

        public void Skip()
        {
            index++;
        }
    }
}