from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageEnhance, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "CapCut_Template" / "media"
ASSETS = ROOT / "Assets"

W, H = 1920, 1080


def font(size, bold=False):
    candidates = [
        ASSETS / "Main Menu Asset" / "LilitaOne-Regular.ttf",
        Path("C:/Windows/Fonts/arialbd.ttf" if bold else "C:/Windows/Fonts/arial.ttf"),
    ]
    for path in candidates:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


TITLE = font(86, True)
SUBTITLE = font(44)
SMALL = font(34)
NAME = font(42, True)


def cover(path, dim=(W, H)):
    img = Image.open(path).convert("RGB")
    iw, ih = img.size
    scale = max(dim[0] / iw, dim[1] / ih)
    nw, nh = int(iw * scale), int(ih * scale)
    img = img.resize((nw, nh), Image.Resampling.LANCZOS)
    left = (nw - dim[0]) // 2
    top = (nh - dim[1]) // 2
    return img.crop((left, top, left + dim[0], top + dim[1]))


def contain(path, max_size):
    img = Image.open(path).convert("RGBA")
    img.thumbnail(max_size, Image.Resampling.LANCZOS)
    return img


def vignette(img, strength=150):
    overlay = Image.new("L", (W, H), 0)
    draw = ImageDraw.Draw(overlay)
    draw.ellipse((-350, -250, W + 350, H + 250), fill=255)
    overlay = overlay.filter(ImageFilter.GaussianBlur(190))
    dark = Image.new("RGB", (W, H), (0, 0, 0))
    mask = Image.eval(overlay, lambda p: int((255 - p) * strength / 255))
    return Image.composite(dark, img, mask)


def add_text(draw, xy, text, fnt, fill, anchor="mm", stroke=0):
    draw.text(xy, text, font=fnt, fill=fill, anchor=anchor, stroke_width=stroke, stroke_fill=(28, 23, 32))


def dialog_box(img, speaker, line):
    draw = ImageDraw.Draw(img, "RGBA")
    draw.rounded_rectangle((180, 795, 1740, 1010), radius=28, fill=(255, 247, 252, 235), outline=(255, 139, 179, 255), width=5)
    draw.rounded_rectangle((220, 735, 560, 810), radius=24, fill=(255, 126, 169, 245))
    add_text(draw, (250, 772), speaker, NAME, (255, 255, 255), anchor="lm", stroke=1)
    add_text(draw, (250, 880), line, SUBTITLE, (45, 39, 55), anchor="lm")
    return img


def paste_char(img, path, side="right", size=(720, 980), y=90):
    char = contain(path, size)
    x = 1120 if side == "right" else 170
    if side == "center":
        x = (W - char.width) // 2
    img.paste(char, (x, y), char)
    return img


