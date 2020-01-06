using System;
using System.Collections.Generic;
using System.Text;

namespace Warlord
{
    public class RC4
    {
        int i;
        int j;

        int[] key = new int[256];
        int[] table = new int[256];



        public RC4(string PublicKey)
        {
            //Initialise the variables that store offset info
            this.initCalculators();

            //Set up the key and table arrays
            this.init(decodeKey(PublicKey));

            //Premix the encryption
            this.premixTable(this.premixString, this.premixCount);
        }

        public string decodeKey(string origKey)
        {
            string table = JavaSubstring(origKey, 0, origKey.Length / 2);
            string key = origKey.Substring(origKey.Length / 2);
            long checkSum = 0L;
            for (int i = 0; i < table.Length; i++)
            {
                int offset = table.IndexOf(key.Substring(i, 1));
                if (offset % 2 == 0)
                    offset *= 2;
                if (i % 3 == 0)
                    offset *= 3;
                if (offset < 0)
                    offset = table.Length % 2;
                checkSum += offset;
                checkSum ^= offset << (i % 3) * 8;
            }

            return checkSum.ToString();
        }

        private void initCalculators()
        {
            i = 0;
            j = 0;
        }

        private void premixTable(string datain, int count)
        {
            for (int a = 0; a < count; a++)
            {
                this.encipher(datain);
            }
        }

        private void init(string skey)
        {
            int keyValue = int.Parse(skey);
            int keyLength = (keyValue & 0xf8) / 8;

            if (keyLength < 20)
            {
                keyLength += 20;
            }

            int keyOffset = keyValue % keyWindow.Length;

            int tGiven = keyValue;
            int tOwn = 0;

            int[] w = new int[keyLength];

            for (int a = 0; a < keyLength; a++)
            {
                tOwn = keyWindow[Math.Abs((keyOffset + a) % keyWindow.Length)];
                w[a] = Math.Abs(tGiven ^ tOwn);
                if (a == 31)
                {
                    tGiven = keyValue;
                }
                else
                {
                    tGiven = (tGiven / 2);
                }

            }

            for (int b = 0; b < 256; b++)
            {
                key[b] = w[b % w.Length];
                table[b] = b;
            }

            int t = 0;
            int u = 0;
            for (int a = 0; a < 256; a++)
            {
                u = (int)((u + table[a] + key[a]) % 256);
                t = table[a];
                table[a] = table[u];
                table[u] = t;
            }


        }

        public string encipher(string data)
        {
            StringBuilder cipher = new StringBuilder(data.Length * 2);

            int t = 0;
            int k = 0;

            for (int a = 0; a < data.Length; a++)
            {
                i = (i + 1) % 256;
                j = (j + table[i]) % 256;
                t = table[i];
                table[i] = table[j];
                table[j] = t;

                k = table[(table[i] + table[j]) % 256];

                int c = (char)data.Substring(a, 1).ToCharArray()[0] ^ k;

                if (c <= 0)
                {
                    cipher.Append("00");
                }
                else
                {
                    cipher.Append(di[c >> 4 & 0xf]);
                    cipher.Append(di[c & 0xf]);
                }

            }

            return cipher.ToString();
        }



        public string deciphper(string data)
        {
            StringBuilder cipher = new StringBuilder(data.Length);
            int t = 0;
            int k = 0;
            for (int a = 0; a < data.Length; a += 2)
            {
                i = (i + 1) % 256;
                j = (j + table[i]) % 256;
                t = table[i];
                table[i] = table[j];
                table[j] = t;
                k = table[(table[i] + table[j]) % 256];
                //t = System.Convert.ToInt32( data.Substring(a, a + 2), 16);
                t = System.Convert.ToInt32(JavaSubstring(data, a, a + 2), 16);
                cipher = cipher.Append((char)(t ^ k));
            }

            return cipher.ToString();
        }

