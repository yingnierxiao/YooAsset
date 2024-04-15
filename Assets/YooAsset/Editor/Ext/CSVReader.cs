/*
 * NReco CSV library (https://github.com/nreco/csv/)
 * Copyright 2017-2018 Vitaliy Fedorchenko
 * Distributed under the MIT license
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// Fast and memory efficient implementation of CSV reader (3x times faster than CsvHelper).
    /// </summary>
    /// <remarks>API is similar to CSVHelper CsvReader.</remarks>
    public class CsvReader
    {
        public string Delimiter { get; private set; }
        int delimLength;

        /// <summary>
        /// Size of the circular buffer. Buffer size limits max length of the CSV line that can be processed. 
        /// </summary>
        /// <remarks>Default buffer size is 32kb.</remarks>
        public int BufferSize { get; set; } = 32768;

        /// <summary>
        /// If true start/end spaces are excluded from field values (except values in quotes). True by default.
        /// </summary>
        public bool TrimFields { get; set; } = true;

        TextReader rdr;

        public CsvReader(TextReader rdr) : this(rdr, ",")
        {
        }

        #region 扩展by zj

        private static string ConfigFlag = "";

        public static void SetLoadConfig(string flag)
        {
            ConfigFlag = flag;
        }


        public CsvReader(string csvPath, bool bTrimFields = true, bool isNotThreeRow = false)
        {
            string fullPath = string.Empty;
            TrimFields = bTrimFields;
            

            if (string.IsNullOrEmpty(fullPath))
            {
                fullPath = csvPath;
            }


            if (!File.Exists(fullPath))
            {
                Debug.Log($"csv model ,read csv {fullPath} error");
                return;
            }

            FileStream sf = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            this.rdr = new StreamReader(sf);
            Delimiter = ",";

            while (rdr.ReadLine() != null)
            {
                count++;
            }
            //从第三行之后开始读取
            count = count - 3;

            sf.Position = 0;

            Read();

            for (int i = 0; i < FieldsCount; i++)
            {
                typeNames.Add(this[i]);
            }

            Read();

            for (int i = 0; i < FieldsCount; i++)
            {
                string name = this[i];
                int index = name.IndexOf("(");

                if (index > 0)
                {
                    name = name.Substring(0, index);
                }

                mapFieldIndex.Add(name, i);
            }
            //第三行注释

            if (isNotThreeRow) {
                count = count + 1;
                return;
            }
            Read();
            comments = new List<string>(FieldsCount);
            for (int i = 0; i < FieldsCount; i++)
            {
                comments.Add(this[i]);
            }
        }

        public void Close()
        {
            if (this.rdr != null)
            {
                this.rdr.Close();
                this.rdr.Dispose();
            }

        }


        public bool GetFieldBool(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            int v;
            int.TryParse(str, out v);
            return v > 0;
        }


        public string GetFieldString(string strFieldName, bool isNeckCheck = true)
        {
            if (!mapFieldIndex.ContainsKey(strFieldName))
            {
                Debug.LogError($"csv not find cell name {strFieldName}");
                return string.Empty;
            }
            string s = this[mapFieldIndex[strFieldName]];
            if ((s == "null" || s == "0") && isNeckCheck)
            {
                s = string.Empty;
            }

            return s;
        }


        public string[] GetFieldStringArray(string strFieldName)
        {
            string s = GetFieldString(strFieldName, false);
            string[] sArr = s.Split('|');

            return sArr;
        }

        public short GetFieldShort(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            short v = 0;
            if (!short.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldShort{strFieldName} [{str}] {m_owner.GetColumnIndex(strFieldName)}");
            }

            return v;
        }

        public ushort GetFieldUshort(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            ushort v = 0;
            if (!ushort.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldUshort{strFieldName} [{str}] {m_owner.GetColumnIndex(strFieldName)}");
            }

            return v;
        }

        public int GetFieldInt(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            int v;
            if (!int.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldInt {strFieldName} [{str}] {m_owner.GetColumnIndex(strFieldName)}");
            }

            return v;
        }

        public uint GetFieldUint(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            uint v = 0;
            if (!uint.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldUint {strFieldName} [{str}] {m_owner.GetColumnIndex(strFieldName)}");
            }

            return v;
        }

        public float GetFieldFloat(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            float v = 0;
            if (!float.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldFloat {strFieldName} [{str}] {m_owner.GetColumnIndex(strFieldName)}");
            }

            return v;
        }

        public long GetFieldLong(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            long v = 0;
            if (!long.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldLong {strFieldName} [{str}]");
            }

            return v;
        }

        public ulong GetFieldUlong(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            ulong v = 0;
            if (!ulong.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldUlong {strFieldName} [{str} {m_owner.GetColumnIndex(strFieldName)}]");
            }

            return v;
        }

        public sbyte GetFieldByte(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            sbyte v = 0;
            if (!sbyte.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldByte {strFieldName} [{str}] {m_owner.GetColumnIndex(strFieldName)}");
            }

            return v;
        }


        public byte GetFieldUbyte(string strFieldName)
        {
            string str = this[mapFieldIndex[strFieldName]];
            byte v = 0;
            if (!byte.TryParse(str, out v))
            {
                //Debug.Log($"GetFieldUbyte {strFieldName} {str}");
            }

            return v;
        }

        public float[] GetFieldVector4(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            float[] fArr = new float[4] { 0, 0, 0, 0 };
            int l = Math.Min(strArr.Length, fArr.Length);
            for (int i = 0; i < l; i++)
            {
                float f = 0;
                float.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public float[] GetFieldVector3(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            float[] fArr = new float[3] { 0, 0, 0 };
            int l = Math.Min(strArr.Length, fArr.Length);
            for (int i = 0; i < l; i++)
            {
                float f = 0;
                float.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public float[] GetFieldVector2(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            float[] fArr = new float[2] { 0, 0 };
            int l = Math.Min(strArr.Length, fArr.Length);
            for (int i = 0; i < l; i++)
            {
                float f = 0;
                float.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public int[] GetFieldIntArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            var l = strArr.Length;
            int[] fArr = new int[l];

            for (int i = 0; i < l; i++)
            {
                int d = 0;
                int.TryParse(strArr[i], out d);
                fArr[i] = d;
            }

            return fArr;
        }


        public short[] GetFieldShortArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            var l = strArr.Length;
            short[] fArr = new short[l];

            for (int i = 0; i < l; i++)
            {
                short f = 0;
                short.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public ushort[] GetFieldUshortArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            var l = strArr.Length;
            ushort[] fArr = new ushort[l];

            for (int i = 0; i < l; i++)
            {
                ushort f = 0;
                ushort.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public uint[] GetFieldUintArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            var l = strArr.Length;
            uint[] fArr = new uint[l];
            for (int i = 0; i < l; i++)
            {
                uint f = 0;
                uint.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public float[] GetFieldFloatArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            int l = strArr.Length;
            float[] fArr = new float[l];

            for (int i = 0; i < l; i++)
            {
                float f = 0;
                float.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public long[] GetFieldLongArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            int l = strArr.Length;
            long[] fArr = new long[l];

            for (int i = 0; i < l; i++)
            {
                long f = 0;
                long.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public ulong[] GetFieldUlongArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            int l = strArr.Length;
            ulong[] fArr = new ulong[l];

            for (int i = 0; i < l; i++)
            {
                ulong f = 0;
                ulong.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public byte[] GetFieldUbyteArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            int l = strArr.Length;
            byte[] fArr = new byte[l];

            for (int i = 0; i < l; i++)
            {
                byte f = 0;
                byte.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        public sbyte[] GetFieldSbyteArray(string strFieldName)
        {
            string[] strArr = GetFieldStringArray(strFieldName);
            int l = strArr.Length;
            sbyte[] fArr = new sbyte[l];

            for (int i = 0; i < l; i++)
            {
                sbyte f = 0;
                sbyte.TryParse(strArr[i], out f);
                fArr[i] = f;
            }

            return fArr;
        }

        #endregion

        public CsvReader(TextReader rdr, string delimiter)
        {
            this.rdr = rdr;
            Delimiter = delimiter;
            delimLength = delimiter.Length;

            if (delimLength == 0)
                throw new ArgumentException("Delimiter cannot be empty.");
        }

        char[] buffer = null;
        int bufferLength;
        int bufferLoadThreshold;
        int lineStartPos = 0;
        int actualBufferLen = 0;
        List<Field> fields = null;
        private Dictionary<string, int> mapFieldIndex = null;
        //类型
        List<string> typeNames;
        //注释
        private List<string> comments;
        int fieldsCount = 0;
        int linesRead = 0;

        private int count = 0;

        public int Count => count;


        private int ReadBlockAndCheckEof(char[] buffer, int start, int len, ref bool eof)
        {
            if (len == 0)
                return 0;
            var read = rdr.ReadBlock(buffer, start, len);
            if (read < len)
                eof = true;
            return read;
        }

        private bool FillBuffer()
        {
            var eof = false;
            var toRead = bufferLength - actualBufferLen;
            if (toRead >= bufferLoadThreshold)
            {
                int freeStart = (lineStartPos + actualBufferLen) % buffer.Length;
                if (freeStart >= lineStartPos)
                {
                    actualBufferLen += ReadBlockAndCheckEof(buffer, freeStart, buffer.Length - freeStart, ref eof);
                    if (lineStartPos > 0)
                        actualBufferLen += ReadBlockAndCheckEof(buffer, 0, lineStartPos, ref eof);
                }
                else
                {
                    actualBufferLen += ReadBlockAndCheckEof(buffer, freeStart, toRead, ref eof);
                }
            }

            return eof;
        }

        private string GetLineTooLongMsg()
        {
            return String.Format("CSV line #{1} length exceedes buffer size ({0})", BufferSize, linesRead);
        }

        private int ReadQuotedFieldToEnd(int start, int maxPos, bool eof, ref int escapedQuotesCount)
        {
            int pos = start;
            int chIdx;
            char ch;
            for (; pos < maxPos; pos++)
            {
                chIdx = pos < bufferLength ? pos : pos % bufferLength;
                ch = buffer[chIdx];
                if (ch == '\"')
                {
                    bool hasNextCh = (pos + 1) < maxPos;
                    if (hasNextCh && buffer[(pos + 1) % bufferLength] == '\"')
                    {
                        // double quote inside quote = just a content
                        pos++;
                        escapedQuotesCount++;
                    }
                    else
                    {
                        return pos;
                    }
                }
            }

            if (eof)
            {
                // this is incorrect CSV as quote is not closed
                // but in case of EOF lets ignore that
                return pos - 1;
            }

            throw new InvalidDataException(GetLineTooLongMsg());
        }

        private bool ReadDelimTail(int start, int maxPos, ref int end)
        {
            int pos;
            int idx;
            int offset = 1;
            for (; offset < delimLength; offset++)
            {
                pos = start + offset;
                idx = pos < bufferLength ? pos : pos % bufferLength;
                if (pos >= maxPos || buffer[idx] != Delimiter[offset])
                    return false;
            }

            end = start + offset - 1;
            return true;
        }

        private Field GetOrAddField(int startIdx)
        {
            fieldsCount++;
            while (fieldsCount > fields.Count)
                fields.Add(new Field());
            var f = fields[fieldsCount - 1];
            f.Reset(startIdx);
            return f;
        }

        public int FieldsCount
        {
            get { return fieldsCount; }
        }

        public string this[int idx]
        {
            get
            {
                if (idx < fieldsCount)
                {
                    var f = fields[idx];
                    return fields[idx].GetValue(buffer);
                }

                return null;
            }
        }

        public int GetValueLength(int idx)
        {
            if (idx < fieldsCount)
            {
                var f = fields[idx];
                return f.Quoted ? f.Length - f.EscapedQuotesCount : f.Length;
            }

            return -1;
        }

        public void ProcessValueInBuffer(int idx, Action<char[], int, int> handler)
        {
            if (idx < fieldsCount)
            {
                var f = fields[idx];
                if ((f.Quoted && f.EscapedQuotesCount > 0) || f.End >= bufferLength)
                {
                    var chArr = f.GetValue(buffer).ToCharArray();
                    handler(chArr, 0, chArr.Length);
                }
                else if (f.Quoted)
                {
                    handler(buffer, f.Start + 1, f.Length - 2);
                }
                else
                {
                    handler(buffer, f.Start, f.Length);
                }
            }
        }

        public bool Read()
        {
        Start:
            if (fields == null)
            {
                fields = new List<Field>();
                mapFieldIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                typeNames = new List<string>();
                fieldsCount = 0;
            }

            if (buffer == null)
            {
                bufferLoadThreshold = Math.Min(BufferSize, 8192);
                bufferLength = BufferSize + bufferLoadThreshold;
                buffer = new char[bufferLength];
                lineStartPos = 0;
                actualBufferLen = 0;
            }

            var eof = FillBuffer();

            fieldsCount = 0;
            if (actualBufferLen <= 0)
            {
                return false; // no more data
            }

            linesRead++;

            int maxPos = lineStartPos + actualBufferLen;
            int charPos = lineStartPos;

            var currentField = GetOrAddField(charPos);
            bool ignoreQuote = false;
            char delimFirstChar = Delimiter[0];
            bool trimFields = TrimFields;

            int charBufIdx;
            char ch;
            for (; charPos < maxPos; charPos++)
            {
                charBufIdx = charPos < bufferLength ? charPos : charPos % bufferLength;
                ch = buffer[charBufIdx];
                switch (ch)
                {
                    case '\"':
                        if (ignoreQuote)
                        {
                            currentField.End = charPos;
                        }
                        else if (currentField.Quoted || currentField.Length > 0)
                        {
                            // current field already is quoted = lets treat quotes as usual chars
                            currentField.End = charPos;
                            currentField.Quoted = false;
                            ignoreQuote = true;
                        }
                        else
                        {
                            var endQuotePos = ReadQuotedFieldToEnd(charPos + 1, maxPos, eof,
                                ref currentField.EscapedQuotesCount);
                            currentField.Start = charPos;
                            currentField.End = endQuotePos;
                            currentField.Quoted = true;
                            charPos = endQuotePos;
                        }

                        break;
                    case '\r':
                        if ((charPos + 1) < maxPos && buffer[(charPos + 1) % bufferLength] == '\n')
                        {
                            // \r\n handling
                            charPos++;
                        }

                        // in some files only \r used as line separator - lets allow that
                        charPos++;
                        goto LineEnded;
                    case '\n':
                        charPos++;
                        goto LineEnded;
                    default:
                        if (ch == delimFirstChar && (delimLength == 1 || ReadDelimTail(charPos, maxPos, ref charPos)))
                        {
                            currentField = GetOrAddField(charPos + 1);
                            ignoreQuote = false;
                            continue;
                        }

                        // space
                        if (ch == ' ' && trimFields)
                        {
                            continue; // do nothing
                        }

                        // content char
                        if (currentField.Length == 0)
                        {
                            currentField.Start = charPos;
                        }

                        if (currentField.Quoted)
                        {
                            // non-space content after quote = treat quotes as part of content
                            currentField.Quoted = false;
                            ignoreQuote = true;
                        }

                        currentField.End = charPos;
                        break;
                }
            }

            if (!eof)
            {
                // line is not finished, but whole buffer was processed and not EOF
                throw new InvalidDataException(GetLineTooLongMsg());
            }

        LineEnded:
            actualBufferLen -= charPos - lineStartPos;
            lineStartPos = charPos % bufferLength;

            if (fieldsCount == 1 && fields[0].Length == 0)
            {
                // skip empty lines
                goto Start;
            }

            return true;
        }


        internal sealed class Field
        {
            internal int Start;
            internal int End;

            internal int Length
            {
                get { return End - Start + 1; }
            }

            internal bool Quoted;
            internal int EscapedQuotesCount;
            string cachedValue = null;

            internal Field()
            {
            }

            internal Field Reset(int start)
            {
                Start = start;
                End = start - 1;
                Quoted = false;
                EscapedQuotesCount = 0;
                cachedValue = null;
                return this;
            }

            internal string GetValue(char[] buf)
            {
                if (cachedValue == null)
                {
                    cachedValue = GetValueInternal(buf);
                }

                return cachedValue;
            }

            string GetValueInternal(char[] buf)
            {
                if (Quoted)
                {
                    var s = Start + 1;
                    var lenWithoutQuotes = Length - 2;
                    var val = lenWithoutQuotes > 0 ? GetString(buf, s, lenWithoutQuotes) : String.Empty;
                    if (EscapedQuotesCount > 0)
                        val = val.Replace("\"\"", "\"");
                    return val;
                }

                var len = Length;
                return len > 0 ? GetString(buf, Start, len) : String.Empty;
            }

            private string GetString(char[] buf, int start, int len)
            {
                var bufLen = buf.Length;
                start = start < bufLen ? start : start % bufLen;
                var endIdx = start + len - 1;
                if (endIdx >= bufLen)
                {
                    var prefixLen = buf.Length - start;
                    var prefix = new string(buf, start, prefixLen);
                    var suffix = new string(buf, 0, len - prefixLen);
                    return prefix + suffix;
                }

                return new string(buf, start, len);
            }
        }
    }
}