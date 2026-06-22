using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SoundMeter
{
    internal sealed class AudioDeviceService
    {
        private const int CLSCTX_INPROC_SERVER = 1;
        private const int DEVICE_STATE_ACTIVE = 1;

        private static readonly Guid ClsidMMDeviceEnumerator = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");
        private static readonly Guid IidMMDeviceEnumerator = new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6");
        private static readonly Guid ClsidPolicyConfig = new Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9");
        private static readonly Guid IidPolicyConfig = new Guid("F8679F50-850A-41CF-9C72-430F290290C8");
        private static readonly PROPERTYKEY PkeyDeviceFriendlyName = new PROPERTYKEY
        {
            fmtid = new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"),
            pid = 14
        };

        public IList<AudioDevice> GetOutputDevices()
        {
            var devices = new List<AudioDevice>();
            IntPtr enumerator = IntPtr.Zero;
            IntPtr collection = IntPtr.Zero;
            IntPtr defaultDevice = IntPtr.Zero;
            string defaultId = null;

            try
            {
                Guid clsidEnumerator = ClsidMMDeviceEnumerator;
                Guid iidEnumerator = IidMMDeviceEnumerator;
                Check(CoCreateInstance(ref clsidEnumerator, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref iidEnumerator, out enumerator));

                if (CallGetDefaultAudioEndpoint(enumerator, EDataFlow.eRender, ERole.eMultimedia, out defaultDevice) == 0)
                {
                    defaultId = GetDeviceId(defaultDevice);
                }

                Check(CallEnumAudioEndpoints(enumerator, EDataFlow.eRender, DEVICE_STATE_ACTIVE, out collection));

                uint count;
                Check(CallCollectionGetCount(collection, out count));

                for (uint index = 0; index < count; index++)
                {
                    IntPtr device = IntPtr.Zero;
                    try
                    {
                        Check(CallCollectionItem(collection, index, out device));
                        string id = GetDeviceId(device);
                        string name = GetDeviceName(device);
                        devices.Add(new AudioDevice(id, name, string.Equals(id, defaultId, StringComparison.OrdinalIgnoreCase)));
                    }
                    finally
                    {
                        Release(device);
                    }
                }
            }
            finally
            {
                Release(defaultDevice);
                Release(collection);
                Release(enumerator);
            }

            devices.Sort((left, right) =>
            {
                if (left.IsDefault != right.IsDefault)
                {
                    return left.IsDefault ? -1 : 1;
                }

                return string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase);
            });

            return devices;
        }

        public void SetDefaultOutputDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentException("Device id is required.", "deviceId");
            }

            IntPtr policyConfig = IntPtr.Zero;
            try
            {
                Guid clsidPolicyConfig = ClsidPolicyConfig;
                Guid iidPolicyConfig = IidPolicyConfig;
                Check(CoCreateInstance(ref clsidPolicyConfig, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref iidPolicyConfig, out policyConfig));
                Check(CallSetDefaultEndpoint(policyConfig, deviceId, ERole.eConsole));
                Check(CallSetDefaultEndpoint(policyConfig, deviceId, ERole.eMultimedia));
                Check(CallSetDefaultEndpoint(policyConfig, deviceId, ERole.eCommunications));
            }
            finally
            {
                Release(policyConfig);
            }
        }

        private static string GetDeviceId(IntPtr device)
        {
            IntPtr idPtr;
            Check(CallDeviceGetId(device, out idPtr));
            try
            {
                return Marshal.PtrToStringUni(idPtr);
            }
            finally
            {
                CoTaskMemFree(idPtr);
            }
        }

        private static string GetDeviceName(IntPtr device)
        {
            IntPtr store = IntPtr.Zero;
            try
            {
                Check(CallDeviceOpenPropertyStore(device, 0, out store));

                PROPERTYKEY key = PkeyDeviceFriendlyName;
                PROPVARIANT value;
                Check(CallPropertyStoreGetValue(store, ref key, out value));

                try
                {
                    if (value.vt == 31 && value.p != IntPtr.Zero)
                    {
                        return Marshal.PtrToStringUni(value.p);
                    }
                }
                finally
                {
                    PropVariantClear(ref value);
                }
            }
            finally
            {
                Release(store);
            }

            return "Unknown audio device";
        }

        private static int CallEnumAudioEndpoints(IntPtr self, EDataFlow dataFlow, int stateMask, out IntPtr collection)
        {
            var method = GetMethod<EnumAudioEndpointsDelegate>(self, 3);
            return method(self, dataFlow, stateMask, out collection);
        }

        private static int CallGetDefaultAudioEndpoint(IntPtr self, EDataFlow dataFlow, ERole role, out IntPtr device)
        {
            var method = GetMethod<GetDefaultAudioEndpointDelegate>(self, 4);
            return method(self, dataFlow, role, out device);
        }

        private static int CallCollectionGetCount(IntPtr self, out uint count)
        {
            var method = GetMethod<CollectionGetCountDelegate>(self, 3);
            return method(self, out count);
        }

        private static int CallCollectionItem(IntPtr self, uint index, out IntPtr device)
        {
            var method = GetMethod<CollectionItemDelegate>(self, 4);
            return method(self, index, out device);
        }

        private static int CallDeviceOpenPropertyStore(IntPtr self, int access, out IntPtr store)
        {
            var method = GetMethod<DeviceOpenPropertyStoreDelegate>(self, 4);
            return method(self, access, out store);
        }

        private static int CallDeviceGetId(IntPtr self, out IntPtr id)
        {
            var method = GetMethod<DeviceGetIdDelegate>(self, 5);
            return method(self, out id);
        }

        private static int CallPropertyStoreGetValue(IntPtr self, ref PROPERTYKEY key, out PROPVARIANT value)
        {
            var method = GetMethod<PropertyStoreGetValueDelegate>(self, 5);
            return method(self, ref key, out value);
        }

        private static int CallSetDefaultEndpoint(IntPtr self, string deviceId, ERole role)
        {
            var method = GetMethod<SetDefaultEndpointDelegate>(self, 13);
            return method(self, deviceId, role);
        }

        private static T GetMethod<T>(IntPtr self, int slot)
        {
            IntPtr vtable = Marshal.ReadIntPtr(self);
            IntPtr address = Marshal.ReadIntPtr(vtable, IntPtr.Size * slot);
            return (T)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(T));
        }

        private static void Check(int hresult)
        {
            if (hresult < 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }
        }

        private static void Release(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                Marshal.Release(pointer);
            }
        }

        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter, int dwClsContext, ref Guid riid, out IntPtr ppv);

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr pv);

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PROPVARIANT pvar);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int EnumAudioEndpointsDelegate(IntPtr self, EDataFlow dataFlow, int stateMask, out IntPtr collection);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetDefaultAudioEndpointDelegate(IntPtr self, EDataFlow dataFlow, ERole role, out IntPtr device);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CollectionGetCountDelegate(IntPtr self, out uint count);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CollectionItemDelegate(IntPtr self, uint index, out IntPtr device);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int DeviceOpenPropertyStoreDelegate(IntPtr self, int access, out IntPtr store);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int DeviceGetIdDelegate(IntPtr self, out IntPtr id);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int PropertyStoreGetValueDelegate(IntPtr self, ref PROPERTYKEY key, out PROPVARIANT value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int SetDefaultEndpointDelegate(IntPtr self, string deviceId, ERole role);
    }

    internal enum EDataFlow
    {
        eRender = 0,
        eCapture = 1,
        eAll = 2
    }

    internal enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPVARIANT
    {
        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr p;
        public int p2;
    }
}