        public string JavaSubstring(string dataIn, int start, int end)
        {
            return dataIn.Substring(start, end - start);
        }
        #region Constants

        string[] di = {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", 
        "A", "B", "C", "D", "E", "F"
        };

        int[] keyWindow = {204
,53
,74
,109
,63
,4
,163
,182
,210
,186
,19
,162
,160
,115
,139
,83
,235
,177
,14
,15
,11
,127
,4
,210
,222
,138
,10
,138
,151
,236
,158
,186
,67
,1
,168
,69
,139
,214
,243
,32
,157
,161
,211
,155
,20
,192
,214
,155
,12
,153
,192
,112
,98
,146
,33
,30
,22
,131
,81
,161
,105
,142
,103
,204
,112
,9
,167
,185
,176
,51
,27
,166
,249
,228
,24
,165
,197
,25
,166
,216
,74
,14
,104
,15
,77
,49
,6
,50
,65
,126
,10
,187
,15
,17
,189
,155
,246
,221
,92
,104
,79
,87
,186
,88
,80
,50
,223
,126
,148
,217
,81
,223
,91
,70
,165
,237
,150
,95
,195
,205
,199
,176
,156
,122
,187
,232
,252
,230
,169
,94
,157
,194
,44
,164
,208
,22
,141
,139
,167
,236
,201
,42
,130
,14
,44
,57
,253
,224
,130
,118
,242
,226
,146
,202
,154
,40
,201
,171
,160
,91
,143
,144
,150
,197
,169
,204
,121
,131
,139
,112
,214
,196
,74
,123
,159
,220
,77
,176
,151
,73
,125
,135
,166
,26
,176
,31
,255
,234
,91
,30
,218
,41
,121
,17
,45
,3
,234
,35
,185
,52
,112
,108
,65
,72
,184
,93
,225
,113
,62
,0
,110
,38
,43
,15
,44
,114
,162
,167
,69
,40
,103
,144
,114
,215
,228
,47
,112
,235
,179
,211
,116
,237
,70
,167
,36
,224
,183
,11
,0
,74
,145
,241
,153
,40
,151
,211
,231
,199
,235
,176
,109
,95
,160
,141
,137
,236
,39
,17
,246
,97
,120
,227
,12
,1
,195
,239
,150
,169
,85
,226
,23
,58
,145
,157
,37
,218
,132
,168
,94
,15
,240
,24
,152
,230
,249
,80
,145
,208
,209
,144
,154
,228
,197
,40
,6
,248
,90
,15
,1
,82
,145
,77
,220
,27
,167
,0
,149
,0
,103
,53
,226
,242
,175
,9
,177
,130
,65
,216
,107
,4
,194
,71
,135
,231
,151
,178
,188
,220
,33
,152
,120
,165
,73
,124
,32
,215
,127
,130
,29
,40
,20
,3
,212
,254
,106
,42
,98
,7
,8
,129
,195
,30
,74
,118
,169
,81
,88
,235
,149
,232
,181
,182
,206
,82
,163
,26
,116
,37
,41
,50
,63
,185
,165
,2
,81
,10
,149
,103
,211
,168
,34
,55
,32
,233
,16
,238
,219
,235
,170
,255
,244
,12
,89
,211
,88
,33
,24
,38
,190
,75
,70
,86
,89
,2
,189
,134
,207
,65
,6
,148
,124
,22
,57
,21
,118
,227
,173
,21
,236
,236
,139
,189
,230
,153
,153
,182
,230
,216
,26
,0
,9
,50
,32
,189
,97
,3
,208
,201
,103
,163
,96
,0
,42
,11
,173
,98
,102
,76
,31
,243
,59
,71
,223
,252
,186
,157
,231
,90
,212
,83
,10
,69
,69
,165
,209
,112
,157
,237
,24
,90
,4
,44
,247
,32
,159
,126
,171
,99
,216
,196
,228
,217
,157
,143
,32
,16
,111
,67
,106
,231
,10
,167
,13
,240
,182
,105
,52
,12
,84
,91
,243
,205
,180
,180
,35
,58
,238
,240
,0
,209
,48
,249
,243
,209
,93
,10
,22
,183
,5
,177
,110
,16
,188
,201
,240
,194
,11
,76
,219
,67
,254
,176
,139
,66
,81
,138
,109
,178
,71
,143
,74
,217
,52
,0
,127
,190
,12
,214
,231
,84
,239
,165
,155
,89
,95
,106
,62
,30
,182
,137
,85
,39
,221
,51
,188
,149
,104
,167
,71
,11
,220
,212
,246
,114
,10
,4
,216
,127
,233
,231
,178
,174
,181
,29
,49
,118
,177
,108
,156
,174
,118
,196
,216
,106
,203
,96
,65
,12
,140
,248
,152
,35
,152
,17
,89
,136
,138
,94
,5
,190
,92
,189
,16
,216
,61
,70
,165
,36
,238
,167
,16
,61
,206
,140
,226
,251
,37
,225
,211
,111
,42
,195
,36
,248
,233
,67
,146
,100
,244
,23
,154
,103
,48
,4
,15
,33
,169
,151
,13
,151
,115
,173
,37
,103
,172
,23
,182
,29
,22
,25
,54
,46
,188
,14
,24
,12
,182
,241
,163
,90
,121
,172
,29
,73
,191
,91
,232
,229
,197
,200
,32
,7
,67
,214
,141
,248
,10
,135
,168
,4
,144
,17
,94
,228
,76
,202
,130
,174
,251
,170
,100
,173
,232
,183
,132
,130
,35
,163
,1
,154
,134
,56
,202
,13
,190
,224
,56
,107
,107
,244
,16
,12
,149
,220
,120
,245
,179
,103
,85
,255
,195
,187
,191
,82
,225
,13
,206
,106
,60
,212
,12
,211
,247
,112
,185
,5
,56
,226
,236
,179
,181
,208
,204
,16
,159
,158
,36
,65
,101
,148
,23
,89
,125
,27
,61
,117
,255
,142
,32
,138
,105
,166
,203
,253
,113
,138
,30
,247
,250
,198
,21
,244
,113
,40
,161
,229
,179
,100
,76
,30
,177
,69
,87
,90
,9
,135
,254
,108
,99
,145
,195
,145
,138
,223
,237
,52
,126
,244
,109
,171
,44
,0
,187
,129
,127
,49
,220
,100
,253
,0
,116
,93
,87
,39
,245
,5
,54
,203
,241
,155
,255
,125
,80
,253
,75
,71
,242
,147
,153
,148
,214
,91
,33
,181
,78
,10
,82
,171
,89
,179
,221
,144
,224
,138
,112
,254
,152
,186
,190
,224
,44
,251
,60
,133
,65
,70
,72
,203
,126
,123
,212
,108
,68
,185
,42
,208
,51
,11
,177
,3
,24
,207
,14
,148
,113
,55
,1
,19
,179
,31
,133
,11
,227
,72
,145
,242
,157
,244
,239
,129
,124
,109
,56
,134
,56
,95
,110
,161
,73
,151
,136
,67
,176
,201
,193
,70
,53
,31
,238
,84
,81
,65
,50
,182
,20
,17
,247
,179
,217
,14
,34
,182
,97
,55
,117
,176
,108
,234
,147
,89
,168
,7
,251
,212
,22
,107
,63
,248
,179
,222
,167
,214
,136
,74
,53
,47
,120
,233
,131
,41
,167
,220
,56
,12
,51
,125
,207
,112
,179
,211
,47
,134
,223
,112
,223
,46
,249
,24
,64
,58
,36
,187
,77
,132
,116
,116
,111
,36
,127
,217
,177
,24
,58
,102
,166
,105
,119
,234
,187
,198
,77
,153
,23
,157
,103
,92
,33
,136
,182
,131
,154
,141
,149
,4
,117
,213
,226
,64
,116
,55
,6
,159
,126
,225};


