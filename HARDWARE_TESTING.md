# Driver compatibility and real-hardware testing

This is the editable GitHub tracker for the driver builds supplied to this project. It lists known local packages, not every Catalyst release ever published. Last inventory update: 2026-10-12. Patcher version: **0.3.2** (complete chipset-vendor bypass).

## Known Catalyst versions

5.12, 5.13, 6.6, 6.7, 6.12, 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7, 7.8, 7.11, 7.12, 8.2, 8.6, 8.9, 8.12, 9.1, 9.3, 9.4, 9.12, 10.2 Legacy, 11.1, 11.8, 11.12, 13.9 Legacy, 14.4, 15.7.1.

## How to update this tracker

Edit this Markdown file on GitHub. After a bench run, change **Tested on real hardware** from `Not yet` to `Yes`, set **Result** to `Pass`, `Partial`, or `Fail`, and link the issue containing your test details. A failed run still counts as tested. Keep separate results for different boards, GPU pairs, operating systems, or patcher versions in the linked issue.

`Validated` means code-pattern, binary-output and Windows checksum checks passed. It does **not** mean CrossFire worked on hardware. `Unsupported` means v0.3.2 deliberately rejects this driver platform; it does not mean that CrossFire can never work on it.

A hardware `Pass` should include successful CrossFire activation, evidence of both GPUs contributing (for example repeatable benchmark scaling), and stability observations. If only the CCC switch has been checked, use `Partial` and explain that in the issue.

## Driver matrix

The module version is the embedded file version, not the Catalyst marketing version. XP/2000 package names are retained, but automated patch validation does not establish Windows 2000 compatibility. Full source/output hashes are in [archive-reference-drivers.json](archive-reference-drivers.json).

