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
using System.Globalization;
using System.IO;
using OpenFAST.util;

namespace OpenFAST.Template.Type.Codec
{
    [Serializable]
    public sealed class MillisecondsSinceMidnight : TypeCodec
    {
        public override ScalarValue Decode(Stream inStream)
        {
            int millisecondsSinceMidnight = INTEGER.Decode(inStream).ToInt();
            
            Calendar cal = new GregorianCalendar();
            int hour = millisecondsSinceMidnight/3600000;
            millisecondsSinceMidnight -= hour*3600000;
            SupportClass.CalendarManager.manager.Set(cal, SupportClass.CalendarManager.HOUR_OF_DAY, hour);
            int minute = millisecondsSinceMidnight/60000;
            millisecondsSinceMidnight -= minute*60000;
            SupportClass.CalendarManager.manager.Set(cal, SupportClass.CalendarManager.MINUTE, minute);
            int second = millisecondsSinceMidnight/1000;
            millisecondsSinceMidnight -= second*1000;
            SupportClass.CalendarManager.manager.Set(cal, SupportClass.CalendarManager.SECOND, second);
            int millisecond = millisecondsSinceMidnight;
            SupportClass.CalendarManager.manager.Set(cal, SupportClass.CalendarManager.MILLISECOND, millisecond);
            DateTime tempAux = SupportClass.CalendarManager.manager.GetDateTime(cal);
            return new DateValue(tempAux);
        }

        public override byte[] EncodeValue(ScalarValue value)
        {
            DateTime date = ((DateValue) value).Value;
            int millisecondsSinceMidnight = Util.MillisecondsSinceMidnight(date);
            return INTEGER.EncodeValue(new IntegerValue(millisecondsSinceMidnight));
        }

        public override bool Equals(Object obj)
        {
            return obj != null && obj.GetType() == GetType();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}