        string premixString = "eb11nmhdwbn733c2xjv1qln3ukpe0hvce0ylr02s12sv96rus2ohexr9cp8rufbmb1mdb732j1l3kehc0l0s2v6u2hx9prfmu";
        int premixCount = 17;


        #endregion

    }

    public class HabboEncoding
    {



        //public void ClearCookies()
        //{
        //    MessageBox.Show(B.Document.Cookie.ToString());
        //}

        //public string GetSSO(string Username, string Password, string Hotel)
        //{
        //WebBrowser B = new WebBrowser();
        //B.Navigate("www.habbo.co.uk");
        // }
        public int GetVL64Length(char[] raw)
        {
            int pos = 0;
            int v = 0;
            bool negative = (raw[pos] & 4) == 4;
            int totalBytes = raw[pos] >> 3 & 7;
            v = raw[pos] & 3;
            pos++;
            int shiftAmount = 2;
            for (int b = 1; b < totalBytes; b++)
            {
                v |= (raw[pos] & 0x3f) << shiftAmount;
                shiftAmount = 2 + 6 * b;
                pos++;
            }

            if (negative == true)
                v *= -1;

            string Tmp;
            Tmp = encodeVL64(v);
            return Tmp.Length;
        }

        public string encodeB64(int value, int length)
        {
            StringBuilder stack = new StringBuilder(length);
            for (int x = 1; x <= length; x++)
            {
                int offset = 6 * (length - x);
                byte val = (byte)(64 + (value >> offset & 0x3f));
                stack.Append((char)val);
            }
            return stack.ToString();
        }

