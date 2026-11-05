"""Check the shipped icon silhouettes at every Windows icon size."""
from io import BytesIO
from pathlib import Path
import struct
from PIL import Image

root=Path(__file__).resolve().parents[1]
data=(root/'assets/Any2Odd.ico').read_bytes()
reserved,kind,count=struct.unpack_from('<HHH',data)
assert (reserved,kind,count)==(0,1,7)
for i,expected in enumerate((16,24,32,48,64,128,256)):
    width,height,_,_,planes,bits,size,offset=struct.unpack_from('<BBBBHHII',data,6+i*16)
    assert (width or 256)==(height or 256)==expected and planes==1 and bits==32
    image=Image.open(BytesIO(data[offset:offset+size])).convert('RGBA')
    assert image.size==(expected,expected)
    alpha=image.getchannel('A')
    x0,y0,x1,y1=alpha.getbbox()
    assert abs(x0-(expected-x1))<=1 and abs(y0-(expected-y1))<=1, 'Off-center silhouette'
    for y in range(expected):
        for x in range(expected):
            if (x+.5-expected/2)**2+(y+.5-expected/2)**2>(expected*.5)**2:
                assert alpha.getpixel((x,y))==0, f'Artwork escapes the circle: {expected}, {x}, {y}'
    if expected==256:
        assert image.tobytes()==Image.open(root/'assets/Any2Odd.png').convert('RGBA').tobytes()
    print('PASS: centered circular transparency, ICO image',expected)
assert Image.open(root/'assets/Any2Odd-wordmark.png').size==(1400,250)
