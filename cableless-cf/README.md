# Cable-less CrossFire for Radeon X1800 XT

A modified video BIOS for the **Radeon X1800 XT (R520)** that lets you enable CrossFire **without the external DVI dongle cable and without a CrossFire Edition master card**.

The mod changes the card's PCI device ID from `0x7100` to `0x7109`. With that ID, the Catalyst driver offers CrossFire over the PCI Express bus alone, so two regular X1800 XT cards can be paired.

> [!WARNING]
> Flashing a video BIOS can brick your card. You do this at your own risk. Back up your original BIOS first, and keep a second (PCI) graphics card or a bootable recovery setup ready.

---

## Background

On the X1800 series, CrossFire normally needs:

1. A **CrossFire Edition master card** with the compositing engine, and
2. An **external dongle cable** that connects the master card's DVI output to the slave card.

Regular X1800 XT cards report device ID `0x7100`, and the driver only offers CrossFire for them in that master/slave cable configuration. Changing the ID to `0x7109` makes the driver allow CrossFire on a pair of regular cards with no cable connected, so frame data goes over PCIe instead.

## Files

| File | Description |
|------|-------------|
| `Club3D.X1800XT.512._051018.rom` | Original, unmodified BIOS |
| `Club3D.X1800XT.512._051018_CFMod.rom` | Modified BIOS (device ID `0x7109`) |

### Source BIOS

| Property | Value |
|----------|-------|
| Card | Club3D Radeon X1800 XT, 512 MB, PCI Express |
| BIOS P/N | `113-A52001-102` |
| Build string | `R520 XT P/N 113-A52001-102 BIOS 1.33V 600e/700m` |
| BIOS date | `10/18/05 13:12` |
| ATOMBIOS | `ATOMBIOSBK-ATI VER009.012.004.002.015963` |
| Clocks | 600 MHz core / 700 MHz memory |
| Subsystem ID | `1002:0B12` |
| Image size | 62,976 bytes (`0x7B` × 512) |

### Checksums

| File | MD5 | SHA-256 |
|------|-----|---------|
| Original | `01a38adc0705e80224fce386f4bdd3b6` | `5ddce42fa58d755ac66d9c493c1394939e5d86ce437ff2d4e5df2af0df2edcb2` |
| CFMod | `49f76eee6507bee08c4162d240e14082` | `44f1d930d0a928cbb13c8457f59193e8d9413a946dbf5f4e14c47999957309a0` |

## What was changed

Only five bytes differ between the two images:

| Offset | Original | Modified | Meaning |
|--------|----------|----------|---------|
| `0x0021` | `0x5A` | `0x3B` | Legacy 8-bit option ROM checksum (whole image must sum to `0x00`) |
| `0x007A` | `0x00` | `0x09` | Device ID in ATI's BIOS info block at `0x0076` (`1002 / 0B12 / 7100` → `7109`) |
| `0x0236` | `0x00` | `0x09` | Device ID in the PCI Data Structure (`PCIR` at `0x0230`), `0x7100` → `0x7109` |
| `0x0255`–`0x0256` | `8F 5B` | `AF 48` | 16-bit value in the ATOM CRC block (`usCRC_BlockOffset` = `0x0255` in the ATOM ROM header at `0x020A`), updated for the new image |

All other data, including clocks, voltages, memory timings and ATOM tables, is identical to the stock BIOS.

## Requirements

- Two Radeon X1800 XT cards (tested with the Club3D 512 MB model above)
- A motherboard with two PCIe x16 slots
- A Catalyst driver version that supports CrossFire on R5xx cards
- DOS boot media with **ATIFlash** (the ATI/AMD BIOS flash utility)

## Flashing

1. Boot into DOS with ATIFlash available.
2. List the adapters and note their indexes:
   ```
   atiflash -i
   ```
3. Back up the original BIOS of **each** card:
   ```
   atiflash -s 0 card0.rom
   atiflash -s 1 card1.rom
   ```
4. Flash the modified BIOS. `-f` is needed because the device ID in the new image doesn't match the one the card currently reports:
   ```
   atiflash -p 0 Club3D.X1800XT.512._051018_CFMod.rom -f
   atiflash -p 1 Club3D.X1800XT.512._051018_CFMod.rom -f
   ```
5. Power off completely, then boot back into Windows.

> [!NOTE]
> Only flash this image onto the **same card model and BIOS revision** (`113-A52001-102`). For other X1800 XT cards, apply the same changes to your own BIOS dump instead (see below).

## Usage

1. Install both cards. **Don't** connect a CrossFire dongle.
2. Install the Catalyst driver. Both cards should now show up with device ID `1002:7109`.
3. Open Catalyst Control Center → **CrossFire** and enable it.

## Tested platforms

| Chipset | Driver | Result |
|---------|--------|--------|
| ATI Radeon Xpress 200 CrossFire (RD480) | Catalyst | ✅ CrossFire can be enabled without the external cable |
| NVIDIA nForce4 | Catalyst | ✅ CrossFire can be enabled without the external cable |

Reports from other chipsets and driver versions are welcome. Please open an issue with your board, chipset, Catalyst version and results.

## Applying the mod to your own BIOS

1. Dump your card's BIOS (`atiflash -s 0 mybios.rom`).
2. Find the `PCIR` signature (pointer at offset `0x18`). The device ID is at `PCIR + 6` (little-endian). Change `00 71` to `09 71`.
3. Change the second copy of the device ID in ATI's BIOS info block as well (at `0x007A` in the BIOS above, right after vendor `0x1002` and subsystem ID).
4. Update the ATOM CRC block value (its offset is stored at `ATOM header + 0x0E`).
5. Fix the 8-bit checksum at `0x21` so that all bytes of the image add up to `0x00` (mod 256).
6. Compare your result against the table above before flashing.

## Restoring the original BIOS

```
atiflash -p 0 card0.rom -f
atiflash -p 1 card1.rom -f
```

## Disclaimer

This project is not affiliated with AMD/ATI or Club3D. All trademarks belong to their respective owners. The BIOS images are provided for preservation and research purposes on legacy hardware. There is no warranty of any kind.
