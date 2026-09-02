"""Normalize generated directional enemy sheets into Unity's exact 8x5 grid."""

from pathlib import Path

import numpy as np
from PIL import Image


OUTPUT_CELL = (192, 208)
OUTPUT_GRID = (8, 5)
SAFE_SIZE = (160, 176)


def alpha_bounds(image: Image.Image):
    alpha = image.getchannel("A").point(lambda value: 255 if value > 16 else 0)
    return alpha.getbbox()


def extract_frames(source: Image.Image, row_counts: list[int]):
    frames = []
    for row, count in enumerate(row_counts):
        top = round(row * source.height / OUTPUT_GRID[1])
        bottom = round((row + 1) * source.height / OUTPUT_GRID[1])
        band = source.crop((0, top, source.width, bottom))
        alpha = np.asarray(band.getchannel("A"))
        occupied = (alpha > 32).sum(axis=0) > 3
        runs = []
        start = None
        for x, present in enumerate(np.append(occupied, False)):
            if present and start is None:
                start = x
            elif not present and start is not None:
                if x - start > 5:
                    runs.append([start, x - 1])
                start = None

        while len(runs) > count:
            closest = min(range(len(runs) - 1), key=lambda index: runs[index + 1][0] - runs[index][1])
            runs[closest][1] = runs[closest + 1][1]
            del runs[closest + 1]
        if len(runs) != count:
            raise RuntimeError(f"Row {row} contains {len(runs)} silhouettes; expected {count}")

        row_frames = []
        for left, right in runs:
            frame = band.crop((max(0, left - 3), 0, min(source.width, right + 4), band.height))
            bounds = alpha_bounds(frame)
            if bounds is None:
                raise RuntimeError(f"Empty source frame at row {row}, x={left}")
            row_frames.append(frame.crop(bounds))

        if count == 7:
            row_frames.insert(4, row_frames[3].copy())
        if len(row_frames) != OUTPUT_GRID[0]:
            raise RuntimeError(f"Row {row} produced {len(row_frames)} frames")
        frames.append(row_frames)
    return frames


def normalize(source_path: Path, output_path: Path, row_counts: list[int]):
    source = Image.open(source_path).convert("RGBA")
    frames = extract_frames(source, row_counts)
    maximum_width = max(frame.width for row in frames for frame in row)
    maximum_height = max(frame.height for row in frames for frame in row)
    scale = min(SAFE_SIZE[0] / maximum_width, SAFE_SIZE[1] / maximum_height)

    output = Image.new(
        "RGBA",
        (OUTPUT_CELL[0] * OUTPUT_GRID[0], OUTPUT_CELL[1] * OUTPUT_GRID[1]),
        (0, 0, 0, 0),
    )
    for row, row_frames in enumerate(frames):
        for column, frame in enumerate(row_frames):
            size = (
                max(1, round(frame.width * scale)),
                max(1, round(frame.height * scale)),
            )
            resized = frame.resize(size, Image.Resampling.LANCZOS)
            x = column * OUTPUT_CELL[0] + (OUTPUT_CELL[0] - resized.width) // 2
            y = row * OUTPUT_CELL[1] + OUTPUT_CELL[1] - resized.height - 12
            output.alpha_composite(resized, (x, y))

    output.save(output_path, optimize=True)


def main():
    project = Path(__file__).resolve().parents[1]
    directory = project / "Art" / "Sprites" / "Enemies" / "Directional"
    specifications = {
        "Rat": ("RatDirectional8_AlphaV2.png", [8, 8, 8, 8, 8]),
        "Wolf": ("WolfDirectional8_AlphaV2.png", [7, 7, 7, 7, 7]),
        "Goblin": ("GoblinDirectional8_Alpha.png", [8, 8, 8, 8, 8]),
        "Orc": ("OrcDirectional8_Alpha.png", [7, 7, 7, 7, 8]),
        "Troll": ("TrollDirectional8_Alpha.png", [8, 8, 8, 8, 8]),
        "Warden": ("WardenDirectional8_Alpha.png", [8, 8, 8, 8, 8]),
    }
    for enemy, (source_name, row_counts) in specifications.items():
        normalize(directory / source_name, directory / f"{enemy}Directional8.png", row_counts)
        print(f"Normalized {enemy}: 1536x1040, 8x5, cell 192x208")


if __name__ == "__main__":
    main()
