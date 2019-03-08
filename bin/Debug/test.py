# uncompyle6 version 3.2.5
# Python bytecode 3.6 (3379)
# Decompiled from: Python 3.6.8 (tags/v3.6.8:3c6b436a57, Dec 24 2018, 00:16:47) [MSC v.1916 64 bit (AMD64)]
# Embedded file name: e:\nk\btd5\monkey_wrench\mask.py
import os.path, itertools
from PIL import Image as PImage
_endian = 'little'
norm2 = lambda t1, t2: sum(((a - b) ** 2 for a, b in zip(t1, t2)))

def make(fnamein, fnameout, smooth=False):
    """Create a mask, optionally applying a smoothing algorithm (for eg generating a mask from scratch)"""
    colourlist = [
     (0, 255, 0),
     (128, 128, 128),
     (0, 128, 0),
     (0, 0, 0),
     (0, 255, 255),
     (0, 0, 255),
     (0, 128, 128),
     (0, 0, 128)]
    palette = list(itertools.chain(*colourlist, *(     [0, 0, 0] * (256 - len(colourlist)),)))

    def write_run(file, rl, data):
        while rl > 0:
            file.write(min(rl, 255).to_bytes(1, _endian))
            file.write(data)
            rl -= 255
            print(rl)
            print(data)
            input()

    changed = False
    img = PImage.open(fnamein)
    width = img.width
    height = img.height
    offsets = (-1 - width, -width, 1 - width,
     -1, 1,
     -1 + width, width, 1 + width)
    if img.mode != 'P' or img.getpalette() != palette:
        img = img.convert('RGB')
        img2 = PImage.new('P', (width, height))
        img2.putpalette(palette)
        data = [0] * (width * height)
        for i, c0 in enumerate(img.getdata()):
            ci, c = min(enumerate(colourlist), key=lambda enum: norm2(c0, enum[1]))
            if c != c0:
                changed = True
            data[i] = ci

        img2.putdata(data)
        img = img2
    if changed:
        if smooth:
            loopno = 0
            while changed:
                if loopno < 20:
                    changed = False
                    img2 = img.copy()
                    data = img.getdata()
                    data_new = list(img.getdata())
                    l = len(data)
                    for i, c0 in enumerate(data):
                        counts = [set() for j in range(len(offsets) + 1)]
                        same = 0
                        diff = 0
                        for offset in offsets:
                            j = i + offset
                            if not j < 0:
                                if j >= l:
                                    continue
                                c = data[j]
                                counts[0].add(c)
                                count = 1
                                while c in counts[count]:
                                    count += 1

                                counts[count].add(c)
                                if c == c0:
                                    same = count
                                else:
                                    diff = max(diff, count)

                        if same > 2:
                            continue
                        if diff < 4:
                            continue
                        changed = True
                        if len(counts[diff]) == 1:
                            data_new[i] = counts[diff].pop()
                        else:
                            if len(counts[diff]) == 2:
                                c1 = counts[diff].pop()
                                c2 = counts[diff].pop()
                                for flag in (1, 2, 4):
                                    if (c1 ^ c2) & flag:
                                        if c1 & flag:
                                            data_new[i] = c2
                                        else:
                                            data_new[i] = c1
                                        break

                    img2.putdata(data_new)
                    img = img2
                    loopno += 1

    lastpos = 0
    lastc = -1
    colours = [-1, -1]
    with open(fnameout, 'wb') as (file):
        file.write(img.width.to_bytes(4, _endian))
        file.write(img.height.to_bytes(4, _endian))
        print(len(img.getdata()))
        for pos, c in enumerate(img.getdata()):
            
            
            if c > 0:
                c |= 8
            rl2 = pos - lastpos
            if rl2 < 2:
                colours[rl2] = c
            else:
                if c != colours[rl2 % 2]:
                    write_run(file, rl2 // 2, (colours[0] | colours[1] << 4).to_bytes(1, _endian))
                    lastpos += 2 * (rl2 // 2)
                    if rl2 % 2 == 0:
                        colours[0] = c
                    else:
                        colours[0] = lastc
                        colours[1] = c
            lastc = c

        rl2 = width * height - lastpos
        write_run(file, rl2 // 2, (colours[0] | colours[1] << 4).to_bytes(1, _endian))
# okay decompiling C:\Users\JamByte\Downloads\New folder\monkey_wrench.exe_extracted\out00-PYZ.pyz_extracted\mask.pyc
make("airfield_left.png", "python test")
print(5//2)
