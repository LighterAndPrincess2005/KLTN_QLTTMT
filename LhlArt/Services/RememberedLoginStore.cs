using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace LhlArt.Services;

internal sealed record RememberedLogin(string Username,string Password);

// Generic credentials are protected by Windows for the signed-in Windows user.
// No password or access token is written to the repository/configuration files.
internal sealed class RememberedLoginStore(string baseUrl)
{
    readonly string target="LhlArt/Login/"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(baseUrl.TrimEnd('/'))));
    const int NotFound=1168;
    public RememberedLogin? Read()
    {
        if(!CredRead(target,1,0,out var pointer)){
            var error=Marshal.GetLastWin32Error();if(error==NotFound)return null;throw new Win32Exception(error);
        }
        try{
            var record=Marshal.PtrToStructure<Credential>(pointer);
            if(record.BlobSize==0||record.BlobSize>512||record.BlobSize%2!=0||string.IsNullOrWhiteSpace(record.UserName))return null;
            var bytes=new byte[record.BlobSize];
            try{Marshal.Copy(record.Blob,bytes,0,bytes.Length);return new(record.UserName,Encoding.Unicode.GetString(bytes));}
            finally{CryptographicOperations.ZeroMemory(bytes);}
        }
        finally{CredFree(pointer);}
    }
    public void Save(string username,string password)
    {
        var bytes=Encoding.Unicode.GetBytes(password);
        if(bytes.Length>512)throw new InvalidOperationException("Thông tin đăng nhập quá dài để lưu trên Windows.");
        var pointer=Marshal.AllocHGlobal(bytes.Length);
        try{
            Marshal.Copy(bytes,0,pointer,bytes.Length);
            var record=new Credential{Type=1,TargetName=target,UserName=username,Blob=pointer,BlobSize=(uint)bytes.Length,Persist=2};
            if(!CredWrite(ref record,0))throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        finally{
            CryptographicOperations.ZeroMemory(bytes);Marshal.Copy(bytes,0,pointer,bytes.Length);Marshal.FreeHGlobal(pointer);
        }
    }
    public void Clear()
    {
        if(!CredDelete(target,1,0)&&Marshal.GetLastWin32Error()!=NotFound)throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
    struct Credential
    {
        public uint Flags,Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint BlobSize;
        public IntPtr Blob;
        public uint Persist,AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string? UserName;
    }
    [DllImport("advapi32.dll",EntryPoint="CredWriteW",CharSet=CharSet.Unicode,SetLastError=true)]
    [return:MarshalAs(UnmanagedType.Bool)]static extern bool CredWrite(ref Credential credential,uint flags);
    [DllImport("advapi32.dll",EntryPoint="CredReadW",CharSet=CharSet.Unicode,SetLastError=true)]
    [return:MarshalAs(UnmanagedType.Bool)]static extern bool CredRead(string target,uint type,uint flags,out IntPtr credential);
    [DllImport("advapi32.dll",EntryPoint="CredDeleteW",CharSet=CharSet.Unicode,SetLastError=true)]
    [return:MarshalAs(UnmanagedType.Bool)]static extern bool CredDelete(string target,uint type,uint flags);
    [DllImport("advapi32.dll")]static extern void CredFree(IntPtr credential);
}