| Catalyst | OS / architecture | Exact source package | Module file version(s) | Patcher 0.3.2 | Earliest observed layout family | Tested on real hardware | Result | Test date | Hardware / issue |
|---|---|---|---|---|---|---|---|---|---|
| 5.12 | XP / 2000 package, XP 32-bit | `5-12_xp-2k_dd_ccc_wdm_enu_28472.exe` | `ati2mtag.sys` 6.14.10.6587 | Validated | 5.12 | Not yet | — | — | — |
| 5.13 | XP / 2000 package, XP 32-bit | `5-13_xp-2k_dd_ccc_wdm_enu_29124.exe` | `ati2mtag.sys` 6.14.10.6587 | Validated | 5.12 | Not yet | — | — | — |
| 6.6 | XP / 2000 package, XP 32-bit | `6-6_xp-2k_dd_ccc_wdm_enu_33678.exe` | `ati2mtag.sys` 6.14.10.6618 | Validated | 5.12 | Not yet | — | — | — |
| 6.7 | XP / 2000 package, XP 32-bit | `6-7_xp-2k_dd_ccc_wdm_enu_34826.exe` | `ati2mtag.sys` 6.14.10.6626 | Validated | 5.12 | Yes | Pass (user reported) | 2026-10-11 | Windows XP; project-owner report |
| 6.12 | XP 32-bit | `6-12_xp_dd_ccc_wdm_enu_38463.exe` | `ati2mtag.sys` 6.14.10.6652 | Validated | 6.12 | Not yet | — | — | — |
| 7.1 | XP 32-bit | `7-1_xp_dd_ccc_wdm_enu_40211.exe` | `ati2mtag.sys` 6.14.10.6660 | Validated | 6.12 | Not yet | — | — | — |
| 7.2 | XP 32-bit | `7-2_xp_dd_ccc_wdm_enu_41238.exe` | `ati2mtag.sys` 6.14.10.6666 | Validated | 6.12 | Not yet | — | — | — |
| 7.3 | Vista 32-bit | `7-3_vista32_dd_ccc_enu_43741.exe` | `atikmdag.sys` 7.01.01.613 | Unsupported | Not established | Not yet | — | — | — |
| 7.3 | Vista 64-bit | `7-3_vista64_dd_ccc_enu_43741.exe` | `atikmdag.sys` 7.01.01.613 | Unsupported | Not established | Not yet | — | — | — |
| 7.3 | XP 32-bit | `7-3_xp_dd_ccc_wdm_enu_43737.exe` | `ati2mtag.sys` 6.14.10.6677 | Validated | 6.12 | Not yet | — | — | — |
| 7.3 | XP 64-bit | `7-3_xp64_dd_ccc_enu_44234-[guru3d.com].exe` | `ati2mtag.sys` 6.14.10.6677 | Unsupported | Not established | Not yet | — | — | — |
| 7.4 | Vista 32-bit | `7-4_vista32_dd_ccc_enu_44985.exe` | `atikmdag.sys` 7.01.01.622 | Unsupported | Not established | Not yet | — | — | — |
| 7.5 | Vista 32-bit | `7-5_vista32_dd_ccc_enu_46757.exe` | `atikmdag.sys` 7.01.01.646 | Unsupported | Not established | Not yet | — | — | — |
| 7.5 | Vista 64-bit | `7-5_vista64_dd_ccc_enu_46757.exe` | `atikmdag.sys` 7.01.01.646 | Unsupported | Not established | Not yet | — | — | — |
| 7.5 | XP 32-bit | `7-5_xp_dd_ccc_wdm_enu_46743.exe` | `ati2mtag.sys` 6.14.10.6698 | Validated | 7.5 | Not yet | — | — | — |
| 7.6 | Vista 32-bit | `7-6_vista32_dd_ccc_wdm_enu_48645.exe` | `atikmdag.sys` 7.01.01.646 | Unsupported | Not established | Not yet | — | — | — |
| 7.6 | XP 32-bit | `7-6_xp_dd_ccc_wdm_enu_48640.exe` | `ati2mtag.sys` 6.14.10.6698 | Validated | 7.6 | Not yet | — | — | — |
| 7.7 | XP 32-bit | `7-7_xp32_dd_ccc_wdm_enu_49709.exe` | `ati2mtag.sys` 6.14.10.6706 | Validated | 7.7 | Not yet | — | — | — |
| 7.8 | XP 32-bit | `7-8_xp32_dd_ccc_wdm_enu_50960.exe` | `ati2mtag.sys` 6.14.10.6715 | Validated | 7.7 | Not yet | — | — | — |
| 7.11 | XP 32-bit | `7-11_xp32_dd_ccc_wdm_enu_54435.exe` | `ati2mtag.sys` 6.14.10.6734 | Validated | 7.11 | Not yet | — | — | — |
| 7.12 | XP 32-bit | `7-12_xp32_dd_ccc_wdm_enu_55811.exe` | `ati2mtag.sys` 6.14.10.6755 | Validated | 7.12 | Not yet | — | — | — |
| 8.2 | Vista 64-bit | `8-2_vista64_dd_58134.exe` | `atikmdag.sys` 7.01.01.730 | Unsupported | Not established | Not yet | — | — | — |
| 8.6 | Vista 32-bit | `8-6_vista32_dd_ccc_wdm_enu_64789.exe` | `atikmdag.sys` 7.01.01.788 | Unsupported | Not established | Not yet | — | — | — |
| 8.6 | XP 32-bit | `8-6_xp32_dd_ccc_wdm_enu_64783.exe` | `ati2mtag.sys` 6.14.10.6822 | Validated | 8.6 | Not yet | — | — | — |
| 8.9 | XP 32-bit | `8-9_xp32_dd_ccc_wdm_enu_68898.exe` | `ati2mtag.sys` 6.14.10.6860 | Validated | 8.6 | Not yet | — | — | — |
| 8.12 | Vista 64-bit | `8-12_vista64_dd_ccc_wdm_enu_72276.exe` | `atikmdag.sys` 7.01.01.849 | Unsupported | Not established | Not yet | — | — | — |
| 8.12 | XP 32-bit | `8-12_xp32_dd_ccc_wdm_enu_72271.exe` | `ati2mtag.sys` 6.14.10.6891 | Validated | 8.6 | Yes | Pass (user reported) | 2026-10-11 | Windows XP; project-owner report |
| 9.1 | XP 32-bit | `9-1_xp32_dd_ccc_wdm_enu_74230.exe` | `ati2mtag.sys` 6.14.10.6903 | Validated | 8.6 | Not yet | — | — | — |
| 9.3 | XP 32-bit | `9-3_xp32_dd_ccc_wdm_enu.exe` | `ati2mtag.sys` 6.14.10.6925 | Validated | 8.6 | Not yet | — | — | — |
| 9.4 | XP 32-bit | `9-4_xp32_dd_ccc_wdm_enu.exe` | `ati2mtag.sys` 6.14.10.6936 | Validated | 8.6 | Not yet | — | — | — |
| 9.12 | Vista / Windows 7 32-bit | `9-12_vista32_win7_32_dd_ccc_wdm_enu.exe` | `atikmdag.sys` 8.01.01.984 | Unsupported | Not established | Not yet | — | — | — |
| 9.12 | Vista / Windows 7 64-bit | `9-12_vista64_win7_64_dd_ccc_wdm_enu.exe` | `atikmdag.sys` 8.01.01.984 | Unsupported | Not established | Not yet | — | — | — |
| 9.12 | XP 32-bit | `9-12_xp32_dd_ccc_wdm_enu.exe` | `ati2mtag.sys` 6.14.10.7028 | Validated | 8.6 | Not yet | — | — | — |
| 9.12 | XP 64-bit | `9-12_xp64_dd_ccc_enu.exe` | `ati2mtag.sys` 6.14.10.7028 | Unsupported | Not established | Not yet | — | — | — |
| 10.2 Legacy | Vista 32-bit | `10-2_legacy_vista32-64_dd_ccc.exe` | `atikmdag.sys` 8.01.01.882 | Unsupported | Not established | Not yet | — | — | — |
| 10.2 Legacy | Vista 64-bit | `10-2_legacy_vista32-64_dd_ccc.exe` | `atikmdag.sys` 8.01.01.882 | Unsupported | Not established | Not yet | — | — | — |
| 10.2 Legacy | XP 32-bit | `10-2_legacy_xp32-64_dd_ccc.exe` | `ati2mtag.sys` 6.14.10.6925 | Validated | 8.6 | Not yet | — | — | — |
| 10.2 Legacy | XP 64-bit | `10-2_legacy_xp32-64_dd_ccc.exe` | `ati2mtag.sys` 6.14.10.6925 | Unsupported | Not established | Not yet | — | — | — |
| 11.1 | Vista / Windows 7 64-bit | `11-1_vista64_win7_64_dd_ccc_enu.exe` | `atikmdag.sys` 8.01.01.1114<br>`atikmpag.sys` 8.14.01.6178 | Unsupported | Not established | Not yet | — | — | — |
| 11.8 | Vista / Windows 7 64-bit | `11-8_vista64_win7_64_dd_ccc_ocl.exe` | `atikmdag.sys` 8.01.01.1178<br>`atikmpag.sys` 8.14.01.6226 | Unsupported | Not established | Not yet | — | — | — |
| 11.12 | Vista / Windows 7 64-bit | `11-12_vista64_win7_64_dd_ccc_ocl.exe` | `atikmdag.sys` 8.01.01.1215<br>`atikmpag.sys` 8.14.01.6243 | Unsupported | Not established | Not yet | — | — | — |
| 13.9 Legacy | Vista / Windows 7 64-bit | `13-9-legacy_vista_win7_64_dd_ccc_whql.exe` | `atikmdag.sys` 8.01.01.1248<br>`atikmpag.sys` 8.14.01.6264 | Unsupported | Not established | Not yet | — | — | — |
| 14.4 | XP 32-bit | `14-4-xp32-64-dd-ccc-pack2.exe` | `ati2mtag.sys` 6.14.10.7279 | Validated | 8.6 | Not yet | — | — | — |
| 14.4 | XP 64-bit | `14-4-xp32-64-dd-ccc-pack2.exe` | `ati2mtag.sys` 6.14.10.7279 | Unsupported | Not established | Not yet | — | — | — |
| 15.7.1 | Windows 7 / 8.1 32-bit | `whql-win8.1-win7-catalyst-15.7.1-oct30.exe` | `atikmdag.sys` 8.01.01.1500<br>`atikmpag.sys` 8.14.01.6463 | Unsupported | Not established | Not yet | — | — | — |
| 15.7.1 | Windows 7 / 8.1 64-bit | `whql-win8.1-win7-catalyst-15.7.1-oct30.exe` | `atikmdag.sys` 8.01.01.1500<br>`atikmpag.sys` 8.14.01.6463 | Unsupported | Not established | Not yet | — | — | — |