        public string encodeB64(string strVal)
        {
            int value = strVal.Length;
            int length = 2;
            StringBuilder stack = new StringBuilder(length);
            for (int x = 1; x <= length; x++)
            {
                int offset = 6 * (length - x);
                byte val = (byte)(64 + (value >> offset & 0x3f));
                stack.Append((char)val);
            }
            return stack.ToString();
        }

        public int decodeB64(string strVal)
        {
            char[] val = strVal.ToCharArray();
            int intTot = 0;
            int y = 0;
            for (int x = (val.Length - 1); x >= 0; x--)
            {
                int intTmp = (int)(byte)((val[x] - 64));
                if (y > 0)
                {
                    intTmp = intTmp * (int)(Math.Pow(64, y));
                }
                intTot += intTmp;
                y++;
            }
            return intTot;
        }

        public string encodeVL64(int i)
        {
            byte[] wf = new byte[6];
            int pos = 0;
            int startPos = pos;
            int bytes = 1;
            int negativeMask = i >= 0 ? 0 : 4;
            i = Math.Abs(i);
            wf[pos++] = (byte)(64 + (i & 3));
            for (i >>= 2; i != 0; i >>= 6)
            {
                bytes++;
                wf[pos++] = (byte)(64 + (i & 0x3f));
            }

            wf[startPos] = (byte)(wf[startPos] | bytes << 3 | negativeMask);

            System.Text.ASCIIEncoding encoder = new ASCIIEncoding();
            string tmp = encoder.GetString(wf);
            return tmp.Replace("\0", "");
        }





        public int decodeVL64(string data)
        {
            return decodeVL64(data.ToCharArray());
        }



        public int decodeVL64(char[] raw)
        {
            int pos = 0;
            int v = 0;
            bool negative = (raw[pos] & 4) == 4;
            int totalBytes = raw[pos] >> 3 & 7;
            v = raw[pos] & 3;
            pos++;
            int shiftAmount = 2;
            for (int b = 1; b < totalBytes; b++)
            {
                v |= (raw[pos] & 0x3f) << shiftAmount;
                shiftAmount = 2 + 6 * b;
                pos++;
            }

            if (negative == true)
                v *= -1;
            return v;
        }

        


    }
}
