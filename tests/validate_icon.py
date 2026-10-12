# SPDX-License-Identifier: GPL-3.0-only
"""Verify XP-style ICO bitmaps and their embedding in the EXE resources."""
from pathlib import Path
import struct
root=Path(__file__).absolute().parents[1]
ico=(root/'assets/nforcer.ico').read_bytes()
assert struct.unpack_from('<HHH',ico)==(0,1,4)
icons=[]
for n,size in enumerate((16,32,48,64)):
    w,h,colors,reserved,planes,bpp,length,offset=struct.unpack_from('<BBBBHHII',ico,6+16*n)
    assert (w,h,planes,bpp)==(size,size,1,32)
    blob=ico[offset:offset+length]
    assert len(blob)==length
    assert struct.unpack_from('<IiiHHI',blob)==(40,size,size*2,1,32,0)
    assert length==40+size*size*4+((size+31)//32)*4*size
    icons.append(blob)
data=(root/'Crossfire-nForcer.exe').read_bytes()
pe=struct.unpack_from('<I',data,60)[0]; opt=pe+24
count=struct.unpack_from('<H',data,pe+6)[0]; osize=struct.unpack_from('<H',data,pe+20)[0]
sections=[struct.unpack_from('<IIII',data,opt+osize+40*n+8) for n in range(count)]
def off(rva):
    for vsize,va,size,raw in sections:
        if va<=rva<va+size:return raw+rva-va
    raise AssertionError('Unmapped RVA')
base=off(struct.unpack_from('<I',data,opt+96+2*8)[0])
resources=[]
def walk(relative,keys):
    p=base+relative
    named,ids=struct.unpack_from('<HH',data,p+12)
    for n in range(named+ids):
        key,target=struct.unpack_from('<II',data,p+16+n*8)
        if target&0x80000000: walk(target&0x7fffffff,keys+[key])
        else:
            rva,size=struct.unpack_from('<II',data,base+target)
            resources.append((keys+[key],data[off(rva):off(rva)+size]))
walk(0,[])
embedded=[blob for keys,blob in resources if keys[0]==3]
assert sorted(embedded)==sorted(icons)
assert any(keys[0]==14 for keys,blob in resources)
print('PASS: 16/32/48/64 px, 32-bit uncompressed ICO entries; all four embedded in the EXE.')
