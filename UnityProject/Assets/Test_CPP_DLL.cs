using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;
using System;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public class Character
{
    public float x;
    public float y;
    public float z;

    public float vx;
    public float vy;
    public float vz;

    public float angle;
    public float vAngle;

    public float pad1;
    public float pad2;
    public float pad3;
    public float pad4;
    public float pad5;
    public float pad6;
    public float pad7;
    public float pad8;
}

public class Test_CPP_DLL : MonoBehaviour
{
    [DllImport("CPP64DLLTest")]
    public static extern int AddFun(int a, int b);

    [DllImport("CPP64DLLTest")]
    public static extern int SubFun(int a, int b);

    [DllImport("CPP64DLLTest", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public unsafe static extern void UpdateCharacters(void* characterPointsStart, int arraySize);
    private Character[] chaArray;

    public GameObject p1;
    public GameObject p2;
    CppPacket<Character> cppPacket;

    public bool startcpp = false;

    void Start()
    {
        chaArray = new Character[2];

        for (int i = 0; i < 2; ++i)
        {
            chaArray[i] = new Character();

            chaArray[i].x = 0;
            chaArray[i].y = 0;
            chaArray[i].z = 0;

            chaArray[i].vx = i * 0.01f + 0.01f;
            chaArray[i].vy = i * 0.01f + 0.01f;
            chaArray[i].vz = 0;

            chaArray[i].angle = 0;
            chaArray[i].vAngle = i + 0.5f;
        }
    }

    
    public void Startcpp()
    {
        ///当你想调用Cpp的时候，封装和固定数组指针。当你固定指针之后，在你释放之前，你的对象内存都是安全的。
        ///在释放之前，不能改变数组元素对象，因为这是不安全的操作。
        this.startcpp = true;
        cppPacket = new CppPacket<Character>(chaArray);
    }

    public void StopCpp()
    {
        ///当你调用Cpp结束的时候，释放你封装和固定的指针。
        this.startcpp = false;
        cppPacket.Dispose();
        cppPacket = null;
    }

    void Update()
	{
        if (startcpp)
        {
            unsafe
            {
                UpdateCharacters(cppPacket, cppPacket.Length);
            }

            p1.transform.position = new Vector3(chaArray[0].x, chaArray[0].y, chaArray[0].z);
            p2.transform.position = new Vector3(chaArray[1].x, chaArray[1].y, chaArray[1].z);

            p1.transform.rotation = Quaternion.Euler(0, 0, chaArray[0].angle);
            p2.transform.rotation = Quaternion.Euler(0, 0, chaArray[1].angle);


            print(chaArray[1].x);
            print(chaArray[1].y);
            print(chaArray[1].z);
            print(chaArray[1].vx);
            print(chaArray[1].vy);
            print(chaArray[1].vz);
            print(chaArray[1].angle);
            print(chaArray[1].vAngle);

            print(chaArray[1].pad1);
            print(chaArray[1].pad2);
            print(chaArray[1].pad3);
            print(chaArray[1].pad4);
            print(chaArray[1].pad5);
            print(chaArray[1].pad6);
            print(chaArray[1].pad7);
            print(chaArray[1].pad8);
        }
    }

    public void ResetGameObject()
    {
        p1.transform.position = Vector3.zero;
        p2.transform.position = Vector3.zero;

        for (int i = 0; i < 2; ++i)
        {
            chaArray[i].x = 0;
            chaArray[i].y = 0;
            chaArray[i].z = 0;

            chaArray[i].vx = i * 0.01f + 0.01f;
            chaArray[i].vy = i * 0.01f + 0.01f;
            chaArray[i].vz = 0;

            chaArray[i].angle = 0;
            chaArray[i].vAngle = i + 0.5f;
        }
    }

    public class CppPacket<T>:IDisposable where T:class
    {

        private IntPtr buffer;
        Action free = null;
        public int Length { get; private set; }
        public CppPacket(T[] chaArray)
        {
            Length = chaArray.Length;
            ///申请一块内存来存放每个对象的指针
            this.buffer = Marshal.AllocHGlobal(8 * chaArray.Length);

            Debug.Log("pointStart  " + Convert.ToString(buffer.ToInt64(), 16));

            for (int i = 0; i < chaArray.Length; i++)
            {
                IntPtr temp = new IntPtr(buffer.ToInt64() + 8 * i);
                Debug.Log(i + "  point  " + Convert.ToString(temp.ToInt64(), 16));
                ///固定每个元素的地址，防止被GC移动或回收
                var hl = GCHandle.Alloc(chaArray[i], GCHandleType.Pinned);
                IntPtr ptr = hl.AddrOfPinnedObject();
                Debug.Log(i + "  element  " + Convert.ToString(ptr.ToInt64(), 16));
                Marshal.WriteInt64(temp, ptr.ToInt64());

                free += () =>
                {
                    ///注册释放函数GCHandle 只能释放从GCHandle转换出来的指针
                    hl.Free();
                };
            }
        }

        public unsafe static implicit operator void* (CppPacket<T> packet)
        {
            return packet.buffer.ToPointer();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。

                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。


                ///取消固定每个元素的地址
                free.Invoke();
                free = null;
                ///释放内存 
                Marshal.FreeHGlobal(buffer);
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~CppPacket()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
