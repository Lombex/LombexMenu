using System;
using System.Runtime.InteropServices;
using UnhollowerBaseLib.Runtime;

namespace LombexMenu.Protection.EventLogic
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Il2CppMethodInfo
    {
        public IntPtr methodPointer;
        public IntPtr invoker_method;
        public IntPtr name;
        public Il2CppClass* klass;
        public Il2CppTypeStruct* return_type;
        public Il2CppParameterInfo* parameters;
        public IntPtr someRtData;
        public IntPtr someGenericData;
        public int customAttributeIndex;
        public uint token;
        public Il2CppMethodFlags flags;
        public Il2CppMethodImplFlags iflags;
        public ushort slot;
        public byte parameters_count;
        public MethodInfoExtraFlags extra_flags;
    }
}
