/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;

using IronPython.Runtime;

[assembly: PythonModule("_random", typeof(IronPython.Modules.PythonRandom))]
namespace IronPython.Modules {
    [PythonType("_random")]
    public static class PythonRandom {
        [PythonType("Random")]
        public class Random {
            System.Random rnd;
            
            public Random() {
                this.Seed();
            }

            public Random(object seed) {
                this.Seed(seed);
            }

            private static uint getfour(byte[] bytes, int start, int end) {
                uint four = 0;
                int bits = end - start;
                int shift = 0;
                if (bits > 32) bits = 32;
                start /= 8;
                while (bits > 0) {
                    uint value = bytes[start];
                    if (bits < 8) value &= (1u << bits) - 1u;
                    value <<= shift;
                    four |= value;
                    bits -= 8;
                    shift += 8;
                    start++;
                }

                return four;
            }

            [PythonName("getrandbits")]
            public object GetRandomBits(int bits) {
                int count = (bits + 7) / 8;
                byte[] bytes = new byte[count];
                rnd.NextBytes(bytes);

                if (bits <= 32) {
                    return (int)getfour(bytes, 0, bits);
                } else if (bits <= 64) {
                    long a = getfour(bytes, 0, bits);
                    long b = getfour(bytes, 32, bits);
                    return a | (b << 32);
                } else {
                    count = (count + 3) / 4;
                    uint[] data = new uint[count];
                    for (int i = 0; i < count; i++) {
                        data[i] = getfour(bytes, i * 32, bits);
                    }
                    int sign = (data[data.Length - 1] & 0x80000000) != 0 ? -1 : 1;
                    return new IronMath.BigInteger(sign, data);
                }
            }

            [PythonName("getstate")]
            public object GetState() {
                return rnd;
            }

            [PythonName("jumpahead")]
            public void JumpAhead(int count) {
                rnd.NextBytes(new byte[4096]);
            }

            [PythonName("random")]
            public object NextRandom() {
                return rnd.NextDouble();
            }

            [PythonName("seed")]
            public void Seed() {
                Seed(DateTime.Now);
            }

            [PythonName("seed")]
            public void Seed(object s) {
                int newSeed;
                if (s is int) {
                    newSeed = (int)s;
                } else {
                    newSeed = s.GetHashCode();
                }
                rnd = new System.Random(newSeed);
            }

            [PythonName("setstate")]
            public void SetState(object state) {
                System.Random random = state as System.Random;
                if(random != null) rnd = random;
                
            }
        }
    }
}
