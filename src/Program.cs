// SPDX-License-Identifier: GPL-3.0-only
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.Reflection;
[assembly: AssemblyVersion("0.2.1.0")]
[assembly: AssemblyTitle("Crossfire nForcer")]

internal static class Native {
    [DllImport("user32.dll")] internal static extern int GetSystemMetrics(int index);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern bool IsWow64Process(IntPtr process,out bool wow64);
    [DllImport("kernel32.dll")] internal static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern bool FlushFileBuffers(IntPtr handle);
    [DllImport("sfc.dll",CharSet=CharSet.Unicode)] internal static extern bool SfcIsFileProtected(IntPtr unused,string path);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)] internal static extern IntPtr OpenSCManager(string machine,string database,uint access);
    [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)] internal static extern IntPtr OpenService(IntPtr manager,string name,uint access);
    [DllImport("advapi32.dll",SetLastError=true)] internal static extern bool QueryServiceStatus(IntPtr service,out ServiceStatus status);
    [DllImport("advapi32.dll")] internal static extern bool CloseServiceHandle(IntPtr handle);
    [StructLayout(LayoutKind.Sequential)] internal struct ServiceStatus { internal uint Type,State,Accepted,Win32Exit,ServiceExit,Checkpoint,WaitHint; }
}
internal static class Guard {
    internal static void Require(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
    internal static string Target() { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),@"drivers\ati2mtag.sys"); }
    internal static void ValidateEnvironment(int major,int minor,bool is32,bool wow64,int boot,bool admin) {
        Require(major==5 && minor==1 && is32 && !wow64,"In-place changes require Windows XP 32-bit. Other OS versions are refused.");
        Require(boot==1 || boot==2,"Boot Windows into Safe Mode before applying or restoring.");
        Require(admin,"Administrator rights are required.");
    }
    internal static void Installed(string target) {
        OperatingSystem os=Environment.OSVersion;
        Require(os.Platform==PlatformID.Win32NT,"Windows is required.");
        // Check OS first, so an unsupported host never reaches system-file writes.
        Require(os.Version.Major==5 && os.Version.Minor==1,"In-place changes require Windows XP 32-bit.");
        bool wow64;
        Require(Native.IsWow64Process(Native.GetCurrentProcess(),out wow64),"Cannot determine OS architecture.");
        WindowsPrincipal user=new WindowsPrincipal(WindowsIdentity.GetCurrent());
        ValidateEnvironment(os.Version.Major,os.Version.Minor,IntPtr.Size==4,wow64,Native.GetSystemMetrics(67),user.IsInRole(WindowsBuiltInRole.Administrator));
        Require(File.Exists(target),"Installed ati2mtag.sys was not found: "+target);
        using(RegistryKey key=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\ati2mtag")) {
            Require(key!=null,"ATI miniport service registration was not found.");
            string image=key.GetValue("ImagePath") as string;
            Require(image!=null,"ATI service ImagePath is missing.");
            image=Environment.ExpandEnvironmentVariables(image.Trim().Trim('"'));
            string windows=Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName;
            if(image.StartsWith(@"\SystemRoot\",StringComparison.OrdinalIgnoreCase)) image=Path.Combine(windows,image.Substring(12));
            else if(image.StartsWith(@"System32\",StringComparison.OrdinalIgnoreCase)) image=Path.Combine(windows,image);
            if(image.StartsWith(@"\??\")) image=image.Substring(4);
            Require(string.Equals(Path.GetFullPath(image),Path.GetFullPath(target),StringComparison.OrdinalIgnoreCase),"ATI service points to a different driver path; refusing replacement.");
        }
        IntPtr manager=Native.OpenSCManager(null,null,1);
        Require(manager!=IntPtr.Zero,"Cannot query service manager.");
        try {
            IntPtr service=Native.OpenService(manager,"ati2mtag",4);
            Require(service!=IntPtr.Zero,"Cannot query ATI service.");
            try { Native.ServiceStatus status; Require(Native.QueryServiceStatus(service,out status),"Cannot read ATI service status."); Require(status.State==1,"ATI miniport is not stopped; refusing replacement."); }
            finally { Native.CloseServiceHandle(service); }
        } finally { Native.CloseServiceHandle(manager); }
        Require(!Native.SfcIsFileProtected(IntPtr.Zero,target),"Windows File Protection protects this driver; no change made. Protection will not be disabled.");
        Require(new DriveInfo(Path.GetPathRoot(target)).DriveFormat=="NTFS","In-place replacement requires an NTFS system volume.");
    }
}
internal static class Files {
    internal static void NewFile(string path,byte[] bytes) {
        using(FileStream stream=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None)) {
            stream.Write(bytes,0,bytes.Length); stream.Flush();
            Guard.Require(Native.FlushFileBuffers(stream.SafeFileHandle.DangerousGetHandle()),"Could not flush file: "+path);
        }
        Guard.Require(Engine.Hash(File.ReadAllBytes(path))==Engine.Hash(bytes),"Read-back verification failed: "+path);
    }
    static void NoReparse(string path) {
        for(string item=Path.GetFullPath(path);item!=null;item=Path.GetDirectoryName(item)) {
            if(File.Exists(item)||Directory.Exists(item)) Guard.Require((File.GetAttributes(item)&FileAttributes.ReparsePoint)==0,"Reparse path refused: "+item);
        }
    }
    // Internal for the transaction test harness. Production only calls this after Guard.Installed.
    internal static void Install(string target,bool restore) {
        NoReparse(target);
        string backup=target+".cfpatch.original";
        NoReparse(backup);
        byte[] before=File.ReadAllBytes(target),after;
        PatchResult patch;
        if(restore) {
            Guard.Require(File.Exists(backup),"Original backup is missing: "+backup);
            after=File.ReadAllBytes(backup); patch=Engine.Patch(after);
            if(Engine.Hash(before)==patch.Before) { Console.WriteLine("Original is already installed."); return; }
            Guard.Require(Engine.Hash(before)==patch.After,"Installed file does not match this backup's patch. Refusing to overwrite a different driver.");
        } else {
            patch=Engine.Patch(before); after=patch.Bytes;
            if(File.Exists(backup)) Guard.Require(Engine.Hash(File.ReadAllBytes(backup))==patch.Before,"A backup of a different driver already exists. Preserve it elsewhere before proceeding.");
            else NewFile(backup,before);
        }
        Console.WriteLine(patch.Describe());
        Console.WriteLine("Original backup: "+backup);
        string id=Guid.NewGuid().ToString("N"),stage=target+".cfpatch-stage-"+id;
        string previous=target+".cfpatch-previous-"+id, log=target+".cfpatch-"+id+".log";
        NewFile(stage,after);
        NewFile(log,Encoding.UTF8.GetBytes("Crossfire nForcer 0.2.1\r\nUTC: "+DateTime.UtcNow.ToString("s")+"\r\nAction: "+(restore?"restore":"apply")+"\r\nTarget: "+target+"\r\nBackup: "+backup+"\r\nPrevious: "+previous+"\r\n"+patch.Describe()+"\r\nState: prepared\r\n"));
        Guard.Require(Engine.Hash(File.ReadAllBytes(target))==Engine.Hash(before),"Driver changed during preparation; no replacement attempted.");
        try {
            File.Replace(stage,target,previous);
            Guard.Require(Engine.Hash(File.ReadAllBytes(target))==Engine.Hash(after),"Installed-file verification failed. Original backup: "+backup);
            Guard.Require(Engine.Hash(File.ReadAllBytes(previous))==Engine.Hash(before),"Previous-file verification failed. Original backup: "+backup);
            File.AppendAllText(log,"State: verified\r\n",Encoding.UTF8);
        } catch(Exception ex) {
            throw new IOException("Replacement did not complete cleanly. Inspect target and backup before rebooting. Original: "+backup+"; previous: "+previous+"; stage: "+stage+"; log: "+log,ex);
        }
        Console.WriteLine(restore?"Original restored and verified.":"Chipset bypass installed and verified.");
        Console.WriteLine("Log: "+log+"\r\nReboot normally when ready. No reboot was initiated.");
    }
}
internal static class Program {
    internal static int Main(string[] args) {
        if(args.Length==0 || (args.Length==1 && string.Equals(args[0],"/menu",StringComparison.OrdinalIgnoreCase))) return Menu();
        return RunCommand(args);
    }
    static int Menu() {
        for(;;) {
            Console.WriteLine("\r\nCrossfire nForcer 0.2.1\r\n----------------------");
            Console.WriteLine("Target: "+Guard.Target());
            Console.WriteLine("\r\n  1  Check installed driver\r\n  2  Apply chipset bypass\r\n  3  Restore original driver\r\n  4  Information\r\n  0  Exit");
            Console.Write("\r\nSelect [0-4], then press Enter: ");
            string choice=Console.ReadLine();
            if(choice==null || choice.Trim()=="0") return 0;
            choice=choice.Trim();
            string command=null;
            if(choice=="1") command="/check";
            else if(choice=="2") command="/apply";
            else if(choice=="3") command="/restore";
            else if(choice=="4") {
                Console.WriteLine("\r\nWindows XP 32-bit SP2/SP3; .NET Framework 2.0.\r\nApply and restore require Safe Mode and administrator rights.\r\nThe patch bypasses the chipset-vendor gate in supported ati2mtag.sys builds.\r\nBackup: "+Guard.Target()+".cfpatch.original\r\nPatch rules: Python patcher 0.3.2. License: GPL-3.0-only.");
            } else { Console.WriteLine("Invalid selection. Enter 0, 1, 2, 3 or 4."); continue; }
            if(command!=null) {
                Console.WriteLine();
                int result=RunCommand(new string[] {command});
                Console.WriteLine(result==0?"\r\nOperation completed.":"\r\nOperation failed. See error above.");
            }
            Console.Write("\r\nPress Enter to return to the menu.");
            if(Console.ReadLine()==null) return 0;
        }
    }
    static int RunCommand(string[] args) {
        try {
            string command=args[0].ToLowerInvariant();
            if(command=="/?" || command=="/help") { Console.WriteLine("Crossfire nForcer 0.2.1 (.NET 2.0, x86)\r\nNo arguments or /menu: open console menu\r\n/apply: patch installed driver in XP Safe Mode\r\n/restore: restore original driver in XP Safe Mode\r\n/check [file]: analyze without writing\r\n/patch-file input output: create a new patched copy\r\n/version: version information"); return 0; }
            if(command=="/version" && args.Length==1) { Console.WriteLine("Crossfire nForcer 0.2.1; patch rules from Python patcher 0.3.2"); return 0; }
            if(command=="/check" && args.Length<=2) { Console.WriteLine(Engine.Patch(File.ReadAllBytes(args.Length==2?args[1]:Guard.Target())).Describe()); Console.WriteLine("Check only. No files changed."); return 0; }
            if(command=="/patch-file" && args.Length==3) {
                string input=Path.GetFullPath(args[1]),output=Path.GetFullPath(args[2]);
                Guard.Require(!string.Equals(input,output,StringComparison.OrdinalIgnoreCase),"Output cannot be input.");
                PatchResult result=Engine.Patch(File.ReadAllBytes(input)); Files.NewFile(output,result.Bytes);
                Console.WriteLine(result.Describe()); Console.WriteLine("Created: "+output); return 0;
            }
            Guard.Require((command=="/apply" || command=="/restore") && args.Length<=1,"Unknown arguments. Use /help.");
            using(Mutex mutex=new Mutex(false,@"Global\CrossFireXpConsole")) {
                bool held=false;
                try { try { held=mutex.WaitOne(0,false); } catch(AbandonedMutexException) { held=true; }
                    Guard.Require(held,"Another patch operation is running.");
                    string target=Guard.Target(); Guard.Installed(target); Files.Install(target,command=="/restore");
                } finally { if(held) mutex.ReleaseMutex(); }
            }
            return 0;
        } catch(Exception ex) { Console.Error.WriteLine("ERROR: "+ex.Message); if(ex.InnerException!=null) Console.Error.WriteLine(ex.InnerException.Message); return 1; }
    }
}
