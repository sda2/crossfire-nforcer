// SPDX-License-Identifier: GPL-3.0-only
using System;
using System.IO;
internal static class TransactionTests {
    delegate void TestAction();
    static void ExpectFailure(TestAction action) { bool failed=false; try {action();} catch(Exception) {failed=true;} if(!failed) throw new Exception("Expected rejection."); }
    static void Same(string path,byte[] bytes) { if(Engine.Hash(File.ReadAllBytes(path))!=Engine.Hash(bytes)) throw new Exception("Unexpected file change: "+path); }
    static int Main(string[] args) {
        string dir=Path.GetFullPath(args[1]); Directory.CreateDirectory(dir);
        byte[] original=File.ReadAllBytes(args[0]);
        string target=Path.Combine(dir,"ati2mtag.sys"); File.WriteAllBytes(target,original);
        Guard.ValidateEnvironment(5,1,true,false,1,true);
        Guard.ValidateEnvironment(5,1,true,false,2,true);
        ExpectFailure(delegate {Guard.ValidateEnvironment(10,0,true,false,1,true);});
        ExpectFailure(delegate {Guard.ValidateEnvironment(5,1,true,false,0,true);});
        ExpectFailure(delegate {Guard.ValidateEnvironment(5,1,true,true,1,true);});
        ExpectFailure(delegate {Guard.ValidateEnvironment(5,1,false,false,1,true);});
        ExpectFailure(delegate {Guard.ValidateEnvironment(5,1,true,false,1,false);});
        Files.Install(target,false); Same(target,Engine.Patch(original).Bytes); Same(target+".cfpatch.original",original);
        ExpectFailure(delegate {Files.Install(target,false);});
        Files.Install(target,true); Same(target,original);
        Files.Install(target,true); Same(target,original);
        // A replaced/updated installed driver must not be overwritten by restore.
        byte[] different=(byte[])original.Clone(); different[0]^=1; File.WriteAllBytes(target,different);
        ExpectFailure(delegate {Files.Install(target,true);}); Same(target,different);
        File.WriteAllBytes(target,original);
        File.WriteAllBytes(target+".cfpatch.original",different);
        ExpectFailure(delegate {Files.Install(target,false);}); Same(target,original);
        File.WriteAllBytes(target+".cfpatch.original",original);
        using(FileStream locked=new FileStream(target,FileMode.Open,FileAccess.Read,FileShare.Read)) {
            ExpectFailure(delegate {Files.Install(target,false);}); Same(target,original);
        }
        Same(target+".cfpatch.original",original);
        Console.WriteLine("Transaction and environment-guard tests passed."); return 0;
    }
}
