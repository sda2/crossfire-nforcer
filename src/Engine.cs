// SPDX-License-Identifier: GPL-3.0-only
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

internal sealed class Edit {
    internal int Offset; internal byte[] Bytes;
    internal Edit(int offset, byte[] bytes) { Offset=offset; Bytes=bytes; }
}
internal sealed class Rule {
    internal string Name; internal int[] Pattern; internal Edit[] Edits; internal int Legacy;
    internal Rule(string name,string pattern,string edits,int legacy) {
        Name=name; Legacy=legacy;
        string[] tokens=pattern.Split(' '); Pattern=new int[tokens.Length];
        for(int i=0;i<tokens.Length;i++) Pattern[i]=tokens[i]=="??" ? -1 : Convert.ToInt32(tokens[i],16);
        List<Edit> changes=new List<Edit>();
        foreach(string edit in edits.Split(',')) {
            string[] pair=edit.Split(':'); byte[] bytes=new byte[pair[1].Length/2];
            for(int i=0;i<bytes.Length;i++) bytes[i]=Convert.ToByte(pair[1].Substring(i*2,2),16);
            changes.Add(new Edit(int.Parse(pair[0]),bytes));
        }
        Edits=changes.ToArray();
    }
}
internal sealed class Section {
    internal int Start,Length; internal uint Rva; internal bool Executable;
}
internal sealed class PatchResult {
    internal byte[] Bytes; internal string Layout,Before,After; internal int Offset;
    internal string Describe() { return "Supported function layout found.\r\nOriginal SHA256: "+Before+"\r\nPatched SHA256: "+After; }
}
internal static class Engine {
    internal static string Hash(byte[] bytes) {
        using(SHA256 sha=SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();
    }
    static void Need(bool condition,string text) { if(!condition) throw new InvalidOperationException(text); }
    static uint U32(byte[] b,int o) { Need(o>=0 && o<=b.Length-4,"Truncated PE."); return BitConverter.ToUInt32(b,o); }
    static int U16(byte[] b,int o) { Need(o>=0 && o<=b.Length-2,"Truncated PE."); return BitConverter.ToUInt16(b,o); }
    static bool Match(byte[] b,int off,int[] pattern) {
        for(int i=0;i<pattern.Length;i++) if(pattern[i]>=0 && b[off+i]!=pattern[i]) return false;
        return true;
    }
    internal static uint Checksum(byte[] b,int offset) {
        ulong sum=0;
        for(int i=0;i<b.Length;i+=2) {
            uint low=(i>=offset && i<offset+4)?0U:b[i];
            uint high=(i+1>=b.Length || (i+1>=offset && i+1<offset+4))?0U:b[i+1];
            sum+=low+(high<<8);
        }
        while((sum>>16)!=0) sum=(sum&65535)+(sum>>16);
        return (uint)(sum+(ulong)b.Length);
    }
    internal static PatchResult Patch(byte[] data) {
        Need(data.Length>=64 && data[0]==77 && data[1]==90,"Expected an uncompressed PE driver.");
        uint raw=U32(data,60); Need(raw<=(uint)Math.Max(0,data.Length-24),"Invalid PE offset.");
        int pe=(int)raw, opt=pe+24;
        Need(U32(data,pe)==0x4550 && U16(data,pe+4)==0x14c,"Only x86 PE drivers are supported.");
        int count=U16(data,pe+6),optSize=U16(data,pe+20);
        Need(count>=1 && count<=96 && optSize>=96 && (long)opt+optSize+40L*count<=data.Length,"Invalid PE headers.");
        Need(U16(data,opt)==0x10b && U16(data,opt+68)==1,"Expected a PE32 native driver.");
        uint imageBase=U32(data,opt+28); int checksum=opt+64;
        byte[] identity=Encoding.Unicode.GetBytes("ati2mtag.sys"); bool identityFound=false;
        for(int i=0;i<=data.Length-identity.Length && !identityFound;i++) {
            bool equal=true;
            for(int j=0;j<identity.Length;j++) { byte c=data[i+j]; if(c>=65 && c<=90) c+=32; if(c!=identity[j]) {equal=false;break;} }
            identityFound=equal;
        }
        Need(identityFound,"ati2mtag.sys identity not found.");
        List<Section> sections=new List<Section>();
        for(int n=0;n<count;n++) {
            int s=opt+optSize+40*n; uint size=U32(data,s+16),start=U32(data,s+20);
            Need((ulong)size+start<=(ulong)data.Length || size==0,"Truncated section.");
            if(size==0) continue;
            Section section=new Section(); section.Start=(int)start; section.Length=(int)size;
            section.Rva=U32(data,s+12); section.Executable=(U32(data,s+36)&0x20000000)!=0; sections.Add(section);
        }
        Rule hit=null; int hitOffset=0,hitState=0,hits=0; long hitRva=0;
        foreach(Rule rule in Rules.All) for(int state=0;state<3;state++) {
            int[] pattern=(int[])rule.Pattern.Clone();
            if(state==1) pattern[rule.Legacy]=0;
            if(state==2) foreach(Edit edit in rule.Edits) for(int j=0;j<edit.Bytes.Length;j++) pattern[edit.Offset+j]=edit.Bytes[j];
            foreach(Section s in sections) if(s.Executable) {
                for(int i=s.Start;i<=s.Start+s.Length-pattern.Length;i++) {
                    if(pattern[0]>=0 && data[i]!=pattern[0]) continue;
                    if(!Match(data,i,pattern)) continue;
                    hits++; hit=rule; hitOffset=i; hitState=state; hitRva=(long)s.Rva+i-s.Start;
                }
            }
        }
        Need(hits==1,"Expected exactly one reviewed chipset gate; found "+hits+".");
        Need(hitState==0,"Already patched. Restore the original before changing patches.");
        bool reference=false;
        if(hit.Name.EndsWith("-inline")) {
            foreach(Section s in sections) if(!s.Executable)
                for(int i=s.Start;i<=s.Start+s.Length-4;i+=4) if(U32(data,i)==(ulong)imageBase+(ulong)hitRva) reference=true;
        } else {
            foreach(Section s in sections) if(s.Executable)
                for(int i=s.Start;i<=s.Start+s.Length-8;i++) {
                    if(data[i]!=0xe8 || data[i+5]!=0x85 || data[i+6]!=0xc0) continue;
                    bool branch=data[i+7]==0x74 || (i+8<s.Start+s.Length && data[i+7]==0x0f && data[i+8]==0x84);
                    if(branch && (long)s.Rva+i-s.Start+5+BitConverter.ToInt32(data,i+1)==hitRva) reference=true;
                }
        }
        Need(reference,"Required caller or dispatch-table reference not found.");
        byte[] result=(byte[])data.Clone(); bool[] allowed=new bool[data.Length];
        foreach(Edit edit in hit.Edits) for(int j=0;j<edit.Bytes.Length;j++) { result[hitOffset+edit.Offset+j]=edit.Bytes[j]; allowed[hitOffset+edit.Offset+j]=true; }
        for(int i=0;i<4;i++) allowed[checksum+i]=true;
        Buffer.BlockCopy(BitConverter.GetBytes(Checksum(result,checksum)),0,result,checksum,4);
        for(int i=0;i<data.Length;i++) Need(data[i]==result[i] || allowed[i],"Unexpected output difference.");
        PatchResult output=new PatchResult(); output.Bytes=result; output.Layout=hit.Name; output.Offset=hitOffset;
        output.Before=Hash(data); output.After=Hash(result); return output;
    }
}