## Other supplied packages

These capture/WDM-only packages contain no display miniport and cannot be used to validate this CrossFire patch.

| Package | Display-driver patch status | Tested on real hardware | Result / issue |
|---|---|---|---|
| `10-2_legacy_xp32-64_wdm.exe` | No display miniport | Not applicable | — |
| `10-2_legacy_vista32-64_wdm.exe` | No display miniport | Not applicable | — |

## Reported hardware results

Catalyst 6.7 and 8.12 were reported working with the complete chipset-bypass patcher on Windows XP on 2026-10-11. Results were supplied by the project owner on 2026-10-12. Benchmark values, GPU configuration and the exact patcher executable version were not recorded in this report.

## Copyable hardware test report

Open a GitHub issue for a run and paste this template, then link it in the matrix.

```markdown
### Driver and patch
- Catalyst version and exact package:
- OS, architecture and service pack:
- Patcher version:
- Original SHA-256:
- Patched SHA-256:

### Hardware
- Motherboard, BIOS version and chipset:
- GPU 1 / GPU 2 (model, memory size and BIOS):
- Master/slave cards, bridge or external dongle:

### Test results
- Test date:
- Original-driver behavior on this board:
- Patched-driver behavior:
- CCC CrossFire activation:
- Single-GPU benchmark / CrossFire benchmark (same settings):
- Repeated runs, stability and artifacts:
- Comparison with CrossFire-supported board, if available:
- Overall result: Pass / Partial / Fail
- Logs, screenshots or other evidence:
```
