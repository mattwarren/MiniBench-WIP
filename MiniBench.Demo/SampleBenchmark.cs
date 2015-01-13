using MiniBench.Core;
using System;
using System.Diagnostics;

namespace MiniBench.Demo
{
    public class SampleBenchmark
    {
        Boolean debug = false;
        //Boolean debug = true;

        [Benchmark]
        [Category("Testing")]
        public void DateTimeNow()
        {
            if (debug && Debugger.IsAttached == false)
                Debugger.Launch();
            DateTime.Now.ToString();

            //--- c:\Users\warma11\Downloads\minibench\MiniBench.Demo\SampleBenchmark.cs -----
            //    13:             if (Debugger.IsAttached == false)
            //00000000  push        rsi 
            //00000001  sub         rsp,20h 
            //00000005  call        000000005F675754 
            //0000000a  test        al,al 
            //0000000c  jne         0000000000000013 
            //    14:                 Debugger.Launch();
            //0000000e  call        000000005ED613B0 
            //    15:             DateTime.Now.ToString();
            //00000013  call        000000005E5D3B90 
            //00000018  mov         rsi,rax 
            //0000001b  call        000000005E591210 
            //00000020  mov         r8,rax 
            //00000023  mov         rcx,rsi 
            //00000026  xor         edx,edx 
            //00000028  call        000000005E591B50 
            //0000002d  nop 
            //    16:         }
            //0000002e  add         rsp,20h 
            //00000032  pop         rsi 
            //00000033  ret 
        }

        [Benchmark]
        [Category("Testing")]
        public string DateTimeNowFixed()
        {
            return DateTime.Now.ToString();
        }

        [Benchmark]
        [Category("Testing")]
        public void DateTimeUtcNow()
        {
            DateTime.UtcNow.ToString();
        }

        [Benchmark]
        [Category("Testing")]
        public void MathSqrt()
        {
            if (debug && Debugger.IsAttached == false)
                Debugger.Launch();
            Math.Sqrt(123.456); // this gets optimised away by the JITter

            //--- c:\Users\warma11\Downloads\minibench\MiniBench.Demo\SampleBenchmark.cs -----
            //    36:             if (Debugger.IsAttached == false)
            //00000000  sub         rsp,28h 
            //00000004  call        000000005F674E64 
            //00000009  test        al,al 
            //0000000b  jne         0000000000000012 
            //    37:                 Debugger.Launch();
            //0000000d  call        000000005ED60AC0 
            //00000012  nop 
            //    39:         }
            //00000013  add         rsp,28h 
            //00000017  ret 
        }
    }
}
