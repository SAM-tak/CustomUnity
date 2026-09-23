# CustomUnity

Some Reusable staffs for Unity from my game project.

for unity package manager :

`https://github.com/SAM-tak/CustomUnity.git?path=Assets/CustomUnity`

[![openupm](https://img.shields.io/npm/v/net.sam-tak.customunity?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/net.sam-tak.customunity/)

## Building the log wrapper plugins

`CustomUnity.Log` and `CustomUnity.Editor.Log` are the only assemblies in this repository
that are compiled outside of Unity. The built dlls are committed under
`Assets/CustomUnity/Plugins` and `Assets/CustomUnity/Editor/Plugins`, so a plain clone
needs no build step. Rebuild them whenever you touch the sources under
`CustomUnity.Log/` or `CustomUnity.Editor.Log/`:

```sh
dotnet build CustomUnity.Log.slnx -c Release
```

The Release build drops the dlls into their `Assets` plugin folders; commit them together
with the source change.

### Why a precompiled plugin instead of an .asmdef

Double-clicking a message in Unity's Console opens the first stack frame that carries
file and line information, and that information comes from the assembly's debug symbols.
These assemblies are built with `DebugType=none`, so the wrapper frames carry none and
the Console jumps to the *call site* instead of into the wrapper. `[HideInCallstack]`
only removes the frame from the displayed callstack
([documented behaviour](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/HideInCallstackAttribute.html)),
so it cannot replace this. Do not move these sources into an `.asmdef` assembly.

### Unity reference resolution

`Unity.props` locates the editor install from `ProjectSettings/ProjectVersion.txt`. If
that version is not installed, or you are not on Windows, point the build at an editor
explicitly:

```sh
dotnet build CustomUnity.Log.slnx -c Release -p:UnityEditorDir="<...>/Unity/Hub/Editor/<version>/Editor"
# or set the UNITY_EDITOR_DIR environment variable
```

`UnityEngine.UI.dll` is taken from `Library/ScriptAssemblies`, so the project must have
been imported by the editor at least once.

## Releasing

Unity 6.3 and newer verify a signature on every tarball package. A package installed
from OpenUPM without one reports *"Unity can't verify this package because it doesn't
have a signature."* Installing from the git URL above is unaffected - only the registry
tarball needs signing.

The signature covers the exact bytes of the tarball, so it cannot be committed here:
OpenUPM has to distribute a tarball we signed ourselves rather than one it built from
the tag. `.github/workflows/release.yml` does that on every `v*` tag - it checks the tag
against `version` in `Assets/CustomUnity/package.json`, signs the package with Unity's
UPM CLI, verifies the result contains `package/.attestation.p7m`, and attaches the
`.tgz` to the GitHub Release.

To release: bump `version` in `Assets/CustomUnity/package.json`, then push a matching
`v<version>` tag.

One-time setup:

- Create a Unity Cloud service account in the organization that owns the package and
  give it the **Package Manager Package Signer** role.
- Add its credentials as repository secrets `UPM_SERVICE_ACCOUNT_KEY_ID` and
  `UPM_SERVICE_ACCOUNT_KEY_SECRET`, and the organization ID as `UPM_ORG_ID`.
- Switch the package's OpenUPM entry to `trackingMode: githubRelease` so OpenUPM
  downloads the signed asset instead of packing the tag itself. Versions released
  before this stay unsigned; there is no way to sign them after the fact.

To sign locally instead, install the CLI with
`curl -fsSL https://cdn.packages.unity.com/upm-cli/install.sh | sh` (Windows:
`irm https://cdn.packages.unity.com/upm-cli/install.ps1 | iex`) and run
`upm pack Assets/CustomUnity --organization-id <org id> --destination dist`. The Package
Manager window can also export a signed tarball, but it only lists packages it knows
about, which does not include a package folder living under `Assets`.
