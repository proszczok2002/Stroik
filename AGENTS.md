# Stroik release maintenance

These instructions record the project owner's standing preference for every new release.

- Keep the newest Stroik.exe, Stroik.exe.sha256, versioned ZIP, current PL/EN guides, license and build-version.txt at the top level of dist.
- Run build-release.ps1 when increasing the version. It archives the previous release files, old bin/obj output and verification copies under the project-root niepotrzebne folder (F:\stroik\niepotrzebne) after a successful new build. Preserve that archive; do not delete it automatically.
- Keep the editable current source, tests, build script, local .NET SDK, Git repository and documentation so development can continue. Earlier source revisions remain in Git history.
- Before publishing, run tests, build the new release, push source, wait for GitHub Actions, then publish the matching tag and GitHub Release with EXE, SHA-256 and ZIP. Download the public files and compare their sizes and hashes with local output.
- Record the release, verification results and any untested hardware or updater path in STAN_PRAC.md.
