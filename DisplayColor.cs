using System;
using System.Runtime.InteropServices;
namespace OdysseyCompanion {
 static class DisplayColor {
  [DllImport("user32")]static extern int GetDisplayConfigBufferSizes(uint flags,out uint paths,out uint modes);
  [DllImport("user32")]static extern int QueryDisplayConfig(uint flags,ref uint paths,IntPtr pathData,ref uint modes,IntPtr modeData,IntPtr topology);
  [DllImport("user32")]static extern int DisplayConfigGetDeviceInfo(IntPtr packet);
  static IntPtr Packet(int type,int size,IntPtr path,int luidOffset,int idOffset){var p=Marshal.AllocHGlobal(size);Marshal.Copy(new byte[size],0,p,size);Marshal.WriteInt32(p,0,type);Marshal.WriteInt32(p,4,size);Marshal.WriteInt64(p,8,Marshal.ReadInt64(path,luidOffset));Marshal.WriteInt32(p,16,Marshal.ReadInt32(path,idOffset));return p;}
  public static float SdrWhiteScale(string monitor){uint paths,modes;if(GetDisplayConfigBufferSizes(2,out paths,out modes)!=0)return 1;
   IntPtr pathData=Marshal.AllocHGlobal((int)paths*72),modeData=Marshal.AllocHGlobal((int)modes*64);
   try{if(QueryDisplayConfig(2,ref paths,pathData,ref modes,modeData,IntPtr.Zero)!=0)return 1;
    for(int i=0;i<paths;i++){var path=IntPtr.Add(pathData,i*72);var name=Packet(1,84,path,0,8);bool matches;
     try{matches=DisplayConfigGetDeviceInfo(name)==0&&Marshal.PtrToStringUni(IntPtr.Add(name,20))==monitor;}finally{Marshal.FreeHGlobal(name);}if(!matches)continue;
     var info=Packet(9,32,path,20,28);bool advanced;
     try{advanced=DisplayConfigGetDeviceInfo(info)==0&&(Marshal.ReadInt32(info,20)&2)!=0;}finally{Marshal.FreeHGlobal(info);}if(!advanced)return 1;
     var white=Packet(11,24,path,20,28);try{if(DisplayConfigGetDeviceInfo(white)==0){float value=Marshal.ReadInt32(white,20)/1000.0f;Files.Log("Display color: advanced color active; SDR white scale "+value);return Math.Max(1,Math.Min(12.5f,value));}}finally{Marshal.FreeHGlobal(white);}
    }return 1;
   }finally{Marshal.FreeHGlobal(pathData);Marshal.FreeHGlobal(modeData);}
  }
 }
}
