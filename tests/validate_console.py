# SPDX-License-Identifier: GPL-3.0-only
"""Developer tests: private fixtures and previous Python source are external inputs."""
import argparse
import ctypes
import hashlib
import json
from pathlib import Path
import struct
import subprocess
import tempfile

parser=argparse.ArgumentParser()
parser.add_argument('--metadata',type=Path,required=True)
parser.add_argument('--fixtures',type=Path,required=True)
parser.add_argument('--python-tests',type=Path,required=True)
parser.add_argument('--transactions',type=Path,required=True)
args=parser.parse_args()
root=Path(__file__).absolute().parents[1]
exe=root/'Crossfire-nForcer.exe'
import sys
sys.path.insert(0,str(args.python_tests))
from test_crossfire_patch import FUNCTIONS,synthetic_pe,patcher
def run(*argv):
    return subprocess.run([str(exe),*map(str,argv)],capture_output=True,text=True,timeout=45)
def good(result):
    assert result.returncode==0,(result.stdout,result.stderr)
def bad(result):
    assert result.returncode!=0,result.stdout
good(run('/version')); good(run('/help'))
def menu(keys, *switches):
    return subprocess.run([str(exe),*switches],input=keys,capture_output=True,text=True,timeout=10)
for keys in ('0\n','', '\ninvalid\n0\n'):
    result=menu(keys); good(result)
    assert 'Select [0-4]' in result.stdout
    assert 'installed and verified' not in result.stdout
    assert 'ERROR:' not in result.stderr
result=menu('4\n\n0\n','/menu'); good(result)
assert 'License: GPL-3.0-only' in result.stdout
assert result.stdout.count('Select [0-4]')==2
# Never permit this test to install on the host. Execute /apply only on a non-XP host.
if sys.getwindowsversion().major!=5:
    bad(run('/apply')); bad(run('/restore'))
    for choice in ('2','3'):
        result=menu(choice+'\n\n0\n'); good(result)
        assert 'ERROR:' in result.stderr and 'Operation failed.' in result.stdout
        assert result.stdout.count('Select [0-4]')==2
results=[]
checksum=ctypes.WinDLL('imagehlp').MapFileAndCheckSumW
checksum.argtypes=[ctypes.c_wchar_p,ctypes.POINTER(ctypes.c_uint32),ctypes.POINTER(ctypes.c_uint32)]
checksum.restype=ctypes.c_uint32
with tempfile.TemporaryDirectory(prefix='cf-console-') as temp:
    folder=Path(temp)
    for n,row in enumerate(json.loads(args.metadata.read_text())):
        source=args.fixtures/row['relative_path']
        before=source.read_bytes()
        assert hashlib.sha256(before).hexdigest()==row['sha256']
        output=folder/(str(n)+'.sys')
        result=run('/patch-file',source,output)
        if row['result']=='validated':
            good(result)
            checked=run('/check',source); good(checked)
            for display in (result.stdout,checked.stdout):
                assert 'Supported function layout found.' in display
                assert 'Layout:' not in display and 'Gate file offset:' not in display
                assert not any(layout in display for layout in FUNCTIONS)
            assert hashlib.sha256(output.read_bytes()).hexdigest()==row['patch']['output_sha256']
            header,computed=ctypes.c_uint32(),ctypes.c_uint32()
            assert checksum(str(output),ctypes.byref(header),ctypes.byref(computed))==0
            assert header.value==computed.value
            bad(run('/patch-file',source,output))
            bad(run('/patch-file',output,folder/'repeat.sys'))
        else:
            bad(result); assert not output.exists()
        assert before==source.read_bytes()
        results.append(dict(archive=row['archive'],machine=row['machine'],module=row['module'],result=row['result']))
    for layout in FUNCTIONS:
        data=synthetic_pe(layout)
        source=folder/'synthetic.sys'; source.write_bytes(data)
        output=folder/(layout+'.sys')
        good(run('/patch-file',source,output))
        assert output.read_bytes()==patcher.make_patch(data)[0]
        mutated=bytearray(data); mutated[0x308]^=1; source.write_bytes(mutated)
        bad(run('/check',source))
        source.write_bytes(data)
        for pos,new in patcher.replacements(layout,'legacy'):
            mutated=bytearray(data); mutated[0x300+pos:0x300+pos+len(new)]=new
            source.write_bytes(mutated); bad(run('/check',source))
    source=folder/'transaction-source.sys'; source.write_bytes(synthetic_pe('cat77-v78-inline'))
    result=subprocess.run([str(args.transactions),str(source),str(folder/'transaction')],capture_output=True,text=True,timeout=30)
    good(result)
    print(result.stdout.splitlines()[-1])
# Check the executable's PE and CLR metadata without claiming an XP hardware run.
data=exe.read_bytes(); pe=struct.unpack_from('<I',data,0x3c)[0]; opt=pe+24
assert struct.unpack_from('<H',data,pe+4)[0]==0x14c
assert struct.unpack_from('<H',data,opt+68)[0]==3 # console subsystem
assert b'v2.0.50727' in data
assert b'v4.0.30319' not in data
print('PASS:',len(results),'archive module entries;',len(FUNCTIONS),'synthetic layouts; output parity, checksums, repeat/unknown rejection, transactions and guards.')
(root/'tests/validation-results.json').write_text(json.dumps(dict(archive_entries=results,synthetic_layouts=list(FUNCTIONS),menu_tests='passed',xp_hardware_tested=False,exe_sha256=hashlib.sha256(data).hexdigest()),indent=2)+'\n')