def make_scene(filename, bg, title=None, subtitle=None, speaker=None, line=None, chars=None, mood="bright"):
    img = cover(bg)
    if mood == "bright":
        img = ImageEnhance.Color(img).enhance(1.16)
        img = ImageEnhance.Brightness(img).enhance(1.05)
    if mood == "dark":
        img = ImageEnhance.Color(img).enhance(0.55)
        img = ImageEnhance.Brightness(img).enhance(0.62)
    if mood == "glitch":
        img = ImageEnhance.Color(img).enhance(1.6)
        img = ImageEnhance.Contrast(img).enhance(1.45)
        r, g, b = img.split()
        r = r.transform(r.size, Image.Transform.AFFINE, (1, 0, -18, 0, 1, 0))
        b = b.transform(b.size, Image.Transform.AFFINE, (1, 0, 18, 0, 1, 0))
        img = Image.merge("RGB", (r, g, b))
    img = vignette(img, 100 if mood == "bright" else 190)
    img = img.convert("RGBA")
    if chars:
        for char_path, side, size in chars:
            paste_char(img, char_path, side, size)
    draw = ImageDraw.Draw(img, "RGBA")
    if title:
        add_text(draw, (W // 2, 430), title, TITLE, (255, 255, 255), stroke=3)
    if subtitle:
        add_text(draw, (W // 2, 535), subtitle, SUBTITLE, (255, 231, 240), stroke=2)
    if speaker and line:
        dialog_box(img, speaker, line)
    if mood == "glitch":
        for y in range(120, 940, 110):
            draw.rectangle((0, y, W, y + 12), fill=(255, 255, 255, 40))
        add_text(draw, (W // 2, 955), "ada yang tidak seharusnya terlihat", SMALL, (255, 65, 95), stroke=2)
    img.convert("RGB").save(OUT / filename, quality=95)


def make_text_card(filename, title, subtitle="", mood="pink"):
    palettes = {
        "pink": ((255, 177, 207), (255, 235, 244), (60, 42, 62)),
        "black": ((12, 10, 14), (72, 10, 30), (255, 238, 245)),
        "red": ((42, 4, 12), (175, 18, 48), (255, 240, 244)),
    }
    a, b, text = palettes[mood]
    img = Image.new("RGB", (W, H), a)
    draw = ImageDraw.Draw(img, "RGBA")
    for i in range(H):
        t = i / H
        col = tuple(int(a[c] * (1 - t) + b[c] * t) for c in range(3))
        draw.line((0, i, W, i), fill=col)
    img = vignette(img, 110 if mood == "pink" else 210).convert("RGBA")
    draw = ImageDraw.Draw(img, "RGBA")
    add_text(draw, (W // 2, 455), title, TITLE, text, stroke=2)
    if subtitle:
        add_text(draw, (W // 2, 560), subtitle, SUBTITLE, text, stroke=1)
    img.convert("RGB").save(OUT / filename, quality=95)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    bgr = ASSETS / "BGR"
    ch = ASSETS / "Character"

    make_text_card("01_title_opening.png", "TIGA SENYUMAN", "SATU HATI", "pink")
    make_scene(
        "02_school_intro.png",
        bgr / "Gerbang sekolah.png",
        speaker="Narasi",
        line="Semua dimulai dari hari biasa di sekolah.",
        chars=[(ch / "Hikari" / "Hikari Biasa.png", "right", (640, 930))],
        mood="bright",
    )
    make_scene(
        "03_three_choices.png",
        bgr / "Kelas.png",
        title="Tiga senyuman.",
        subtitle="Tiga jalan cerita.",
        chars=[
            (ch / "Hikari" / "Hikari Biasa.png", "left", (540, 850)),
            (ch / "Miyu" / "Miyu Biasa.png", "center", (540, 850)),
            (ch / "Yumi" / "Yumi Biasa.png", "right", (540, 850)),
        ],
        mood="bright",
    )
    make_scene(
        "04_hikari_dialog.png",
        bgr / "Perpustakaan.jpg",
        speaker="Hikari",
        line="Kalau kamu memilihku... jangan menyesal, ya.",
        chars=[(ch / "Hikari" / "Hikari Close Up.png", "right", (760, 980))],
        mood="bright",
    )
    make_scene(
        "05_miyu_dialog.png",
        bgr / "Kafe.jpg",
        speaker="Miyu",
        line="Aku cuma ingin kamu jujur kali ini.",
        chars=[(ch / "Miyu" / "Miyu Close Up 1.png", "right", (760, 980))],
        mood="bright",
    )
    make_scene(
        "06_yumi_dialog.png",
        bgr / "Taman Kota.jpg",
        speaker="Yumi",
        line="Senyum itu bisa menyembunyikan banyak hal.",
        chars=[(ch / "Yumi" / "Yumi Close Up.png", "right", (760, 980))],
        mood="bright",
    )
    make_text_card("07_turning_point.png", "TAPI...", "tidak semua pilihan membawa bahagia", "black")
    make_scene(
        "08_glitch_classroom.png",
        bgr / "Kelas.png",
        title="pilihanmu tersimpan",
        subtitle="bahkan saat kamu ingin lupa",
        chars=[(ch / "Miyu" / "Miyu Marah.png", "right", (690, 940))],
        mood="glitch",
    )
    make_scene(
        "09_dark_room.png",
        bgr / "Kamar pemain.png",
        speaker="???",
        line="Kenapa kamu membuka rute itu lagi?",
        chars=[(ch / "Yumi" / "Yumi Marah.png", "right", (700, 940))],
        mood="dark",
    )
    make_scene(
        "10_festival_flash.png",
        bgr / "Festival.jpg",
        title="ROMANCE",
        subtitle="MYSTERY  |  VISUAL NOVEL",
        chars=[(ch / "Hikari" / "Hikari Marah Festival.png", "right", (700, 940))],
        mood="glitch",
    )
    make_text_card("11_release_card.png", "DEMO SEGERA HADIR", "Tiga Senyuman Satu Hati", "red")
    make_text_card("12_end_card.png", "WISHLIST / FOLLOW", "pilihan terakhir ada di tanganmu", "black")


if __name__ == "__main__":
    main()
