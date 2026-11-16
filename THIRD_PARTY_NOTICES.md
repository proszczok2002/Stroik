# Third-party software

Stroik for Windows includes the unmodified x64 **NVDA Controller Client 2026.3.1**,
copyright NV Access Limited and contributors, under the **GNU Lesser General
Public License version 2.1**. The full license is in `NVDA-LGPL-2.1.txt`.

- Official binary: https://download.nvaccess.org/releases/stable/nvda_2026.3.1_controllerClient.zip
- Corresponding source: https://github.com/nvaccess/nvda/tree/release-2026.3.1
- Client source: https://github.com/nvaccess/nvda/tree/release-2026.3.1/extras/controllerClient
- Unmodified DLL SHA-256: `8c8d9e1952c8b52820988081ea58c26175e69c0fbb5a09e8f2034aae70d55f0a`

The library is dynamically loaded. To use a compatible modified controller
client, set the `STROIK_NVDA_CLIENT` environment variable to its absolute DLL
path before starting Stroik. This override bypasses the bundled library's
integrity check. You may modify/rebuild/replace that library and reverse-engineer
Stroik solely to debug modifications to it as permitted by the LGPL.
Stroik's own source remains under the MIT license.

The bundled client supports synchronous speech on NVDA 2024.1 or later.
Speech-state reporting requires NVDA 2026.3 or later. Other readers, or unavailable
completion APIs, receive an accessible guidance dialog with a Start button.
Stroik does not install an NVDA add-on or change screen-reader settings.
