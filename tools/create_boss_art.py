from pathlib import Path
from math import cos, sin, pi
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[1] / 'projects/learning/NeonArenaRebuild/Assets/Resources/Polish'
for name, sides, color in [('Warden', 6, '#ffbe4a'), ('Overlord', 8, '#ff5479')]:
    image = Image.new('RGBA', (512, 512))
    draw = ImageDraw.Draw(image)
    points = lambda radius, count, offset=0: [(256+cos(i*2*pi/count+offset)*radius, 256+sin(i*2*pi/count+offset)*radius) for i in range(count)]
    draw.polygon(points(224, sides, pi/8), fill='#172735', outline=color, width=14)
    draw.polygon(points(175, sides, pi/8), fill='#293c4b', outline='#fff0cc', width=5)
    for x,y in points(194, sides, pi/8):
        draw.ellipse((x-16,y-16,x+16,y+16), fill=color)
    draw.polygon([(176,198),(216,226),(204,259),(168,237)], fill=color)
    draw.polygon([(336,198),(296,226),(308,259),(344,237)], fill=color)
    draw.polygon([(206,292),(256,317),(306,292),(284,351),(228,351)], fill=color)
    if name == 'Overlord':
        draw.polygon([(178,122),(210,170),(256,103),(302,170),(334,122),(326,190),(187,190)], fill=color)
    image.resize((128,128), Image.Resampling.LANCZOS).save(root / (name+'.png'))
